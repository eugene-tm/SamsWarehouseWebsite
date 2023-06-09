using Microsoft.EntityFrameworkCore;


namespace SamsWarehouse.Models
{
    public class MyContext : DbContext
    {
        public MyContext(DbContextOptions<MyContext> options) : base(options) 
        {

        }


		public DbSet<Item> Items { get; set; }
        public DbSet<User> Users { get; set; } 
        public DbSet<ShoppingList> ShoppingLists { get; set; }
        public DbSet<ListItem> ListItems { get; set; }

    }
}
