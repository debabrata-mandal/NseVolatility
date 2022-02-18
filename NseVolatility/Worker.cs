using ConsoleTables;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
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
        private IEnumerable<DailyVolatility> volatilityData;
        public Worker() { }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string input = "X";
            while (!stoppingToken.IsCancellationRequested)
            {
                if (string.Equals(input, "X", StringComparison.OrdinalIgnoreCase))
                {
                    if (!GetDate())
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
                        DisplayResult(results, volatilityData.FirstOrDefault().Date);
                    }
                }
                input = Console.ReadLine();
            }
            await Task.CompletedTask;
        }
        private static void DisplayResult(IEnumerable<Result> results, string date)
        {
            ColorConsole.WriteWrappedHeader($"NSE data for {date}");

            ConsoleTable
            .From(results)
            .Configure(o => o.NumberAlignment = Alignment.Right).Write(Format.Alternative);
        }

        private bool GetDate()
        {
            CsvParserOptions csvParserOptions = new(true, ',');
            CsvUserDetailsMapping csvMapper = new();
            CsvParser<DailyVolatility> csvParser = new(csvParserOptions, csvMapper);

            DateTime today = DateTime.Now;
            bool isDatePredicted = false;
            int maxTry = 30;
            while (!isDatePredicted && maxTry-- > 0)
            {
                try
                {
                    string date = today.ToString("ddMMyyyy");
                    string json = (new WebClient()).DownloadString($"https://www1.nseindia.com/archives/nsccl/volt/FOVOLT_{date}.csv");
                    volatilityData = csvParser.ReadFromFile($"{date}.csv", Encoding.ASCII).ToList().Select(n => n.Result);
                    return true;
                }
                catch
                {
                    today = today.AddDays(-1);
                }
            }
            return false;
        }
    }
}
