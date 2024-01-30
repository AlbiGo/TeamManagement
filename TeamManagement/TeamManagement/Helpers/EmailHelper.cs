
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using TeamMan.Models;
using System.Net;
using System.Text;
using Ionic.Zip;
using MimeKit.Cryptography;
using System.IO;
using System.IO.Compression;
using System.Net.Mime;
using Microsoft.AspNetCore.Hosting;
using TeamMan.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Policy;

namespace TeamMan.Helpers
{
    public class EmailHelper 
    {
        private ApplicationContext _applicationContext;
        private IHostingEnvironment _env;
        public EmailHelper(ApplicationContext applicationContext,
             IHostingEnvironment env)
        {
            _applicationContext = applicationContext;
            _env = env;

        }

        public bool emailValidator(string email)
        {
            try
            {
                MailAddress m = new MailAddress(email);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        public string sendEmailOnCreateUser(string subject , RegistrationViewModel receiver ,string message ,string callbackURL)
        {
            try
            {
              
                    if (this.emailValidator(receiver.Email))
                    {

                        var smtpClient = new SmtpClient()
                        {
                            Host = "Smtp.Gmail.com",
                            Port = 587,
                            EnableSsl = true,
                            Timeout = 10000,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = true,
                            Credentials = new NetworkCredential("workalbi99@gmail.com", "albi1997") //Admin Email
                        };
                    var userName = receiver.FirstName + receiver.LastName;
                    var builder = new BodyBuilder();
                    var pathToFile = _env.ContentRootPath 
                           + Path.DirectorySeparatorChar.ToString()
                           + "Templates"
                           + Path.DirectorySeparatorChar.ToString()
                           + "Email"
                           + Path.DirectorySeparatorChar.ToString()
                           + "CreatedAccount.html";
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToFile))
                    {

                        builder.HtmlBody = SourceReader.ReadToEnd();

                    }
                    string messageBody = string.Format(builder.HtmlBody,
                                           subject,
                                           String.Format("{0:dddd, d MMMM yyyy}", DateTime.Now),
                                           userName,
                                           receiver.Email,
                                           receiver.password,
                                           message,
                                           callbackURL
                                           ); var MailMessage = new MailMessage()
                    {
                        From = new MailAddress("workalbi99@gmail.com", "Albi Work"),
                        To = { receiver.Email },
                        Subject = subject,
                        Body = messageBody,
                        BodyEncoding = Encoding.UTF8,
                        IsBodyHtml = true
                        };
                        
                        smtpClient.Send(MailMessage);
                        string s = "Email has been sent successfully.";
                        return s;
                    }
                    else
                    {
                        throw new Exception("Email Addres is not valid");
                    }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        
    }
        public SmtpClient createStmp()
        {
            var smtpClient = new SmtpClient()
            {
                Host = "Smtp.Gmail.com",
                Port = 587,
                EnableSsl = true,
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = true,
                Credentials = new NetworkCredential("workalbi99@gmail.com", "albi1997") //Admin Email
            };
            return smtpClient;
        }

        public string sendEmail(TeamMan.Models.Task task, string receiver, string message)
        {
            try
            {
                var user = _applicationContext.Users.Where(p => p.Id == task.UserID).FirstOrDefault();
                if (user != null)
                {
                    var userEmailAddress = user.Email;
                    if (this.emailValidator(userEmailAddress))
                    {

                        var smtpClient = new SmtpClient()
                        {
                            Host = "Smtp.Gmail.com",
                            Port = 587,
                            EnableSsl = true,
                            Timeout = 10000,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = true,
                            Credentials = new NetworkCredential("workalbi99@gmail.com", "albi1997") //Admin Email
                        };
                        var MailMessage = new MailMessage()
                        {
                            From = new MailAddress("workalbi99@gmail.com", "Albi Work"),
                            To = { receiver },
                            Subject = "Task " + task.taskNumber.ToString(),
                            Body = message,
                            BodyEncoding = Encoding.UTF8
                        };
                        if (task.Status == 3)
                        {
                            string fileName = "Assets\\Designs\\" + task.designPath;
                            Attachment data = new Attachment(fileName, MediaTypeNames.Application.Octet);
                            MailMessage.Attachments.Add(data);
                        }
                        smtpClient.Send(MailMessage);
                        string s = "Email has been sent successfully.";
                        return s;
                    }
                    else
                    {
                        throw new Exception("Email Addres is not valid");
                    }

                }
                else
                {
                    throw new Exception("User does not exist");
                }



            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string SendSimpleEmail(string message ,string receiver)
        {
            try
            {
                var stmp = this.createStmp();
                var MailMessage = new MailMessage()
                {
                    From = new MailAddress("workalbi99@gmail.com", "Albi Work"),
                    To = { receiver },
                    Subject = "Task Removed",
                    Body = message,
                    BodyEncoding = Encoding.UTF8
                };
                stmp.Send(MailMessage);
                string s = "Email has been sent successfully.";
                return s;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            

        }
        public string sendEmailToSignedUpUser(string subject, string message, string callbackURL , string password , string username,string email)
        {
            try
            {
                var stmp = this.createStmp();
                var builder = new BodyBuilder();
                var pathToFile = _env.ContentRootPath
                       + Path.DirectorySeparatorChar.ToString()
                       + "Templates"
                       + Path.DirectorySeparatorChar.ToString()
                       + "Email"
                       + Path.DirectorySeparatorChar.ToString()
                       + "CreatedAccount.html";
                using (StreamReader SourceReader = System.IO.File.OpenText(pathToFile))
                {

                    builder.HtmlBody = SourceReader.ReadToEnd();

                }
                string messageBody = string.Format(builder.HtmlBody,
                                       subject,
                                       String.Format("{0:dddd, d MMMM yyyy}", DateTime.Now),
                                       username,
                                       email,
                                       password,
                                       message,
                                       callbackURL
                                       ); var MailMessage = new MailMessage()
                                       {
                                           From = new MailAddress("workalbi99@gmail.com", "Albi Work"),
                                           To = { email },
                                           Subject = subject,
                                           Body = messageBody,
                                           BodyEncoding = Encoding.UTF8,
                                           IsBodyHtml = true
                                       };
                stmp.Send(MailMessage);
                string s = "Email has been sent successfully.";
                return s;
            }
        

            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
