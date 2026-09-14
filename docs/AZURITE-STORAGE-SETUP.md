\# CoffeeNChill — Azurite \& Azure Storage Setup



\## Overview



CoffeeNChill uses Azurite as a local emulator for Azure Storage during development.



The local Azurite instance provides:



\* Azure Blob Storage

\* Azure Queue Storage

\* Azure Table Storage



The amended Part 1 specification uses Azure Table Storage for menu items and Azure Blob Storage for staff documents.



\---



\## 1. Azurite Docker Setup



\### Docker Image



The Azurite image was published to Docker Hub:



`mikhail10x/coffeenchill-azurite:v1.0`



\### Persistent Storage Directory



A local directory was created for Azurite data:



```text

C:\\CoffeeNChill\\azurite-data

```



\### Run Azurite



```powershell

docker run -d `

&#x20; --name coffeenchill-azurite `

&#x20; -p 10000:10000 `

&#x20; -p 10001:10001 `

&#x20; -p 10002:10002 `

&#x20; -v C:\\CoffeeNChill\\azurite-data:/data `

&#x20; mikhail10x/coffeenchill-azurite:v1.0

```



\### Storage Ports



| Service       |  Port |

| ------------- | ----: |

| Blob Storage  | 10000 |

| Queue Storage | 10001 |

| Table Storage | 10002 |



\### Verify Container



```powershell

docker ps

```



View Azurite logs:



```powershell

docker logs coffeenchill-azurite

```



The logs should show the Blob, Queue and Table services listening on ports 10000, 10001 and 10002 respectively.



\---



\## 2. Azure Table Storage — MenuItems



A table named `MenuItems` was created using Azure Storage Explorer connected to the local Azurite emulator.



\### Table Schema



| Property     | Type    | Purpose                                 |

| ------------ | ------- | --------------------------------------- |

| PartitionKey | String  | Menu category                           |

| RowKey       | String  | Unique SKU/ID                           |

| Name         | String  | Menu item name                          |

| Description  | String  | Menu item description                   |

| Price        | Double  | Item price                              |

| IsAvailable  | Boolean | Whether the item is currently available |



\### Test Data



| PartitionKey | RowKey  | Name                | Description                      | Price | IsAvailable |

| ------------ | ------- | ------------------- | -------------------------------- | ----: | ----------- |

| Hot Drinks   | COF-001 | Cappuccino          | Freshly brewed cappuccino        | 35.00 | true        |

| Cold Drinks  | DRK-001 | Iced Coffee         | Chilled coffee with milk         | 32.00 | true        |

| Pastries     | PAS-001 | Chocolate Croissant | Buttery croissant with chocolate | 28.00 | true        |



The table can be viewed and managed through Azure Storage Explorer under the local Azurite development storage account.



\---



\## 3. Azure Blob Storage — staff-docs



The amended Part 1 specification uses Azure Blob Storage for staff documents.



A Blob container named:



```text

staff-docs

```



was created in the local Azurite Blob Storage service.



A test PDF document was uploaded to verify that the container accepts Blob uploads successfully.



The Blob service is available through Azurite on port `10000`.



\---



\## 4. Azure Functions Local Storage Configuration



Azure Functions can connect to the local Azurite instance using:



```text

AzureWebJobsStorage=UseDevelopmentStorage=true

```



The `local.settings.json` file is intentionally excluded from Git using `.gitignore`.



A typical local Functions configuration is:



```json

{

&#x20; "IsEncrypted": false,

&#x20; "Values": {

&#x20;   "AzureWebJobsStorage": "UseDevelopmentStorage=true",

&#x20;   "FUNCTIONS\_WORKER\_RUNTIME": "dotnet-isolated"

&#x20; }

}

```



Any additional Azure service connection strings required by the application should remain local and must not be committed to source control.



\---



\## 5. Verification



The storage setup was verified using:



\* Docker Desktop

\* PowerShell

\* Azure Storage Explorer

\* Azurite container logs



Verification confirmed:



1\. The Azurite Docker container starts successfully.

2\. Blob Storage listens on port 10000.

3\. Queue Storage listens on port 10001.

4\. Table Storage listens on port 10002.

5\. The `MenuItems` table was created successfully.

6\. Test menu entities were inserted successfully.

7\. The `staff-docs` Blob container was created successfully.

8\. A test document was uploaded successfully.



\---



\## 6. Docker Hub Image



The Azurite image was tagged and published as:



```text

mikhail10x/coffeenchill-azurite:v1.0

```



It can be pulled using:



```powershell

docker pull mikhail10x/coffeenchill-azurite:v1.0

```



\---



\## Member Contribution



\*\*Mikhail Govender — Azure Storage \& Azurite Setup\*\*



\* Configured Azurite using Docker with persistent local storage.

\* Created the `MenuItems` Azure Table Storage table.

\* Added test menu entities to `MenuItems`.

\* Created the `staff-docs` Azure Blob Storage container.

\* Uploaded a test staff document.

\* Verified the storage services using Azure Storage Explorer and Docker logs.

\* Tagged and published the Azurite Docker image to Docker Hub.

\* Documented the local Azure Storage setup.



