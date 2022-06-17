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

        public User GetUser()
        {
            // https://stackoverflow.com/questions/36401026/how-to-get-user-information-in-dbcontext-using-net-core
            //return _context.HttpContext.User?.Identity?.Name;
            User user = null;
            string userDataJson = _context.HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "userData")?.Value;
            //string userDataJson = _context.HttpContext.User?.Identity.Name
            if (userDataJson != null)
            {
                user = JsonConvert.DeserializeObject<User>(userDataJson);
            }

            return user;
            //return userData;
            //return User.Claims.FirstOrDefault(c => c.Type == "userData")?.Value;
        }
    }
}