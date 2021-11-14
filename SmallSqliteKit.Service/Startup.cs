using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using SmallSqliteKit.Service.Data;
using SmallSqliteKit.Service.Jobs;
using SmallSqliteKit.Service.Services;

namespace SmallSqliteKit.Service
{
    public class Startup
    {
        private IWebHostEnvironment _hostingEnvironment;

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            _hostingEnvironment = env;
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(Configuration);

            services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Trace);
            });

            services.Configure<DropboxOptions>(Configuration.GetSection("Dropbox"));

            services.AddDbContext<SqliteDataContext>();
            services.AddScoped<IConfigRepository, ConfigRepository>();
            services.AddScoped<IDatabaseBackupRepository, DatabaseBackupRepository>();
            services.AddScoped<IBackupAuditRepository, BackupAuditRepository>();
            services.AddScoped<IBackupFilePurger, BackupFilePurger>();
            services.AddScoped<IDropboxUploadClient, DropboxUploadClient>();

            services.AddMvc().AddSessionStateTempDataProvider();
            services.AddRazorPages();
            services.AddCors();
            
            services.AddHostedService<DatabaseBackupJob>();
            services.AddHostedService<DropboxUploadJob>();
            services.AddHostedService<PurgeJob>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseSerilogRequestLogging();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(options => options.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"));

            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            scope.ServiceProvider.GetRequiredService<SqliteDataContext>().Database.Migrate();
        }
    }
}
