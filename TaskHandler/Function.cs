using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Npgsql;
using System.Text.Json;
using System.Text.Json.Serialization;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TaskHandler;

public class TaskItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; set; }
}

public class Function
{
    private readonly AmazonDynamoDBClient _dynamoDbClient;
    private readonly string _postgresConnectionString;

    public Function()
    {
        // DynamoDB Local Configuration
        var dynamoDbConfig = new AmazonDynamoDBConfig
        {
            ServiceURL = Environment.GetEnvironmentVariable("DYNAMODB_ENDPOINT") ?? "http://host.docker.internal:4566"
        };
        _dynamoDbClient = new AmazonDynamoDBClient(dynamoDbConfig);

        // Postgres Local Configuration
        _postgresConnectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING") 
            ?? "Host=host.docker.internal;Port=5432;Database=tasksdb;Username=postgres;Password=postgres";
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            context.Logger.LogInformation($"Handling request: {request.HttpMethod} {request.Path}");

            if (request.HttpMethod == "POST")
            {
                return await HandlePostTaskAsync(request, context);
            }
            else if (request.HttpMethod == "GET")
            {
                return await HandleGetTasksAsync(context);
            }
            else
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 405,
                    Body = "Method Not Allowed"
                };
            }
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error processing request: {ex.Message}");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new { error = ex.Message })
            };
        }
    }

    private async Task<APIGatewayProxyResponse> HandlePostTaskAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        if (string.IsNullOrEmpty(request.Body))
        {
            return new APIGatewayProxyResponse { StatusCode = 400, Body = "Request body is empty" };
        }

        var task = JsonSerializer.Deserialize<TaskItem>(request.Body);
        if (task == null)
        {
            return new APIGatewayProxyResponse { StatusCode = 400, Body = "Invalid JSON" };
        }

        if (task.Id == 0)
        {
            task.Id = new Random().Next(1, int.MaxValue);
        }

        var table = Table.LoadTable(_dynamoDbClient, "Tasks");
        var document = new Document();
        document["Id"] = task.Id;
        document["Title"] = task.Title ?? "";
        document["Description"] = task.Description ?? "";
        document["IsCompleted"] = task.IsCompleted;

        await table.PutItemAsync(document);

        context.Logger.LogInformation($"Task saved to DynamoDB: {task.Id}");

        return new APIGatewayProxyResponse
        {
            StatusCode = 201,
            Body = JsonSerializer.Serialize(task),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    private async Task<APIGatewayProxyResponse> HandleGetTasksAsync(ILambdaContext context)
    {
        var tasks = new List<TaskItem>();

        using var connection = new NpgsqlConnection(_postgresConnectionString);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand("SELECT Id, Title, Description, IsCompleted FROM Tasks", connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            tasks.Add(new TaskItem
            {
                Id = reader.GetInt32(0),
                Title = !reader.IsDBNull(1) ? reader.GetString(1) : null,
                Description = !reader.IsDBNull(2) ? reader.GetString(2) : null,
                IsCompleted = reader.GetBoolean(3)
            });
        }

        context.Logger.LogInformation($"Fetched {tasks.Count} tasks from Postgres");

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(tasks),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
}
