using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeKingAPI.Interfaces;
using TradeKingAPI.Models.Auth;
using TradeKingAPI.Models.Streaming;

namespace TradeKingAPI.Database
{
    public class SqliteWrapper : IDisposable, IDbSource
    {
        private string _dbDirectoryPath = @"C:\Users\" + Environment.UserName + @"\Documents\TradeKingAPI\";
        private string _sqliteDbName = "TradeKingAPI.sqlite";
        private string _sqliteDbPath;
        SQLiteConnection _dbConnection;
        private bool _disposed = false;

        public SqliteWrapper()
        {
            bool createInitialTables = false;
            _sqliteDbPath = _dbDirectoryPath + _sqliteDbName;

            if (! Directory.Exists(_dbDirectoryPath))
            {
                Directory.CreateDirectory(_dbDirectoryPath);
            }

            if (!File.Exists(_sqliteDbPath))
            {
                SQLiteConnection.CreateFile(_dbDirectoryPath + _sqliteDbName);
                createInitialTables = true;
            }

            _dbConnection = new SQLiteConnection(string.Format("Data Source={0};Version=3;datetimeformat=CurrentCulture", _sqliteDbPath));
            _dbConnection.Open();
            _disposed = false;

            if (createInitialTables)
            {
                CreateOAuthKeyTable();
                CreateStreamDataTable();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dbConnection.Close();
                    _dbConnection.Dispose();
                }

                _disposed = true;
            }
        }

        public OAuthKeys GetOAuthKeys()
        {
            string sql = "SELECT * FROM OAuthKeys";
            using (SQLiteCommand command = new SQLiteCommand(sql, _dbConnection))
            {
                var keys = new List<OAuthKeys>();
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var key = new OAuthKeys
                        {
                            ConsumerKey = reader["ConsumerKey"].ToString(),
                            ConsumerSecret = reader["ConsumerSecret"].ToString(),
                            Token = reader["Token"].ToString(),
                            TokenSecret = reader["TokenSecret"].ToString(),
                        };

                        keys.Add(key);
                    }

                    if (keys.Count == 0)
                    {
                        // TODO
                        return null;
                    }
                    else if (keys.Count == 1)
                    {
                        return keys[0];
                    }
                    else
                    {
                        // TODO: return all keys or display all and allow user to select?
                        return keys[0];
                    }
                }
            }
        }

        public List<string> GetTickers()
        {
            string sql = "SELECT * FROM Tickers ORDER BY Ticker";
            using (SQLiteCommand command = new SQLiteCommand(sql, _dbConnection))
            {
                var tickers = new List<string>();
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tickers.Add(reader["Ticker"].ToString());
                    }
                }

                return tickers;
            }
        }

        public void AddTicker(string ticker, string exchange=null)
        {
            if (string.IsNullOrEmpty(exchange))
                exchange = "";

            string sql = string.Format("INSERT INTO Tickers (Ticker, Exchange) VALUES ('{0}', '{1}')", ticker, exchange);

            ExecuteNonQuery(sql);
        }

        public void DeleteTicker(string ticker)
        {
            string sql = string.Format("DELETE FROM Tickers WHERE Ticker = '{0}'", ticker);

            ExecuteNonQuery(sql);
        }

        public void SaveStreamQuote(Quote quote)
        {
            string sql = string.Format("INSERT INTO StreamData (Ask, AskSize, Bid, BidSize, DateTime, QuoteCondition, Ticker, TimeStamp, DataType) VALUES " +
                                        "({0}, {1}, {2}, {3}, '{4}', '{5}', '{6}', {7}, '{8}')",
                                        Convert.ToDecimal(quote.Ask),
                                        Convert.ToInt32(quote.Asksz),
                                        Convert.ToDecimal(quote.Bid),
                                        Convert.ToInt32(quote.Bidsz),
                                        DateTime.Parse(quote.Datetime),
                                        quote.Qcond, quote.Symbol, 
                                        Convert.ToInt32(quote.Timestamp),
                                        "Quote"
                                        );

            ExecuteNonQuery(sql);
        }

        public IEnumerable<Quote> GetAllStreamQuotes()
        {
            var quotes = new List<Quote>();
            string sql = "SELECT * FROM StreamData WHERE DataType = 'Quote' ORDER BY TimeStamp";

            using (SQLiteCommand command = new SQLiteCommand(sql, _dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        quotes.Add(new Quote
                        {
                            Ask = reader["Ask"].ToString(),
                            Asksz = reader["AskSize"].ToString(),
                            Bid = reader["Bid"].ToString(),
                            Bidsz = reader["BidSize"].ToString(),
                            Datetime = reader["DateTime"].ToString(),
                            Qcond = reader["QuoteCondition"].ToString(),
                            Symbol = reader["Ticker"].ToString()
                        });
                    }
                }

                return quotes;
            }
        }

        public IEnumerable<Trade> GetAllStreamTrades()
        {
            var trades = new List<Trade>();

            string sql = "SELECT * FROM StreamData WHERE DataType = 'Trade' ORDER BY TimeStamp";

            using (SQLiteCommand command = new SQLiteCommand(sql, _dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        trades.Add(new Trade
                        {
                            Cvol = reader["CumulativeVolume"].ToString(),
                            Datetime = reader["DateTime"].ToString(),
                            Last = reader["LastPrice"].ToString(),
                            Symbol = reader["Ticker"].ToString(),
                            Timestamp = reader["TimeStamp"].ToString(),
                            Vl = reader["Volume"].ToString(),
                            Vwap = reader["VolumeWeight"].ToString()
                        });
                    }
                }

                return trades;
            }
        }

        public void SaveStreamTrade(Trade trade)
        {
            string sql = string.Format("INSERT INTO StreamData (DateTime, Ticker, TimeStamp, DataType, CumulativeVolume, LastPrice, Volume, VolumeWeight) VALUES " +
                                         "('{0}', '{1}', {2}, '{3}', {4}, {5}, {6}, {7})",
                                         DateTime.Parse(trade.Datetime),
                                         trade.Symbol,
                                         Convert.ToInt32(trade.Timestamp),
                                         "Trade",
                                         Convert.ToInt32(trade.Cvol),
                                         Convert.ToDecimal(trade.Last),
                                         Convert.ToInt32(trade.Vl),
                                         Convert.ToDecimal(trade.Vwap)
                                         );

            ExecuteNonQuery(sql);
        }

        private void CreateOAuthKeyTable()
        {
            string sql = @"CREATE TABLE IF NOT EXISTS OAuthKeys (
                                ConsumerKey VARCHAR(64),
                                ConsumerSecret VARCHAR(64),
                                Token VARCHAR(64),
                                TokenSecret VARCHAR(64)
                             )";

            ExecuteNonQuery(sql);
        }

        private void CreateTickerTable()
        {
            string sql = @"CREATE TABLE IF NOT EXISTS Tickers (
                                Ticker VARCHAR(8),
                                Exchange VARCHAR(32),
                             )";

            ExecuteNonQuery(sql);
        }

        private void CreateStreamDataTable()
        {
            
            string sql = @"CREATE TABLE IF NOT EXISTS StreamData (
                                DataType VARCHAR(5),
                                DateTime DATETIME,
                                Ticker VARCHAR(8), 
                                CompanyName  VARCHAR(32),
                                Exchange VARCHAR(32),
                                TimeStamp INTEGER,
                                Ask REAL,
                                AskSize INTEGER,
                                Bid REAL,
                                BidSize INTEGER,
                                QuoteCondition VARCHAR(16),
                                CumulativeVolume INTEGER,
                                LastPrice REAL,
                                Volume INTEGER,
                                VolumeWeight INTEGER
                             )";

            ExecuteNonQuery(sql);
        }

        private void ExecuteNonQuery(string sql)
        {
            using (SQLiteCommand command = new SQLiteCommand(sql, _dbConnection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
