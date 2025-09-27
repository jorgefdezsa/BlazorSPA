var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureFunctionsProject<Projects.Validator_FX>("validator-fx");

builder.AddProject<Projects.Middleware_API>("middleware-api");

builder.Build().Run();
