using System;
using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public enum ScheduleType
    {
        Place,
        Delievery
    }

    public class Schedule
    {
        [Key]
        public string Id { get; set; }
        public ScheduleType ScheduleType {get; set; }
        [Required]
        public DayOfWeek Day { get; set; }
        public int? StartHour { get; set; }
        public int? EndHour { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
    }
}
