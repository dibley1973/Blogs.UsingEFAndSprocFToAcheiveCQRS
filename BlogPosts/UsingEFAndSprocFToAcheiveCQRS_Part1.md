# Using Entity Framework and the Store Procedure Framework To Achieve CQRS - Part #1

> CQRS stands for Command Query Responsibility Segregation. 
> At its heart is the notion that you can use a different model to update information than the model you use to read information. 
>
> -- <cite>Martin Fowler</cite>

This blog post is going to show you how you can achieve this separation of data concerns using Entity Framework for your command stack and the Stored Procedure Framework for your query stack.

## Background

We are probably all aware of the benefits that using Entity Framework brings when using for data access in a .Net application, for example type safety, object representation, change tracking and the unit of work pattern providing transactional safety. We have probably all worked on projects where we have used EF for querying, but this can all be a bit "heavy" when all you really need is a quick query to show a list of objects, with maybe only a subset of the objects properties. We may also have accidentally coded in an N+1 error and hit the database thousands of times for what really should have been a simple query.

So is there an alternative? Of course there is, there are many alternatives. What I'd like to demonstrate today is one of these alternatives and that is to separate the writes from the reads, the commands from the queries, and show a basic implementation of the CQRS using *Entity Framework* within the *Command Stack* and the *Stored Procedure Framework* in the *Query Stack*.

Below is a high level diagram of how the architecture will look. Starting at the user interface all interaction will be through a services layer. This could easily be WebApi, MVC controllers, a stand Windows service or any other endpoint that you choose. In our case we will use a service DLL and our "UI" will actually be a "Integration Test" project. The service will have two stacks to interact with for data access. 

![High Level CQRS Diagram](https://github.com/dibley1973/Blogs.UsingEFAndSprocFToAcheiveCQRS/blob/master/BlogPosts/CQRS_Diagram.png?raw=true "High Level CQRS Diagram")

The first is the *Query Stack* which will be used for pulling data out of the data store, which in our case will be a SQL Server database. The query stack will house the ReadModel which will leverage the *Stored Procedure Framework* (known as SprocF in this blog post) to return light weight DTOs to the service.

The second stack is the *Command Stack* which will contain any Domain objects or Domain logic that should be applied to the objects and the Domain Objects will be contained in a repository. Database access will be leveraged though *Entity Framework* (known as EF in this blog post). Lastly the Command Stack will contain the commands which the service will use to manipulate the data within the database via the Domain and EF.

## Assumptions
This blog post assumes the reader is competent in C#, and a number of common patterns and practices. 

## Solution Setup
Before we can start to write any code we will set up the solution to organise the code, so lets set up some solution folders to organise the code. So we will start of with folders for Client, Services, Query Stack, Command Stack and lastly Database.

|
+--Code
|  +-- 01.Client
|  +-- 02.Services
|  +-- 03.QueryStack
|  +-- 04.CommandStack
|  +-- 05.Database
|

### Client
For the "client" in a further effort to keep things simple, I am just going to create a *Unit Test Project* and use that as an integration test harness for the Service project.

### Services
For the services I will create a C# *Class Library* called "Blogs.EfAndSprocfForCqrs.Services".

### Query Stack
In the Query Stack I will create a C# *Class Library* read model called "Blogs.EfAndSprocfForCqrs.ReadModel"

### Command Stack
In the Command Stack I will create a C# *Class Library* read model called "Blogs.EfAndSprocfForCqrs.DomainModel". Normally I might separate The domain model, domain services and repository out into separate DLLs. I would also likely use interfaces to reduce coupling and promote testability. But as that is not what this post is about I'll try and keep it a little more simple. 

### Database
We will create a database project "Blogs.EfAndSprocfForCqrs.Database" to hold our schema and seeding data.

### Full Solution Set-up
So the full solution set-up can be see in the image below:

![Solution Set-up](https://github.com/dibley1973/Blogs.UsingEFAndSprocFToAcheiveCQRS/blob/master/BlogPosts/SolutionExplorer_01.png?raw=true "Solution Set-up")

## Data Setup
So now lets take a look at the data we want to set up.  We are going to be concentrating on three entities; "Order", "Product", "Customer" with a relationships that allows each single "Customer" to have many "Orders" and each single "Order" to have many "Products". We will need some addition linking tables like the *ProductOrdered* table which identifies a product which is on an order.

![Order Database Entities](https://github.com/dibley1973/Blogs.UsingEFAndSprocFToAcheiveCQRS/blob/master/BlogPosts/OrderDatabaseEntities_01.png?raw=true "Order Database Entities")

Using the "Script.PostDeployment.sql" below we will we will seed the tables with the data that follows.

    :r "..\SeedData\dbo.Product.data.sql"
    :r "..\SeedData\dbo.Customer.data.sql"
    :r "..\SeedData\dbo.Order.data.sql"
    :r "..\SeedData\dbo.ProductOrdered.data.sql"

### dbo.Product

    SET IDENTITY_INSERT [dbo].[Product] ON
    INSERT INTO [dbo].[Product] ([Id], [Key], [Name], [Description], [CreatedTimestamp]) VALUES (2, N'compressor-mac-2hp', N'2HP MAC Tools Compressor', N'Super dandy 2HP  portable compressor by MAC Tools ', N'2015-11-23 16:00:23')
    INSERT INTO [dbo].[Product] ([Id], [Key], [Name], [Description], [CreatedTimestamp]) VALUES (5, N'snapon-ratchet-ring-metric-10-21', N'Snap-On Metric Ratchet Ring Set 10-21mm', N'Snap-On Metric Ratchet set 10mm - 21mm', N'2015-11-24 00:00:00')
    INSERT INTO [dbo].[Product] ([Id], [Key], [Name], [Description], [CreatedTimestamp]) VALUES (7, N'snap-on-ratchet-screwdriver-red', N'Snap-On Ratchet Screwdriver in Red', N'Snap-On Ratchet Screwdriver in Red with six bits included', N'2015-11-24 01:01:24')
    INSERT INTO [dbo].[Product] ([Id], [Key], [Name], [Description], [CreatedTimestamp]) VALUES (8, N'usmash-4lb-hammer', N'U-Smash 4lb lump hammer', N'U-Smash 4lb lump hammer', N'2015-11-25 16:00:35')
    INSERT INTO [dbo].[Product] ([Id], [Key], [Name], [Description], [CreatedTimestamp]) VALUES (9, N'pry-master-prybar', N'Pri-Master 24" Pry-Bar', N'Pri-Master 24" Pry-Bar with plastic ergonmoic handle', N'2015-11-26 17:00:02')
    INSERT INTO [dbo].[Product] ([Id], [Key], [Name], [Description], [CreatedTimestamp]) VALUES (10, N'snap-on-6-inch-slip-plyers', N'Snap-On 6 Inch Slip Plyers', N'Snap-On 6 Inch Slip Plyers, ideal for removing brake spring clips', N'2015-12-02 09:33:22')
    SET IDENTITY_INSERT [dbo].[Product] OFF

### dbo.Customer

    INSERT INTO [dbo].[Customer] ([Id], [Name], [RegisteredDate], [Active]) VALUES (N'17e3a22e-07e5-4ab2-8e62-1b15f9916909', N'Mike Finnegan', N'1961-01-19', 1)
    INSERT INTO [dbo].[Customer] ([Id], [Name], [RegisteredDate], [Active]) VALUES (N'baded780-bbfa-4a61-a22a-7746d87be19c', N'David Frieburger', N'1969-09-26', 1)
    
### dbo.Order

    INSERT INTO [dbo].[Order] ([Id], [CustomerId], [CustomerOrderNumber], [CreatedOnTimeStamp]) VALUES (N'4a61a22a-bade-d780-bbfa-be19c7746d87',    N'17e3a22e-07e5-4ab2-8e62-1b15f9916909', N'0000001', N'2016-01-02 11:08:34')

### dbo.ProductOrdered

    SET IDENTITY_INSERT [dbo].[ProductOrdered] ON
    INSERT INTO [dbo].[ProductOrdered] ([Id], [OrderId], [ProductId], [PurchasePrice]) VALUES (1, N'4a61a22a-bade-d780-bbfa-be19c7746d87', 5, CAST(102.0000 AS Money))
    INSERT INTO [dbo].[ProductOrdered] ([Id], [OrderId], [ProductId], [PurchasePrice]) VALUES (2, N'4a61a22a-bade-d780-bbfa-be19c7746d87', 7, CAST(75.0000 AS Money))
    INSERT INTO [dbo].[ProductOrdered] ([Id], [OrderId], [ProductId], [PurchasePrice]) VALUES (3, N'4a61a22a-bade-d780-bbfa-be19c7746d87', 9, CAST(25.0000 AS Money))
    SET IDENTITY_INSERT [dbo].[ProductOrdered] OFF
    
## Validate the data is correct
While we are here lets just validate that the data we have seeded relates correctly. using the "[dbo].[GetOrderDetailsForOrderId]" Stored procedure lets just check we have an "Order", a "Customer", and three "Products" on the order. (We will use this stored procedure later, so it is worth creating now as its value is two-fold.

### [dbo].[GetOrderDetailsForOrderId]
If you want to see the result just select and run all of the code from the "DECLARE" within the comment block until the just before the END keyword.

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
        SELECT      @OrderId                [Id]
        ,           [Name]
        ,           [RegisteredDate]
        ,           [Active]
        FROM        [dbo].[Customer]        [customer]
        INNER JOIN  [dbo].[Order]           [order]
                ON  [order].[CustomerId]    = [customer].[Id]
        WHERE       [order].[Id]            = @OrderId;

        /* Products on the order */
        SELECT      [ordered].[Id]
        ,           [ordered].[OrderId]
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
 
We should see three RecordSets, like below;


|Id                                   | CustomerId                           | CustomerOrderNumber | CreatedOnTimeStamp      |
|-------------------------------------|--------------------------------------|---------------------|-------------------------|
|4A61A22A-BADE-D780-BBFA-BE19C7746D87 | 17E3A22E-07E5-4AB2-8E62-1B15F9916909 | 0000001             | 2016-01-02 11:08:34.000 |


| Id                                  | Name             | RegisteredDate | Active |
|-------------------------------------|------------------|----------------|--------|
|17E3A22E-07E5-4AB2-8E62-1B15F9916909 | Mike Finnegan    | 1961-01-19     | 1      |


|Id | ProductId | Key                              | Name                                    | Description                                               | PurchasePrice |
|---|-----------|----------------------------------|-----------------------------------------|-----------------------------------------------------------|---------------|
|1  | 5         | snapon-ratchet-ring-metric-10-21 | Snap-On Metric Ratchet Ring Set 10-21mm | Snap-On Metric Ratchet set 10mm - 21mm                    | 102.00        |
|2  | 7         | snap-on-ratchet-screwdriver-red  | Snap-On Ratchet Screwdriver in Red      | Snap-On Ratchet Screwdriver in Red with six bits included | 75.00         |
|3  | 9         | pry-master-prybar                | Pri-Master 24" Pry-Bar                  | 24" Pry-Bar with plastic ergonmoic handle                 | 25.00         |

All going well we shall call this a day and in the next article move on to the ReadModel and pull this data from the database using the **Stored Procedure framework**.

This post and all corresponding code and data can be found on my GitHub project - [https://github.com/dibley1973/Blogs.UsingEFAndSprocFToAcheiveCQRS](https://github.com/dibley1973/Blogs.UsingEFAndSprocFToAcheiveCQRS)
