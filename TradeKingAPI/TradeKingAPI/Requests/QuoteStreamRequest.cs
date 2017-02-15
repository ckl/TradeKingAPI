using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradeKingAPI.Base.Responses;
using TradeKingAPI.Models.Streaming;

namespace TradeKingAPI.Requests
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

        public async void Execute(Action<List<StreamDataItem>> callback)
        {
            var url = "market/quotes.json?symbols=" + string.Join(",", _tickers);

            try
            {
                _response = await _requestHandler.ExecuteStreamRequest<HttpWebResponse>(url);
            }
            catch (WebException ex)
            {
                Console.WriteLine(DateTime.Now + ": Web exception: " + ex.Message);
                DoRetry(5000, callback);
                return;
            }

            var responseStream = _response.GetResponseStream();

            if (responseStream != null)
            {
                _streamReader = new StreamReader(responseStream, Encoding.UTF8);
                var read = new char[_bufferSize];
                string str = null;

                try
                {
                    while (!_streamReader.EndOfStream)
                    {
                        var count = _streamReader.Read(read, 0, _bufferSize);
                        str = new string(read, 0, count);

                        BaseResponse item = null;
                        var streamItems = new List<StreamDataItem>();

                        var datas = str.Split(new string[] { "}}" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var data in datas)
                        {
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

                        if (streamItems.Count > 0)
                        {
                            callback(streamItems);
                        }
                    }

                    _response.Close();
                    _response.Dispose();
                    _streamReader.Close();
                    _streamReader.Dispose();

                    Console.WriteLine("Stream is closed");
                }
                catch (IOException ex)
                {
                    // TK API closed the connection, cleanup and retry in 1 second
                    DoRetry(1000, callback);
                }
                catch (JsonReaderException ex)
                {
                    Console.WriteLine("Error reading JSON. Message: {0}\n{1}", ex.Message, str);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unhandled exception: {0} [{1}]", ex.Message, ex.GetType().ToString());
                }
            }
            else
                throw new Exception("No response was returned from the server.");
        }

        private void DoRetry(int msDelay, Action<List<StreamDataItem>> callback)
        {
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
            

            if (_retry)
            {
                Thread.Sleep(msDelay);
                Execute(callback);
            }
        }

        public void Close()
        {
            Console.WriteLine("Releasing stream resources...");
            _retry = false;

            _response.Close();
            _response.Dispose();
            _streamReader.Close();
            _streamReader.Dispose();
        }
    }
}
