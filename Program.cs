using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Todo_Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Creates or updates the database automatically when you run locally
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.UseDefaultFiles();   // serves wwwroot/index.html at "/"
app.UseStaticFiles();
app.MapControllers();
app.Run();

public partial class Program { }