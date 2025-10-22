using AppServices;

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

app.MapGet("/ping", () => "pong")
    .WithDescription("A simple ping endpoint to check if the service is running.");
app.MapGet("/dummies", (ApplicationDataContext db) => db.Dummies)
    .WithDescription("Gets all dummy records from the database.");
app.MapPost("/dummy-logic", async (ApplicationDataContext db, Dummy dummyToChange, IDummyLogic logic) =>
{
    logic.IncrementDecimal(dummyToChange, 1.5m);
    return Results.Ok(dummyToChange);
})
    .WithDescription("Increments the DecimalProperty of the provided Dummy object by 1.5 using the DummyLogic service.");

app.Run();
