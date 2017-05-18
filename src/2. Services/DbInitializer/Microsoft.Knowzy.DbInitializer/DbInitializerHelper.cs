// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Knowzy.Domain;
using Microsoft.Knowzy.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Knowzy.DbInitializer
{
    public static class DbInitializerHelper
    {
        public static async Task InitializeDatabase(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            var connectionString = configuration.GetConnectionString("Knowzy");
            if (!await DatabaseSchemaAlreadyExists(connectionString))
            {
                await CreateSchema(connectionString);
                await Seed(configuration, hostingEnvironment);
            }
        }

        private static async Task<bool> DatabaseSchemaAlreadyExists(string connectionString)
        {
            try
            {
                using (var sqlCommand = new SqlCommand(
                    "select case when exists((select * from information_schema.tables where table_name = 'Order')) then 1 else 0 end",
                    new SqlConnection(connectionString)))
                {
                    sqlCommand.Connection.Open();
                    return (int)await sqlCommand.ExecuteScalarAsync() == 1;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static async Task CreateSchema(string connectionString)
        {
            using (var sqlCommand = new SqlCommand(CreateDbSchemaScript, new SqlConnection(connectionString)))
            {
                sqlCommand.Connection.Open();
                await sqlCommand.ExecuteNonQueryAsync();
                sqlCommand.Connection.Close();
            }
        }

        private static async Task Seed(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            var connectionString = configuration.GetConnectionString("Knowzy");
            var customerJsonPath = $"{hostingEnvironment.WebRootPath}{configuration["AppSettings:CustomerJsonPath"]}";
            var productJsonPath = $"{hostingEnvironment.WebRootPath}{configuration["AppSettings:ProductJsonPath"]}";
            var orderJsonPath = $"{hostingEnvironment.WebRootPath}{configuration["AppSettings:OrderJsonPath"]}";

            await SeedCustomers(customerJsonPath, connectionString);
            await SeedProducts(productJsonPath, connectionString);
            await SeedOrders(orderJsonPath, connectionString);
        }

        private static async Task SeedOrders(string orderJsonPath, string connectionString)
        {
            var data = await GetData<OrderImport>(orderJsonPath);

            var allOrderLines = new List<OrderLine>();

            if (data?.Receivings != null)
            {
                var postalCarriers = data.Shippings.Select(shipping => shipping.PostalCarrier)
                    .GroupBy(postalCarrier => postalCarrier.Id)
                    .Select(postalCarrier => postalCarrier.First())
                    .ToList();

                allOrderLines.AddRange(data.Receivings.SelectMany(receiving => receiving.OrderLines.Select(
                    orderLine => new OrderLine
                    {
                        OrderId = receiving.Id,
                        Price = orderLine.Price,
                        Quantity = orderLine.Quantity,
                        ProductId = orderLine.ProductId
                    })));

                await Execute(async db =>
                {
                    await db.ExecuteAsync(InsertPostalCarriersQuery, postalCarriers);
                    await db.ExecuteAsync(InsertOrdersQuery, data.Receivings.Select(receiving => new
                    {
                        receiving.CompanyName,
                        receiving.ContactPerson,
                        receiving.Address,
                        receiving.CustomerId,
                        receiving.Email,
                        receiving.PhoneNumber,
                        PostalCarrierId = receiving.PostalCarrier.Id,
                        receiving.Status,
                        receiving.Tracking,
                        receiving.TimeStamp,
                        receiving.Id,
                        receiving.StatusUpdated,
                        OrderType = "Receiving"
                    }));
                }, connectionString);
            }

            if (data?.Shippings != null)
            {
                await Execute(async db =>
                {
                    await db.ExecuteAsync(InsertOrdersQuery, data.Shippings.Select(shipping => new
                    {
                        shipping.CompanyName,
                        shipping.ContactPerson,
                        shipping.Address,
                        shipping.CustomerId,
                        shipping.Email,
                        shipping.PhoneNumber,
                        PostalCarrierId = shipping.PostalCarrier.Id,
                        shipping.Status,
                        shipping.Tracking,
                        shipping.TimeStamp,
                        shipping.Id,
                        shipping.StatusUpdated,
                        OrderType = "Shipping"
                    }));
                }, connectionString);

                allOrderLines.AddRange(data.Shippings.SelectMany(shipping => shipping.OrderLines.Select(
                    orderLine => new OrderLine
                    {
                        OrderId = shipping.Id,
                        Price = orderLine.Price,
                        Quantity = orderLine.Quantity,
                        ProductId = orderLine.ProductId
                    })));
            }

            await Execute(async db =>
            {
                await db.ExecuteAsync(InserOrderLinesQuery, allOrderLines);
            }, connectionString);
        }

        private static async Task SeedProducts(string productJsonPath, string connectionString)
        {
            var data = await GetData<List<Product>>(productJsonPath);

            if (data != null)
            {
                await Execute(async db =>
                {
                    await db.ExecuteAsync(InsertProductsQuery, data);
                }, connectionString);
            }
        }

        private static async Task SeedCustomers(string customerJsonPath, string connectionString)
        {
            var data = await GetData<List<Customer>>(customerJsonPath);

            if (data != null)
            {
                await Execute(async db =>
                {
                    await db.ExecuteAsync(InserCustomersQuery, data);
                }, connectionString);
            }
        }

        private static async Task<T> GetData<T>(string jsonPath)
        {
            var dataAsString = await ReadDataFromFile(jsonPath);
            return JsonConvert.DeserializeObject<T>(dataAsString, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        private static async Task<string> ReadDataFromFile(string path)
        {
            string result;

            using (var reader = File.OpenText(path))
            {
                result = await reader.ReadToEndAsync();
            }

            return result;
        }

        private static async Task Execute(Func<IDbConnection, Task> query, string connectionString)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                db.Open();
                await query.Invoke(db);
            }
        }

        private const string InsertProductsQuery = @"INSERT INTO [products] ([Id], [Category], [Description]
                                                                             ,[Image], [Name], [Price], [QuantityInStock]) 
                                                    VALUES (@Id, @Category, @Description, @Image, @Name, @Price, @QuantityInStock)";

        private const string InserCustomersQuery = @"INSERT INTO [dbo].[Customers] ([Id], [Address], [CompanyName]
                                                                                    ,[ContactPerson], [Email] ,[PhoneNumber])
                                                    VALUES (@Id, @Address, @CompanyName, @ContactPerson, @Email, @PhoneNumber)";

        private const string InsertPostalCarriersQuery = @"INSERT INTO [dbo].[PostalCarriers] ([Id] ,[Name])
                                                           VALUES (@Id, @Name)";

        private const string InsertOrdersQuery = @"INSERT INTO [dbo].[Order] ([Id], [Address], [CompanyName], [ContactPerson]
                                                                              ,[CustomerId], [Email], [OrderType], [PhoneNumber]
                                                                              ,[PostalCarrierId], [Status], [StatusUpdated], [TimeStamp], [Tracking])
                                                       VALUES (@Id, @Address, @CompanyName, @ContactPerson, @CustomerId, @Email, @OrderType, @PhoneNumber, @PostalCarrierId, 
                                                               @Status, @StatusUpdated, @TimeStamp, @Tracking)";

        private const string InserOrderLinesQuery = @"INSERT INTO [dbo].[OrderLines] ([OrderId], [Price]
                                                                                      ,[ProductId], [Quantity])
                                                      VALUES (@OrderId, @Price, @ProductId, @Quantity)";

        private const string CreateDbSchemaScript = @"
CREATE TABLE [dbo].[Customers]([Id] [nvarchar](450) NOT NULL, 
                               [Address] [nvarchar](max) NOT NULL,
	                           [CompanyName] [nvarchar](max) NOT NULL,
	                           [ContactPerson] [nvarchar](max) NOT NULL,
	                           [Email] [nvarchar](max) NOT NULL,
	                           [PhoneNumber] [nvarchar](max) NOT NULL,
CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED ([Id] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) 
ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

CREATE TABLE [dbo].[Order]([Id] [nvarchar](450) NOT NULL,
	                       [Address] [nvarchar](max) NULL,
	                       [CompanyName] [nvarchar](max) NULL,
	                       [ContactPerson] [nvarchar](max) NULL,
	                       [CustomerId] [nvarchar](450) NULL,
	                       [Email] [nvarchar](max) NULL,
	                       [OrderType] [nvarchar](max) NOT NULL,
	                       [PhoneNumber] [nvarchar](max) NULL,
	                       [PostalCarrierId] [int] NOT NULL,
	                       [Status] [int] NOT NULL,
	                       [StatusUpdated] [datetime2](7) NULL,
	                       [TimeStamp] [datetime2](7) NULL,
	                       [Tracking] [nvarchar](max) NOT NULL,
CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED ([Id] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) 
ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

CREATE TABLE [dbo].[OrderLines]([Id] [int] IDENTITY(1,1) NOT NULL,
	                            [OrderId] [nvarchar](450) NULL,
	                            [Price] [decimal](37, 2) NOT NULL,
	                            [ProductId] [nvarchar](450) NULL,
	                            [Quantity] [int] NOT NULL,
CONSTRAINT [PK_OrderLines] PRIMARY KEY CLUSTERED ([Id] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY];

CREATE TABLE [dbo].[PostalCarriers]([Id] [int] NOT NULL,
	                                [Name] [nvarchar](max) NOT NULL,
CONSTRAINT [PK_PostalCarriers] PRIMARY KEY CLUSTERED ([Id] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) 
ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

CREATE TABLE [dbo].[Products]([Id] [nvarchar](450) NOT NULL,
	                          [Category] [nvarchar](max) NOT NULL,
	                          [Description] [nvarchar](max) NOT NULL,
	                          [Image] [nvarchar](max) NULL,
	                          [Name] [nvarchar](max) NOT NULL,
	                          [Price] [decimal](37, 2) NOT NULL,
	                          [QuantityInStock] [nvarchar](max) NOT NULL,
CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([Id] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) 
ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

ALTER TABLE [dbo].[Order]  
WITH CHECK ADD  CONSTRAINT [FK_Order_Customers_CustomerId] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([Id]);

ALTER TABLE [dbo].[Order] CHECK CONSTRAINT [FK_Order_Customers_CustomerId];

ALTER TABLE [dbo].[Order]  
WITH CHECK ADD  CONSTRAINT [FK_Order_PostalCarriers_PostalCarrierId] FOREIGN KEY([PostalCarrierId])
REFERENCES [dbo].[PostalCarriers] ([Id])
ON DELETE CASCADE;

ALTER TABLE [dbo].[Order] CHECK CONSTRAINT [FK_Order_PostalCarriers_PostalCarrierId];

ALTER TABLE [dbo].[OrderLines]  
WITH CHECK ADD  CONSTRAINT [FK_OrderLines_Order_OrderId] FOREIGN KEY([OrderId])
REFERENCES [dbo].[Order] ([Id]);

ALTER TABLE [dbo].[OrderLines] CHECK CONSTRAINT [FK_OrderLines_Order_OrderId];

ALTER TABLE [dbo].[OrderLines]  
WITH CHECK ADD  CONSTRAINT [FK_OrderLines_Products_ProductId] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id]);

ALTER TABLE [dbo].[OrderLines] CHECK CONSTRAINT [FK_OrderLines_Products_ProductId];";
    }
}
