using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NseVolatility
{
    public class Worker : BackgroundService
    {
        private string _day1CSV;
        private string _day2CSV;

        public Worker()
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var input = "X";
            while (!stoppingToken.IsCancellationRequested)
            {
                if (string.Equals(input, "X", StringComparison.OrdinalIgnoreCase))
                {
                    if (!GetDates())
                    {
                        Console.WriteLine("Dates are not valid please try again");
                        continue;
                    }

                    Console.WriteLine($"{_day1CSV}{_day2CSV}");
                }
                input = Console.ReadLine();
            }

            await Task.CompletedTask;
        }

        private bool GetDates()
        {
            if (!GetDate("Day1"))
            {
                return false;
            }
            _day1CSV = "Day1.csv";
            if (!GetDate("Day2"))
            {
                return false;
            }
            _day2CSV = "Day2.csv";
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
