using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TradeKing.API.Base.Responses;
using TradeKing.API.Models.Streaming;

namespace TradeKing.API.Requests
{
    public class QuoteStreamRequest
    {
        private int _bufferSize = 1024;
        private OAuthRequestHandler _requestHandler;
        private bool _retry = true;

        private WebResponse _response;
        private StreamReader _streamReader;
        private List<string> _tickers;

        public QuoteStreamRequest(List<string> tickers)
        {
            _requestHandler = new OAuthRequestHandler();
            _tickers = tickers;
        }

        public async Task Execute(Action<List<StreamDataItem>> callback)
        {
            var url = "market/quotes.json?symbols=" + string.Join(",", _tickers);

            _response = await _requestHandler.ExecuteStreamRequest<HttpWebResponse>(url);

            var responseStream = _response.GetResponseStream();

            if (responseStream != null)
            {
                _streamReader = new StreamReader(responseStream, Encoding.UTF8);
                var read = new char[_bufferSize];
                string str = null;

                while (!_streamReader.EndOfStream)
                {
                    var count = _streamReader.Read(read, 0, _bufferSize);
                    str = new string(read, 0, count);

                    BaseResponse item = null;
                    var streamItems = new List<StreamDataItem>();

                    var datas = str.Split(new string[] { "}}" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var data in datas)
                    {
                        try {
                            var json = data + "}}";
                            if (json.Contains("quote"))
                            {
                                item = JsonConvert.DeserializeObject<QuoteResponse>(json);
                                streamItems.Add(((QuoteResponse)item).Response);
                            }
                            else if (json.Contains("trade"))
                            {
                                item = JsonConvert.DeserializeObject<TradeResponse>(json);
                                streamItems.Add(((TradeResponse)item).Response);
                            }
                        }
                        catch (JsonReaderException ex)
                        {
                            var ex2 = ex;
                        }
                    }

                    if (streamItems.Count > 0)
                    {
                        callback(streamItems);
                    }
                }

                CloseStream();
                Console.WriteLine("Releasing stream resources...");
            }
            else {
                throw new Exception("No response was returned from the server.");
            }
           
        }

        public void CloseStream()
        {
            _retry = false;

            if (_response != null)
            {
                _response.Close();
                _response.Dispose();
                _response = null;
            }

            if (_streamReader != null)
            {
                _streamReader.Close();
                _streamReader.Dispose();
                _streamReader = null;
            }
        }
    }
}
