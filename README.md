# CoffeeNChill Canteen Management System

A cloud-based Canteen Management System for CoffeeNChill, built with Azure Functions, Azure Table Storage, and Azure Blob Storage. Part 1 provides the cloud storage foundation for menu management and staff document handling.

---

## Project Overview

CoffeeNChill replaces paper-based menus and filing-cabinet documents with:

* **Menu Items** stored dynamically in Azure Table Storage so prices and availability can be updated efficiently.
* **Staff Documents** stored in an Azure Blob Storage container for centralized access to recipe sheets, equipment manuals, health and safety policies, and other operational documents.

For local development, Azure Storage services are emulated using Azurite running inside Docker containers.

The Azure Functions application provides HTTP APIs for menu and staff document management.

---

## Repository Structure

The repository contains the Azure Functions application, storage services, documentation, and Postman collections.

```text
WE-TARDS/
├── .gitignore
├── README.md
├── CoffeeNChill.slnx
├── CoffeeNChill.csproj
├── CoffeeNChill.Functions.csproj
├── Program.cs
├── host.json
├── local.settings.json              # Not committed — see .gitignore
│
├── CreateMenuItem.cs
├── GetAllMenuItems.cs
├── GetMenuItemsByCategory.cs
├── UpdateMenuItems.cs
├── DeleteMenuItems.cs
├── MenuItem.cs
├── TableStorageService.cs
│
├── UploadStaffDocument.cs
├── ListStaffDocuments.cs
├── DownloadStaffDocument.cs
│
├── CoffeeNChill Documents.postman_collection.json
├── CoffeeNChill_Menu_API.postman_collection.json
│
├── docs/
│   └── AZURITE-STORAGE-SETUP.md
│
└── scripts/
    └── start-azurite.ps1
```

---

## Prerequisites

* Docker Desktop
* Git
* Microsoft Azure Storage Explorer
* .NET SDK
* Azure Functions Core Tools v4
* Postman

---

# Part 1 — Azure Storage & Azurite

## Azurite Setup

Azurite is used as the local Azure Storage emulator.

The Azurite instance provides:

* Blob Storage
* Queue Storage
* Table Storage

### Docker Image

The CoffeeNChill Azurite image is published to Docker Hub as:

```text
mikhail10x/coffeenchill-azurite:v1.0
```

### Persistent Storage Directory

Create a directory for persistent Azurite data:

```powershell
mkdir C:\CoffeeNChill
mkdir C:\CoffeeNChill\azurite-data
```

### Start Azurite

```powershell
docker run -d `
  --name coffeenchill-azurite `
  -p 10000:10000 `
  -p 10001:10001 `
  -p 10002:10002 `
  -v C:\CoffeeNChill\azurite-data:/data `
  mikhail10x/coffeenchill-azurite:v1.0
```

Alternatively, the included startup script can be used:

```powershell
.\scripts\start-azurite.ps1
```

### Storage Ports

| Service       |  Port |
| ------------- | ----: |
| Blob Storage  | 10000 |
| Queue Storage | 10001 |
| Table Storage | 10002 |

### Verify Azurite

Check that the container is running:

```powershell
docker ps
```

View the Azurite logs:

```powershell
docker logs coffeenchill-azurite
```

The logs should show the Blob, Queue, and Table services listening on ports 10000, 10001, and 10002.

---

# Azure Table Storage — MenuItems

A Table Storage table named `MenuItems` is used to store CoffeeNChill menu items.

The table was created locally using Azure Storage Explorer connected to the Azurite development storage account.

## MenuItems Schema

| Property     | Type    | Description                             |
| ------------ | ------- | --------------------------------------- |
| PartitionKey | String  | Menu category                           |
| RowKey       | String  | Unique SKU / item ID                    |
| Name         | String  | Menu item name                          |
| Description  | String  | Description of the menu item            |
| Price        | Double  | Item price                              |
| IsAvailable  | Boolean | Whether the item is currently available |

### Sample Menu Data

| Category    | SKU     | Name                | Description                      | Price | Available |
| ----------- | ------- | ------------------- | -------------------------------- | ----: | --------- |
| Hot Drinks  | COF-001 | Cappuccino          | Freshly brewed cappuccino        | 35.00 | true      |
| Cold Drinks | DRK-001 | Iced Coffee         | Chilled coffee with milk         | 32.00 | true      |
| Pastries    | PAS-001 | Chocolate Croissant | Buttery croissant with chocolate | 28.00 | true      |

The `MenuItems` table is accessed by the Azure Functions application through `TableStorageService`.

---

# Azure Blob Storage — staff-docs

The amended Part 1 specification uses **Azure Blob Storage** for staff documents.

A Blob Storage container named:

```text
staff-docs
```

was created locally using Azure Storage Explorer connected to Azurite.

A test PDF document was uploaded to verify that the Blob container accepts and stores staff documents.

Azurite's Blob Storage service is available on:

```text
http://127.0.0.1:10000
```

The local storage account used by Azurite is:

```text
devstoreaccount1
```

---

# Local Azure Functions Configuration

For local development, Azure Functions can connect to Azurite using:

```text
AzureWebJobsStorage=UseDevelopmentStorage=true
```

A typical local configuration is:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}
```

The actual `local.settings.json` file is excluded from Git using `.gitignore`.

**Do not commit storage account keys, passwords, or other secrets to the repository.**

---

# HTTP Azure Functions

## Menu API

| Function               | Method | Route                           |
| ---------------------- | ------ | ------------------------------- |
| CreateMenuItem         | POST   | `/api/menu`                     |
| GetAllMenuItems        | GET    | `/api/menu`                     |
| GetMenuItemsByCategory | GET    | `/api/menu/category/{category}` |
| UpdateMenuItem         | PUT    | `/api/menu/{category}/{id}`     |
| DeleteMenuItem         | DELETE | `/api/menu/{category}/{id}`     |

The Menu API uses the `MenuItems` Azure Table Storage table.

---

## Staff Document API

| Function              | Method | Route                                |
| --------------------- | ------ | ------------------------------------ |
| UploadStaffDocument   | POST   | `/api/documents/upload`              |
| ListStaffDocuments    | GET    | `/api/documents`                     |
| DownloadStaffDocument | GET    | `/api/documents/download/{fileName}` |

The Staff Document API is intended to manage documents stored in the `staff-docs` Azure Blob Storage container.

---

# Running the Functions Application Locally

1. Start Docker Desktop.
2. Start the Azurite container.
3. Ensure the required Azure Functions local settings are configured.
4. Start the Functions application using Visual Studio or Azure Functions Core Tools:

```powershell
func start
```

5. The Functions host will display the available HTTP-triggered functions and their local URLs.

---

# Standalone Docker Execution

## Run Azurite

```powershell
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 `
  mikhail10x/coffeenchill-azurite:v1.0
```

## Run the Functions Container

The Functions Docker image is maintained by the integration member and is published as:

```text
mikhail10x/coffeenchill-functions:v1.0
```

The exact environment variables and storage configuration should be supplied according to the final Functions container implementation.

---

# Docker Hub

| Image                                    | Description                              |
| ---------------------------------------- | ---------------------------------------- |
| `mikhail10x/coffeenchill-azurite:v1.0`   | Azurite Azure Storage emulator           |
| `mikhail10x/coffeenchill-functions:v1.0` | CoffeeNChill Azure Functions application |

### Pull Azurite

```powershell
docker pull mikhail10x/coffeenchill-azurite:v1.0
```

### Pull Functions Application

```powershell
docker pull mikhail10x/coffeenchill-functions:v1.0
```

---

# Testing with Postman

The repository contains Postman collections for the Menu API and Staff Document API.

Before testing:

1. Start Azurite.
2. Start the Azure Functions application.
3. Import the relevant Postman collection.
4. Execute the HTTP requests against the local Functions host.

The Menu API collection contains requests for the five menu operations:

* Create menu item
* Get all menu items
* Get menu items by category
* Update menu item
* Delete menu item

The Staff Document collection contains requests for:

* Uploading a document
* Listing documents
* Downloading a document

---

# Project Members & Contributions

| Member            | Responsibilities                                                                                     |
| ----------------- | ---------------------------------------------------------------------------------------------------- |
| Mikhail Govender  | Azure Storage & Azurite setup, MenuItems table, staff-docs Blob container, Docker Hub Azurite image  |
| Abhay Sevprasad   | Menu API functions and Menu Postman collection                                                       |
| Brandon Devan     | Staff Document API functions and Document Postman collection                                         |
| Lieshan Valliadum | Functions Dockerfile, integration, merged Postman collection, demonstration video and final delivery |

## Detailed Contributions

### Mikhail Govender — Azure Storage & Azurite

* Configured Azurite using Docker with persistent local storage.
* Created the `MenuItems` Azure Table Storage table.
* Added test menu entities to `MenuItems`.
* Created the `staff-docs` Azure Blob Storage container.
* Uploaded a test staff document.
* Verified Azure Storage services using Docker logs and Azure Storage Explorer.
* Tagged and published `mikhail10x/coffeenchill-azurite:v1.0` to Docker Hub.
* Created reusable Azurite startup documentation and a PowerShell startup script.
* Documented the local Azure Storage setup.

### Abhay Sevprasad — Menu API

* Implemented the five HTTP-triggered Functions for menu CRUD operations.
* Integrated the Menu API with the `MenuItems` Azure Table Storage table.
* Tested the Menu API endpoints.
* Created the Menu API Postman collection.

### Brandon Devan — Staff Document API

* Implemented the HTTP-triggered Functions for staff document operations.
* Integrated the document functionality with the project's configured storage service.
* Tested the document API endpoints.
* Created the Staff Document Postman collection.

### Lieshan Valliadum — Integration & Delivery

* Authored the Dockerfile for the Azure Functions application.
* Built and published the Functions Docker image.
* Integrated the Postman collections.
* Recorded and published the demonstration video.
* Completed final repository integration and delivery.

---

# Evidence

Development evidence includes:

1. Azurite Docker container running.
2. Azurite Docker logs showing Blob, Queue, and Table services.
3. `MenuItems` table with test entities in Azure Storage Explorer.
4. `staff-docs` Blob container with a test document.
5. Docker Hub repository showing the `v1.0` Azurite image.
6. Postman testing of the HTTP Functions.

---

# Video Demonstration

YouTube demonstration link:

```text
[TO BE ADDED BY GROUP]
```

---

# Notes

* Azurite is used for local development and testing.
* Azurite emulates Azure Blob, Queue, and Table Storage.
* The amended Part 1 specification uses **Azure Blob Storage** for the `staff-docs` container.
* Local emulator data is stored outside the repository and should not be committed to Git.
* `local.settings.json` is excluded from source control because it may contain connection strings or other sensitive configuration.
* Storage account credentials must never be committed to the repository.

