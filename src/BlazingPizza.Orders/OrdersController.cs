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
        private readonly OrdersService _db;

        public OrdersController(OrdersService db)
        {
            _db = db;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<List<OrderWithStatus>>> GetOrders(string userId)
        {
            var orders = await _db.GetOrdersForUser(userId);

            return orders.Select(o => OrderWithStatus.FromOrder(o)).ToList();
        }

        [HttpGet("{orderId}/{userId}")]
        public async Task<ActionResult<OrderWithStatus>> GetOrderWithStatus(Guid orderId, string userId)
        {
            Order order = await _db.GetOrder(orderId);

            if (order == null)
            {
                return NotFound();
            }

            return OrderWithStatus.FromOrder(order);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> PlaceOrder(Order order)
        {
            order.CreatedTime = DateTime.UtcNow;
            order.DeliveryLocation = new LatLong(51.5001, -0.1239);
            //TODO: Should we let MongoDB do this?
            order.OrderId = Guid.NewGuid();

            // Enforce existence of Pizza.SpecialId and Topping.ToppingId
            // in the database - prevent the submitter from making up
            // new specials and toppings
            foreach (var pizza in order.Pizzas)
            {
                pizza.SpecialId = pizza.Special.Id;
                pizza.Special = pizza.Special;

                foreach (var topping in pizza.Toppings)
                {
                    topping.ToppingId = topping.Topping.Id;
                    topping.Topping = null;
                }
            }

            await _db.SaveOrder(order);
            return order.OrderId;
        }
    }
}
