using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OidcAuth.Models
{
    [Table("tExceptionLog")]
    public class ExceptionLog
    {
        [Key]
        public long LogId { get; set; }
        public DateTime LogDate { get; set; }

        public string EnvType { get; set; } // Development, Staging, Production
        public string LogType { get; set; }  // info, warning, exception, etc
        public string LogSubject { get; set; }
        public string LogDetails { get; set; }
    }
}
