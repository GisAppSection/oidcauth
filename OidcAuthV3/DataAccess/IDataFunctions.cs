using Microsoft.AspNetCore.Http;
using OidcAuthV3.Models;
using System.Threading.Tasks;

namespace OidcAuthV3.DataAccess
{
    public interface IDataFunctions
    {
        Staff GetCurrentStaffM();
        string GetBaseRedirectUri(string serviceCode, string agencyCode);

        string GetAuthCode(string serviceCode, string agencyCode, HttpContext httpcontext);

        Task<JwtJson> GetJwt(string code);

        Task<Staff> GetStaffDetails(JwtJson jwt);

    }
}
