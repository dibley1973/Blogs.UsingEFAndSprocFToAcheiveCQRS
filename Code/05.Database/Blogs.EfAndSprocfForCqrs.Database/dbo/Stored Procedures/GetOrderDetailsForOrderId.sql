
CREATE PROCEDURE [dbo].[GetOrderDetailsForOrderId]
(
    @OrderId uniqueidentifier
)
AS
BEGIN
    /*  
    DECLARE @OrderId uniqueidentifier = '4A61A22A-BADE-D780-BBFA-BE19C7746D87';
    -- */

    /* Order */
    SELECT      [Id]
    ,           [CustomerId]
    ,           [CustomerOrderNumber]
    ,           [CreatedOnTimeStamp]
    FROM        [dbo].[Order]
    WHERE       [Id] = @OrderId;

    /* Customer who ordered */
    SELECT      [customer].[Id]                [Id]
    ,           [Name]
    ,           [RegisteredDate]
    ,           [Active]
    FROM        [dbo].[Customer]        [customer]
    INNER JOIN  [dbo].[Order]           [order]
            ON  [order].[CustomerId]    = [customer].[Id]
    WHERE       [order].[Id]            = @OrderId;

    /* Products on the order */
    SELECT      [ordered].[Id]
    ,           [ordered].[ProductId]
    ,           [product].[Key]
    ,           [product].[Name]
    ,           [product].[Description]
    ,           [ordered].[PurchasePrice]
    FROM        [dbo].[ProductOrdered]  [ordered]
    INNER JOIN  [dbo].[Product]         [product]
            ON  [product].[Id]          = [ordered].[ProductId]
    WHERE       [ordered].[OrderId]     = @OrderId;
END