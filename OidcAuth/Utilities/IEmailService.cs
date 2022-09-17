using System.Threading.Tasks;


// https://steemit.com/utopian-io/@babelek/how-to-send-email-using-asp-net-core-2-0
namespace OidcAuth.Utilities
{
    public interface IEmailService
    {
        Task SendEmailAsync(string ToEmail, string CcEmail, string BccEmail, string subject, string message);
    }
}