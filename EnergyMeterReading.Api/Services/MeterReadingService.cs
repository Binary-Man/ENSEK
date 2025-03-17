using EnergyMeterReading.Api.Models;
using EnergyMeterReading.Api.Data;
using EnergyMeterReading.Api.Validators;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using System.Diagnostics.Metrics;

namespace EnergyMeterReading.Api.Services
{
    public class MeterReadingService
    {
        private readonly ApplicationDbContext _context;
        private readonly MeterReadingValidator _validator;
        private readonly ILogger<MeterReadingService> _logger;


        public MeterReadingService(
            ApplicationDbContext context,
        MeterReadingValidator validator,
         ILogger<MeterReadingService> logger)
        {
            _context = context;
            _validator = validator;
            _logger = logger;
        }

        public async Task<(int SuccessCount, int FailCount, List<string> Errors)> ProcessMeterReadingsFromCsvAsync(Stream csvStream)
        {
            var successCount = 0;
            var failCount = 0;
            var errors = new List<string>();

            try
            {
                using var reader = new StreamReader(csvStream);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));

                // Skip header row
                csv.Read();
                csv.ReadHeader();

                var validReadings = new List<MeterReading>();

                // Process each row
                while (csv.Read())
                {
                    try
                    {
                        var accountId = int.Parse(csv.GetField<string>("AccountId") ?? "0");

                        // Parse the date time (British format)
                        var dateTimeStr = csv.GetField<string>("MeterReadingDateTime");
                        if (!DateTime.TryParseExact(dateTimeStr, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out DateTime meterReadingDateTime))
                        {
                            errors.Add($"Invalid date format for Account ID {accountId}: {dateTimeStr}");
                            failCount++;
                            continue;
                        }

                        // Parse meter reading value
                        var meterReadValue = csv.GetField<string>("MeterReadValue");
                        if (!int.TryParse(meterReadValue, out int readingValue))
                        {
                            errors.Add($"Invalid meter reading value for Account ID {accountId}: {meterReadValue}");
                            failCount++;
                            continue;
                        }

                        // Create meter reading object
                        var reading = new MeterReading
                        {
                            AccountId = accountId,
                            ReadingDate = meterReadingDateTime,
                            ReadingValue = readingValue
                        };

                        // Validate reading
                        var (isValid, errorMessage) = await _validator.ValidateAsync(reading);

                        if (isValid)
                        {
                            validReadings.Add(reading);
                            successCount++;
                        }
                        else
                        {
                            errors.Add($"Validation failed for Account ID {accountId}: {errorMessage}");
                            failCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error processing row: {ex.Message}");
                        failCount++;
                    }
                }

                // Save valid readings to database
                if (validReadings.Any())
                {
                    _context.MeterReadings.AddRange(validReadings);
                    await _context.SaveChangesAsync();
                }

                return (successCount, failCount, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing meter readings CSV");
                errors.Add($"Error processing CSV file: {ex.Message}");
                return (successCount, failCount, errors);
            }
        }
    }
}
