using AppServices;
using WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddSqliteDbContext<ApplicationDataContext>("database");
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseCors();
app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "v1"));
app.UseHttpsRedirection();

app.MapWishlistEndpoints();

app.Run();
