using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;

namespace BlazingPizza.Orders
{
    public class OrderStatusService : OrderStatus.OrderStatusBase
    {
        private OrdersService _db;

        public OrderStatusService(OrdersService db)
        {
            _db = db;
        }

        public override async Task GetStatus(StatusRequest request, IServerStreamWriter<StatusUpdate> responseStream, ServerCallContext context)
        {
            var order = await _db.GetOrder(new Guid(request.Id));
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var orderStatus = OrderWithStatus.FromOrder(order);
                await responseStream.WriteAsync(new StatusUpdate()
                {
                    StatusText = orderStatus.StatusText,
                    Progress = orderStatus.Progress
                });

                if (orderStatus.StatusText == "Completed")
                {
                    break;
                }

                await Task.Delay(5000);
            }
        }
    }
}
