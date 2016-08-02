CREATE TABLE [dbo].[Customer] (
    [Id]             UNIQUEIDENTIFIER NOT NULL,
    [Name]           NVARCHAR (255)   NOT NULL,
    [RegisteredDate] DATE             NOT NULL,
    [Active]         BIT              CONSTRAINT [DF_Customer_Active] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED ([Id] ASC)
);

