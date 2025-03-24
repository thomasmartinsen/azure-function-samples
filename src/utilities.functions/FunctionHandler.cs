using Microsoft.Extensions.Configuration;

namespace Functions;

public class FunctionHandler : IFunctionHandler
{
    private readonly IConfigurationRoot _config;
}
