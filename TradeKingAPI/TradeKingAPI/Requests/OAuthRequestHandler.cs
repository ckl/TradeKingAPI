using OAuth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TradeKingAPI.Database;
using TradeKingAPI.Models.Auth;
using TradeKingAPI.Models.Responses;

namespace TradeKingAPI.Requests
{
    public class OAuthRequestHandler
    {
        private OAuthKeys _oauthKeys;

        public OAuthRequestHandler()
        {
            using (var sqlite = new SqliteWrapper())
            {
                Console.WriteLine("Reading OAuth keys from SQLite...");
                _oauthKeys = sqlite.GetOAuthKeys();
            }
        }

        public async Task<WebResponse> ExecuteRequest<T>(string url, List<string> parameters, string method="GET")
        {
            var client = new OAuthRequest
            {
                Method = method,
                Type = OAuthRequestType.RequestToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ConsumerKey = _oauthKeys.ConsumerKey,
                ConsumerSecret = _oauthKeys.ConsumerSecret,
                RequestUrl = $"https://stream.tradeking.com/v1/" + url,
                Version = "1.0a",
                Token = _oauthKeys.Token,
                TokenSecret = _oauthKeys.TokenSecret
            };

            Console.WriteLine("{0} {1}", method, url);
            var request = (HttpWebRequest)WebRequest.Create(client.RequestUrl);
            request.Headers.Add("Authorization", client.GetAuthorizationHeader());
            var response = await request.GetResponseAsync();

            return response;
        }
    }
}
