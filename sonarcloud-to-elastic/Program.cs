namespace sonarcloud_to_elastic
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;
    using System.Text;
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    

    internal class Program
    {
        static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            // Build a config object, using env vars and JSON providers.
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            // Access configuration settings
            var sonarComponentKeys = config["SonarCloud:ComponentKeys"];
            Console.WriteLine($"sonarComponentKeys: {sonarComponentKeys}");

            // From Environment Variables
            string sonarCloudToken = Environment.GetEnvironmentVariable("SONARCLOUD_TOKEN");
            Console.WriteLine($"sonarCloudToken: {sonarCloudToken}");

            

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
                /*
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "b67886db23bc8a9d2f985ec98d0f45d1201936e3");
                using HttpResponseMessage response = await client.GetAsync("https://sonarcloud.io/api/issues/search?componentKeys=rh-sakti_sakti-fuse-main&impactSoftwareQualities=SECURITY&impactSeverities=HIGH");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                logger.LogInformation("responseBody:");
                logger.LogInformation(responseBody);
                */

                foreach (SoftwareQuality softwareQuality in Enum.GetValues(typeof(SoftwareQuality)))
                {
                    foreach (Severity severity in Enum.GetValues(typeof(Severity)))
                    {
                        string strQ = softwareQuality.ToString();
                        string strS = severity.ToString();

                        int iPage = 1;
                        logger.LogInformation($"Software Quality: {softwareQuality}, Severity: {severity}, Page: {iPage}");
                        string sonarResponse = await SonarCloudProcess.GetIssues(strQ, strS, 500, iPage);

                        // Parsing Sonar Response...
                        JObject jsonObject = JObject.Parse(sonarResponse);
                        JArray arrayOfIssue = (JArray)jsonObject["issues"];
                        int totalIssues = (int)jsonObject["total"];
                        logger.LogInformation($"Total Issues: {totalIssues}");

                        // Loop for each issue
                        if (arrayOfIssue.Count > 0)
                        {
                            foreach (var issue in arrayOfIssue)
                            {
                                string elasticResponse = await ElasticProcess.PostIssue(issue.ToString());
                            }
                        }

                        int remainingIssues = totalIssues;
                        while (remainingIssues > 500)
                        {
                            remainingIssues -= 500;
                            iPage++;
                            logger.LogInformation($"Software Quality: {softwareQuality}, Severity: {severity}, Page: {iPage}");
                            sonarResponse = await SonarCloudProcess.GetIssues(strQ, strS, 500, iPage);
                        }
                    }
                }

                // if (true)
                // {
                //     throw new Exception("Break!!!");
                // }

                /*

                foreach (var issue in arrayOfIssue)
                {
                    // Post Issue to Elastic
                    var jsonPost = JsonConvert.SerializeObject(issue);
                    var dataPost = new StringContent(jsonPost, Encoding.UTF8, "application/json");
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

                */
            }
            catch (HttpRequestException e)
            {
                logger.LogError("Exception Caught!");
                logger.LogError($"Message :{e.Message} ", e.Message);
            }

            Console.WriteLine("Hello, World!");
        }

        enum SoftwareQuality
        {
            SECURITY/*,
            RELIABILITY,
            MAINTAINABILITY*/
        }

        enum Severity
        {
            HIGH/*,
            MEDIUM,
            LOW*/
        }
    }
}
