CREATE TABLE [dbo].[ProductOrdered] (
    [Id]            INT              IDENTITY (1, 1) NOT NULL,
    [OrderId]       UNIQUEIDENTIFIER NOT NULL,
    [ProductId]     INT              NOT NULL,
    [PurchasePrice] MONEY            NOT NULL,
    CONSTRAINT [PK_ProductOrdered] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ProductOrdered_dbo.Order] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Order] ([Id]),
    CONSTRAINT [FK_dbo.ProductOrdered_dbo.Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([Id])
);

