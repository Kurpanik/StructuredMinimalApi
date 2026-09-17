var builder = DistributedApplication.CreateBuilder(args);

var jwtKey = builder.AddParameter(
    "jwt-key",
    new GenerateParameterDefault { MinLength = 64, Special = false },
    secret: true,
    persist: true
);

var sqlServer = builder.AddSqlServer("sqlserver")
    .WithDataVolume();

var database = sqlServer.AddDatabase("chirper");

builder.AddProject<Projects.Chirper_Api>("api")
    .WithReference(database)
    .WaitFor(database)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WithUrl("/scalar", "API Docs");

builder.Build().Run();
