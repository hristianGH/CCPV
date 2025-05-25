using CCPV.Main.API.Data;
using CCPV.Main.API.Misc;
using Microsoft.EntityFrameworkCore;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlServer(Environment.GetEnvironmentVariable(Constants.SQLConnection) ??
    builder.Configuration.GetConnectionString(Constants.DefaultConnection)));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .MinimumLevel.Debug()
    .CreateLogger();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    string requestId = context.TraceIdentifier;
    string ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

    using (Serilog.Context.LogContext.PushProperty("RequestId", requestId))
    using (Serilog.Context.LogContext.PushProperty("ClientIP", ip))
    {
        await next.Invoke();
    }

});
app.UseAuthorization();

app.MapControllers();

app.Run();
