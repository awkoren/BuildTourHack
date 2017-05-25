using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Knowzy.OrdersAPI.Data;

namespace Microsoft.Knowzy.OrdersAPI.Controllers
{
    [Route("api/[controller]")]
    public class ShippingController : Controller
    {
        private IOrdersStore _ordersStore;
        public ShippingController(IOrdersStore ordersStore)
        {
            _ordersStore = ordersStore;
        }
        // GET api/values
        [HttpGet]
        public IEnumerable<Domain.Shipping> Get()
        {
            return _ordersStore.GetShippings();
        }
    }
}
