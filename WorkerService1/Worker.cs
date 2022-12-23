using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;
using WorkerService1.Database;
using WorkerService1.Helper;
using WorkerService1.Model;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

namespace WorkerService1
{
    public class Worker : BackgroundService
    {
        private readonly string apiUserList;
        private readonly string apiConversationOpen;
        private readonly string apiPostMessage;
        private readonly string token;
        private readonly string type;
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRepository _repository;
        private readonly IConfiguration _configuration;


        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _repository = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IRepository>();
            _configuration = configuration;
            apiUserList = _configuration["ApiUserList"].ToString();
            apiConversationOpen = _configuration["ApiConversationOpen"].ToString();
            apiPostMessage = _configuration["ApiPostMessage"].ToString();
            token = _configuration["Token"].ToString();
            type = _configuration["Type"].ToString();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                GetAllEvents();
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        /// <summary>
        /// GetAllEvents
        /// </summary>
        /// <returns></returns>
        public void  GetAllEvents()
        {
            try
            {
                var MinutesMargin = DateTime.Now + TimeSpan.FromMinutes(5);

                var data = _repository.GetAll<TblSetEvent>(x => x.Time == DateTime.Now && x.Time <= MinutesMargin, u => u.User);

                var response = data.Select(x => new EventViewModel
                {
                    EventId = x.EventId,
                    UserId = x.UserId,
                    UserEmail = x.User.Email,
                    UserName = x.User.FirstName,
                    Title = x.Title,
                    Description = x.Descrption,
                    Time = x.Time,
                    channelId=x.User.ChannelId,

                }).ToList();

                foreach (var item in response)
                {
                    _logger.LogInformation($"Event reminder for {item.Time} mailed for event ID: {item.EventId}");
                    SendEmail((EventViewModel)item);
                    if (string.IsNullOrEmpty(item.channelId))
                    {
                        UserList(item.UserEmail);
                    }
                    else
                    {
                        SendMessage(item.channelId, $"{item.Title} {item.Description}");
                    }   
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} {ex.StackTrace}");

            }
        }

        /// <summary>
        /// SendEmail
        /// </summary>
        /// <param name="eve"></param>
        public void SendEmail(EventViewModel eve)
        {
            string emailTemplatePath = $"{Environment.CurrentDirectory}\\Email_Template\\";     
            string body = null;
            emailTemplatePath = $"{emailTemplatePath}Reminder_Email.html";
            StreamReader ReminderEmail = new StreamReader(emailTemplatePath);
            body = ReminderEmail.ReadToEnd();
            ReminderEmail.Close();

            body = body.Replace("[FullName]", eve.EventId.ToString());
            body = body.Replace("[Title]", eve.Title);
            body = body.Replace("[Description]", eve.Description);
            body = body.Replace("[Time]", eve.Time.ToString());
            body = body.Replace("[Name]", eve.UserName);

            var username = eve.UserEmail;
            var subject = $" Event reminder {username}";
            var users = _repository.GetAll<TblUserRegsitration>();
            EmailSettings(users.FirstOrDefault(x => x.UserId == eve.UserId).Email, subject, body.ToString());
        }

        /// <summary>
        /// EmailSettings
        /// </summary>
        /// <param name="email"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public bool EmailSettings(string email, string subject, string body)
        {
            try
            {
                SmtpSettings smtpSettings = new SmtpSettings
                {
                    Username = _configuration.GetValue<string>("SmtpSettingsCom:Username"),
                    Password = _configuration.GetValue<string>("SmtpSettingsCom:Password"),
                    Host = _configuration.GetValue<string>("SmtpSettingsCom:Host"),
                    Port = _configuration.GetValue<int>("SmtpSettingsCom:Port"),
                    EnableSsl = _configuration.GetValue<bool>("SmtpSettingsCom:EnableSsl")
                };
                var MyMessage = new MailMessage();
                using SmtpClient client = new SmtpClient
                {
                    Host = smtpSettings.Host,
                    EnableSsl = (bool)smtpSettings.EnableSsl,
                    Port = smtpSettings.Port,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password)
                };
                string to = email != null ? string.Join(",", email) : null;
                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(smtpSettings.Username);
                    mail.To.Add(to);
                    mail.Subject = subject;
                    mail.Body = body.ToString();
                    mail.IsBodyHtml = true;
                    client.Send(mail);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        
        private HttpClient client = new HttpClient();

        /// <summary>
        /// UserList
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task UserList(string email)
        {
            try
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");

                var req = new HttpRequestMessage(HttpMethod.Get, apiUserList);
                req.Headers.Authorization = new AuthenticationHeaderValue(type, token);

                Console.WriteLine(req.ToString());
                var response1 = await client.SendAsync(req);

                string output = await response1.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Root>(output);
                var newString = email.Split('@').First();
                var data1 = result.members.Where(x => x.name == newString).FirstOrDefault();
                string id = data1.id;

                await GetUserById(id, email);
            }
            catch(Exception ex)
            {
                _logger.LogError($"{ex.Message} {ex.StackTrace}");
            }
        }

        /// <summary>
        /// GetUserById
        /// </summary>
        /// <param name="id"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task GetUserById(string id ,string email)
        {
            try
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");

                var data = new { users = id };

                var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(type, token);
                var res = await client.PostAsync(apiConversationOpen, content);

                var output = await res.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<Root>(output);
                string channelId = result.channel.id;


                var tblData = _repository.Get<TblUserRegsitration>(x => x.Email == email);

                tblData.ChannelId = channelId;

                _repository.Update(tblData);
                _repository.SaveChanges();

                var data1 = _repository.GetAll<TblSetEvent>(u => u.User);

                var response = data1.Select(x => new EventViewModel
                {
                    EventId = x.EventId,
                    UserId = x.UserId,
                    UserEmail = x.User.Email,
                    UserName = x.User.FirstName,
                    Title = x.Title,
                    Description = x.Descrption,
                    Time = x.Time,
                    channelId = x.User.ChannelId,
                }).ToList();
                foreach (var item in response)
                {
                    await SendMessage(item.channelId, $"{item.Title} {item.Description} ");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"{ex.Message} {ex.StackTrace}");
            }
        }

        /// <summary>
        /// SendMessage
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessage(string channelId ,string message)
        {
            try
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");

                var data = new { channel = channelId, text = message };

                var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(type, token);
                var res = await client.PostAsync(apiPostMessage, content);

                var output = await res.Content.ReadAsStringAsync();


                //var result = JsonConvert.DeserializeObject<Root>(output);

            }
            catch(Exception ex)
            {
                _logger.LogError($"{ex.Message} {ex.StackTrace}");
            }
        }

    }
}