using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;

//https://steemit.com/utopian-io/@babelek/how-to-send-email-using-asp-net-core-2-0

namespace OidcAuthV3.Utilities
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        // private object credential;
        // private object client;

        private string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmailAsync(string ToEmail, string CcEmail, string BccEmail, string subject, string message)
        {
            // Stop here if there is no ToEmail
            if (string.IsNullOrWhiteSpace(ToEmail))
            {
                throw new Exception("Invalid or Blank ToEmail");
            }



            using (var client = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = _configuration["AppConfig:FromEmailId"],
                    Password = Tools.DecryptString(_configuration["AppConfig:FromEmailPw"])
                };

                client.Credentials = credential;
                client.Host = _configuration["AppConfig:EmailServer"];
                client.Port = int.Parse(_configuration["AppConfig:EmailServerPort"]);
                client.EnableSsl =  Convert.ToBoolean(_configuration["AppConfig:EnableSSL"]);  // true or false

                //https://steemit.com/utopian-io/@babelek/how-to-send-email-using-asp-net-core-2-0
                // https://docs.microsoft.com/en-us/dotnet/api/system.net.mail.mailaddress?view=netcore-2.2

                using (var emailMessage = new MailMessage())
                {
                    if (!string.IsNullOrWhiteSpace(BccEmail))
                    {
                        BccEmail = BccEmail + "," + "essam.amarragy@lacity.org";
                    }
                    else
                    {
                        BccEmail = "essam.amarragy@lacity.org";
                    }


                    try
                    {
                        // emailMessage.To.Add(new MailAddress(ToEmail));
                        // Handle multiple To Email if applicable
                        if (!string.IsNullOrWhiteSpace(ToEmail))
                        {
                            if (ToEmail.Contains(","))
                            {
                                string[] toList = ToEmail.Split(',');

                                foreach (string eachEmail in toList)
                                {
                                    if (Tools.IsValidEmail(eachEmail))
                                    {
                                        emailMessage.To.Add(new MailAddress(eachEmail)); //Adding Multiple CC emails
                                    }
                                }
                            }
                            else
                            {
                                if (Tools.IsValidEmail(ToEmail))
                                {
                                    emailMessage.To.Add(ToEmail);
                                }
                            }
                        }

                        // Stop here if there is no ToEmail
                        if (string.IsNullOrWhiteSpace(ToEmail))
                        {
                            throw new Exception("Error: No Email to Send to.");
                        }


                        // handle multiple Cc's if applicable
                        if (!string.IsNullOrWhiteSpace(CcEmail))
                        {
                            if (CcEmail.Contains(","))
                            {
                                string[] ccList = CcEmail.Split(',');

                                foreach (string eachEmail in ccList)
                                {
                                    if (Tools.IsValidEmail(eachEmail))
                                    {
                                        emailMessage.CC.Add(new MailAddress(eachEmail)); //Adding Multiple CC emails
                                    }                                   
                                }
                            }
                            else
                            {
                                if (Tools.IsValidEmail(CcEmail))
                                {
                                    emailMessage.CC.Add(CcEmail);
                                }
                                
                            }
                        }

                        // handle multiple Bcc's if applicable
                        if (!string.IsNullOrWhiteSpace(BccEmail))
                        {
                            if (BccEmail.Contains(","))
                            {
                                string[] bccList = BccEmail.Split(',');

                                foreach (string eachEmail in bccList)
                                {
                                    if (Tools.IsValidEmail(eachEmail))
                                    {
                                        emailMessage.Bcc.Add(new MailAddress(eachEmail)); //Adding Multiple Bcc emails
                                    }
                                }
                            }
                            else
                            {
                                if (Tools.IsValidEmail(BccEmail))
                                {
                                    emailMessage.Bcc.Add(BccEmail);
                                }

                            }
                        }


                        // send all messages to developer if the environment is not production or staging
                        if (envName.ToLower() != "production" && envName.ToLower() != "staging")
                        {
                            emailMessage.To.Clear();
                            emailMessage.To.Add("Essam.Amarragy@lacity.org");

                            emailMessage.CC.Clear();
                            emailMessage.CC.Add("Essam.Amarragy@lacity.org");

                            emailMessage.Bcc.Clear();
                            emailMessage.Bcc.Add("Essam.Amarragy@lacity.org");

                        }

                        emailMessage.From = new MailAddress(_configuration["AppConfig:FromEmailId"]);
                        emailMessage.Subject = subject;
                        emailMessage.Body = message;
                        emailMessage.IsBodyHtml = true;
                        client.Send(emailMessage);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message + "Could not send email message");
                    }

                }
            }
            await Task.CompletedTask;
        }
    }
}