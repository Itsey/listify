using Microsoft.EntityFrameworkCore;

namespace Listify.Models;
public class ListifyContext : DbContext {

    public DbSet<ActiveList> ActiveLists { get; set; }

    public DbSet<MetaEntry> Metas { get; set; }

    public ListifyContext(DbContextOptions<ListifyContext> options)
        : base(options) {
    }
}
