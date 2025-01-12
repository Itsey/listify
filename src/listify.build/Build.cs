using System;
using System.IO;
using System.Linq;
using Listify.Model;
using Nuke.Common;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
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

    private AbsolutePath SourceDirectory => RootDirectory / "src";
    private AbsolutePath ArtifactsDirectory;

    private LocalBuildConfig settings;

    public Target Wrapup => _ => _
        .After(Initialise)
        .Executes(() => {
            b.Info.Log("Build >> Wrapup >> All Done.");
            Log.Information("Build>Wrapup>  Finish - Build Process Completed.");
            b.Flush();
        });

    public Target Initialise => _ => _
           .Before(ExamineStep, Wrapup)
           .Triggers(Wrapup)
           .Executes(() => {
               if (Solution == null) {
                   Log.Error("Build>Initialise>Solution is null.");
                   throw new InvalidOperationException("The solution must be set");
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
               var cfg = ListifyConfig.Create(configPath, "1101");
               settings.Config = cfg;

               settings.ArtifactsDirectory = cfg.ArtefactsDirecory;

               if (settings.NonDestructive) {
                   Log.Information("Build>Initialise>  Finish - In Non Destructive Mode.");
               }
               else {
                   Log.Information("Build>Initialise> Finish - In Destructive Mode.");
               }
           });
}