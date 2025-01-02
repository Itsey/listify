using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Plisky.Nuke.Fusion;
using Serilog;

public partial class Build : NukeBuild {

    public Target ArrangeStep => _ => _
        .Before(ConstructStep)
        .DependsOn(Initialise)
        .Triggers(Clean, MollyCheck)
        .Executes(() => {
            Log.Information("--> Arrange <-- ");
        });

    private Target Clean => _ => _
        .DependsOn(Initialise)
        .After(ArrangeStep, Initialise)
        .Before(ConstructStep)
        .Executes(() => {
            DotNetTasks.DotNetClean(s => s.SetProject(Solution));
            settings.ArtifactsDirectory.CreateOrCleanDirectory();
        });

    private Target MollyCheck => _ => _
       .After(Clean, ArrangeStep)
       .Before(ConstructStep)
       .Executes(() => {
           Log.Information("Mollycoddle Structure Linting Starts.");

           //TODO: Bug LFY-8. https://plisky.atlassian.net/browse/LFY-8
           var mc = new MollycoddleTasks();
           mc.PerformScan(s => s
               .AddRuleHelp(true)
               .SetRulesFile(@"C:\files\code\git\mollycoddle\src\_Dependencies\RulesFiles\XXVERSIONNAMEXX\defaultrules.mollyset")
               .SetPrimaryRoot(@"C:\Files\OneDrive\Dev\PrimaryFiles")
               .SetDirectory(GitRepository.LocalDirectory));

           Log.Information("Mollycoddle Structure Linting Completes.");
       });
}