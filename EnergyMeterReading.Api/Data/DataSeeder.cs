using EnergyMeterReading.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
namespace EnergyMeterReading.Api.Data
{
    public static class DataSeeder
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Apply any pending migrations
            await context.Database.EnsureCreatedAsync();

            // Check if accounts are already seeded
            if (await context.Accounts.AnyAsync())
            {
                return; // Already seeded
            }

            try
            {
                // Path to the CSV file (assuming it's in the project directory)
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "TestData", "Test_Accounts.csv");

                if (!File.Exists(filePath))
                {
                    // Log and exit if file doesn't exist
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError($"Test accounts CSV file not found at {filePath}");
                    return;
                }

                // Read and parse the CSV file
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));

                var accounts = new List<Account>();

                // Skip header row
                csv.Read();
                csv.ReadHeader();

                // Read rows
                while (csv.Read())
                {
                    var account = new Account
                    {
                        Id = int.Parse(csv.GetField<string>("Id") ?? "0"),
                        FirstName = csv.GetField<string>("FirstName") ?? "",
                        LastName = csv.GetField<string>("LastName") ?? ""
                    };

                    accounts.Add(account);
                }

                // Add accounts to database
                await context.Accounts.AddRangeAsync(accounts);
                await context.SaveChangesAsync();

                // Log success
                var loggerSuccess = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                loggerSuccess.LogInformation($"Successfully seeded {accounts.Count} test accounts");
            }


           catch (Exception ex)
            {
                // Log error
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Error seeding test accounts");
            }

        }
    }
}
