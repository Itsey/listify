using System;
using System.Diagnostics.CodeAnalysis;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Plisky.Nuke.Fusion;
using Serilog;

public partial class Build : NukeBuild {

    // Standard entrypoint for compiling the app.  Arrange [Construct] Examine Package Release Test
    public Target ConstructStep => _ => _
        .Before(ExamineStep, Wrapup)
        .After(ArrangeStep)
        .Triggers(Compile)
        .DependsOn(Initialise, ArrangeStep)
        .Executes(() => {
        });

    public bool ValidateSettings([NotNullWhen(true)] LocalBuildConfig sets) {
        if (sets == null) {
            throw new InvalidOperationException("The settings must be set");
        }
        if (sets.Config == null) {
            throw new InvalidOperationException("The settings config must be set");
        }
        if (sets.Config.BuildSection == null) {
            throw new InvalidOperationException("The settings build section must be set");
        }
        if (sets.Config.BuildSection.VersioningToken == null) {
            throw new InvalidOperationException("The settings versioning token must be set");
        }
        return true;
    }

    public Target QueryNextVersion => _ => _
      .After(ConstructStep)
      .DependsOn(Initialise)
      .Before(Compile)
      .Executes(() => {


          ValidateSettings(settings);

          if (Solution == null) {
              Log.Error("Build>ApplyVersion>Solution is null.");
              throw new InvalidOperationException("The solution must be set");
          }


          var vc = new VersonifyTasks();
          vc.PassiveCommand(s => s
          .SetVersionPersistanceValue(settings.Config.BuildSection.VersioningToken)
          .SetOutputStyle("con-nf")
          .SetRoot(Solution.Directory));

          Log.Information($"Version Is:{vc.VersionLiteral}");
      });

    public Target VersionQuickStep => _ => _
      .After(ConstructStep)
      .DependsOn(Initialise)
      .Before(Compile)
      .Executes(() => {
          Log.Information($"Manual Quick Step QV:{QuickVersion}");

          if (!string.IsNullOrEmpty(QuickVersion)) {
              var vc = new VersonifyTasks();
              vc.OverrideCommand(s => s
                .SetVersionPersistanceValue(settings?.Config?.BuildSection?.VersioningToken)
                .SetDebug(true)
                .SetRoot(Solution.Directory)
                .SetQuickValue(QuickVersion)
              );
          }

      });


    public Target ApplyVersion => _ => _
       .After(ConstructStep)
       .DependsOn(Initialise, NexusLive)
       .Before(Compile)
       .Executes(() => {

           bool dryRun = false;
           if (IsLocalBuild) {
               // Passive Get Current Version
               Log.Information("Local Build - Versioning Set To Dry Run");
               dryRun = true;
           }

           Log.Information($"Versioning Token : {settings.Config.BuildSection.VersioningToken}");

           var vc = new VersonifyTasks();
           vc.PassiveCommand(s => s
               .SetVersionPersistanceValue(settings.Config.BuildSection.VersioningToken)
               .AsDryRun(dryRun)
               .SetRoot(Solution.Directory)
           );

           var mmPath = settings.DependenciesDirectory / "packaging";
           mmPath /= "autoversion.txt";

           vc.FileUpdateCommand(s => s
               .SetVersionPersistanceValue(settings.Config.BuildSection.VersioningToken)
               .AddMultimatchFile(mmPath)
               .PerformIncrement(true)
               .SetRoot(Solution.Directory)
               .AsDryRun(dryRun)
           );

           ActiveVersion = vc.VersionLiteral;
           ActiveReleaseName = vc.ReleaseName;
           ActiveShortVersion = vc.ShortVersion;


       });

    public string ActiveVersion { get; private set; }
    public string ActiveReleaseName { get; private set; }
    public string ActiveShortVersion { get; private set; }

    private Target Compile => _ => _
        .Before(ExamineStep)
        .DependsOn(ApplyVersion)
        .Executes(() => {
            DotNetTasks.DotNetBuild(s => s
              .SetProjectFile(Solution)
              .SetConfiguration(Configuration)
              .SetDeterministic(IsServerBuild)
              .SetContinuousIntegrationBuild(IsServerBuild)
          );
        });
}