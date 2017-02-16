using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeKing.API.Models.Auth;

namespace TradeKingAPI.Helpers
{
    public class OAuthKeyManager
    {
        private static readonly Lazy<OAuthKeyManager> _mySingleton = new Lazy<OAuthKeyManager>(() => new OAuthKeyManager());

        public OAuthKeys OAuthKeys { get; set; }

        private OAuthKeyManager() { }

        public static OAuthKeyManager Instance
        {
            get
            {
                return _mySingleton.Value;
            }
        }
    }
}
