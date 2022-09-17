using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OidcAuth.Models
{
	[Table("tServiceInfo")]
	public class ServiceInfo
	{
		[Key]
		public int RecordId { get; set; }
		public string AgencyCd { get; set; }
		public string ServiceCode { get; set; }
		public string ServiceName { get; set; }
		public string DevRedirectUri { get; set; }
		public string StagingRedirectUri { get; set; }
		public string ProdRedirectUri { get; set; }
	}
}
