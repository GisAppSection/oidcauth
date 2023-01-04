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
        private readonly IAppUserData _appUserData;

        public AccountController(IDataFunctions dataFunctions, IEmailService emailService, IConfiguration configuration, IAppUserData appUserData)
        {
            //_configuration = configuration;
            //_env = env;
            _dataFunctions = dataFunctions;
            _emailService = emailService;
            _configuration = configuration;
            _appUserData = appUserData;
            string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        }

        // For dotnet apps remove the two paramerters serviceCode and agency Code.
        public IActionResult Login(string serviceCode, string agencyCode)
        {
            // validation
            if (string.IsNullOrEmpty(serviceCode) || string.IsNullOrEmpty(agencyCode))
            {
                throw new Exception("OidcAuth: Error Login100: Invalid Login.");
                //ViewBag.Message = "An Error Occured. Incomplete Information.";
                //return View("_Error");
            }
            try
            {
                string getCodeUri = _dataFunctions.GetAuthCode(serviceCode, agencyCode);  // , HttpContext
                return Redirect(getCodeUri);
            }
            catch
            {
                throw new Exception("OidcAuth: Error Login110: Something went wrong, please try again later.");
            }

        }


        public async Task<IActionResult> CallBack(string code, string error, string state)
        {
             if (!string.IsNullOrWhiteSpace(error))
            {
                if (_configuration["AppConfig:SendAdminEmails"] == "y")
                {
                    // email admin
                    var emailTo = _configuration["AppConfig:AppAdminEmail"];
                    await _emailService.SendEmailAsync(emailTo, "", "", "OidcAuth: Error CallBack100: GoogleIDM received error", "Error CallBack100: Could Not Login User " + error);
                }

                throw new Exception("OidcAuth: Error CallBack100: GoogleIDM received error " + error);
            }

            // validate that the state received = state sent
            // var stateSent = HttpContext.Session.GetString("state");
            var stateSent = _appUserData.GoogleIDMState;
            var stateReceived = state;

            if (_configuration["AppConfig:SendAdminEmails"] == "y" && string.IsNullOrEmpty(stateReceived) )
            {
                // email admin
                var emailTo = _configuration["AppConfig:AppAdminEmail"];
                await _emailService.SendEmailAsync(emailTo, "", "", "OidcAuth: Error CallBack110: stateSent is not equal to stateReceived, Check session variables.", "OidcAuth: CallBack110: State Received= null " + " and State Sent= " + stateSent);
            }

            if (stateSent.ToLower() != stateReceived.ToLower())
            {
                if (_configuration["AppConfig:SendAdminEmails"] == "y")
                {
                    // email admin
                    var emailTo = _configuration["AppConfig:AppAdminEmail"];
                    await _emailService.SendEmailAsync(emailTo, "", "", "OidcAuth: Error CallBack110: stateSent is not equal to stateReceived, Check session variables.", "OidcAuth: CallBack110: State Received= " + stateReceived + " is not equal to State Sent= " + stateSent);
                }
                // Activate the line below at a later time.
                //throw new Exception("Error CallBack110: stateSent is not equal to stateReceived, Check session variables.");
            }

            string[] stateArray = state.Split('|');
            string serviceCode = stateArray[0];
            string agencyCode = stateArray[1];
            //let's write state, code, and agencycode in logs
            //_dataFunctions.WriteException("Info log", "state :" + state);
            //_dataFunctions.WriteException("Info log", "serviceCode :" + serviceCode);
            //_dataFunctions.WriteException("Info log", "agencyCode :" + agencyCode);

            if (string.IsNullOrWhiteSpace(code))
            {
                if (_configuration["AppConfig:SendAdminEmails"] == "y")
                {
                    // email admin
                    var emailTo = _configuration["AppConfig:AppAdminEmail"];
                    await _emailService.SendEmailAsync(emailTo, "", "", "OidcAuth: Error AC120: GoogleIDM code not received.", "Error AC 120: not receiving code from Google IDM " + error);
                }

                    throw new Exception("OidcAuth: Error CallBack120: did not receive code from Google IDM.");
            }


            JwtJson jwt = await _dataFunctions.GetJwt(code);


            Staff staff = await _dataFunctions.GetStaffDetails(jwt);

            // let us save the staff object to the oidcauth database within the tCityStaff


            // email staff values as json to developer to monitor the system for a few weeks.
            try
            {
                string staffJson = JsonConvert.SerializeObject(staff);
                //if (_configuration["AppConfig:SendAdminEmails"] == "y")
                //{
                //    await _emailService.SendEmailAsync("essam.amarragy@lacity.org", "", "", "staff object values from oidc auth", staffJson);
                //}

            }
            catch
            {
                throw new Exception("OidcAuth: Error CallBack120: Could not Serialize object staff for emailing admin.");
            }


            // throw new Exception("OidcAuth: Test Exception Error Controller and Logging to database");

            // User Claims & SignIn Start
            IList<Claim> userClaims = new List<Claim>
                {
                    // this will place all user info in a serialized staffData claim
                    new Claim("staffData", JsonConvert.SerializeObject(staff)),
                    //new Claim(ClaimTypes.Name, user.UserId.ToString()),  // claim Name = userId for now
                };



            // place all user data in a session
            //HttpContext.Session.SetJson("staffData", staff);

            var userIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);

            ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);

            // Using a local identity and signing in.
            _ = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);


            // This is a call to database
            string baseUrl = _dataFunctions.GetBaseRedirectUri(serviceCode, agencyCode);
            //"http://localhost/apermits/oidc/loginboeuser.cfm";


            StringBuilder serviceUri = new StringBuilder();
            serviceUri = serviceUri.Append(baseUrl);
            string eemail = Tools.Eencrypt(staff.Email);
            string epaySrId = Tools.Eencrypt(staff.PaySrId);
            string ephotoUrl = Tools.Eencrypt(staff.PhotoUrl);

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
