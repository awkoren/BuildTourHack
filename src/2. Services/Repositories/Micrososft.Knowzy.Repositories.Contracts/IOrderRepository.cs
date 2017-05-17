﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Knowzy.Domain;
using Microsoft.Knowzy.Models.ViewModels;

namespace Micrososft.Knowzy.Repositories.Contracts
{
    public interface IOrderRepository
    {
        Task<IEnumerable<ShippingsViewModel>> GetShippings();
        Task<IEnumerable<ReceivingsViewModel>> GetReceivings();
        Task<ShippingViewModel> GetShipping(string orderId);
        Task<ReceivingViewModel> GetReceiving(string orderId);
        Task<IEnumerable<Product>> GetProducts();
        Task<IEnumerable<PostalCarrier>> GetPostalCarriers();
        Task<IEnumerable<Customer>> GetCustomers();
        Task<int> GetShippingCount();
        Task<int> GetReceivingCount();
        Task<int> GetProductCount();
        Task AddShipping(Shipping shipping);
        Task UpdateShipping(Shipping shipping);
        Task AddReceiving(Receiving receiving);
        Task UpdateReceiving(Receiving receiving);
    }
}
