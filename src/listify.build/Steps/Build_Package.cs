using Nuke.Common;

public partial class Build : NukeBuild {

    private Target PackageStep => _ => _
        .After(ExamineStep)
        .Before(ReleaseStep)
        .DependsOn(Initialise, ExamineStep)
        .Executes(() => {
        });
}