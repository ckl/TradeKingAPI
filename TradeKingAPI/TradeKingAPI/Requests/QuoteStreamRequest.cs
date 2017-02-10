﻿using Newtonsoft.Json;
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

        public QuoteStreamRequest()
        {
            _requestHandler = new OAuthRequestHandler();
        }

        public async void Execute(Action<List<StreamDataItem>> callback)
        {
            _response = await _requestHandler.ExecuteRequest<HttpWebResponse>("market/quotes.json?symbols=GDQMF,AIRRF,CANWF,SSPXF", null);
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
                    _response.Close();
                    _response.Dispose();
                    _streamReader.Close();
                    _streamReader.Dispose();
                    _response = null;

                    if (_retry)
                    {
                        Thread.Sleep(1000);
                        Execute(callback);
                    }
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
