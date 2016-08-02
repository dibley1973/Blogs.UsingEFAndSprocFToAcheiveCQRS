CREATE TABLE [dbo].[Order] (
    [Id]                  UNIQUEIDENTIFIER NOT NULL,
    [CustomerId]          UNIQUEIDENTIFIER NOT NULL,
    [CustomerOrderNumber] NVARCHAR (50)    NOT NULL,
    [CreatedOnTimeStamp]  DATETIME         NOT NULL,
    CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.Order_dbo.Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_dbo.Order_CustomerId]
    ON [dbo].[Order]([CustomerId] ASC);

