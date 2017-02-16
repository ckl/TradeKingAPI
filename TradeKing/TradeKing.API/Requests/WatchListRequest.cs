using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeKing.API.Models.Responses;
using TradeKing.API.Models.WatchList;
using TradeKing.API.Requests;

namespace TradeKing.API.Requests
{
    public class WatchListRequest
    {
        private string _watchListId;
        private OAuthRequestHandler _requestHandler;

        public WatchListRequest(string watchListId)
        {
            _watchListId = watchListId;
            _requestHandler = new OAuthRequestHandler();
        }

        public async Task<Watchlists> Execute()
        {
            var url = string.Format("watchlists/{0}.json", _watchListId);
            var resp = await _requestHandler.ExecuteRequest<WatchListResponse>(url);

            return resp.Response.Watchlists;
        }
    }
}
