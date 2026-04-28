# The LocalStack Chronicles: .NET 8 & PostgreSQL Lambda

This project is a hands-on demonstration of building and debugging an AWS Lambda function in a completely local environment. It uses **LocalStack** to emulate AWS services and **Docker** to host a PostgreSQL database.

## 📖 The "Survival Guide" Blog Series
This repository is the code companion to my Medium series, where I break down the 5 major hurdles I faced while building this:
- [Chapter 1: The Silence of the Terminal](LINK_HERE)
- [Chapter 2: The Three-Second Death Clock](LINK_HERE)
- [Chapter 3: The Ghost in the Machine](LINK_HERE)
- [Chapter 4: Identity Crisis & The Missing 'S'](LINK_HERE)
- [Chapter 5: The Final Type War](LINK_HERE)

---

## 🛠️ Tech Stack
- **Runtime:** .NET 8 (C#)
- **Cloud Emulation:** LocalStack (Lambda, Gateway)
- **Database:** PostgreSQL 17
- **Tools:** Visual Studio 2022, AWS CLI, Docker Compose

---

## 🚀 Getting Started

### 1. Clone & Spin up Infrastructure
Clone the repo and start the containers using Docker Compose:
```powershell
git clone <your-gitlab-url>
cd <repo-folder>
docker-compose up -d

### Prepare the Database

Since this is a local setup, you need to initialize the table. Run this command to create the schema:
docker exec -i postgres_db psql -U admin -d taskdb -c "
CREATE TABLE IF NOT EXISTS tasks (
    id SERIAL PRIMARY KEY,
    title TEXT NOT NULL,
    description TEXT,
    iscompleted BOOLEAN DEFAULT FALSE
);"


### Deploy to LocalStack

Open the solution in Visual Studio, build the project, and then use the following command to package and create the function:
# Package the Lambda

dotnet lambda package -o bin/Release/net8.0/TaskHandler.zip

# Create the function in LocalStack
aws lambda create-function `
    --endpoint-url http://localhost:4566 `
    --function-name TaskHandler `
    --runtime dotnet8 `
    --handler TaskHandler::TaskHandler.Function::FunctionHandler `


## Invoke and Test
aws lambda invoke --endpoint-url http://localhost:4566 --function-name TaskHandler --payload ([Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes('{"httpMethod": "GET"}'))) response.json

    --zip-file fileb://bin/Release/net8.0/TaskHandler.zip `
    --timeout 30 `
    --environment "Variables={POSTGRES_CONNECTION_STRING='Host=host.docker.internal;Port=5432;Database=taskdb;Username=admin;Password=password'}"
