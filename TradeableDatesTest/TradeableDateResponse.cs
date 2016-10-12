using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeableDatesTest
{
    public class TradeableDateResponse
    {
        public int TradeableTotal { get; set; }
        public List<string> TradeableDates { get; set; }
        public bool Success { get; set; }
    }
}
