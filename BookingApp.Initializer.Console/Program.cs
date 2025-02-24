// See https://aka.ms/new-console-template for more information
using BookingApp.Core.Domain.Models;
using BookingApp.Core.Interfaces;
using BookingApp.Infrastructure.Data;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

internal class Program
{

    internal static string setUpSQL = @"
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
    RemainingInventory INT,
    ExpirationDate DATE
);


-- Create Members table
CREATE TABLE Members (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100),
    Surname NVARCHAR(100),
    BookingCount INT,
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
";

    private static void Main()
    {

        var serviceProvider = ConfigureServices();

        var repository = serviceProvider.GetRequiredService<IAsyncRepository>();

        Console.WriteLine("The setup will clean all data, CTRL+C to quit or press any key to continue");
        Console.ReadKey();
        try
        {
            repository.CleanAllTables(setUpSQL).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Something went wrong {ex.Message}");
            throw;
        }

        var retryMemberSeeding = true;
        while (retryMemberSeeding)
        {
            retryMemberSeeding = false;
            Console.WriteLine("Enter CSV file path for member seeding:");
            string csvPathForMembers = Console.ReadLine();

            if (File.Exists(csvPathForMembers))
            {
                try
                {
                    List<Member> dataList = HandleCsv(csvPathForMembers)
                                                .Select(ConvertToMembers).ToList();

                    repository.BulkAddAsync(dataList, CancellationToken.None).GetAwaiter().GetResult();

                    Console.WriteLine("Data successfully added to the database.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something wrong with file {csvPathForMembers}");
                    retryMemberSeeding = true;
                }
            }
            else
            {
                Console.WriteLine("CSV file not found.");
                retryMemberSeeding = true;
            }
        }

        var retryInventorySeeding = true;
        while (retryInventorySeeding)
        {
            retryInventorySeeding = false;
            Console.WriteLine("Enter CSV file path for Inventory seeding:");
            string csvPathForInventory = Console.ReadLine();

            if (File.Exists(csvPathForInventory))
            {
                try
                {
                    List<Inventory> dataList = HandleCsv(csvPathForInventory)
                                                    .Select(ConvertToInventory).ToList();

                    repository.BulkAddAsync(dataList, CancellationToken.None).GetAwaiter().GetResult();

                    Console.WriteLine("Data successfully added to the database.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something wrong with file {csvPathForInventory}");
                    retryInventorySeeding = true;
                }
            }
            else
            {
                Console.WriteLine("CSV file not found.");
                retryInventorySeeding = true;
            }
        }


    }

    private static Inventory ConvertToInventory(string[] values)
    {

        return new Inventory
        {
            Title = values[0],
            Description = values[1],
            RemainingInventory = Convert.ToByte(values[2]),
            ExpirationDate = DateOnly.FromDateTime(DateTime.ParseExact(values[3], "d/M/yyyy", null)),
        };
    }

    private static Member ConvertToMembers(string[] values)
    {
        return new Member
        {
            Name = values[0],
            Surname = values[1],
            BookingCount = Convert.ToByte(values[2]),
            DateJoined = DateTime.SpecifyKind(DateTime.Parse(values[3]), DateTimeKind.Utc),
        };
    }

    private static ServiceProvider ConfigureServices()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        return new ServiceCollection()
            .AddDbContext<BookingContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")))
            .AddScoped<IAsyncRepository, BookingRepository>()
            .AddSingleton<IConfiguration>(configuration)
            .BuildServiceProvider();

    }

    private static IEnumerable<string[]> HandleCsv(string filePath)
    {
        var textFile = File.ReadLines(filePath);


        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                BadDataFound = null,
                LineBreakInQuotedFieldIsBadData = true,

            }))
            {
                var records = new List<string[]>();

                // Read header to determine the number of columns
                if (csv.Read())
                {
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        var row = new string[csv.Parser.Count];
                        var emptyColDetected = false;
                        for (int i = 0; i < csv.Parser.Count; i++)
                        {
                            var field = csv.GetField(i);
                            if (string.IsNullOrEmpty(field))
                            {
                                emptyColDetected = true;
                                break;
                            }
                            else
                            {
                                row[i] = field;
                            }
                        }
                        if (!emptyColDetected)
                        {
                            records.Add(row);
                        }
                    }

                }
                return records;
            }
        }
    }




}
