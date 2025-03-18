# Energy Meter Reading API

A C# Web API that processes and validates meter reading CSV files, storing valid readings in a SQL database.

## Features

- Upload meter reading CSV files via REST API
- Validate meter readings against the following criteria:
  - Reading must have a valid account ID
  - Reading value must be in format NNNNN (5 digits)
  - No duplicate readings for the same account/datetime
  - New readings cannot be older than existing readings (optional)
- Return report of successful and failed readings
- Future version would include a React client application for CSV upload

## Project

- **API**: ASP.NET Core Web API
- **Database**: SQL Server (via Entity Framework Core)
- **Client**: React application (Not yet Built)

## Requirements

- .NET 8 SDK and above
- SQL Server (or SQL Server LocalDB)
- Node.js (for client application)

## Getting Started

### API Setup

1. Clone the repository
2. Navigate to the API project folder
3. Update the connection string in `appsettings.json` if needed
4. Run the database migrations:

```
dotnet ef database update
```

5. Run the application:

```
dotnet run
```

The API will be available at `https://localhost:7001` and will automatically seed the database with test accounts from the provided CSV.

### Client Setup

On the todo list:

## API Endpoints

### POST /api/meter-reading-uploads

Send POST request via the .http file (EnergyMeterReading.Api.http) in the application once the application is running.

OR

Uploads and processes a meter reading CSV file.

**Request:**

- Content-Type: `multipart/form-data`
- Body: CSV file

**Response:**

```json
{
  "successfulReadings": 10,
  "failedReadings": 5,
  "errors": [
    "Account ID 1234 does not exist",
    "Meter reading value must be in the format NNNNN (5 digits), got: 123"
  ]
}
```

## CSV File Format

The meter reading CSV file should have the following format:

```
AccountId,MeterReadingDateTime,MeterReadValue
1234,22/04/2019 12:25,12345
5678,22/04/2019 12:26,54321
```

## Data Models

### Account

- `Id` (int): Unique identifier for the account
- `FirstName` (string): First name of the account holder
- `LastName` (string): Last name of the account holder

### Meter Reading

- `Id` (int): Unique identifier for the reading
- `AccountId` (int): Foreign key to the Account
- `ReadingDate` (DateTime): Date and time of the reading
- `MeterValue` (int): The meter reading value (5 digits)

## Testing

To test the API, you can use tools like Postman or the built in Swagger UI. Use the provided `Meter_Reading.csv` file as a test payload for the `/api/meter-reading-uploads` endpoint.
