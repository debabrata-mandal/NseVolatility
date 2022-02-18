using ConsoleTables;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TinyCsvParser;

namespace NseVolatility
{
    public class Result
    {
        public string Symbol { get; set; }
        public string Volatility { get; set; }
        public string Change { get; set; }
    }
    public class Worker : BackgroundService
    {
        private IEnumerable<DailyVolatility> volatilityData;

        public Worker()
        {

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string input = "X";
            while (!stoppingToken.IsCancellationRequested)
            {
                if (string.Equals(input, "X", StringComparison.OrdinalIgnoreCase))
                {
                    if (!GetDates())
                    {
                        Console.WriteLine("Dates are not valid please try again");
                        continue;
                    }
                    if (null != volatilityData)
                    {
                        IEnumerable<Result> results = volatilityData.OrderByDescending(n => n.CurrentDayVolatility).Select(item => new Result
                        {
                            Symbol = item.Symbol,
                            Volatility = $"{Math.Round(item.CurrentDayVolatility * 100, 4)}%",
                            Change = $"{Math.Round((item.CurrentDayVolatility - item.PreviousDayVolatility) * 100, 2)}%"
                        });
                        DisplayResult(results);
                    }
                }
                input = Console.ReadLine();
            }
            await Task.CompletedTask;
        }
        private static void DisplayResult(IEnumerable<Result> results)
        {
            ConsoleTable
            .From(results)
            .Configure(o => o.NumberAlignment = Alignment.Right).Write(Format.Alternative);
        }

        private bool GetDates()
        {
            CsvParserOptions csvParserOptions = new(true, ',');
            CsvUserDetailsMapping csvMapper = new();
            CsvParser<DailyVolatility> csvParser = new(csvParserOptions, csvMapper);

            if (!GetDate("Day1"))
            {
                return false;
            }
            volatilityData = csvParser.ReadFromFile("Day1.csv", Encoding.ASCII).ToList().Select(n => n.Result);
            return true;
        }

        private static bool GetDate(string day)
        {
            Console.WriteLine($"Please enter {day} in form of DDMMYYYY format for example 9th March 2022 as 09032022");
            string date = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(date) || date.Length != 8)
            {
                Console.WriteLine("Error: Please enter a valid date");
                return default;
            }
            try
            {
                string json = (new WebClient()).DownloadString($"https://www1.nseindia.com/archives/nsccl/volt/FOVOLT_{date}.csv");
                File.WriteAllText($"{day}.csv", json);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: CSV not available for {date}, actual error| {ex.Message}");
            }
            return default;
        }
    }
}
