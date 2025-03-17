namespace EnergyMeterReading.Api.Models.Response
{
    public class MeterReadingUploadResponse
    {
        public int SuccessfulReadings { get; set; }
        public int FailedReadings { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}