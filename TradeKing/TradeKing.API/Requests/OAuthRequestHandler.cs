using Newtonsoft.Json;
using OAuth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TradeKing.API.Base.Responses;
using TradeKing.API.Database;
using TradeKing.API.Interfaces;
using TradeKing.API.Models.Auth;
using TradeKing.API.Models.Responses;

namespace TradeKing.API.Requests
{
    public class OAuthRequestHandler
    {
        private OAuthKeys _oauthKeys;

        public OAuthRequestHandler()
        {
            using (var db = DbFactory.GetDbSource())
            {
                Console.WriteLine("Reading OAuth keys from SQLite...");
                _oauthKeys = db.GetOAuthKeys();

                if (_oauthKeys == null)
                {
                    throw new NullReferenceException("No OAuth Keys found");
                }
            }
        }

        public async Task<T> ExecuteRequest<T>(string url, string method="GET")
        {
            var client = new OAuthRequest
            {
                Method = method,
                Type = OAuthRequestType.RequestToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ConsumerKey = _oauthKeys.ConsumerKey,
                ConsumerSecret = _oauthKeys.ConsumerSecret,
                RequestUrl = $"https://api.tradeking.com/v1/" + url,
                Version = "1.0a",
                Token = _oauthKeys.Token,
                TokenSecret = _oauthKeys.TokenSecret
            };

            var request = (HttpWebRequest)WebRequest.Create(client.RequestUrl);
            request.Headers.Add("Authorization", client.GetAuthorizationHeader());
            var response = await request.GetResponseAsync();

            var responseStream = response.GetResponseStream();
            T baseResponse = default(T);

            if (responseStream == null)
            {
                throw new ArgumentNullException("ResponseStream");
            }

            var streamReader = new StreamReader(responseStream, Encoding.UTF8);

            var data = await streamReader.ReadToEndAsync();
            baseResponse = JsonConvert.DeserializeObject<T>(data);

            return baseResponse;
        }

        public async Task<WebResponse> ExecuteStreamRequest<T>(string url, string method="GET")
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

            var request = (HttpWebRequest)WebRequest.Create(client.RequestUrl);
            request.Headers.Add("Authorization", client.GetAuthorizationHeader());
            var response = await request.GetResponseAsync();

            return response;
        }
    }
}
