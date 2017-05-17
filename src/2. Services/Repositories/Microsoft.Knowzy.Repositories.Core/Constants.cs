namespace Microsoft.Knowzy.Repositories.Core
{
    public static class Constants
    {
        public const string GetShippingsQuery = @"SELECT [Id], [CompanyName], [Tracking], [Status] 
                                                  FROM [Order] 
                                                  WHERE [OrderType] = 'Shipping';";

        public const string GetReceivingsQuery = @"SELECT [Id], [CompanyName], [Tracking], [Status] 
                                                  FROM [Order] 
                                                  WHERE [OrderType] = 'Receiving';";

        public const string GetOrderQuery = @"SELECT [Order].[Id], [Address], [CompanyName], [ContactPerson], [CustomerId]
                                              ,[Email], [PhoneNumber], [PostalCarrierId], [Name] as [PostalCarrierName]
                                              ,[Status], [Tracking]
                                              FROM [Order] INNER JOIN [PostalCarriers] ON [PostalCarriers].[Id] = [Order].[PostalCarrierId]   
                                              WHERE [Order].[Id] = @OrderId;
                                              SELECT [OrderLines].[Id], [OrderLines].[Price] AS [ProductPrice], [Quantity], [ProductId], [Image] AS [ProductImage]
                                              FROM [OrderLines] INNER JOIN [Products] ON [OrderLines].[ProductId] = [Products].[Id]
                                              WHERE [OrderLines].[OrderId] = @OrderId;";

        public const string GetShippingsCountQuery = @"SELECT COUNT(*) 
                                                      FROM [Order]
                                                      WHERE [OrderType] = 'Shipping'";

        public const string GetReceivingsCountQuery = @"SELECT COUNT(*) 
                                                      FROM [Order]
                                                      WHERE [OrderType] = 'Receiving'";

        public const string GetProductCountQuery = @"SELECT COUNT(*) 
                                                     FROM [Products]";

        public const string GetAllQuery = @"SELECT * 
                                            FROM [{0}]";

        public const string InsertOrderQuery = @"INSERT INTO [Order] ([Id], [Address] ,[CompanyName]
                                                    ,[ContactPerson], [Email], [OrderType]
                                                    ,[PhoneNumber], [PostalCarrierId], [Status], [Tracking])
                                                    VALUES
                                                    (@Id, @Address, @CompanyName, @ContactPerson, @Email
                                                     , @OrderType, @PhoneNumber, @PostalCarrierId, @Status, @Tracking)";

        public const string UpdateOrderQuery = @"UPDATE [Order] SET [Address] = @Address , [CompanyName] = @CompanyName
                                                 ,[ContactPerson] = @ContactPerson, [Email] = @Email
                                                 ,[PhoneNumber] = @PhoneNumber, [PostalCarrierId] = @PostalCarrierId
                                                 , [Status] = @Status, [Tracking] = @Tracking
                                                 WHERE [Id] = @OrderId";

        public const string InsertOrderLineQuery = @"INSERT INTO [OrderLines] ([OrderId], [Price], [ProductId], [Quantity])
                                                    VALUES
                                                    (@OrderId, @Price, @ProductId, @Quantity)";

        public const string DeleteOrderLinesQuery = @"DELETE FROM [OrderLines]
                                                      WHERE [OrderId] = @OrderId";
    } 
}
