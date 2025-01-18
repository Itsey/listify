namespace Listify.Model;
using Microsoft.Extensions.Configuration;
using Plisky.Diagnostics;

public class ListifyConfig {
    public Bilge b = new("Listify-Model-Config");

    public ListifyAppConfig AppSection { get; set; }
    public ListifyBuildConfig BuildSection { get; set; }
    public string ArtefactsDirecory { get; set; }
    public string ExecutingMachineName { get; set; }
    internal ListifyConfig() {

        /*
               settings.VersioningPersistanceToken = settings.Config["versioning-token"];
        */
    }

    public IConfigurationRoot ActiveConfig { get; set; }

    public static ListifyConfig Create(string dependenciesDirectory, string environment) {

        var result = new ListifyConfig();
        result.GetInitialConfiguration(dependenciesDirectory, environment);

        return result;
    }

    public void GetInitialConfiguration(string dependenciesDirectory, string environmentId) {
        string machineName = Environment.MachineName;
        var configs = new List<string>();
        const string APPNAME = "listify";
        const string APPSETTINGS = $"{APPNAME}-settings";
        const string SECTIONNAMEAPP = $"{APPNAME}AppConfig";
        const string SECTIONNAMEBUILD = $"{APPNAME}BuildConfig";
        // Environment configuration takes the default, followed by any environment based overrides followed by any machine specific overrides.
        configs.Add(Path.Combine(dependenciesDirectory, $"{APPSETTINGS}.json"));
        configs.Add(Path.Combine(dependenciesDirectory, $"{APPSETTINGS}-{environmentId}.json"));
        configs.Add(Environment.ExpandEnvironmentVariables($"%PLISKYAPPROOT%\\config\\{APPSETTINGS}.donotcommit"));
        configs.Add(Environment.ExpandEnvironmentVariables($"%PLISKYAPPROOT%\\config\\{APPSETTINGS}-{environmentId}.donotcommit"));
        configs.Add(Environment.ExpandEnvironmentVariables($"%PLISKYAPPROOT%\\config\\{APPNAME}-{machineName}-override.json"));

        var cfg = new ConfigurationBuilder();
        configs.ForEach(c => {
            string cfgStatus = "File Not Found.";
            if (File.Exists(c)) {
                cfgStatus = "Configuration Loaded.";
                cfg.AddJsonFile(c, optional: true, reloadOnChange: true);
            }
            b.Verbose.Log($"Add Config [{c}] {cfgStatus}");
        });
        cfg.AddEnvironmentVariables();
        ActiveConfig = cfg.Build();

        if (ActiveConfig == null) {
            throw new InvalidOperationException("Configuration not loaded.");
        }

        BuildSection = ActiveConfig.GetRequiredSection(SECTIONNAMEBUILD).Get<ListifyBuildConfig>() ?? throw new InvalidOperationException("Build Configuration Missing");
        AppSection = ActiveConfig.GetRequiredSection(SECTIONNAMEAPP).Get<ListifyAppConfig>() ?? throw new InvalidOperationException("Application Configuration Missing.");

        ArtefactsDirecory = Environment.ExpandEnvironmentVariables(BuildSection.BuildScratchDirectory);
        ExecutingMachineName = machineName;
        b.Verbose.Log($"Artefacts Dir {ArtefactsDirecory}");
    }


}

