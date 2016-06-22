using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace HAMS.WebUI.Models
{

    public class MessageServices
    {
        public async static Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var _email = "hamstest@gmail.com";
                var _epass = "$ecre!123";//ConfigurationManager.AppSettings["$ecre!123"];
                var _dispName = "HAMS Admin";
                MailMessage myMessage = new MailMessage();
                //myMessage.CC.Add()
                myMessage.To.Add(email);
                //myMessage.To.Add()
                myMessage.From = new MailAddress(_email, _dispName);
                myMessage.Subject = subject;
                myMessage.Body = message;
                myMessage.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com"))
                {
                    smtp.EnableSsl = true;
                    //smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587; // 587
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(_email, _epass);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.SendCompleted += (s, e) => { smtp.Dispose(); };
                    await smtp.SendMailAsync(myMessage);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}