using Microsoft.EntityFrameworkCore;
using DemoApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Leer variables de entorno directamente
builder.Configuration.AddEnvironmentVariables();

var appName = builder.Configuration["APP_NAME"];
var appEnv  = builder.Configuration["APP_ENV"];

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();