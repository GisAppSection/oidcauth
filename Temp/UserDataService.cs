using OidcAuthV3.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Linq;

// reading user data from a claim

namespace OidcAuthV3.Utilities
{
    public class UserDataService : IUserDataService
    {
        private readonly IHttpContextAccessor _context;
        public UserDataService(IHttpContextAccessor context)
        {
            _context = context;
        }

        public Staff GetStaff()
        {
            // https://stackoverflow.com/questions/36401026/how-to-get-user-information-in-dbcontext-using-net-core
            //return _context.HttpContext.User?.Identity?.Name;
            Staff staff = null;
            string staffDataJson = _context.HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "staffData")?.Value;
            //string userDataJson = _context.HttpContext.User?.Identity.Name
            if (staffDataJson != null)
            {
                staff = JsonConvert.DeserializeObject<Staff>(staffDataJson);
            }

            return staff;
            //return userData;
            //return User.Claims.FirstOrDefault(c => c.Type == "userData")?.Value;
        }
    }
}