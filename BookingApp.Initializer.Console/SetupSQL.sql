-- Drop the table if it exists

IF OBJECT_ID('dbo.Bookings', 'U') IS NOT NULL
    DROP TABLE dbo.Bookings;

-- Drop the table if it exists
IF OBJECT_ID('dbo.Members', 'U') IS NOT NULL
    DROP TABLE dbo.Members;

-- Drop the table if it exists
IF OBJECT_ID('dbo.Inventories', 'U') IS NOT NULL
    DROP TABLE dbo.Inventories;



-- Create Inventories table
CREATE TABLE Inventories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(100),
    Description NVARCHAR(1500),
    RemainingInventory SMALLINT,
    ExpirationDate DATE
);


-- Create Members table
CREATE TABLE Members (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100),
    Surname NVARCHAR(100),
    BookingCount SMALLINT,
    DateJoined DATETIMEOFFSET
);

-- Create Bookings table
CREATE TABLE Bookings (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MemberId INT,
    InventoryId INT,
    BookingDate DATETIME,
    CONSTRAINT FK_Bookings_Member FOREIGN KEY (MemberId)
    REFERENCES Members(Id),
    CONSTRAINT FK_Bookings_Inventory FOREIGN KEY (InventoryId)
    REFERENCES Inventories(Id)
);



