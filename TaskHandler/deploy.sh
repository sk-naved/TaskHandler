#!/bin/bash
# 1. Package the code
dotnet lambda package -o bin/Release/net10.0/TaskHandler.zip

# 2. Delete the old function (if it exists) to ensure a fresh start
awslocal lambda delete-function --function-name TaskHandler 2>/dev/null

# 3. Create the function in LocalStack
awslocal lambda create-function \
    --function-name TaskHandler \
    --runtime dotnet8 \
    --role arn:aws:iam::000000000000:role/lambda-role \
    --handler TaskHandler::TaskHandler.Function::FunctionHandler \
    --zip-file fileb://bin/Release/net10.0/TaskHandler.zip \
    --environment "Variables={DYNAMO_URL=http://host.docker.internal:4566,DB_HOST=host.docker.internal}"

echo "🚀 Lambda Deployed to LocalStack!"