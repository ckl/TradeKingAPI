using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeKing.API.Models.Auth;
using TradeKing.API.Models.Streaming;

namespace TradeKing.API.Interfaces
{
    public interface IDbSource : IDisposable
    {
        OAuthKeys GetOAuthKeys();
        List<string> GetTickers();
        void AddTicker(string ticker, string exchange = null);
        void DeleteTicker(string ticker);
        void SaveStreamQuote(Quote quote);
        void SaveStreamTrade(Trade trade);
        IEnumerable<Quote> GetStreamQuotes(string ticker);
        IEnumerable<Quote> GetAllStreamQuotes();
        IEnumerable<Trade> GetAllStreamTrades();
    }
}
