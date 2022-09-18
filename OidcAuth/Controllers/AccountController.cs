using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OidcAuthDataAccess;
using OidcAuth.Models;
using OidcAuth.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using OidcAuth.DataAccess;

namespace OidcAuth.Controllers
{
    public class AccountController : Controller
    {
        private readonly IDataFunctions _dataFunctions;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AccountController(IDataFunctions dataFunctions, IEmailService emailService, IConfiguration configuration)
        {
            //_configuration = configuration;
            //_env = env;
            _dataFunctions = dataFunctions;
            _emailService = emailService;
            _configuration = configuration;
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
                throw new Exception("Error: Something went wrong, please try again later." + ex.Message);
            }

        }


        public async Task<IActionResult> CallBack(string code, string error, string state)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                // email admin
                var emailTo = _configuration["AppConfig:ConnStringOidcAuthDb"];
                await _emailService.SendEmailAsync(emailTo,"","", "GoogleIDM Error", "Counld Not Login User" + error);
                ViewBag.Message = "Something went wrong. The Support team was notified of the error.";

                return View("_Error");
                //return RedirectToAction("Index", "Home", new { status = "Failed" });
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
            //let's write state, code, and agencycode in logs
            //_dataFunctions.WriteException("Info log", "state :" + state);
            //_dataFunctions.WriteException("Info log", "serviceCode :" + serviceCode);
            //_dataFunctions.WriteException("Info log", "agencyCode :" + agencyCode);
            JwtJson jwt = await _dataFunctions.GetJwt(code);

            Staff staff = await _dataFunctions.GetStaffDetails(jwt);

            // let us save the staff object to the oidcauth database within the tCityStaff


            // email staff values as json to developer to monitor the system for a few weeks.
            try
            {
                string staffJson = JsonConvert.SerializeObject(staff);
                await _emailService.SendEmailAsync("essam.amarragy@lacity.org", "", "", "staff object values from oidc auth", staffJson);
            }
            catch
            {
                // do nothing
            }


            // throw new Exception("Test Exception Error Controller and Logging to database");

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

            
            // This is a call to database
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
