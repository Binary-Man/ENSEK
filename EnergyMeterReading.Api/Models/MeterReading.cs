using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyMeterReading.Api.Models
{
    public class MeterReading
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int AccountId { get; set; }
        [Required]
        public DateTime ReadingDate { get; set; }
        [Required]
        public int ReadingValue { get; set; }
        [Required]

        [ForeignKey("AccountId")]
        public Account Account { get; set; }
    }

}