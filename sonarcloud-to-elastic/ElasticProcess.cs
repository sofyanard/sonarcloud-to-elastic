using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace sonarcloud_to_elastic
{
    static class ElasticProcess
    {
        static readonly HttpClient client = new HttpClient();

        static ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        static ILogger logger = factory.CreateLogger("ElasticProcess");

        static IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

        static string elasticPassword = Environment.GetEnvironmentVariable("ELASTIC_TOKEN");
        static string elasticUrl = config["Elastic:Url"];
        static string elasticUser = config["Elastic:User"];
        static string elasticIndex = config["Elastic:Index"];

        public static async Task<string> PostIssue(string issue)
        {
            string pattern = $@"^(.{{0,{100}}}).*";
            string msgIssue = Regex.Replace(issue, pattern, "$1");
            logger.LogInformation($"Data to post: {msgIssue}...");

            var jsonPost = JsonConvert.SerializeObject(issue);
            var dataPost = new StringContent(jsonPost, Encoding.UTF8, "application/json");
            var byteArray = Encoding.ASCII.GetBytes($"{elasticUser}:{elasticPassword}");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            Uri requestUri = new Uri($"{elasticUrl}/{elasticIndex}/_doc/");
            try
            {
                logger.LogInformation($"Posting to Elastic: {requestUri}...");
                using HttpResponseMessage response = await client.PostAsync(requestUri, dataPost);
                response.EnsureSuccessStatusCode();
                logger.LogInformation("Post is succeed, getting the response...");
                string responseBody = await response.Content.ReadAsStringAsync();
                string truncated = Regex.Replace(responseBody, pattern, "$1");
                logger.LogInformation($"Response: {truncated}...");

                return responseBody;
            }
            catch (Exception e)
            {
                logger.LogError("Request Error: {0}", e.Message);
                throw;
            }
            
        }
    }
}
