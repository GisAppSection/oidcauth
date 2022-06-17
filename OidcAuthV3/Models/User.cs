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
        

        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }

        public string UserFullName
        {
            get
            {
                var array = new[] { UserFirstName, UserLastName };
                string fullName = string.Join(" ", array.Where(s => !string.IsNullOrWhiteSpace(s)));
                return fullName;
            }

            set { UserFullName = value; }

        }


        public string PaySrId { get; set; }
        public string Dept { get; set; }

        public string AgencyCd { get; set; }
        public string UserWorkPhone { get; set; }
        public string UserMobilePhone { get; set; }
        public string UserEmail { get; set; }
        public string UserPhotoUrl { get; set; }
        public string access_token { get; set; }
        public int? expires_in { get; set; }
        public string refresh_token { get; set; }

    }
}
