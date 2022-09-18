using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OidcAuth.Models
{
    [Table("tIdmDeptName")]
    public class IdmDeptName
    {
            [Key]
            public int DeptRecordId { get; set; }
            public string IdmDept { get; set; }
            public string DeptCd { get; set; }
            public string DeptAka { get; set; }
    }
}
