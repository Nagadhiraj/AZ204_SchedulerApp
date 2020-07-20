using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using SendSMSReminder.Model;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace SendSMSReminder
{
    public static class AppointmentReminder
    {
        [FunctionName("AppointmentReminder")]
        public static async System.Threading.Tasks.Task RunAsync([TimerTrigger("0 0 9 * * *")]TimerInfo myTimer, 
            [CosmosDB(ConnectionStringSetting = "COSMOSDB_CONNECTION")] DocumentClient dbclient, 
            ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            Uri driverCollectionUri = UriFactory.CreateDocumentCollectionUri(databaseId: GetEnvironmentVariable("COSMOSDB_DATABASE_NAME"), collectionId: "AppointmentMaster");
            string accountSID = GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
            string authToken = GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
            // Initialize the TwilioClient.
            TwilioClient.Init(accountSID, authToken);
            var apiKey = GetEnvironmentVariable("SEND_GRID_API"); //insert your Sendgrid API Key
            var sgclient = new SendGridClient(apiKey);
            var from = new EmailAddress(GetEnvironmentVariable("EMAIL_ADD"), "Holistic Fitness");
            var subject = "Appoinment Reminder for Holistic Fitness";

            var date = DateTime.Now.ToString("M/dd/yyyy", CultureInfo.InvariantCulture);
            
            var options = new FeedOptions { EnableCrossPartitionQuery = true }; // Enable cross partition query
            IDocumentQuery<AppointmentEntity> query = dbclient.CreateDocumentQuery<AppointmentEntity>(driverCollectionUri, options)
                                                 .Where(x => x.date == date && x.isActive == true && x.isAvailable == false)
                                                 .AsDocumentQuery();

            var appointments = new List<AppointmentEntity>();

            while (query.HasMoreResults)
            {
                foreach (AppointmentEntity app in await query.ExecuteNextAsync())
                {
                    //appointments.Add(app);
                    try
                    {
                        // Send an SMS message.
                        var bodymessage = "Reminder! You have an appointment today with "+ app.modifiedBy + " at "+ app.from + ". Thank you, Holistic Fitness";
                        var message = MessageResource.Create(
                            to: new PhoneNumber("+91"+app.phone),
                            from: new PhoneNumber(GetEnvironmentVariable("TWILIO_PHONE_NUMBER")),
                            body: bodymessage);

                        // Send an email
                        
                        var to = new EmailAddress(app.email, "Client");
                        var plainTextContent = "You have an appointment today with " + app.modifiedBy + " at " + app.from + ". Thank you, Holistic Fitness";
                        var htmlContent = "<strong>HOLISTIC FITNESS by Mrinmoyee Sinha</strong>";
                        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                        var response = await sgclient.SendEmailAsync(msg);
                    }
                    catch (TwilioException ex)
                    {
                        // An exception occurred making the REST call
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            

        }
        public static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }

    public static class BirthdayWish
    {
        [FunctionName("BirthdayWish")]
        public static async System.Threading.Tasks.Task RunAsync([TimerTrigger("0 0 8 * * *")] TimerInfo myTimer,
            [CosmosDB(ConnectionStringSetting = "COSMOSDB_CONNECTION")] DocumentClient dbclient,
            ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            Uri driverCollectionUri = UriFactory.CreateDocumentCollectionUri(databaseId: GetEnvironmentVariable("COSMOSDB_DATABASE_NAME"), collectionId: "User");
            string accountSID = GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
            string authToken = GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
            // Initialize the TwilioClient.
            TwilioClient.Init(accountSID, authToken);
            var date = DateTime.Now.ToString("M/dd", CultureInfo.InvariantCulture);

            var options = new FeedOptions { EnableCrossPartitionQuery = true }; // Enable cross partition query
            IDocumentQuery<UserEntity> query = dbclient.CreateDocumentQuery<UserEntity>(driverCollectionUri, options)
                                                 .Where(x => x.dateOfBirth.Contains(date) && x.isActive == true)
                                                 .AsDocumentQuery();

            var users = new List<UserEntity>();

            while (query.HasMoreResults)
            {
                foreach (UserEntity user in await query.ExecuteNextAsync())
                {
                    users.Add(user);
                    try
                    {
                        var bodymessage = "We wish you a very HAPPY BIRTHDAY "+ user.name + ". We hope you stay fit and healthy as always. Regards, Holistic Fitness";
                        // Send an SMS message.
                        var message = MessageResource.Create(
                            to: new PhoneNumber("+91" + user.phone),
                            from: new PhoneNumber(GetEnvironmentVariable("TWILIO_PHONE_NUMBER")),
                            body: bodymessage);
                    }
                    catch (TwilioException ex)
                    {
                        // An exception occurred making the REST call
                        Console.WriteLine(ex.Message);
                    }
                }
            }



        }
        public static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }

}
