namespace Listify.Model;
public record FtpDeployment {
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string Server { get; init; }
    public required string ServerPath { get; init; }
    public required int TimeoutInMins { get; init; }
    public required bool SkipWebContentFolder { get; init; }
    public required bool ActuallyDeployFiles { get; init; }
    public required bool OfflineSiteDuringDeployment { get; init; }

    public bool ValidSettings() {
        if (string.IsNullOrEmpty(Username)) {
            return false;
        }

        if (string.IsNullOrEmpty(Password)) {
            return false;
        }

        if (string.IsNullOrEmpty(Server)) {
            return false;
        }

        if (string.IsNullOrEmpty(ServerPath)) {
            return false;
        }

        return true;
    }
}

