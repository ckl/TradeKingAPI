using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeKingAPI.Interfaces;

namespace TradeKingAPI.Database
{
    public static class DbFactory
    {
        public static IDbSource GetDbSource()
        {
            return new SqliteWrapper();
        }
    }
}
