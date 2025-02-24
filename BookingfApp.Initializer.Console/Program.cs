// See https://aka.ms/new-console-template for more information
using BookingApp.Core.Interfaces;
using BookingApp.Infrastructure.Data;
using BookingApp.Core.Domain.Models;
using BookingApp.Infrastructure.Data;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

internal class Program
{
    private static void Main()
    {

        var serviceProvider = ConfigureServices();

        var repository = serviceProvider.GetRequiredService<IAsyncRepository>();

        Console.WriteLine("Enter CSV file path:");
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
            .AddScoped<IAsyncRepository, AppRepository>()
            .AddSingleton<IConfiguration>(configuration)
            .BuildServiceProvider();

    }

    private static IEnumerable<string[]> HandleCsv(string filePath)
    {
        var textFile = File.ReadLines(filePath);


        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
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
