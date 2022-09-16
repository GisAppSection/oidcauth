using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OidcAuthUtilities
{
    [Table("tSQLSessions")]
    public partial class Sqlsessions
    {
        [StringLength(449)]
        public string Id { get; set; }
        [Required]
        public byte[] Value { get; set; }
        public DateTimeOffset ExpiresAtTime { get; set; }
        public long? SlidingExpirationInSeconds { get; set; }
        public DateTimeOffset? AbsoluteExpiration { get; set; }
    }
}
