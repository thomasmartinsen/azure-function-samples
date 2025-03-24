var builder = DistributedApplication.CreateBuilder(args);

var sample01 = builder.AddProject<Projects.sample_01>("sample01");

builder.Build().Run();
