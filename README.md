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
