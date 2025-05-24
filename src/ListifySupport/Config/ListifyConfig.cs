namespace Listify.Model;

using Microsoft.Extensions.Configuration;
using Plisky.Diagnostics;

public class ListifyConfig {
    public static Bilge b = new("Listify-Model-Config");

    public ListifyAppConfig? AppSection { get; set; }
    public ListifyBuildConfig? BuildSection { get; set; }
    public string? ArtefactsDirecory { get; set; }
    public string? ExecutingMachineName { get; set; }
    public IConfigurationRoot? ActiveConfig { get; set; }


    internal ListifyConfig() {
    }

    public static ListifyConfig Create(string configDirectory, string environment) {
        b.Info.Flow();
        var result = new ListifyConfig();
        result.GetInitialConfiguration(configDirectory, environment);

        return result;
    }

    public void GetInitialConfiguration(string configDirectory, string environmentId) {
        b.Info.Flow();

        string machineName = Environment.MachineName;
        var configs = new List<string>();
        const string APPNAME = "listify";
        const string APPSETTINGS = $"{APPNAME}-settings";
        const string SECTIONNAMEAPP = $"{APPNAME}AppConfig";
        const string SECTIONNAMEBUILD = $"{APPNAME}BuildConfig";
        // Environment configuration takes the default, followed by any environment based overrides followed by any machine specific overrides.
        configs.Add(Path.Combine(configDirectory, $"{APPSETTINGS}.json"));
        configs.Add(Path.Combine(configDirectory, $"{APPSETTINGS}-{environmentId}.json"));
        configs.Add(Path.Combine(configDirectory, $"{APPSETTINGS}-{environmentId}.donotcommit"));
        configs.Add(Environment.ExpandEnvironmentVariables($"%PLISKYAPPROOT%\\config\\{APPSETTINGS}.donotcommit"));
        configs.Add(Environment.ExpandEnvironmentVariables($"%PLISKYAPPROOT%\\config\\{APPSETTINGS}-{environmentId}.donotcommit"));
        configs.Add(Environment.ExpandEnvironmentVariables($"%PLISKYAPPROOT%\\config\\{APPNAME}-{machineName}-override.json"));

        var cfg = new ConfigurationBuilder();
        configs.ForEach(c => {

            string cfgStatus = File.Exists(c) ? "Exists" : "Missing";
            if (File.Exists(c)) {
                cfg.AddJsonFile(c, optional: true, reloadOnChange: true);
            }
            Console.WriteLine($"Add Config [{c}] {cfgStatus}");
            b.Verbose.Log($"Add Config [{c}] {cfgStatus}");
        });
        cfg.AddEnvironmentVariables();
        ActiveConfig = cfg.Build();

        if (ActiveConfig == null) {
            throw new InvalidOperationException("Listify>>Configuration>> Active configuraiton could not be loaded from all of the config applied.");
        }

        BuildSection = ActiveConfig.GetRequiredSection(SECTIONNAMEBUILD).Get<ListifyBuildConfig>() ?? throw new InvalidOperationException("Build Configuration Missing");
        AppSection = ActiveConfig.GetRequiredSection(SECTIONNAMEAPP).Get<ListifyAppConfig>() ?? throw new InvalidOperationException("Application Configuration Missing.");

        ArtefactsDirecory = Environment.ExpandEnvironmentVariables(BuildSection.BuildScratchDirectory);
        ExecutingMachineName = machineName;
        b.Verbose.Log($"Artefacts Dir {ArtefactsDirecory}");
    }
}