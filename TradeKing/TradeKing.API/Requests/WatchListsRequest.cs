using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeKing.API.Models.Responses;
using TradeKing.API.Models.WatchList;

namespace TradeKing.API.Requests
{
    public class WatchListsRequest
    {
        private OAuthRequestHandler _requestHandler;

        public WatchListsRequest()
        {
            _requestHandler = new OAuthRequestHandler();
        }

        public async Task<IEnumerable<Models.WatchLists.Watchlist>> Execute()
        {
            var url = "watchlists.json";
            var resp = await _requestHandler.ExecuteRequest<WatchListsResponse>(url);
            return resp.Response.Watchlists.Watchlist;
        }
    }
}
