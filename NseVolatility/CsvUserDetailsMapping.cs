using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyCsvParser.Mapping;

namespace NseVolatility
{
    public class CsvUserDetailsMapping : CsvMapping<DailyVolatility>
    {
        public CsvUserDetailsMapping()
            : base()
        {
            MapProperty(0, x => x.Date);
            MapProperty(1, x => x.Symbol);
            MapProperty(5, x => x.PreviousDayVolatility);
            MapProperty(6, x => x.CurrentDayVolatility);
        }
    }
    public class DailyVolatility
    {
        public string Date { get; set; }
        public string Symbol { get; set; }
        public double PreviousDayVolatility { get; set; }
        public double CurrentDayVolatility { get; set; }
    }
}
