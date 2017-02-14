using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeKingAPI.Models.Responses;
using TradeKingAPI.Models.WatchList;

namespace TradeKingAPI.Requests
{
    public class WatchListRequest
    {
        private OAuthRequestHandler _requestHandler;

        public WatchListRequest()
        {
            _requestHandler = new OAuthRequestHandler();
        }

        public async Task<Watchlists> Execute()
        {
            var url = "market/watchlist.json";
            var resp = await _requestHandler.ExecuteRequest<WatchListResponse>(url);

            return resp.Response.Watchlists;
        }
    }
}
