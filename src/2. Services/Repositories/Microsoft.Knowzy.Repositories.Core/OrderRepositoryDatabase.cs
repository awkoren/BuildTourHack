using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Knowzy.Domain;
using Microsoft.Knowzy.Models.ViewModels;
using Micrososft.Knowzy.Repositories.Contracts;

namespace Microsoft.Knowzy.Repositories.Core
{
    public class OrderRepositoryDatabase : IOrderRepository
    {
        #region Fields

        private readonly IConfiguration _configuration;

        #endregion

        #region Constructor

        public OrderRepositoryDatabase(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #endregion

        #region Public Methods

        public async Task<IEnumerable<ShippingsViewModel>> GetShippings()
        {
            return await Get(async db => await db.QueryAsync<ShippingsViewModel>(Constants.GetShippingsQuery));
        }

        public async Task<IEnumerable<ReceivingsViewModel>> GetReceivings()
        {
            return await Get(async db => await db.QueryAsync<ReceivingsViewModel>(Constants.GetReceivingsQuery));
        }

        public async Task<ShippingViewModel> GetShipping(string orderId)
        {
            return await Get(async db =>
            {
                var queryResult = await db.QueryMultipleAsync(Constants.GetOrderQuery, new
                {
                    OrderId = orderId
                });
                var shippingViewModel = queryResult.Read<ShippingViewModel>().Single();
                shippingViewModel.OrderLines = queryResult.Read<OrderLineViewModel>().ToList();
                return shippingViewModel;
            });

        }

        public async Task<ReceivingViewModel> GetReceiving(string orderId)
        {
            return await Get(async db =>
            {
                var queryResult = await db.QueryMultipleAsync(Constants.GetOrderQuery, new
                {
                    OrderId = orderId
                });
                var shippingViewModel = queryResult.Read<ReceivingViewModel>().Single();
                shippingViewModel.OrderLines = queryResult.Read<OrderLineViewModel>().ToList();
                return shippingViewModel;
            });
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            return await Get(async db => await db.QueryAsync<Product>(string.Format(Constants.GetAllQuery, "Products")));
        }

        public async Task<IEnumerable<PostalCarrier>> GetPostalCarriers()
        {
            return await Get(async db => await db.QueryAsync<PostalCarrier>(string.Format(Constants.GetAllQuery, "PostalCarriers")));
        }

        public async Task<IEnumerable<Customer>> GetCustomers()
        {
            return await Get(async db => await db.QueryAsync<Customer>(string.Format(Constants.GetAllQuery, "Customers")));
        }

        public async Task<int> GetShippingCount()
        {
            return await Get(async db => await db.QuerySingleOrDefaultAsync<int>(Constants.GetShippingsCountQuery));
        }

        public async Task<int> GetReceivingCount()
        {
            return await Get(async db => await db.QuerySingleOrDefaultAsync<int>(Constants.GetReceivingsCountQuery));
        }

        public async Task<int> GetProductCount()
        {
            return await Get(async db => await db.QuerySingleOrDefaultAsync<int>(Constants.GetReceivingsCountQuery));
        }

        public async Task AddShipping(Shipping shipping)
        {
            await Execute(async db =>
            {
                await InsertOrder(db, Constants.InsertOrderQuery, shipping);
            });
        }

        public async Task UpdateShipping(Shipping shipping)
        {
            await Execute(async db =>
            {
                await UpdateOrder(db, Constants.UpdateOrderQuery, shipping);
            });
        }

        public async Task AddReceiving(Receiving receiving)
        {
            await Execute(async db =>
            {
                await InsertOrder(db, Constants.InsertOrderQuery, receiving);
            });
        }

        public async Task UpdateReceiving(Receiving receiving)
        {
            await Execute(async db =>
            {
                await UpdateOrder(db, Constants.UpdateOrderQuery, receiving);
            });
        }

        #endregion

        #region Private Methods

        private async Task InsertOrder<T>(IDbConnection db, string query, T entity) where T : Order
        {
            var orderId = OrderRepositoryHelper.GenerateString(10);

            using (var transaction = db.BeginTransaction())
            {
                try
                {
                    await db.ExecuteAsync(query, new
                    {
                        Id = orderId,
                        entity.Address,
                        entity.CompanyName,
                        entity.ContactPerson,
                        entity.Email,
                        OrderType = typeof(T).Name,
                        entity.PhoneNumber,
                        entity.PostalCarrierId,
                        entity.Status,
                        entity.Tracking
                    }, transaction);
                    if (entity.OrderLines != null && entity.OrderLines.Count > 0)
                    {
                        await db.ExecuteAsync(Constants.InsertOrderLineQuery, entity.OrderLines.Select(orderLine => new OrderLine
                            {
                                OrderId = orderId,
                                ProductId = orderLine.ProductId,
                                Quantity = orderLine.Quantity,
                                Price = orderLine.Price
                            }
                        ), transaction);
                    }
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                }
            }
        }

        private async Task UpdateOrder<T>(IDbConnection db, string query, T entity) where T : Order
        {
            using (var transaction = db.BeginTransaction())
            {
                try
                {
                    await db.ExecuteAsync(Constants.DeleteOrderLinesQuery, new {orderId = entity.Id}, transaction);
                    await db.ExecuteAsync(query, new
                    {
                        entity.Address,
                        entity.CompanyName,
                        entity.ContactPerson,
                        entity.Email,
                        entity.PhoneNumber,
                        entity.PostalCarrierId,
                        entity.Status,
                        entity.Tracking,
                        OrderId = entity.Id
                    }, transaction);
                    if (entity.OrderLines != null && entity.OrderLines.Count > 0)
                    {
                        await db.ExecuteAsync(Constants.InsertOrderLineQuery, entity.OrderLines.Select(orderLine => new OrderLine
                            {
                                OrderId = entity.Id,
                                ProductId = orderLine.ProductId,
                                Quantity = orderLine.Quantity,
                                Price = orderLine.Price
                            }
                        ), transaction);
                    }
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                }
            }
        }

        private async Task Execute(Func<IDbConnection, Task> query)
        {
            using (IDbConnection db = new SqlConnection(_configuration.GetConnectionString("Knowzy")))
            {
                db.Open();
                await query.Invoke(db);
            }
        }

        private async Task<T> Get<T>(Func<IDbConnection, Task<T>> query)
        {
            using (IDbConnection db = new SqlConnection(_configuration.GetConnectionString("Knowzy")))
            {    
                db.Open();
                return await query.Invoke(db);
            }
        }      

        #endregion
    }
}
