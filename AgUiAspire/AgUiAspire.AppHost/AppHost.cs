var builder = DistributedApplication.CreateBuilder(args);

var server = builder.AddProject<Projects.Server>("server");

var blazor = builder.AddProject<Projects.Blazor_Client>("blazor");

builder.Build().Run();
