using AppServices;
using WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddSqliteDbContext<ApplicationDataContext>("database");
builder.Services.AddOpenApi();
builder.Services.AddScoped<IDummyLogic, DummyLogic>();

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "v1");
});

app.UseHttpsRedirection();

app.MapDemoEndpoints();

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }
