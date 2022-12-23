using Microsoft.EntityFrameworkCore;
using Serilog.Events;
using Serilog;
using WorkerService1;
using WorkerService1.Database;


     IHost host = Host.CreateDefaultBuilder(args).UseWindowsService()
      .ConfigureAppConfiguration((builderContext, config) =>
      {
          config.SetBasePath(System.AppDomain.CurrentDomain.BaseDirectory);
      })
        .ConfigureServices((ctx, services) =>
        {
            services.AddDbContext<UserRg_DbContext>(Options =>
            {
                Options.UseSqlServer(
                ctx.Configuration.GetConnectionString("DbConn")
                );
            });


            services.AddHostedService<Worker>();
            services.AddLogging(configure => configure.AddSerilog());
            services.AddScoped<IRepository, Repository>();



        }).UseSerilog((ctx, loggerConfiguration) =>
                loggerConfiguration.ReadFrom.Configuration(ctx.Configuration))

        .Build();
var logger = host.Services.GetService<ILogger<Program>>();

logger!.LogInformation($"Before host.RunAsync");

string strPath = "\\Log\\LogFile.txt";
var fileName  = Path.GetFileName(strPath);
using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, true))
    file.WriteLine($"We're Running WorkerServiceAndSerilog {DateTime.Now}");






await host.RunAsync();

