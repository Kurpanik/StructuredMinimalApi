global using Chirper.Common.Api.Extensions;
global using Chirper.Common.Api.Requests;
global using Chirper.Common.Api.Results;
global using Chirper.Data;
global using Chirper.Data.Types;
global using FluentValidation;
global using Microsoft.AspNetCore.Http.HttpResults;
global using Microsoft.EntityFrameworkCore;
global using System.Security.Claims;
using Chirper;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddDatabase();
builder.AddJwtAuthentication();
builder.Services.AddRequestValidation();
builder.Services.AddRequestLogging();
builder.Services.AddApiDocumentation();
builder.Services.AddErrorHandling();

var app = builder.Build();

app.UseHttpLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseRequestTimeouts();
app.UseOutputCache();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options
        .WithTitle("Chirper API")
        .AddPreferredSecuritySchemes("Bearer")
    );

    await using var scope = app.Services.CreateAsyncScope();
    var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await database.Database.MigrateAsync();
}

app.MapDefaultEndpoints();
app.MapEndpoints();

await app.RunAsync();
