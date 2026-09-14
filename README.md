# CoffeeNChill Canteen Management System

A cloud-based Canteen Management System for CoffeeNChill, built with Azure Functions, Azure Table Storage, and Azure File Share (via Azurite emulation locally). Part 1 delivers the cloud foundation for menu management and staff document handling.

---

## Project Overview

CoffeeNChill replaces paper menus and filing-cabinet documents with:

- Menu Items stored dynamically in Azure Table Storage so prices and availability can be updated instantly across campus.
- Staff Documents (barista recipe sheets, equipment cleaning manuals, health & safety policies) stored in a centralized Azure File Share.

The system runs locally in isolated Docker containers via Azurite emulation, and the Azure Functions app is containerized and published to Docker Hub.

---

## Repository Structure

CoffeeNChill/
├── CoffeeNChill.sln
├── README.md
├── .gitignore
├── docs/
│   └── CoffeeNChill.postman_collection.json
└── CoffeeNChill/
    ├── Program.cs
    ├── host.json
    ├── CoffeeNChill.csproj
    ├── local.settings.json          (not committed — see .gitignore)
    └── Functions/
        ├── CreateMenuItem.cs
        ├── DeleteMenuItems.cs
        ├── GetAllMenuItems.cs
        ├── GetMenuItemsByCategory.cs
        ├── UpdateMenuItems.cs
        ├── MenuItem.cs
        ├── TableStorageService.cs
        ├── UploadStaffDocument.cs
        ├── ListStaffDocuments.cs
        └── DownloadStaffDocument.cs

---

## Prerequisites

- Docker Desktop
- Microsoft Azure Storage Explorer (optional, for inspecting storage)
- Git
- .NET 8 SDK (or .NET 10 with the isolated worker model)
- Azure Functions Core Tools v4
- Postman (for testing HTTP endpoints)

---

## Azurite Setup (Local Azure Storage Emulator)

Azurite emulates Blob, Queue, and Table storage locally.

### Docker Image

mikhail10x/coffeenchill-azurite:v1.0

### Run Azurite

Create a local directory for persistent data:

mkdir C:\CoffeeNChill
mkdir C:\CoffeeNChill\azurite-data

Start the container:

docker run -d --name coffeenchill-azurite -p 10000:10000 -p 10001:10001 -p 10002:10002 -v C:\CoffeeNChill\azurite-data:/data mikhail10x/coffeenchill-azurite:v1.0

### Storage Ports

| Service         | Port  |
| --------------- | ----: |
| Blob Storage    | 10000 |
| Queue Storage   | 10001 |
| Table Storage   | 10002 |

Verify the container is running:

docker ps
docker logs coffeenchill-azurite

---

## Storage Resources

### Azure Table: MenuItems

| Property      | Type    | Description                          |
| ------------- | ------- | ------------------------------------ |
| PartitionKey  | String  | Menu category (e.g., "Hot Drinks")   |
| RowKey        | String  | Unique item SKU / ID (e.g., COF-001) |
| Name          | String  | Item name (e.g., "Cappuccino")       |
| Description   | String  | Short menu description               |
| Price         | Double  | Item price                           |
| IsAvailable   | Boolean | Availability status                  |

### Sample Menu Data

| Category    | SKU     | Name                | Price | Available |
| ----------- | ------- | ------------------- | ----: | --------- |
| Hot Drinks  | COF-001 | Cappuccino          | 35.00 | true      |
| Cold Drinks | DRK-001 | Iced Coffee         | 32.00 | true      |
| Pastries    | PAS-001 | Chocolate Croissant | 28.00 | true      |

### Azure File Share: staff-docs

Stores operational staff documents (PDFs, manuals, policies). Note: Azurite does not support Azure File Shares, so a live Azure Storage account is used for this service during development. The connection string is stored under the FileShareStorage key in local.settings.json.

---

## Local Functions Configuration

In CoffeeNChill/local.settings.json (not committed to Git):

{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "FileShareStorage": "<YOUR_AZURE_STORAGE_CONNECTION_STRING>"
  }
}

---

## HTTP Azure Functions

### Menu API

| Function                  | Method | Route                                    |
| ------------------------- | ------ | ---------------------------------------- |
| CreateMenuItem            | POST   | /api/menu                                |
| GetAllMenuItems           | GET    | /api/menu                                |
| GetMenuItemsByCategory    | GET    | /api/menu/category/{category}            |
| UpdateMenuItem            | PUT    | /api/menu/{category}/{id}                |
| DeleteMenuItem            | DELETE | /api/menu/{category}/{id}                |

### Document API

| Function                | Method | Route                                       |
| ----------------------- | ------ | ------------------------------------------- |
| UploadStaffDocument     | POST   | /api/documents/upload                       |
| ListStaffDocuments      | GET    | /api/documents                              |
| DownloadStaffDocument   | GET    | /api/documents/download/{fileName}          |

Upload accepts multipart/form-data with a file field. Download streams the requested file back to the client.

---

## Running the Functions App Locally

1. Start Azurite (see above).
2. In Visual Studio (or terminal), run: func start
3. The console should list all 8 functions with their URLs.

---

## Standalone Docker Execution

### Run Azurite Container

docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 mikhail10x/coffeenchill-azurite:v1.0

### Run Functions Container

docker run -p 7071:80 -e AzureWebJobsStorage="UseDevelopmentStorage=true" -e FileShareStorage="<YOUR_AZURE_STORAGE_CONNECTION_STRING>" mikhail10x/coffeenchill-functions:v1.0

The Functions container is published to Docker Hub as mikhail10x/coffeenchill-functions:v1.0.

---

## Docker Hub

| Image                                | Description                  |
| ------------------------------------ | ---------------------------- |
| mikhail10x/coffeenchill-azurite      | Azurite storage emulator     |
| mikhail10x/coffeenchill-functions    | Azure Functions application  |

Pull:

docker pull mikhail10x/coffeenchill-azurite:v1.0
docker pull mikhail10x/coffeenchill-functions:v1.0

---

## Testing with Postman

1. Import docs/CoffeeNChill.postman_collection.json into Postman.
2. Ensure Azurite and the Functions app are running.
3. Execute the collection — all 8 endpoints should return 200 OK.

Example cURL commands:

curl -X POST http://localhost:7089/api/documents/upload -F "file=@test.txt"

curl http://localhost:7089/api/documents

curl http://localhost:7089/api/documents/download/test.txt

---

## Project Members & Contributions

| Member            | Responsibilities                                                                 |
| ----------------- | -------------------------------------------------------------------------------- |
| Mikhail Govender  | Azure Storage & Azurite setup, MenuItems table, staff-docs share, Docker Hub Azurite image |
| Abhay Sevprasad   | Menu API functions (5 CRUD endpoints) + Postman menu collection                  |
| Brandon Devan     | Document API functions (3 endpoints) + Postman document collection + README      |
| Lieshan Valliadum | Dockerfile for Functions app, merged Postman collection, YouTube video, final GitHub integration |

### Detailed Contributions

Mikhail Govender — Azure Storage & Azurite
- Configured Azurite via Docker with persistent local storage.
- Created the MenuItems table and seeded test entities.
- Created the staff-docs File Share and uploaded a test document.
- Verified connectivity via Docker logs and Azure Storage Explorer.
- Tagged and published mikhail10x/coffeenchill-azurite:v1.0 to Docker Hub.
- Authored the storage setup documentation in the README.

Abhay Sevprasad — Menu API
- Implemented 5 HTTP-triggered Azure Functions for menu CRUD operations.
- Integrated with Azure Table Storage (MenuItems table) via TableStorageService.
- Tested all endpoints locally with Postman.
- Exported the Menu Postman collection.

Brandon Devan — Document API
- Implemented 3 HTTP-triggered Azure Functions for staff document operations.
- Integrated with Azure File Share (staff-docs) using the FileShareStorage connection string.
- Tested all endpoints locally with Postman and cURL.
- Exported the Documents Postman collection.
- Merged and authored the final README.

Lieshan Valliadum — Integration & Delivery
- Authored the Dockerfile for the Azure Functions project.
- Built and pushed mikhail10x/coffeenchill-functions:v1.0 to Docker Hub.
- Merged Postman collections from Abhay and Brandon into CoffeeNChill.postman_collection.json.
- Recorded and published the demonstration video.
- Submitted the final GitHub repository link.

---

## Evidence

Screenshots captured during development include:

1. Azurite Docker container running.
2. MenuItems table with test entities.
3. staff-docs File Share with a test document.
4. Docker Hub repository showing v1.0 tags.
5. Postman collection results for all 8 endpoints.

---

## Video Demonstration

YouTube demonstration link:

https://www.youtube.com/watch?v=XXXXXXXXXXX

---

## Notes

- Azurite is used for local development and testing. It emulates Blob, Queue, and Table storage.
- Azure File Share is not supported by Azurite; a live Azure Storage account is used for staff-docs during development.
- local.settings.json is excluded from source control to protect connection strings.
