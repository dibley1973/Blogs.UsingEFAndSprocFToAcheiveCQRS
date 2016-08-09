# Using Entity Framework and the Store Procedure Framework To Achieve CQRS - Part #3
This article follows on from Part 2 where we built the *QueryStack*. In this article we will build the *Command Stack* and we will leverage *EntityFramework* to do this.

So moving on to the "Blogs.EfAndSprocfForCqrs.DomainModel" project, as we intend to use the *Entity Framework* for data access in the *CommandStack* lets use *NuGet* to add the *Entity Framework* library to this project. You can use the *Package Manager* in *Visual Studio* or the console. I prefer to use the GUI in Visual studio but if you prefer to use the console then the command line is  below.

    PM> Install-Package EntityFramework
    
The next task is to set up the *DomainModel* folder structure in the project.

    |--04.CommandStack
    |  +--Blogs.EfAndSprocfForCqrs.DomainModel
    |     +--Context
    |     +--Dtos
    |     +--Entities
    |     +--Repositories
    |     +--Transactional
    |

Create the *Order* Entity...


create *CommandContext*...

    
So lets start by creating a repository for the Order



Now we can look at creating a unit of work.


