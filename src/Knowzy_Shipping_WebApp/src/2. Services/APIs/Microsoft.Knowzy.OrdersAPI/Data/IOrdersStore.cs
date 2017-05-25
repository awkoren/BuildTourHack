using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Microsoft.Knowzy.OrdersAPI.Data
{
    public interface IOrdersStore : IDisposable
    {
        Task<bool> Connected();
        IEnumerable<Domain.Shipping> GetShippings();
    }
}