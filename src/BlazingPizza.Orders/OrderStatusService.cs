using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;

namespace BlazingPizza.Orders
{
    public class OrderStatusService : OrderStatus.OrderStatusBase
    {
        private PizzaStoreContext _db;

        public OrderStatusService(PizzaStoreContext db)
        {
            _db = db;
        }
        public override async Task GetStatus(StatusRequest request, IServerStreamWriter<StatusUpdate> responseStream, ServerCallContext context)
        {
            var order = await _db.GetOrderWithStatus(int.Parse(request.Id));
            while (!context.CancellationToken.IsCancellationRequested)
            {
                order = OrderWithStatus.FromOrder(order.Order);
                await responseStream.WriteAsync(new StatusUpdate()
                {
                    StatusText = order.StatusText,
                    Progress = order.Progress
                });

                if (order.StatusText == "Completed")
                {
                    break;
                }

                await Task.Delay(5000);
            }
        }
    }
}
