using System;
using System.ComponentModel.DataAnnotations;

namespace OidcAuthV3.Models
{
    public class ExceptionLog
    {
        [Key]
        public long LogId { get; set; }
        public DateTime LogDate { get; set; }
        public string LogSubject { get; set; }
        public string LogDetails { get; set; }
    }
}
