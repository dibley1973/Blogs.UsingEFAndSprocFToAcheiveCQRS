/*
Post-Deployment Script Template                            
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.        
 Use SQLCMD syntax to include a file in the post-deployment script.            
 Example:      :r .\myfile.sql                                
 Use SQLCMD syntax to reference a variable in the post-deployment script.        
 Example:      :setvar TableName MyTable                            
               SELECT * FROM [$(TableName)]                    
--------------------------------------------------------------------------------------
*/
:r "..\SeedData\dbo.Product.data.sql"
:r "..\SeedData\dbo.Customer.data.sql"
:r "..\SeedData\dbo.Order.data.sql"
:r "..\SeedData\dbo.ProductOrdered.data.sql"