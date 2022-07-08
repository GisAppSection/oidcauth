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
        private readonly IDataFunctions _dataFunctions;

       public AccountController(IDataFunctions dataFunctions)
        {
            //_configuration = configuration;
            //_env = env;
            _dataFunctions = dataFunctions;

            string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        }



        // For dotnet apps remove the two paramerters serviceCode and agency Code.
        public IActionResult Login(string serviceCode, string agencyCode)
        {
            if (string.IsNullOrEmpty(serviceCode) || string.IsNullOrEmpty(agencyCode))
            {
                ViewBag.Message = "An Error Occured.";
                return View("_Error");
            }
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

            Staff staff = await _dataFunctions.GetStaffDetails(jwt);


            // User Claims & SignIn Start
            IList<Claim> userClaims = new List<Claim>
                {
                    // this will place all user info in a serialized staffData claim
                    new Claim("staffData", JsonConvert.SerializeObject(staff)),
                    //new Claim(ClaimTypes.Name, user.UserId.ToString()),  // claim Name = userId for now
                };



            // place all user data in a session
            HttpContext.Session.SetJson("staffData", staff);

            var userIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);

            ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);

            // Using a local identity and signing in.
            _ = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);


            string baseUrl = _dataFunctions.GetBaseRedirectUri(serviceCode, agencyCode);
            //"http://localhost/apermits/oidc/loginboeuser.cfm";



            StringBuilder serviceUri = new StringBuilder();
            serviceUri = serviceUri.Append(baseUrl);
            string eemail = Tools.eencrypt(staff.Email);
            string epaySrId = Tools.eencrypt(staff.PaySrId);
            string ephotoUrl = Tools.eencrypt(staff.PhotoUrl);

            serviceUri = serviceUri.Append("?eemail=" + eemail);
            serviceUri = serviceUri.Append("&epaySrId=" + epaySrId);
            serviceUri = serviceUri.Append("&ephotoUrl=" + ephotoUrl);

            // use the following url for testing
            //return RedirectToAction("Index","Home"); 
            
            // use the following return when redirecting to permits.
             return Redirect(serviceUri.ToString());

        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }



    }
}
