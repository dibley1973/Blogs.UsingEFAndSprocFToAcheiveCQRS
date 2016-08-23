CREATE TABLE [dbo].[Product] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [Key]              NVARCHAR (128) NOT NULL,
    [Name]             NVARCHAR (255) NOT NULL,
    [Description]      NVARCHAR (MAX) NOT NULL,
    [CreatedTimestamp] DATETIME       NOT NULL,
    [Price] MONEY NOT NULL, 
    CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED ([Id] ASC)
);

