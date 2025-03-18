using EnergyMeterReading.Api.Models.Response;
using EnergyMeterReading.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnergyMeterReadingApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class MeterReadingController : ControllerBase
    {
        private readonly MeterReadingService _meterReadingService;
        private readonly ILogger<MeterReadingController> _logger;

        public MeterReadingController(
            MeterReadingService meterReadingService,
            ILogger<MeterReadingController> logger)
        {
            _meterReadingService = meterReadingService;
            _logger = logger;
        }

        [HttpPost("meter-reading-uploads")]
        public async Task<ActionResult<MeterReadingUploadResponse>> UploadMeterReadings(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new MeterReadingUploadResponse
                {
                    SuccessfulReadings = 0,
                    FailedReadings = 0,
                    Errors = new List<string> { "No file uploaded or file is empty" }
                });
            }

            if (!file.ContentType.Equals("text/csv") && !file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new MeterReadingUploadResponse
                {
                    SuccessfulReadings = 0,
                    FailedReadings = 0,
                    Errors = new List<string> { "Uploaded file must be a CSV file" }
                });
            }

            try
            {
                // Process the CSV file
                using var stream = file.OpenReadStream();
                var (successCount, failCount, errors) = await _meterReadingService.ProcessMeterReadingsFromCsvAsync(stream);

                // Create and return the response
                var response = new MeterReadingUploadResponse
                {
                    SuccessfulReadings = successCount,
                    FailedReadings = failCount,
                    Errors = errors
                };

                _logger.LogInformation(
                    $"Meter reading upload completed. Successful: {successCount}, Failed: {failCount}", 
                    successCount, failCount);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading meter readings");
                return StatusCode(500, new MeterReadingUploadResponse
                {
                    SuccessfulReadings = 0,
                    FailedReadings = 0,
                    Errors = new List<string> { "An error occurred while processing the file: " + ex.Message }
                });
            }
        }
    }
}