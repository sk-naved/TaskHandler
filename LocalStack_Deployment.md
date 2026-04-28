# Deployment Guide (LocalStack)

This document provides the necessary `awslocal` and .NET commands to package and deploy your Task Handler Lambda function locally.

## Prerequisites
- **LocalStack**: Make sure LocalStack is running.
- **awslocal**: Ensure the `awslocal` CLI wrapper is installed (`pip install awscli-local`).
- **Dependencies**: DynamoDB and Postgres should be accessible at `http://host.docker.internal:4566` and `host.docker.internal:5432` respectively.

## 1. Package the Lambda Function

First, publish the application using `dotnet` and package it into a zip file.
Open a PowerShell terminal and run:

```powershell
cd c:\Workspace\TaskHandler\TaskHandler

# Publish the project
dotnet publish -c Release -o ./publish

# Zip the published output
Compress-Archive -Path ./publish/* -DestinationPath ./publish/function.zip -Force
```

## 2. Deploy to LocalStack

Once the zip file is ready, create the Lambda function using `awslocal`. Since the project targets `net10.0`, you may need `provided.al2023` runtime or simply `dotnet10` depending on LocalStack's latest supported runtime strings. For the `dotnet` managed runtime, it usually looks like `dotnet8` or similar if supported. 

```powershell
awslocal lambda create-function `
    --function-name TaskHandler `
    --runtime dotnet8 `
    --handler TaskHandler::TaskHandler.Function::FunctionHandler `
    --role arn:aws:iam::000000000000:role/lambda-ex `
    --zip-file fileb://publish/function.zip `
    --environment "Variables={DYNAMODB_ENDPOINT=http://host.docker.internal:4566,POSTGRES_CONNECTION_STRING=Host=host.docker.internal;Port=5432;Database=tasksdb;Username=postgres;Password=postgres}"
```
*(Note: If LocalStack errors regarding `dotnet8` runtime not being compatible with the built assembly, consider changing `<TargetFramework>net10.0</TargetFramework>` in `TaskHandler.csproj` to `<TargetFramework>net8.0</TargetFramework>` since that is the official Lambda AWS managed runtime).*

## 3. Creating the DynamoDB Table (Optional)

If your Tasks table doesn't exist yet, you can create it via:

```powershell
awslocal dynamodb create-table `
    --table-name Tasks `
    --attribute-definitions AttributeName=Id,AttributeType=S `
    --key-schema AttributeName=Id,KeyType=HASH `
    --billing-mode PAY_PER_REQUEST
```

## 4. Invoking the Lambda Function

**To test the POST request (DynamoDB):**
```powershell
awslocal lambda invoke `
    --function-name TaskHandler `
    --cli-binary-format raw-in-base64-out `
    --payload '{"httpMethod": "POST", "body": "{\"title\":\"My Task\", \"description\":\"Task details\", \"isCompleted\":false}"}' `
    output.json

cat output.json
```

**To test the GET request (Postgres):**
```powershell
awslocal lambda invoke `
    --function-name TaskHandler `
    --cli-binary-format raw-in-base64-out `
    --payload '{"httpMethod": "GET"}' `
    output.json

cat output.json
```
