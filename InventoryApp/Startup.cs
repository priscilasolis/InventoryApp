using Hangfire;
using InventoryApp.Repositories;
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

[assembly: OwinStartupAttribute(typeof(InventoryApp.Startup))]
namespace InventoryApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            GlobalConfiguration.Configuration

                .UseSqlServerStorage("DefaultConnection");

            /*para crear la fecha en la que se enviara el correo, es necesario hacer un string donde se ponga lo siguiente "mi ho * * d" donde
            mi=minutos ho=horas y d=día de la semana, minutos no cambia, la hora esta en formato de 24 horas
            y el día va de 0 a 6 donde el 0 es domingo y el 6 es sabado*/
            string hora;
            hora = "27 17 * * 0";
            RecurringJob.AddOrUpdate(() => NewEmail(), hora, TimeZoneInfo.Local);

            app.UseHangfireDashboard();

            app.UseHangfireServer();

        }
        //Este metodo manda un correo
        public void NewEmail()
        {
            var repo = new InventoryRepository();

            MailMessage mail = new MailMessage();
            SmtpClient client = new SmtpClient()
            {
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Host = "smtp.gmail.com",
                Credentials = new System.Net.NetworkCredential("erickelias881@gmail.com", "Erick2127")
            };
            List<string> superusersEmails = repo.GetSuperUsers().Select(u => u.Email).ToList();

            foreach (string email in superusersEmails)
            {
                mail.To.Add(new MailAddress(email));
            }
            mail.From = new MailAddress("erickelias881@gmail.com");
            mail.Subject = "InventoryApp - Weekly report";
            

            string body = "<h1>This is the report of the week</h1>";

            var lowInventory = repo.GetItemsByThreshold();
            var mostSold = repo.GetMostSoldWeek();

            body += "<h2>Most sold items</h2>";
            foreach (var item in mostSold)
            {
                body += string.Format("<p>{0}. {1}</p>", mostSold.IndexOf(item), item.Name);
            }

            body += "<h2>Items with low quantity in stock</h2>";
            foreach (var item in lowInventory)
            {
                if (item.Quantity < item.Threshold)
                {
                    body += string.Format("<p style=\"color: red;\">{0} - {1}; Threshold: {2}</p>", item.Name, item.Quantity, item.Threshold);
                }
                else
                {
                    body += string.Format("<p>{0} - {1}; Threshold: {2}</p>", item.Name, item.Quantity, item.Threshold);
                }
            }

            mail.Body = body;
            

            mail.IsBodyHtml = true;
            client.Send(mail);
        }
    }
}