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
    public class Worker : BackgroundService
    {
        private IEnumerable<DailyVolatility> _day1CSV;
        private IEnumerable<DailyVolatility> _day2CSV;

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
                    if (null != _day1CSV && null != _day2CSV)
                    {
                        foreach (DailyVolatility item in _day1CSV.OrderBy(n => n.CurrentDayVolatility))
                        {
                            DailyVolatility day2Item = _day2CSV.FirstOrDefault(n => string.Equals(n.Symbol, item.Symbol, StringComparison.OrdinalIgnoreCase));
                            if (null == day2Item)
                            {
                                Console.WriteLine($"Error: Symbol {item.Symbol} not found for previous day");
                            }
                            double difference = Math.Round((item.CurrentDayVolatility - day2Item.CurrentDayVolatility) * 100, 2);

                            Console.WriteLine($"{item.Symbol} - {Math.Round(item.CurrentDayVolatility, 4) * 100}%({ difference}%)");
                        }
                    }
                }
                input = Console.ReadLine();
            }
            await Task.CompletedTask;
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
            _day1CSV = csvParser.ReadFromFile("Day1.csv", Encoding.ASCII).ToList().Select(n => n.Result);

            if (!GetDate("Day2"))
            {
                return false;
            }
            _day2CSV = csvParser.ReadFromFile("Day2.csv", Encoding.ASCII).ToList().Select(n => n.Result);
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
