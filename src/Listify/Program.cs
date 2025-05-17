using Listify.Model;
using Listify.Models;
using Microsoft.EntityFrameworkCore;

namespace Listify;

public class Program {

    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        string cfgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");
#if DEBUG
        cfgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\..\\_Dependencies\\", "configuration");
#endif

        var cfg = ListifyConfig.Create(cfgPath, "1101");


        // Add services to the container.
        builder.Services.AddControllersWithViews();

        if (cfg == null || cfg.AppSection == null || string.IsNullOrWhiteSpace(cfg.AppSection.DbConstr)) {
            throw new InvalidOperationException("Application is missing key configuration data for this environment");
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
    }
}