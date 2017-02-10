﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeKingAPI.Models.Auth;
using TradeKingAPI.Models.Streaming;

namespace TradeKingAPI.Database
{
    public class SqliteWrapper : IDisposable
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

            _dbConnection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", _sqliteDbPath));
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
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    reader.Read();

                    var keys = new OAuthKeys
                    {
                        ConsumerKey = reader["ConsumerKey"].ToString(),
                        ConsumerSecret = reader["ConsumerSecret"].ToString(),
                        Token = reader["Token"].ToString(),
                        TokenSecret = reader["TokenSecret"].ToString(),
                    };

                    return keys;
                }
            }
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

            using (SQLiteCommand command = new SQLiteCommand(sql, _dbConnection))
            {
                command.ExecuteNonQuery();
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

            using (SQLiteCommand command = new SQLiteCommand(sql, _dbConnection))
            {
                command.ExecuteNonQuery();
            }
        }

        private void CreateOAuthKeyTable()
        {
            string sql = @"CREATE TABLE IF NOT EXISTS OAuthKeys (
                                ConsumerKey VARCHAR(64),
                                ConsumerSecret VARCHAR(64),
                                Token VARCHAR(64),
                                TokenSecret VARCHAR(64)
                             )";

            using (SQLiteCommand command = new SQLiteCommand(sql, _dbConnection))
            {
                command.ExecuteNonQuery();
            }
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

            using (SQLiteCommand command = new SQLiteCommand(sql, _dbConnection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
