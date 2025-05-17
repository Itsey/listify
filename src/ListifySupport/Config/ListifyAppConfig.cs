namespace Listify.Model;

public record ListifyAppConfig {
    public string? PrimaryUrl { get; set; }

    public string? DbConstr { get; set; }
}