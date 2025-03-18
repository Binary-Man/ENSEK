using EnergyMeterReading.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace EnergyMeterReading.Api.Data
{
    public class DataSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DataSeeder> _logger;
        private readonly IWebHostEnvironment _environment;

        public DataSeeder(
            ApplicationDbContext context,
            ILogger<DataSeeder> logger,
            IWebHostEnvironment environment
            )
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        public async Task SeedDataAsync()
        {
            // Apply any pending migrations
            await _context.Database.MigrateAsync();

            // Check if accounts are already seeded
            if (await _context.Accounts.AnyAsync())
            {
                return; // Already seeded
            }

            try
            {
                // Path to the CSV file (assuming it's in the project directory)
                string filePath = Path.Combine(_environment.ContentRootPath, "Data", "TestData", "Test_Accounts.csv");

                if (!File.Exists(filePath))
                {
                    // Log and exit if file doesn't exist                    
                    _logger.LogError($"Test accounts CSV file not found at {filePath}");
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
                        Id = int.Parse(csv.GetField<string>("AccountId") ?? "0"),
                        FirstName = csv.GetField<string>("FirstName") ?? "",
                        LastName = csv.GetField<string>("LastName") ?? ""
                    };

                    accounts.Add(account);
                }

                // Add accounts to database
                await _context.Accounts.AddRangeAsync(accounts);
                await _context.SaveChangesAsync();

                // Log success

                _logger.LogInformation($"Successfully seeded {accounts.Count} test accounts");
            }


            catch (Exception ex)
            {
                // Log error                
                _logger.LogError(ex, "Error seeding test accounts");
            }
        }
    } 
}
