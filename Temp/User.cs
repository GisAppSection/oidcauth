using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using OidcAuthV3.Models;

namespace OidcAuthV3.Models
{
    public class User
    {


        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName
        {
            get
            {
                var array = new[] { FirstName, LastName };
                string fullName = string.Join(" ", array.Where(s => !string.IsNullOrWhiteSpace(s)));
                return fullName;
            }

            set { FullName = value; }

        }

        public string PaySrId { get; set; }
        public string Dept { get; set; }

        public string AgencyCd { get; set; }
        public string WorkPhone { get; set; }
        public string MobilePhone { get; set; }
        public string Email { get; set; }
        public string PhotoUrl { get; set; }
        public string access_token { get; set; }
        public int? expires_in { get; set; }
        public string refresh_token { get; set; }

    }
}
