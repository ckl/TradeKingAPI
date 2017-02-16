using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TradeKing.API.Models.Article;
using TradeKing.API.Models.Responses;

namespace TradeKing.API.Requests
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
