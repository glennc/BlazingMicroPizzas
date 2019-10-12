using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazingPizza.Orders
{
    public class PizzaStoreContext : DbContext
    {
        public PizzaStoreContext()
        {
        }

        public PizzaStoreContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Topping> Toppings { get; set; }

        public DbSet<Pizza> Pizzas { get; set; }

        public DbSet<PizzaSpecial> Specials { get; set; }

        public async Task<OrderWithStatus> GetOrderWithStatus(int orderId)
        {
            var order = await this.Orders
                .Where(o => o.OrderId == orderId)
                .Include(o => o.DeliveryLocation)
                .Include(o => o.Pizzas).ThenInclude(p => p.Special)
                .Include(o => o.Pizzas).ThenInclude(p => p.Toppings).ThenInclude(t => t.Topping)
                .SingleOrDefaultAsync();

            return OrderWithStatus.FromOrder(order);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuring a many-to-many special -> topping relationship that is friendly for serialisation
            modelBuilder.Entity<PizzaTopping>().HasKey(pst => new { pst.PizzaId, pst.ToppingId });
            modelBuilder.Entity<PizzaTopping>().HasOne<Pizza>().WithMany(ps => ps.Toppings);
            modelBuilder.Entity<PizzaTopping>().HasOne(pst => pst.Topping).WithMany();
            // Inline the Lat-Long pairs in Order rather than having a FK to another table
            modelBuilder.Entity<Order>().OwnsOne(o => o.DeliveryLocation);
        }
    }
}
