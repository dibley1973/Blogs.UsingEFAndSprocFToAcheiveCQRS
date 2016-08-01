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



