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


            // keep this one to use for logout
            //var idTokenString = jwt.id_token;
            //var accessTokenString = jwt.access_token;

            //string refreshTokenString = null;
            //if (responseString.Contains("refresh_token"))
            //{
            //    refreshTokenString = jwt.refresh_token;
            //}



            //string[] arrStrings = idTokenString.Split('.');
            //byte[] data = Convert.FromBase64String(arrStrings[1]);
            //string decodedIdToken = Encoding.UTF8.GetString(data);

            //var idTokenPayLoad = System.Text.Json.JsonSerializer.Deserialize<IdTokenPayLoad>(decodedIdToken);

            return jwt;
        }


        public async Task<Staff> GetStaffDetails(JwtJson jwt)
        {
            // keep this one to use for logout
            var idTokenString = jwt.id_token;



            string[] arrStrings = idTokenString.Split('.');
            byte[] data = Convert.FromBase64String(arrStrings[1]);
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

            // HttpClient End

            var staffDataJson = System.Text.Json.JsonSerializer.Deserialize<StaffData>(jsonResult);


            string oidcAgencyCd = null;
            var organizations = staffDataJson.organizations;
            var depts = organizations.ToArray();
            var dept = depts[0].department;
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
            else
            {
                oidcAgencyCd = null;
            }

            //var accessTokenString = jwt.access_token;

            string refreshTokenString = null;
            if (!string.IsNullOrEmpty(jwt.refresh_token))
            {
                refreshTokenString = jwt.refresh_token;
            }


            // values returned by google IDM+
            var oidcEmail = staffDataJson.primaryEmail;
            var oidcLastName = staffDataJson.name.familyName;
            var oidcFirstName = staffDataJson.name.givenName;
            var oidcPaySrId = staffDataJson.customSchemas.LACityEmployeeID.employeeId;
            var oidcPhoneNumer = staffDataJson.customSchemas.LACityCustomAttributes.LACityWorkNumber;
            var oidcMobilePhone = staffDataJson.customSchemas.LACityCustomAttributes.LACityMobileNumber;
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


    }
}
