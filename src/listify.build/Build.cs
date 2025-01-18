using System;
using System.IO;
using System.Linq;
using Flurl.Http;
using Listify.Model;
using Nuke.Common;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Utilities.Collections;
using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using Serilog;

[AzurePipelines(AzurePipelinesImage.UbuntuLatest, AutoGenerate = false, InvokedTargets = new[] { nameof(ArrangeStep), nameof(ConstructStep), nameof(ExamineStep), nameof(PackageStep), nameof(ReleaseStep) })]
public partial class Build : NukeBuild {
    public Bilge b = new("Nuke", tl: System.Diagnostics.SourceLevels.Verbose);

    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    private readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitRepository]
    private readonly GitRepository GitRepository;

    [Solution]
    private readonly Solution Solution;

    [Parameter("NoSuccessNotify")]
    readonly bool NoSuccessNotify = true;

    [Parameter("SimplifyLogging")]
    readonly bool SingleThreadedTrace = false;

    [Parameter("OverrideSkipWebContent")]
    readonly bool? OverrideForceWebContentDeployment = null;

    [Parameter("EnvironmentId")]
    readonly string EnvironmentId = "1101";

    private AbsolutePath SourceDirectory => RootDirectory / "src";
    private AbsolutePath ArtifactsDirectory;

    private LocalBuildConfig settings;

    public Target Wrapup => _ => _
        .DependsOn(Initialise)
        .After(Initialise)
        .Executes(() => {
            b.Info.Log("Build >> Wrapup >> All Done.");
            Log.Information("Build>Wrapup>  Finish - Build Process Completed.");
            b.Flush().Wait();
            System.Threading.Thread.Sleep(10);
        });

    protected override void OnBuildFinished() {
        string pth = settings.Config.BuildSection.DiscordHookUrl;
        if (!string.IsNullOrWhiteSpace(pth)) {


            string lb = Build.IsLocalBuild ? $"Local [{settings.Config.ExecutingMachineName}]" : $"Server [{settings.Config.ExecutingMachineName}]";

            string wrked = string.Empty;
            if (IsSucceeding) {
                wrked = "Succeeded";
                if (NoSuccessNotify) {
                    return;
                }
            } else {
                wrked = "Failed (";
                FailedTargets.ForEach(x => {
                    wrked += x.Name + ", ";
                });
                wrked += ")";
            }
            var ressy = pth.PostJsonAsync(new {
                content = $"{lb} Listify Build {wrked} for {EnvironmentId} @ {DateTime.Now.Hour}:{DateTime.Now.Minute}"
            });
            ressy.Wait();
        } else {
            Log.Information("Build>Wrapup>  Discord Hook URL is not set, skipping notification.");

        }
    }

    public Target Initialise => _ => _
           .Before(ExamineStep, Wrapup)
           .Triggers(Wrapup)
           .Executes(() => {
               if (Solution == null) {
                   Log.Error("Build>Initialise>Solution is null.");
                   throw new InvalidOperationException("The solution must be set");
               }

               if (SingleThreadedTrace) {
                   // This is to work around a bug where trace was not being written.
                   Bilge.SimplifyRouter();
               }

               Bilge.AddHandler(new TCPHandler("127.0.0.1", 9060, true));

               Bilge.SetConfigurationResolver((a, b) => {
                   return System.Diagnostics.SourceLevels.Verbose;
               });

               b = new Bilge("Nuke", tl: System.Diagnostics.SourceLevels.Verbose);


               Bilge.Alert.Online("Listify-Build");
               b.Info.Log("Listify Build Process Initialised, preparing Initialisation section.");


               settings = new LocalBuildConfig();
               settings.NonDestructive = false;
               settings.MainProjectName = "Listify";

               settings.DependenciesDirectory = Solution.Projects.First(x => x.Name == "_Dependencies").Directory;

               string configPath = Path.Combine(settings.DependenciesDirectory, "configuration\\");

               // TODO: Environment identifiers are not set up for deployment yet just this one.
               Log.Information($"Build>Initialise>  Enviorment {EnvironmentId}.");
               var cfg = ListifyConfig.Create(configPath, EnvironmentId);
               settings.Config = cfg;

               settings.ArtifactsDirectory = cfg.ArtefactsDirecory;

               if (settings.NonDestructive) {
                   Log.Information("Build>Initialise>  Finish - In Non Destructive Mode.");
               } else {
                   Log.Information("Build>Initialise> Finish - In Destructive Mode.");
               }


           });


}