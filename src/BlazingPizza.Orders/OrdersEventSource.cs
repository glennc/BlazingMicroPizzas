using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlazingPizza.Orders
{
    internal sealed class OrdersEventSource : EventSource
    {
        public static readonly OrdersEventSource Log = new OrdersEventSource();

        private PollingCounter _totalOrdersCounter;

        private long _totalOrders;

        internal OrdersEventSource()
            : base("BlazingOrders.Pizza")
        {

        }

        internal void OrderCreated()
        {
            Interlocked.Increment(ref _totalOrders);
        }

        protected override void OnEventCommand(EventCommandEventArgs command)
        {
            if (command.Command == EventCommand.Enable)
            {
                _totalOrdersCounter ??= new PollingCounter("total-orders", this, () => Volatile.Read(ref _totalOrders))
                {
                    DisplayName = "Total Orders"
                };
            }
        }
    }
}
