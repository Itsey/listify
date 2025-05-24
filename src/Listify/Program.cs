using Listify.Model;
using Listify.Models;
using Microsoft.EntityFrameworkCore;
using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;

namespace Listify;

public class Program {


    protected static Bilge? b;

    public static void Main(string[] args) {
        string envt = GetEnvironment();

        string loggingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

#if DEBUG
        if (envt == "1101") {
            loggingDirectory = "X:\\Temp\\logs\\";
        }
#endif

        Console.WriteLine($"LD : {loggingDirectory}");
        Bilge.AddHandler(new RollingFileSystemHandler(new RollingFSHandlerOptions() {
            Directory = loggingDirectory,
            FileName = "lfy-%dd%mm%yy-%pid.txt",
            FilenameIsMask = true,
            MaxRollingFileSize = "60mb"
        }));

#if DEBUG
        Bilge.AddHandler(new TCPHandler(new TCPHandlerOptions("127.0.0.1", 9060, true)));
#endif
        Bilge.SetConfigurationResolver((nme, curntLvl) => System.Diagnostics.SourceLevels.Verbose);
        b = new Bilge(tl: System.Diagnostics.SourceLevels.Verbose);
        Bilge.Alert.Online("Listify");
        b.Info.Log("About To Get App Configuration");

        AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
            if (b != null) {
                b.Error.Log("Unhandled Exception in Main AppDomain -- Listify Crash");
                b.Error.Dump(e, "Unhandled Exception Handler");
                Bilge.ForceFlush();
            } else {
                Console.WriteLine("Unhandled Exception in Main AppDomain -- Listify Crash - Bilge not initialised.");
            }
        };

        var builder = WebApplication.CreateBuilder(args);


        string cfgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");
#if DEBUG
        if (envt == "1101") {
            cfgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\..\\_Dependencies\\", "configuration");
        }
#endif

        Console.WriteLine($"LD : {envt} {cfgPath}");
        b.Info.Log($"Config Coming Online. {envt}", cfgPath);
        var cfg = ListifyConfig.Create(cfgPath, envt);


        // Add services to the container.
        builder.Services.AddControllersWithViews();

        if (cfg == null || cfg.AppSection == null) {
            throw new InvalidOperationException($"Listify - Criticial configuration data is missing.  Environment {envt}. ConfigPath {cfgPath}");
        }

        if (string.IsNullOrWhiteSpace(cfg.AppSection.DbConstr)) {
            throw new InvalidOperationException($"Listify - Database Connectifity Configuration is missing.  Environment {envt}. ConfigPath {cfgPath}");
        }

        builder.Services.AddDbContext<ListifyContext>(options => options.UseSqlServer(cfg.AppSection.DbConstr));


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment()) {
            app.UseExceptionHandler("/Home/Error");
        }
        app.UseRouting();

        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.Run();
        Bilge.ForceFlush();
    }

    private static string GetEnvironment() {
        string envt = "1101";
        string fname = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "envt.txt");
        if (File.Exists(fname)) {
            envt = File.ReadAllText(fname);
        }
        return envt;
    }
}