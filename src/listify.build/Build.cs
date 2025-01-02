using System;
using System.IO;
using Nuke.Common;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Serilog;


[AzurePipelines(AzurePipelinesImage.UbuntuLatest, AutoGenerate = false, InvokedTargets = new[] { nameof(ArrangeStep), nameof(ConstructStep), nameof(ExamineStep), nameof(PackageStep), nameof(ReleaseStep) })]
public partial class Build : NukeBuild {

    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    private readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitRepository]
    private readonly GitRepository GitRepository;

    [Solution]
    private readonly Solution Solution;

    private AbsolutePath SourceDirectory => RootDirectory / "src";

    //AbsolutePath ArtifactsDirectory => Path.GetTempPath() + "\\artifacts";
    private AbsolutePath ArtifactsDirectory; //=> @"D:\Scratch\_build\vsfbld\";

    private LocalBuildConfig settings;

    public Target Initialise => _ => _
           .Before(ExamineStep)
           .Executes(() => {
               if (Solution == null) {
                   Log.Error("Build>Initialise>Solution is null.");
                   throw new InvalidOperationException("The solution must be set");
               }
               settings = new LocalBuildConfig();

               if (IsLocalBuild) {
                   Log.Information("Local Build Active");
                   settings.ArtifactsDirectory = @"D:\Scratch\_build\vsfbld\";
                   settings.VersioningPersistanceToken = @"D:\Scratch\_build\vstore\listify.vstore";
               }
               else {
                   Log.Information("Remote Build Active");
                   settings.ArtifactsDirectory = Path.Combine(Path.GetTempPath(), "_build");
               }


               settings.NonDestructive = false;

               settings.MainProjectName = "Listify";

               if (settings.NonDestructive) {
                   Log.Information("Build>Initialise>  Finish - In Non Destructive Mode.");
               }
               else {
                   Log.Information("Build>Initialise> Finish - In Destructive Mode.");
               }
           });
}