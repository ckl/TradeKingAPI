using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeKingAPI.Models.Auth;
using TradeKingAPI.Models.Streaming;

namespace TradeKingAPI.Interfaces
{
    public interface IDbSource : IDisposable
    {
        OAuthKeys GetOAuthKeys();
        List<string> GetTickers();
        void AddTicker(string ticker, string exchange = null);
        void DeleteTicker(string ticker);
        void SaveStreamQuote(Quote quote);
        void SaveStreamTrade(Trade trade);
        IEnumerable<Quote> GetAllStreamQuotes();
        IEnumerable<Trade> GetAllStreamTrades();
    }
}
