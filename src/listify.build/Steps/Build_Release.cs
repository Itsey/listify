using Nuke.Common;

public partial class Build : NukeBuild {

    public Target ReleaseStep => _ => _
      .DependsOn(Initialise, PackageStep)
      .After(PackageStep)
      .Executes(() => {
      });
}