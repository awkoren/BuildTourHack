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

using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Microsoft.Knowzy.DbInitializer
{
    public static class DbInitializerHelper
    {
        public static async Task CreateDatabase(string connectionString)
        {
            using (var sqlCommand = new SqlCommand(CreateDbScript, new SqlConnection(connectionString)))
            {
                sqlCommand.Connection.Open();
                await sqlCommand.ExecuteNonQueryAsync();
                sqlCommand.Connection.Close();
            }
        }

        public static async Task Seed()
        {
            //WIP
        }





















        private const string CreateDbScript = @"
CREATE TABLE [dbo].[Customers](
	                                  [Id] [nvarchar](450) NOT NULL,
	                                  [Address] [nvarchar](max) NOT NULL,
	                            [CompanyName] [nvarchar](max) NOT NULL,
	[ContactPerson] [nvarchar](max) NOT NULL,
	[Email] [nvarchar](max) NOT NULL,
	[PhoneNumber] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
CREATE TABLE [dbo].[Order](
	[Id] [nvarchar](450) NOT NULL,
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
 CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

CREATE TABLE [dbo].[OrderLines](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [nvarchar](450) NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[ProductId] [nvarchar](450) NULL,
	[Quantity] [int] NOT NULL,
 CONSTRAINT [PK_OrderLines] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

CREATE TABLE [dbo].[PostalCarriers](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_PostalCarriers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

CREATE TABLE [dbo].[Products](
	[Id] [nvarchar](450) NOT NULL,
	[Category] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[Image] [nvarchar](max) NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[QuantityInStock] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

ALTER TABLE [dbo].[Order]  WITH CHECK ADD  CONSTRAINT [FK_Order_Customers_CustomerId] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([Id]);

ALTER TABLE [dbo].[Order] CHECK CONSTRAINT [FK_Order_Customers_CustomerId];

ALTER TABLE [dbo].[Order]  WITH CHECK ADD  CONSTRAINT [FK_Order_PostalCarriers_PostalCarrierId] FOREIGN KEY([PostalCarrierId])
REFERENCES [dbo].[PostalCarriers] ([Id])
ON DELETE CASCADE;

ALTER TABLE [dbo].[Order] CHECK CONSTRAINT [FK_Order_PostalCarriers_PostalCarrierId];

ALTER TABLE [dbo].[OrderLines]  WITH CHECK ADD  CONSTRAINT [FK_OrderLines_Order_OrderId] FOREIGN KEY([OrderId])
REFERENCES [dbo].[Order] ([Id]);

ALTER TABLE [dbo].[OrderLines] CHECK CONSTRAINT [FK_OrderLines_Order_OrderId];

ALTER TABLE [dbo].[OrderLines]  WITH CHECK ADD  CONSTRAINT [FK_OrderLines_Products_ProductId] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id]);

ALTER TABLE [dbo].[OrderLines] CHECK CONSTRAINT [FK_OrderLines_Products_ProductId];
";
    }
}
