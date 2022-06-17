using Microsoft.AspNetCore.Http;
using OidcAuthV3.Models;
using System.Threading.Tasks;

namespace OidcAuthV3.DataAccess
{
    public interface IDataFunctions
    {
        User GetCurrentUserM();
        string GetBaseRedirectUri(string serviceCode, string agencyCode);

        string GetAuthCode(string serviceCode, string agencyCode, HttpContext httpcontext);

        Task<JwtJson> GetJwt(string code);

        Task<User> GetUserDetails(JwtJson jwt);
    }
}
