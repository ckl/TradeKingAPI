using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TradeKingAPI.Models.Article;
using TradeKingAPI.Models.Responses;

namespace TradeKingAPI.Requests
{
    public class NewsRequest
    {
        private List<string> _tickers;
        private OAuthRequestHandler _requestHandler;

        public NewsRequest(List<string> tickers)
        {
            _tickers = tickers;
            _requestHandler = new OAuthRequestHandler();
        }

        public async Task<IEnumerable<Article>> Execute(int maxhits=5)
        {
            var url = "market/news/search.json?symbols=" + string.Join(",", _tickers) + "&maxhits=" + maxhits.ToString();
            var resp = await _requestHandler.ExecuteRequest<ArticleResponse>(url);

            return resp.Response.Articles.Article.ToList();
        }
    }
}
