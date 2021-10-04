CREATE TABLE [dbo].[Address](
        [AddressID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
        [AddressLine1] [nvarchar](60) NOT NULL,
        [AddressLine2] [nvarchar](60) NULL,
	    [City] [nvarchar](30) NOT NULL,
	    [StateProvinceID] [int] NOT NULL,
	    [PostalCode] [nvarchar](15) NOT NULL,
	    [rowguid] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	    [ModifiedDate] [datetime] NOT NULL,
CONSTRAINT [PK_Address_AddressID] PRIMARY KEY CLUSTERED
(
        [AddressID] ASC
) ON [PRIMARY])

DECLARE @i int
SET @i = 0
 
WHILE @i < 1000000
    BEGIN
        INSERT INTO dbo.Address( AddressLine1, 
                                    AddressLine2, 
                                    City, 
                                    StateProvinceID, 
                                    PostalCode, 
                                    rowguid, 
                                    ModifiedDate )
         VALUES(CONVERT(nvarchar(60), NEWID()) ,
                CONVERT(nvarchar(60), NEWID()) , 
                rand () *5 , 
                rand (), 
                rand () *7, 
                NEWID(), 
                DATEADD (day, (ABS(CHECKSUM(NEWID())) % 1625), GETDATE()))
        SET @i = @i + 1
    END

SET STATISTICS TIME ON 
SELECT * FROM dbo.Address
SET STATISTICS TIME OFF