SET IDENTITY_INSERT [dbo].[ProductOrdered] ON
INSERT INTO [dbo].[ProductOrdered] ([Id], [OrderId], [ProductId], [PurchasePrice]) VALUES (1, N'4a61a22a-bade-d780-bbfa-be19c7746d87', 5, CAST(102.0000 AS Money))
INSERT INTO [dbo].[ProductOrdered] ([Id], [OrderId], [ProductId], [PurchasePrice]) VALUES (2, N'4a61a22a-bade-d780-bbfa-be19c7746d87', 7, CAST(75.0000 AS Money))
INSERT INTO [dbo].[ProductOrdered] ([Id], [OrderId], [ProductId], [PurchasePrice]) VALUES (3, N'4a61a22a-bade-d780-bbfa-be19c7746d87', 9, CAST(25.0000 AS Money))
SET IDENTITY_INSERT [dbo].[ProductOrdered] OFF
