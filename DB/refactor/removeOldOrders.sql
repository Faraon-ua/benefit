declare @listOfIDs table (id varchar(128));

INSERT INTO @listOfIDs 
SELECT Id
  FROM [Benefit.com].[dbo].[Orders]
  where Time < '2019-01-01 00:00:00'

delete from Transactions where OrderId in (SELECT id FROM @listOfIDs)
delete from OrderProducts where OrderId in (SELECT id FROM @listOfIDs)
delete from Orders where Id in (SELECT id FROM @listOfIDs)