using DataLayer;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace ServiceLayer
{
    public class MailService
    {
        private readonly IConfiguration _config;

        public MailService(IConfiguration config)
        {
            _config = config;
        }
        public bool SendEmail(User updatedUser, string newPassword)
        {
            try
            {
                //get credentials for mail that will send e-mails to users
                string senderEmail = _config.GetValue<string>("SenderEmail");
                string senderPassword = _config.GetValue<string>("SenderPassword");
                //mail text content
                string subject = "HermesChat: Account Recovery";
                string body = @$"<p>Hello, dear {updatedUser.Username}!
                            <br/>
                            Here is your new password : <b>{newPassword}</b>
                            See you on https://hermeschatapp20220326121130.azurewebsites.net/
                            <br/>
                            Regards, Hermes App Teeam</p>";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(senderEmail);

                SmtpClient smtp = new SmtpClient();
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(mail.From.Address, senderPassword);
                smtp.Host = "smtp.gmail.com";

                mail.To.Add(new MailAddress(updatedUser.Email));
                mail.IsBodyHtml = true;
                mail.Subject = subject;
                mail.Body = body;
                //send mail
                smtp.Send(mail);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}