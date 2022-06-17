using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OidcAuthV3.DataAccess;
using OidcAuthV3.Models;
using OidcAuthV3.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OidcAuthV3.Controllers
{
    public class AccountController : Controller
    {
        //private readonly IWebHostEnvironment _env;
        //private readonly IHttpClientFactory _httpClientFactory;
        //private readonly IConfiguration _configuration;
        //private readonly string client_id;
        //private readonly string client_secret;
        //private readonly string auth_uri;
        //private readonly string token_uri;
        //private readonly string redirect_uri;
        //private readonly string revoke_uri;
        private readonly IDataFunctions _dataFunctions;


        public AccountController(IDataFunctions dataFunctions)
        {
            //_configuration = configuration;
            //_env = env;
            _dataFunctions = dataFunctions;

            string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        }

        public IActionResult Login(string serviceCode, string agencyCode)
        {
            try
            {
                string getCodeUri = _dataFunctions.GetAuthCode(serviceCode, agencyCode, HttpContext);
                return Redirect(getCodeUri);
            }
            catch (Exception ex)
            {
                throw new Exception("Error: Something went wrong, please try again later." + ex);
            }

        }


        public async Task<IActionResult> CallBack(string code, string error, string state)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                // email error to eng.dspAccounts@lacity.org
                return RedirectToAction("Index", "Home", new { status = "Failed" });
            }

            // validate that the state received = state sent
            var stateSent = HttpContext.Session.GetString("state");
            var stateReceived = state;

            if (stateSent.ToLower() != stateReceived.ToLower())
            {
                ViewBag.Message = "Something went wrong. The Support team was notified of the error.";
                // email admin 
                return View("_Error");
            }

            string[] stateArray = state.Split('|');
            string serviceCode = stateArray[0];
            string agencyCode = stateArray[1];

            JwtJson jwt = await _dataFunctions.GetJwt(code);

            User user = await _dataFunctions.GetUserDetails(jwt);


            // User Claims & SignIn Start
            IList<Claim> userClaims = new List<Claim>
                {
                    // this will place all user info in a serialized userData claim
                    new Claim("userData", JsonConvert.SerializeObject(user)),
                    //new Claim(ClaimTypes.Name, user.UserId.ToString()),  // claim Name = userId for now
                };



            // place all user data in a session
            HttpContext.Session.SetJson("userData", user);

            var userIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);

            ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);

            // Using a local identity and signing in.
            _ = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);


            //return View("UserDetails", user);
            return View("UserDetails");

            // return RedirectToAction("PostData",user);

        }



    }
}
