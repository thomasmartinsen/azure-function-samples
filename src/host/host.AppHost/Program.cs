var builder = DistributedApplication.CreateBuilder(args);

var functionSample01 = builder.AddProject<Projects.azure_function_sample_01>("functionsample01");

builder.Build().Run();
