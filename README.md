# FU-News Management System - Assignment 01

A comprehensive News Management and Analytics System built for university environments (FU-News). The solution features a Distributed Modular Architecture backend containerized with Docker, balanced via Nginx, and an interactive web frontend built on .NET 8.

This is the Github link : 
https://github.com/DoThang-254/Assigment-01.git

This is the detail requirements : 
https://drive.google.com/file/d/1pyGHJy4AZLFIcoBaCgNc3dGrW503T1bH/view?usp=sharing

This is the demo of Devops : 
https://docs.google.com/document/d/1t6TvbyGMaiL6wQJD9xEt4cIsFA1RMhWj38LiNmBvEz8/edit?usp=sharing


## 📁 System Architecture & Directory Layout

Below is the duster-free repository tree mapping out the core application layers, infrastructure setup, and deployment files:

### 📂 System Architecture & Directory Layout

Below is the cluster-free repository tree mapping out the core application layers, infrastructure setup, and deployment files:

```
Assigment-01/
├── .github/workflows/                             # CI/CD Pipelines (GitHub Actions)
├── docker-compose.yml                             # Docker Multi-Container orchestration tool
├── nginx.conf                                     # Nginx Reverse Proxy & Load Balancer configuration
├── DoQuangThang_SE1885_A01_BE/                    # [BACKEND SYSTEM SOLUTION]
│   ├── AIAPI/                                     # AI Models and Core Intelligent Services integration
│   ├── AnalyticsAPI/                              # Data telemetry and metrics processing API
│   ├── BusinessLogic/                             # Core business rules handling (Services & Dtos)
│   ├── DataAccess/                                # Database persistence (Models, Repositories, Migrations)
│   ├── Presentation/                              # Primary API Gateway / Public Gateway Endpoints
│   └── WorkerService/                             # Background Services & Scheduled Tasks
└── DoQuangThang_SE1885_A01_FE/                    # [FRONTEND SYSTEM SOLUTION]
    └── DoQuangThang_SE1885_A01_FE/                # Client Web Application (User Interface)
        ├── Pages/                                 # Dynamic Razor views (Accounts, News, Categories, Tags...)
        ├── Models/                                # Client-side Data Transfer / Binding Models
        ├── Services/                              # Dedicated API Consumers / HttpClients
        └── Hubs/                                  # Real-time WebSockets communication (SignalR)
```


🚀 Local Setup & Execution Guide
Follow these steps sequentially to configure, initialize, and execute the entire platform on your local machine.

## Prerequisites
.NET 8.0 SDK or higher.

SQL Server LocalDB or SQL Server Management Studio (SSMS).

## Step 1: Database Connection Configuration
Locate the appsettings.json configuration file inside the main executing API projects (Presentation and AnalyticsAPI). Update the connection string values to target your host SQL database instance:

JSON
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_LOCAL_SERVER_NAME;Database=FUNewsDB;Trusted_Connection=True;TrustServerCertificate=True;"
}

## Step 2: Initialize Database Migrations
Open your terminal window at the DataAccess project path directory and apply pending EF Core migrations to scaffold the database schema automatically:

Bash
dotnet ef database update

## Step 3: Launching the Applications
To spin up the system correctly, you need to launch the Backend API gateway cluster first, followed by the Front-End client application.

1. Spin up the Backend Server
Navigate to the primary Presentation entry point assembly and run the host thread:

Bash
cd DoQuangThang_SE1885_A01_BE/Presentation
dotnet run

cd DoQuangThang_SE1885_A01_BE/AIAPI
dotnet run

cd DoQuangThang_SE1885_A01_BE/AnalyticsAPI
dotnet run

2. Spin up the Frontend UI Client
Open a secondary terminal tab, navigate to the web client project root, and execute the web process:

Bash
cd DoQuangThang_SE1885_A01_FE/DoQuangThang_SE1885_A01_FE
dotnet run