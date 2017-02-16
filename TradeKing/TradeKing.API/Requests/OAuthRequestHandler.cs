using Newtonsoft.Json;
using OAuth;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TradeKing.API.Database;
using TradeKingAPI.Helpers;

namespace TradeKing.API.Requests
{
    public class OAuthRequestHandler
    {
        public OAuthRequestHandler()
        {
            using (var db = DbFactory.GetDbSource())
            {
                if (OAuthKeyManager.Instance.OAuthKeys == null)
                {
                    Console.WriteLine("Reading OAuth keys from SQLite...");
                    OAuthKeyManager.Instance.OAuthKeys = db.GetOAuthKeys();
                }

                if (OAuthKeyManager.Instance.OAuthKeys == null)
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
                ConsumerKey = OAuthKeyManager.Instance.OAuthKeys.ConsumerKey,
                ConsumerSecret = OAuthKeyManager.Instance.OAuthKeys.ConsumerSecret,
                RequestUrl = $"https://api.tradeking.com/v1/" + url,
                Version = "1.0a",
                Token = OAuthKeyManager.Instance.OAuthKeys.Token,
                TokenSecret = OAuthKeyManager.Instance.OAuthKeys.TokenSecret
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
                ConsumerKey = OAuthKeyManager.Instance.OAuthKeys.ConsumerKey,
                ConsumerSecret = OAuthKeyManager.Instance.OAuthKeys.ConsumerSecret,
                RequestUrl = $"https://stream.tradeking.com/v1/" + url,
                Version = "1.0a",
                Token = OAuthKeyManager.Instance.OAuthKeys.Token,
                TokenSecret = OAuthKeyManager.Instance.OAuthKeys.TokenSecret
            };

            var request = (HttpWebRequest)WebRequest.Create(client.RequestUrl);
            request.Headers.Add("Authorization", client.GetAuthorizationHeader());
            var response = await request.GetResponseAsync();

            return response;
        }
    }
}
