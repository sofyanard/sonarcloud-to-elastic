using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace sonarcloud_to_elastic
{
    static class SonarCloudProcess
    {
        static readonly HttpClient client = new HttpClient();

        static ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        static ILogger logger = factory.CreateLogger("Program");

        static IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

        static string sonarCloudToken = Environment.GetEnvironmentVariable("SONARCLOUD_TOKEN");
        static string sonarUrl = config["SonarCloud:Url"];
        static string sonarComponentKeys = config["SonarCloud:ComponentKeys"];

        public static async Task<string> GetIssues(string softwareQualities, string severities, int pageSize=500, int page=1)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sonarCloudToken);
            Uri requestUri = new Uri(sonarUrl + "/issues/search?" + "componentKeys=" + sonarComponentKeys + "&impactSoftwareQualities=" + softwareQualities + "&impactSeverities=" + severities + "&ps=" + pageSize + "&p=" + page + "&_source=false");
            try
            {
                logger.LogInformation("Requesting to SonarCloud API: {0}...", requestUri);
                using HttpResponseMessage response = await client.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();
                logger.LogInformation("Request is succeed, getting the response...");
                string responseBody = await response.Content.ReadAsStringAsync();
                string pattern = $@"^(.{{0,{100}}}).*";
                string truncated = Regex.Replace(responseBody, pattern, "$1");
                logger.LogInformation("Response: {0}...", truncated);

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
