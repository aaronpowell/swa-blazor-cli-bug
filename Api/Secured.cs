using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Linq;

namespace BlazorApp.Api
{
    public static class Secured
    {
        [FunctionName("Secured")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "secured/HelloYou")] HttpRequest req,
            ILogger log,
            ClaimsPrincipal principal)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            bool isClaimValid = true;

            if (principal == null && !principal.Identity.IsAuthenticated)
            {
                log.LogWarning("Request was not authenticated.");
                isClaimValid = false;
            }

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name ??= data?.name;

            string responseMessage = (isClaimValid, string.IsNullOrEmpty(name)) switch
            {
                (true, true) => "Request secured but didn't get your name",
                (true, false) => $"Hey {name}, you made a secured request (in principal). IsInRole says: {principal.IsInRole("authenticated")}. {principal.Claims.Select(claim => $"Claim: {claim.Subject} - {claim.Value}\r\n")}",
                (false, false) => $"This isn't very secure, is it {name}",
                _ => "Don't know who you are, but you're not secure"
            };

            return new OkObjectResult(responseMessage);
        }
    }
}
