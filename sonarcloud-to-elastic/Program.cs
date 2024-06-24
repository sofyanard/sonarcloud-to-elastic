namespace sonarcloud_to_elastic
{
    using Microsoft.Extensions.Logging;
    using System.Text;
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Newtonsoft.Json;
    using System.Text.Json.Nodes;

    internal class Program
    {
        static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            /*
            // create a logger factory
            var loggerFactory = LoggerFactory.Create(
                    builder => builder
                            // add console as logging target
                            .AddConsole()
                            // add debug output as logging target
                            .AddDebug()
                            // set minimum level to log
                            .SetMinimumLevel(LogLevel.Debug)
            );

            // create a logger
            var logger = loggerFactory.CreateLogger<Program>();

            // logging
            logger.LogTrace("Trace message");
            logger.LogDebug("Debug message");
            logger.LogInformation("Info message");
            logger.LogWarning("Warning message");
            logger.LogError("Error message");
            logger.LogCritical("Critical message");
            */

            // https://learn.microsoft.com/en-us/dotnet/core/extensions/logging

            using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger logger = factory.CreateLogger("Program");
            logger.LogInformation("Hello World! Logging is {Description}.", "fun");

            // logging
            logger.LogTrace("Trace message");
            logger.LogDebug("Debug message");
            logger.LogInformation("Info message");
            logger.LogWarning("Warning message");
            logger.LogError("Error message");
            logger.LogCritical("Critical message");



            // https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient

            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                /*
                using HttpResponseMessage response = await client.GetAsync("http://www.contoso.com/");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);
                */

                /* Basic Authentication
                 * https://stackoverflow.com/questions/73493567/how-to-add-basic-authentication-to-http-request
                 * https://code-maze.com/aspnetcore-basic-authentication-with-httpclient/
                 *
                using var client = new HttpClient();
                var byteArray = Encoding.ASCII.GetBytes("username:password");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                var response = await client.PostAsync(url, data);
                var result = await response.Content.ReadAsStringAsync();
                 *
                 */

                /* Bearer Token 
                 * https://code-maze.com/add-bearertoken-httpclient-request/
                 */
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "b67886db23bc8a9d2f985ec98d0f45d1201936e3");
                using HttpResponseMessage response = await client.GetAsync("https://sonarcloud.io/api/issues/search?componentKeys=rh-sakti_sakti-fuse-main&impactSoftwareQualities=SECURITY&impactSeverities=HIGH");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                logger.LogInformation("responseBody:");
                logger.LogInformation(responseBody);

                // Parsing...

                

                // Post to Elastic
                // var jsonPost = JsonConvert.SerializeObject(responseBody);
                var dataPost = new StringContent(responseBody, Encoding.UTF8, "application/json");
                var byteArray = Encoding.ASCII.GetBytes("elastic:JynEP9RYl792*khVwnTi");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                logger.LogInformation("posting start...");
                using HttpResponseMessage response2 = await client.PostAsync("http://localhost:9200/books/_doc/", dataPost);
                response2.EnsureSuccessStatusCode();
                logger.LogInformation("posting success...");
                string responseBody2 = await response2.Content.ReadAsStringAsync();

                logger.LogInformation("responseBody2:");
                logger.LogInformation(responseBody2);
            }
            catch (HttpRequestException e)
            {
                logger.LogError("Exception Caught!");
                logger.LogError("Message :{0} ", e.Message);
            }

            Console.WriteLine("Hello, World!");
        }
    }
}
