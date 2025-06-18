# SPDocsAPI - Document Management REST API

A .NET 8 Web API for document management with Azure SQL Server integration and stored procedure support.

## Project Structure

```
SPDocsAPI/
├── Controllers/         # API Controllers
│   ├── DocumentsController.cs
│   └── WeatherForecastController.cs (demo)
├── Models/             # Entity Models
│   └── Document.cs
├── DTOs/               # Data Transfer Objects
│   └── DocumentDto.cs
├── Services/           # Business Logic Layer
│   └── DocumentService.cs
├── Interfaces/         # Service Contracts
│   └── IDocumentService.cs
├── Data/               # Database Context & SQL Scripts
│   ├── ApplicationDbContext.cs
│   └── StoredProcedures.sql
└── Properties/         # Launch Settings
```

## Features

- **Full CRUD Operations** for document management
- **Azure SQL Server** integration with Entity Framework Core 8
- **Stored Procedure** support for complex queries
- **RESTful API** design with proper HTTP status codes
- **Swagger/OpenAPI** documentation
- **Dependency Injection** for clean architecture
- **Async/Await** patterns for better performance
- **Logging** integration

## Setup Instructions

### 1. Database Configuration

1. **Update Connection String** in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=tcp:your-server.database.windows.net,1433;Initial Catalog=your-database;Persist Security Info=False;User ID=your-username;Password=your-password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
     }
   }
   ```

2. **Run the SQL Scripts** in your Azure SQL Database:
   - Execute the scripts from `Data/StoredProcedures.sql`
   - This will create the Documents table and required stored procedures

### 2. Install Dependencies

```bash
dotnet restore
```

### 3. Database Migration (Optional)

If you want to use Entity Framework migrations instead of manual SQL scripts:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Run the Application

```bash
dotnet run
```

The API will be available at:
- **HTTPS**: `https://localhost:7xxx`
- **HTTP**: `http://localhost:5xxx`
- **Swagger UI**: `https://localhost:7xxx/swagger`

## API Endpoints

### Lessons Controller (`/api/lessons`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/lessons/GetLessonID?category={category}` | Get lesson ID for a category (stored procedure) |

### Documents Controller (`/api/documents`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/documents` | Get all active documents |
| GET | `/api/documents/{id}` | Get document by ID |
| POST | `/api/documents` | Create new document |
| PUT | `/api/documents/{id}` | Update document |
| DELETE | `/api/documents/{id}` | Delete document (soft delete) |
| GET | `/api/documents/by-type/{type}` | Get documents by type (stored procedure) |
| GET | `/api/documents/by-user/{username}` | Get documents by user (stored procedure) |
| PATCH | `/api/documents/{id}/status` | Activate/deactivate document (stored procedure) |

### Sample API Requests

#### Get Lesson ID
```http
GET /api/lessons/GetLessonID?category=Math
```

Response:
```json
"MATH-20250618-123"
```

#### Create Document
```json
POST /api/documents
Content-Type: application/json

{
  "title": "Project Contract",
  "description": "Main project contract document",
  "documentType": "Contract",
  "createdBy": "john.doe@company.com",
  "filePath": "/documents/contracts/project-contract.pdf",
  "fileSize": 1024000
}
```

#### Update Document
```json
PUT /api/documents/1
Content-Type: application/json

{
  "title": "Updated Project Contract",
  "description": "Updated main project contract document",
  "documentType": "Contract",
  "modifiedBy": "jane.smith@company.com",
  "filePath": "/documents/contracts/project-contract-v2.pdf",
  "fileSize": 1536000
}
```

#### Update Document Status
```json
PATCH /api/documents/1/status
Content-Type: application/json

false
```

## Stored Procedures

The API includes support for these stored procedures:

1. **GetNextCategoryCode** - Generate next lesson ID for a given category
2. **GetDocumentsByType** - Filter documents by document type
3. **GetDocumentsByUser** - Get documents created or modified by a specific user
4. **ActivateDeactivateDocument** - Change document active status

## Architecture

- **Controllers**: Handle HTTP requests and responses
- **Services**: Contain business logic and data access
- **Interfaces**: Define service contracts for dependency injection
- **DTOs**: Data transfer objects for API requests/responses
- **Models**: Entity Framework models representing database tables
- **Data**: Database context and configuration

## Technology Stack

- **.NET 8** - Web API framework
- **Entity Framework Core 8** - ORM for database operations
- **Azure SQL Server** - Cloud database
- **Swagger/OpenAPI** - API documentation
- **Microsoft.Data.SqlClient** - SQL Server data provider

## Error Handling

The API includes comprehensive error handling:
- **400 Bad Request** - Invalid input data
- **404 Not Found** - Resource not found
- **500 Internal Server Error** - Server-side errors

All errors are logged for debugging purposes.

## Next Steps

1. Configure your Azure SQL Server connection string
2. Run the SQL scripts to create the database schema
3. Test the API endpoints using Swagger UI
4. Implement authentication/authorization as needed
5. Add more domain-specific models and controllers
6. Configure production logging and monitoring 