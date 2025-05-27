using CCPV.Main.API.Clients;
using CCPV.Main.API.Data;
using CCPV.Main.API.Handler;
using CCPV.Main.API.Metrics;
using CCPV.Main.API.Middleware;
using CCPV.Main.API.Misc;
using CCPV.Main.Background;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Refit;

public class Startup
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services.AddDbContext<ApiDbContext>(options =>
            options.UseSqlServer(Environment.GetEnvironmentVariable(Constants.RemoteSqlConnection) ??
            _configuration.GetConnectionString(Constants.DefaultConnection)));

        services.AddHangfire(config =>
    config.UseSqlServerStorage(_configuration.GetConnectionString(Constants.DefaultConnection)));
        services.AddHangfireServer();

        services.AddRefitClient<ICoinloreApi>()
        .ConfigureHttpClient(c =>
        {
            c.BaseAddress = new Uri(Constants.CoinloreUri);
        });

        services.AddMemoryCache();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Handlers
        services.AddScoped<IPortfolioHandler, PortfolioHandler>();
        services.AddScoped<IUserHandler, UserHandler>();
        services.AddScoped<IUploadHandler, UploadHandler>();

        // Metrics and Prometheus
        services.AddScoped<APIMetricsCollector>();

        //Background jobs 
        services.AddScoped<IBackgroundJob, MetricsLoggingBackgroundJob>();

    }

    public void Configure(IApplicationBuilder app)
    {
        if (_environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseHttpsRedirection();
        app.UseHttpMetrics();
        app.UseRouting();

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

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapMetrics();
        });
        app.UseHangfireDashboard();

        BackgroundJobFactory.RegisterRecurringJobs(app.ApplicationServices);

    }
}
