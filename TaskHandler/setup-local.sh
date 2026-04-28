# 1. Create DynamoDB Table
awslocal dynamodb create-table --table-name Tasks --attribute-definitions AttributeName=Id,AttributeType=S --key-schema AttributeName=Id,KeyType=HASH --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5

# 2. Deploy the C# code (After you build in VS)
awslocal lambda create-function --function-name TaskHandler --runtime dotnet8 --role arn:aws:iam::000000000000:role/lambda-role --handler TaskHandler::TaskHandler.Function::FunctionHandler --zip-file fileb://bin/Release/net8.0/publish/TaskHandler.zip

# 3. Link to API Gateway
awslocal apigateway create-rest-api --name TaskAPI
# (Further commands to create resource/method/integration)