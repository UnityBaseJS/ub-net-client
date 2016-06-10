using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Softengi.UbClient.Sessions
{
	internal class UbAuthentication : AuthenticationBase
	{
		internal UbAuthentication(string login, string password, string appName)
		{
			_login = login;
			_passwordHash = Crypto.Nsha256("salt" + password);
			_appName = appName;
		}

		internal override void Authenticate(UbTransport transport)
		{
			var firstQueryString =
				new Dictionary<string, string>
				{
					{"AUTHTYPE", "UB"},
					{"userName", _login},
					{"password", string.Empty}
				};
			var firstResponse = transport.Get<UbHandShakeAuthResponse>("auth", firstQueryString, null, false);

			var clientNonce = Crypto.Nsha256(DateTime.UtcNow.ToString("o").Substring(0, 16));
			var serverNonce = firstResponse.Result;
			if (string.IsNullOrEmpty(serverNonce))
				throw new UbException("No server nonce.");

			var secondQueryString =
				new Dictionary<string, string>
				{
					{"AUTHTYPE", "UB"},
					{"userName", _login},
					{"password", Crypto.Nsha256(_appName + serverNonce + clientNonce + _login + _passwordHash)},
					{"clientNonce", clientNonce}
				};
			if (firstResponse.ConnectionID != null)
				secondQueryString.Add("connectionID", firstResponse.ConnectionID);

			var secondResonse = transport.Get<UbAuthSecondResponse>("auth", secondQueryString, null, false);
			_sessionID = Crypto.Hexa8(secondResonse.SessionID.Split('+')[0]);
			_secretWord = secondResonse.SessionID;
		}

		internal override string AuthHeader()
		{
			return "UB " + Crypto.Signature(_sessionID, _secretWord, _passwordHash);
		}

		private readonly string _login;
		private readonly string _appName;
		private readonly string _passwordHash;
		private string _sessionID;
		private string _secretWord;

		public class UbHandShakeAuthResponse
		{
			[JsonProperty("result")]
			public string Result { get; set; }

			[JsonProperty("connectionID")]
			public string ConnectionID { get; set; }
		}

		public class UbAuthSecondResponse
		{
			[JsonProperty("result")]
			public string SessionID { get; set; }

			[JsonProperty("logonname")]
			public string LogonName { get; set; }
		}
	}
}