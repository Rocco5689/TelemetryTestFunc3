using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System.Linq;
using Newtonsoft;
using Newtonsoft.Json;
using System.IO;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using System.Reflection;
using System.Collections.Generic;

namespace TelemetryTestFunc
{
    public class Function1
    {
        private readonly TelemetryClient telemetryClient;
        private readonly ITelemetryChannel channel;

        public Function1(TelemetryClient _telemetryClient, ITelemetryChannel _channel)
        {
            //_telemetryClient.InstrumentationKey = "0c460c22-1b0a-4185-acdd-12a0f8a4adb6";
            telemetryClient = _telemetryClient;
            channel = _channel;
        }

        [FunctionName("Function1")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request." + " ");
            
            try
            {
                throw new Exception("Exception");
            }
            catch (Exception exc)
            {
                var exceptionTelemetry = new ExceptionTelemetry(exc);
                var itemProperty = telemetryClient.Context.GetType().GetMembers(BindingFlags.NonPublic).ToList();

                log.LogInformation(channel.GetType().ToString());

                foreach (var item in itemProperty)
                {
                    log.LogInformation(item.Name);
                }

                telemetryClient.TrackException(exceptionTelemetry);
            }
            

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
