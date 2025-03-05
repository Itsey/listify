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

    public Target VersionQuickStep => _ => _
      .After(ConstructStep)
      .DependsOn(Initialise)
      .Before(Compile)
      .Executes(() => {
          Log.Information($"Manual Quick Step QV:{QuickVersion}");

          if (!string.IsNullOrEmpty(QuickVersion)) {
              var vc = new VersonifyTasks();
              vc.OverrideCommand(s => s
                .SetVersionPersistanceValue(settings.Config.BuildSection.VersioningToken)
                .SetDebug(true)
                .SetRoot(Solution.Directory)
                .SetQuickValue(QuickVersion)
              );
          }

      });


    public Target ApplyVersion => _ => _
       .After(ConstructStep)
       .DependsOn(Initialise)
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



       });

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