using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private bool _keepProcessing = true;
        private bool _isClosed = false;

        private WebResponse _response;
        private StreamReader _streamReader;
        private List<string> _tickers;
        private StringBuilder _stringBuilder;
        private object _lock = new object();

        public QuoteStreamRequest(List<string> tickers)
        {
            _requestHandler = new OAuthRequestHandler();
            _tickers = tickers;
            _stringBuilder = new StringBuilder();
        }

        public async Task Execute(Action<List<StreamDataItem>> callback)
        {
            var url = "market/quotes.xml?symbols=" + string.Join(",", _tickers);

            //try
            //{
                _response = await _requestHandler.ExecuteStreamRequest<HttpWebResponse>(url);

                var responseStream = _response.GetResponseStream();

                if (responseStream == null)
                {
                    throw new Exception("No response was returned from the server.");
                }

                _streamReader = new StreamReader(responseStream, Encoding.UTF8);
                var read = new char[_bufferSize];

                var count = _streamReader.Read(read, 0, _bufferSize);
                while (count > 0)
                {
                    if (!_keepProcessing)
                    {
                        Console.WriteLine("Releasing stream resources...");

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

                        break;
                    }

                    var str = new string(read, 0, count).Replace("<status>connected</status>", "");
                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        _stringBuilder.Append(str);
                        var streamData = ProcessStringBuilder();

                        callback(streamData);
                    }
                    count = _streamReader.Read(read, 0, _bufferSize);
                }


                Console.WriteLine("Releasing stream resources...");
                CloseStream();

            //}
            //catch (IOException)
            //{
            //    // carry on
            //    Console.WriteLine("Caught IOException");
            //    await Execute(callback);
            //}
            //catch (ObjectDisposedException ex)
            //{
            //    Console.WriteLine("Caught ObjectDisposedException: " + ex.Message);
            //    await Execute(callback);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Caught Exception: {0} [{1}", ex.Message, ex.StackTrace);
            //    Console.WriteLine(obj);
            //    await Execute(callback);
            //}
        }

        private List<StreamDataItem> ProcessStringBuilder()
        {
            var streamData = new List<StreamDataItem>();
            var str = _stringBuilder.ToString();
            var tradeOpenIndex = str.IndexOf("<trade>", StringComparison.Ordinal);
            var tradeCloseIndex = str.IndexOf("</trade>", StringComparison.Ordinal);

            while (tradeOpenIndex != -1 && tradeCloseIndex != -1)
            {
                var trade = str.Substring(tradeOpenIndex + 7, tradeCloseIndex - (tradeOpenIndex + 7)).Trim();
                var cvol = GetProperty(trade, "cvol");
                var datetime = GetProperty(trade, "datetime");
                var exch = GetProperty(trade, "exch");
                var last = GetProperty(trade, "last");
                var symbol = GetProperty(trade, "symbol");
                var timestamp = GetProperty(trade, "timestamp");
                var vl = GetProperty(trade, "vl");
                var vwap = GetProperty(trade, "vwap");

                _stringBuilder = _stringBuilder.Remove(tradeOpenIndex, tradeCloseIndex + 8 - tradeOpenIndex);

                str = _stringBuilder.ToString();
                tradeOpenIndex = str.IndexOf("<trade>", StringComparison.Ordinal);
                tradeCloseIndex = str.IndexOf("</trade>", StringComparison.Ordinal);

                streamData.Add(new Trade
                {
                    Datetime = datetime,
                    Symbol = symbol,
                    Timestamp = timestamp,
                    Cvol = cvol,
                    Last = last,
                    Vl = vl,
                    Vwap = vwap
                });
            }

            str = _stringBuilder.ToString();
            var quoteOpenIndex = str.IndexOf("<quote>", StringComparison.Ordinal);
            var quoteCloseIndex = str.IndexOf("</quote>", StringComparison.Ordinal);

            while (quoteOpenIndex != -1 && quoteCloseIndex != -1)
            {
                var quote = str.Substring(quoteOpenIndex + 7, quoteCloseIndex - (quoteOpenIndex + 7)).Trim();
                var ask = GetProperty(quote, "ask");
                var asksz = GetProperty(quote, "asksz");
                var bid = GetProperty(quote, "bid");
                var bidsz = GetProperty(quote, "bidsz");
                var datetime = GetProperty(quote, "datetime");
                var exch = GetProperty(quote, "exch");
                var qcond = GetProperty(quote, "qcond");
                var symbol = GetProperty(quote, "symbol");
                var timestamp = GetProperty(quote, "timestamp");


                _stringBuilder = _stringBuilder.Remove(quoteOpenIndex, quoteCloseIndex + 8 - quoteOpenIndex);

                str = _stringBuilder.ToString();
                quoteOpenIndex = str.IndexOf("<quote>", StringComparison.Ordinal);
                quoteCloseIndex = str.IndexOf("</quote>", StringComparison.Ordinal);

                streamData.Add(new Quote
                {
                    Datetime = datetime,
                    Symbol = symbol,
                    Timestamp = timestamp,
                    Ask = ask,
                    Asksz = asksz,
                    Bid = bid,
                    Bidsz = bidsz,
                    Qcond = qcond
                });
            }

            return streamData;
        }

        private string GetProperty(string element, string tag)
        {
            element = element.Trim();
            return element.Substring(element.IndexOf("<" + tag + ">", StringComparison.Ordinal) + tag.Length + 2, element.IndexOf("</" + tag + ">", StringComparison.Ordinal) - (element.IndexOf("<" + tag + ">", StringComparison.Ordinal) + tag.Length + 2));
        }

        public void CloseStream()
        {
            lock (_lock)
            {
                if (_isClosed)
                    return;

                _keepProcessing = false;

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

                _tickers = null;
                _isClosed = true;
            }
        }
    }
}
