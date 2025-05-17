namespace Listify.Models;
public class MetaEntry {

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
}