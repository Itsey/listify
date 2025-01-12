using Nuke.Common;
using Nuke.Common.Tools.DotNet;

public partial class Build : NukeBuild {

    // Package Step - Well known step for bundling prior to the app release.   Arrange Construct Examine [Package] Release Test
    private Target PackageStep => _ => _
        .After(ExamineStep)
        .Before(ReleaseStep, Wrapup)
        .DependsOn(Initialise, ExamineStep)
        .Executes(() => {
            var ad = settings.ArtifactsDirectory / "publish";

            // Definitely need to work out how to EnablePublishTrimmed current deployment is too large.
            DotNetTasks.DotNetPublish(s => s
                .SetProject(Solution)
                .SetConfiguration(Configuration)
                .SetOutput(ad)
                .SetNoRestore(true)
                .SetNoBuild(true)
                .SetVerbosity(DotNetVerbosity.detailed)
            //.EnablePublishTrimmed()
            //.SetArgumentConfigurator(a => a.Add("/p:PublishReadyToRun=true"))
            );
        });
}