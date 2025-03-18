using EnergyMeterReading.Api.Models;
using EnergyMeterReading.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace EnergyMeterReading.Api.Validators
{
    public class MeterReadingValidator
    {
        private readonly ApplicationDbContext _context;
        public MeterReadingValidator(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<(bool IsValid, string ErrorMessage)> ValidateAsync(MeterReading reading)
        {
            // Validate Account ID exists
            bool accountExists = await _context.Accounts.AnyAsync(a => a.Id == reading.AccountId);

            if (!accountExists)
            {
                return (false, $"Account Id {reading.Id} does not exist.");
            }

            // Validate meter reading format (NNNNN)
            if (!Regex.IsMatch(reading.MeterValue.ToString(), @"^\d{5}$"))
            {
                return (false, $"Meter reading value must be in the format NNNNN (5 digits), got: {reading.MeterValue}");
            }

            // Check for duplicate readings (same account, same date/time)
            // Only checked these two entities as you shouldn't be able to take two reading on the day
            bool duplicateExists = await _context.MeterReadings.AnyAsync(m =>
                m.AccountId == reading.AccountId &&
                m.ReadingDate == reading.ReadingDate);

            if (duplicateExists)
            {
                return (false, $"Duplicate meter reading for Account ID {reading.AccountId} at {reading.ReadingDate}");
            }

            // Optional: Check if reading is newer than existing readings (NICE TO HAVE)
            var latestReading = await _context.MeterReadings
                .Where(m => m.AccountId == reading.AccountId)
                .OrderByDescending(m => m.ReadingDate)
                .FirstOrDefaultAsync();

            if (latestReading != null && reading.ReadingDate < latestReading.ReadingDate)
            {
                return (false, $"New reading date ({reading.ReadingDate}) is older than existing reading ({latestReading.ReadingDate})");
            }

            return (true, string.Empty);
        }
    }

}