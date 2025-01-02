using Nuke.Common;
using Nuke.Common.Tools.DotNet;

public partial class Build : NukeBuild {

    // Standard entrypoint for compiling the app.  Arrange [Construct] Examine
    public Target ConstructStep => _ => _
        .Before(ExamineStep)
        .After(ArrangeStep)
        .Triggers(Compile)
        .DependsOn(Initialise, ArrangeStep)
        .Executes(() => {
        });

    private Target Restore => _ => _
       .After(ConstructStep)
       .Before(Compile)
       .Executes(() => {
       });

    private Target Compile => _ => _
        .Before(ExamineStep)
        .Executes(() => {
            DotNetTasks.DotNetBuild(s => s
              .SetProjectFile(Solution)
              .SetConfiguration(Configuration)
              .SetDeterministic(IsServerBuild)
              .SetContinuousIntegrationBuild(IsServerBuild)
          );
        });
}