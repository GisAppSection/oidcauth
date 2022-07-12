using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OidcAuthV3.Models;
using OidcAuthV3.Utilities;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;

using Microsoft.AspNetCore.Mvc;
using OidcAuthV3.DataAccess;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace OidcAuthV3.DataAccess
{
    public class DataFunctions : IDataFunctions
    {
        private OidcAuthDbContext _oidcAuthContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private IStaffDataService _staffDataService;
        private IWebHostEnvironment _env;
        private IConfiguration _configuration;
        private IEmailService _emailService;

        private readonly string client_id;
        private readonly string client_secret;
        private readonly string auth_uri;
        private readonly string token_uri;
        private readonly string redirect_uri;
        private readonly string revoke_uri;

        private string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        public DataFunctions(OidcAuthDbContext oidcAuthContext, IConfiguration configuration, IWebHostEnvironment env, IEmailService emailService, IStaffDataService staffDataService, IHttpClientFactory httpClientFactory)
        {
            _oidcAuthContext = oidcAuthContext;
            _staffDataService = staffDataService;
            _configuration = configuration;
            _emailService = emailService;
            _env = env;



            client_id = _configuration["GoogleIdm:client_id"];
            client_secret = _configuration["GoogleIdm:client_secret"];
            auth_uri = _configuration["GoogleIdm:auth_uri"];
            token_uri = _configuration["GoogleIdm:token_uri"];
            _httpClientFactory = httpClientFactory;

            // send control to the following GoogleIdm once user is authenticated.  This is the home/callback
            redirect_uri = _configuration["GoogleIdm:redirect_uri"];
            revoke_uri = _configuration["GoogleIdm:revoke_uri"];
        }

        public Staff GetCurrentStaffM()
        {
            Staff staff = _staffDataService.GetStaff();
            return staff;
        }

        public string GetBaseRedirectUri(string serviceCode, string agencyCode)
        {
            if (envName.ToLower() == "development")
            {
                return _oidcAuthContext.ServiceInfo.Where(t => t.AgencyCd.ToLower() == agencyCode.ToLower() && t.ServiceCode.ToLower() == serviceCode.ToLower()).FirstOrDefault().DevRedirectUri;
            }
            else if (envName.ToLower() == "staging")
            {
                return _oidcAuthContext.ServiceInfo.Where(t => t.AgencyCd.ToLower() == agencyCode.ToLower() && t.ServiceCode.ToLower() == serviceCode.ToLower()).FirstOrDefault().StagingRedirectUri;
            }
            else if (envName.ToLower() == "production")
            {
                return _oidcAuthContext.ServiceInfo.Where(t => t.AgencyCd.ToLower() == agencyCode.ToLower() && t.ServiceCode.ToLower() == serviceCode.ToLower()).FirstOrDefault().ProdRedirectUri;
            }
            return null;


        }


        public string GetAuthCode(string serviceCode, string agencyCode, HttpContext httpContext)
        {
            string scope = "openid profile email https://www.googleapis.com/auth/admin.directory.user.readonly";
            Guid stateGuid = Guid.NewGuid();
            string stateSent = serviceCode + "|" + agencyCode + "|" + stateGuid.ToString();
            httpContext.Session.SetString("state", stateSent);
            StringBuilder sb = new StringBuilder();

            sb.Append(auth_uri);
            sb.Append("?client_id=" + client_id);
            sb.Append("&redirect_uri=" + redirect_uri);
            sb.Append("&scope=" + scope);
            sb.Append("&access_type=offline");  // online or offline, use offline to get refresh_token
            sb.Append("&include_granted_scopes=true");
            sb.Append("&response_type=code");
            sb.Append("&state=" + stateSent);
            sb.Append("&prompt=select_account");

            // prompt can be:  none, consent, or select_account 

            return sb.ToString();
        }

        public async Task<JwtJson> GetJwt(string code)
        {
            // move to function
            // string scope = "openid profile email https://www.googleapis.com/auth/admin.directory.user.readonly";
            StringBuilder sb = new StringBuilder();

            //sb.Append(token_uri);
            sb.Append("code=" + code);
            sb.Append("&client_id=" + client_id);
            sb.Append("&client_secret=" + client_secret);
            sb.Append("&redirect_uri=" + redirect_uri);
            sb.Append("&grant_type=authorization_code");


            //sb.Append("&scope=" + scope);

            string requestData = sb.ToString();

            // string responseString;

            // The code below works for a Post Request that requires setting Content-Type and submission as form fields
            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(token_uri);
            HttpContent content = new StringContent(requestData); // converts requestData to form fields
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            httpClient.DefaultRequestHeaders.Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
            // content.Headers.Add("Accept", "application/json");
            HttpResponseMessage httpResponse = await httpClient.PostAsync(token_uri, content);

            // validate that httpResponse has status = 200 before contuing.

            var responseString = httpResponse.Content.ReadAsStringAsync().Result;

            var jwt = System.Text.Json.JsonSerializer.Deserialize<JwtJson>(responseString);

            return jwt;
        }


        public async Task<Staff> GetStaffDetails(JwtJson jwt)
        {
            // keep this one to use for logout
            var idTokenString = jwt.id_token;

            string[] arrStrings = idTokenString.Split('.');

            string string0 = arrStrings[0].Trim();
            string string1 = arrStrings[1].Trim();
            string string2 = arrStrings[2].Trim();

            //// email developer
            //// comment this if the application runs successfully
            //try
            //{
            //    _emailService.SendEmailAsync("essam.amarragy@lacity.org", "", "", "string1 value", string1);
            //}
            //catch
            //{
            //    // do nothing
            //}

            StaffData staffDataJson = null;

            try
            {

                // testing:
                //string1 = "eyJpc3MiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20iLCJhenAiOiI3OTUyODE5OTEwMzgtbWlrZDgxaDU4ajI5cTV1dWJidDBtbnU4MThiamJnNWEuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJhdWQiOiI3OTUyODE5OTEwMzgtbWlrZDgxaDU4ajI5cTV1dWJidDBtbnU4MThiamJnNWEuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJzdWIiOiIxMTU0NTc1MTQ5NDEzNDk0NDYxNzciLCJoZCI6ImxhY2l0eS5vcmciLCJlbWFpbCI6ImFkYW0uYW5hbmRAbGFjaXR5Lm9yZyIsImVtYWlsX3ZlcmlmaWVkIjp0cnVlLCJhdF9oYXNoIjoiY0RDdV9BbEdiWXRUMzBFeGhRNWszZyIsIm5hbWUiOiJBZGFtIEFuYW5kIiwicGljdHVyZSI6Imh0dHBzOi8vbGgzLmdvb2dsZXVzZXJjb250ZW50LmNvbS9hL0FJdGJ2bW5OY1hFNGVveHdHU2pBSzV1UGZBR2VpMG84VTJiZ1U3THFZUGFmPXM5Ni1jIiwiZ2l2ZW5fbmFtZSI6IkFkYW0iLCJmYW1pbHlfbmFtZSI6IkFuYW5kIiwibG9jYWxlIjoiZW4iLCJpYXQiOjE2NTcxNDkxNzMsImV4cCI6MTY1NzE1Mjc3M30";


                string1 = Tools.AdjustBase64String(string1);

                byte[] data = Convert.FromBase64String(string1);
                string decodedIdToken = Encoding.UTF8.GetString(data);
                var idTokenPayLoad = System.Text.Json.JsonSerializer.Deserialize<IdTokenPayLoad>(decodedIdToken);



                // HttpClient Start
                var userDetailsUrl = "https://admin.googleapis.com/admin/directory/v1/users/" + idTokenPayLoad.sub + "?projection=full&viewType=domain_public";


                HttpClient httpClient = _httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri(userDetailsUrl);

                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header

                httpClient.DefaultRequestHeaders.Authorization
                             = new AuthenticationHeaderValue("Bearer", jwt.access_token);

                var result = httpClient.GetAsync(httpClient.BaseAddress).Result;
                var jsonResult = result.Content.ReadAsStringAsync().Result;

                staffDataJson = System.Text.Json.JsonSerializer.Deserialize<StaffData>(jsonResult);
            }
            catch (Exception ex)
            {
                _emailService.SendEmailAsync("essam.amarragy@lacity.org", "", "", "oidcAuth Exception", ex.ToString());
            }


            // HttpClient End




            string oidcAgencyCd = null;
            var organizations = staffDataJson.organizations;
            var depts = organizations.ToArray();
            var dept = depts[0].department;

            if (!string.IsNullOrEmpty(dept))
            {
                if (dept.Contains("Public Works") && dept.Contains("Engineering"))
                {
                    oidcAgencyCd = "BOE";

                }

                else if (dept.Contains("Public Works") && dept.Contains("Accounting"))
                {
                    oidcAgencyCd = "OOA";

                }

                else if (dept.Contains("Public Works") && dept.Contains("Light"))
                {
                    oidcAgencyCd = "BSL";

                }

                else if (dept.Contains("Public Works") && dept.Contains("Street"))
                {
                    oidcAgencyCd = "BSS";

                }

                else if (dept.Contains("Park") && dept.Contains("Rap"))
                {
                    oidcAgencyCd = "RAP";

                }

                else if (dept.Contains("Fire") && dept.Contains("LAFD"))
                {
                    oidcAgencyCd = "LAFD";

                }

                else if (dept.Contains("LADBS") && dept.Contains("DBS"))
                {
                    oidcAgencyCd = "LADBS";

                }

                // other deptartments has to be coded or create a lookup table.
            }

            else
            {
                oidcAgencyCd = null;
            }


            // set some defaults:
            string oidcPaySrId = null;
            string oidcPhoneNumer = null;
            string oidcMobilePhone = null;

            //var accessTokenString = jwt.access_token;
            // we are not using refresh_token in this application
            string refreshTokenString = null;
            if (!string.IsNullOrEmpty(jwt.refresh_token))
            {
                refreshTokenString = jwt.refresh_token;
            }
            // we are not using refresh_token in this application

            // values returned by google IDM
            var oidcEmail = staffDataJson.primaryEmail;
            var oidcLastName = staffDataJson.name.familyName;
            var oidcFirstName = staffDataJson.name.givenName;
            // customSchemas can be nulll for consultants
            if (staffDataJson.customSchemas != null)
            {
                oidcPaySrId = staffDataJson.customSchemas?.LACityEmployeeID?.employeeId;
                oidcPhoneNumer = staffDataJson.customSchemas?.LACityCustomAttributes?.LACityWorkNumber;
                oidcMobilePhone = staffDataJson.customSchemas?.LACityCustomAttributes?.LACityMobileNumber;
            }

            var oidcPhotoUrl = staffDataJson.thumbnailPhotoUrl;
            var oidcDept = dept;

            var staff = new Staff();




            staff.Email = oidcEmail;
            staff.LastName = oidcLastName;
            staff.FirstName = oidcFirstName;
            staff.PaySrId = oidcPaySrId;
            staff.WorkPhone = oidcPhoneNumer;
            staff.MobilePhone = oidcMobilePhone;
            staff.PhotoUrl = oidcPhotoUrl;
            staff.Dept = oidcDept;
            staff.AgencyCd = oidcAgencyCd;
            staff.access_token = jwt.access_token;
            staff.expires_in = jwt.expires_in;

            if (!string.IsNullOrEmpty(jwt.refresh_token))
            {
                staff.refresh_token = jwt.refresh_token;
            }

            return staff;
        }

        public long GetNextLogId()
        {
            SqlParameter result = new SqlParameter("@result", System.Data.SqlDbType.BigInt)
            {
                Direction = System.Data.ParameterDirection.Output
            };

            _oidcAuthContext.Database.ExecuteSqlRaw("SELECT @result = (NEXT VALUE FOR LogIdSequence)", result);

            return (long)result.Value;
        }

        public bool WriteException(string exceptionSubject, string exceptionDetails)
        {
            ExceptionLog exceptionLog = new ExceptionLog();

            try
            {
                exceptionLog.LogId = GetNextLogId();
                exceptionLog.LogDate = DateTime.Now;
                exceptionLog.LogSubject = exceptionSubject;
                exceptionLog.LogDetails = exceptionDetails;
                _oidcAuthContext.ExceptionLog.Add(exceptionLog);
                _oidcAuthContext.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }


        public List<ExceptionLog> ListExceptionLogsM()
        {
            var exceptionLogs = _oidcAuthContext.ExceptionLog.OrderByDescending(t => t.LogDate).Take(300).ToList();
            return exceptionLogs;
        }


        public bool DeleteExceptionLogM(long logId)
        {
            var exceptionLog = _oidcAuthContext.ExceptionLog.Where(t => t.LogId == logId).FirstOrDefault();
            if (exceptionLog != null)
            {
                _oidcAuthContext.Remove(exceptionLog);
                _oidcAuthContext.SaveChanges();
                return true;

            }
            return false;

        }

        public bool DeleteExceptionLog30M()
        {
            DateTime lastDate = DateTime.Now.AddDays(-30);
            List<ExceptionLog> exceptionLogs = _oidcAuthContext.ExceptionLog.Where(t => t.LogDate  < lastDate).ToList();
            if (exceptionLogs.Count > 0)
            {
                _oidcAuthContext.Remove(exceptionLogs);
                _oidcAuthContext.SaveChanges();
                return true;

            }
            return false;

        }


    }
}
