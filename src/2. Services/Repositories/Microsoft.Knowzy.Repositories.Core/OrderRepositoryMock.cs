using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Knowzy.Domain;
using Microsoft.Knowzy.Models.ViewModels;
using Micrososft.Knowzy.Repositories.Contracts;

namespace Microsoft.Knowzy.Repositories.Core
{
    public class OrderRepositoryMock : IOrderRepository
    {
        #region Public Methods
        public Task<IEnumerable<ShippingsViewModel>> GetShippings()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ReceivingsViewModel>> GetReceivings()
        {
            throw new NotImplementedException();
        }

        public Task<ShippingViewModel> GetShipping(string orderId)
        {
            throw new NotImplementedException();
        }

        public Task<ReceivingViewModel> GetReceiving(string orderId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Product>> GetProducts()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PostalCarrier>> GetPostalCarriers()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Customer>> GetCustomers()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetShippingCount()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetReceivingCount()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetProductCount()
        {
            throw new NotImplementedException();
        }

        public Task AddShipping(Shipping shipping)
        {
            throw new NotImplementedException();
        }

        public Task UpdateShipping(Shipping shipping)
        {
            throw new NotImplementedException();
        }

        public Task AddReceiving(Receiving receiving)
        {
            throw new NotImplementedException();
        }

        public Task UpdateReceiving(Receiving receiving)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
