namespace Listify.Model;

public record ListifyBuildConfig {
    public FtpDeployment? FtpDeployment { get; init; }
    public required string BuildScratchDirectory { get; init; }
    public required string VersioningToken { get; init; }
    public required string DiscordHookUrl { get; init; }
    public required string MollyActiveMachines { get; init; }
    public required string MollyRulesToken { get; init; }
    public required string MollyPrimaryToken { get; init; }
}