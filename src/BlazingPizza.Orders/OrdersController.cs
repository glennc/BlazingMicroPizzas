using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazingPizza.Orders
{
    [Route("orders")]
    [ApiController]
    public class OrdersController : Controller
    {
        private readonly PizzaStoreContext _db;

        public OrdersController(PizzaStoreContext db)
        {
            _db = db;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<List<OrderWithStatus>>> GetOrders(string userId)
        {
            var orders = await _db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.DeliveryLocation)
                .Include(o => o.Pizzas).ThenInclude(p => p.Special)
                .Include(o => o.Pizzas).ThenInclude(p => p.Toppings).ThenInclude(t => t.Topping)
                .OrderByDescending(o => o.CreatedTime)
                .ToListAsync();

            return orders.Select(o => OrderWithStatus.FromOrder(o)).ToList();
        }

        [HttpGet("{orderId}/{userId}")]
        public async Task<ActionResult<OrderWithStatus>> GetOrderWithStatus(int orderId, string userId)
        {
            //TODO: Prob just remove user from this and ensure unique Ids.
            var order = await _db.Orders
                .Where(o => o.OrderId == orderId)
                .Where(o => o.UserId == userId)
                .Include(o => o.DeliveryLocation)
                .Include(o => o.Pizzas).ThenInclude(p => p.Special)
                .Include(o => o.Pizzas).ThenInclude(p => p.Toppings).ThenInclude(t => t.Topping)
                .SingleOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            return OrderWithStatus.FromOrder(order);
        }

        [HttpPost]
        public async Task<ActionResult<int>> PlaceOrder(Order order)
        {
            order.CreatedTime = DateTime.Now;
            order.DeliveryLocation = new LatLong(51.5001, -0.1239);

            // Enforce existence of Pizza.SpecialId and Topping.ToppingId
            // in the database - prevent the submitter from making up
            // new specials and toppings
            foreach (var pizza in order.Pizzas)
            {
                pizza.SpecialId = pizza.Special.Id;
                pizza.Special = null;

                foreach (var topping in pizza.Toppings)
                {
                    topping.ToppingId = topping.Topping.Id;
                    topping.Topping = null;
                }
            }

            _db.Orders.Attach(order);
            await _db.SaveChangesAsync();
            return order.OrderId;
        }
    }
}
