using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Functions;

internal class Function1(ILogger<Function1> logger, IConfiguration configuration)
{
    private IConfiguration _config => configuration;
    private ILogger<Function1> _logger => logger;

    [Function("GetFunction1")]
    public async Task<IActionResult> GetFunction1(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        _logger.LogInformation("Get function called.");

        string? input = req.Query.ContainsKey("name") ? req.Query["name"] : "world";
        input ??= "world";

        string? key = _config["FUNCTION_KEY"];
        _ = key ?? throw new Exception("key is missing");
        _logger.LogInformation($"The very secret key is {key}.");

        return new OkObjectResult($"hello {input}, the very secret key has been logged.");
    }
}
