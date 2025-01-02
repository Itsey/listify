using System.Linq;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Serilog;

public partial class Build : NukeBuild {

    // Examine is the well known step for post compilation, pre package and deploy.
    public Target ExamineStep => _ => _
        .After(ConstructStep)
        .Before(PackageStep)
        .DependsOn(Initialise)
        .Triggers(UnitTest)
        .Executes(() => {
            Log.Information("--> Examine Step <-- ");
        });

    private Target UnitTest => _ => _
      .DependsOn(Compile)
      .After(ExamineStep)
      .Before(PackageStep)
      .Executes(() => {
          var testProjects = Solution.GetAllProjects("*.Test");
          if (testProjects.Any()) {
              DotNetTasks.DotNetTest(s => s
                  .EnableNoRestore()
                  .EnableNoBuild()
                  .SetConfiguration(Configuration)
                  .SetProjectFile(testProjects.First().Directory));
          }
      });
}