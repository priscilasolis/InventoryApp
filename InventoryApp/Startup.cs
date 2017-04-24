using Hangfire;
using Microsoft.Owin;
using Owin;
using System;
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
            RecurringJob.AddOrUpdate(() => newEmail(), hora, TimeZoneInfo.Local);

            app.UseHangfireDashboard();

            app.UseHangfireServer();

        }
        //Este metodo manda un correo
        public void newEmail()
        {
            MailMessage mail = new MailMessage();
            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Host = "smtp.gmail.com";
            client.Credentials = new System.Net.NetworkCredential("erickelias881@gmail.com", "Erick2127");
            mail.To.Add(new MailAddress("alcantar1272@gmail.com"));
            mail.From = new MailAddress("erickelias881@gmail.com");
            mail.Subject = "This is an email.";
            mail.Body = "<h1>This is the body of one email</h1><p>This is a paragraph.</p>";
            mail.IsBodyHtml = true;
            client.Send(mail);
        }
    }
}