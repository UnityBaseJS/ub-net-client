using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Softengi.UbClient.Sessions
{
	internal class UbNegotiateAuthenticationMethod : UbAuthenticationMethodBase
	{
		internal override UbSession Authenticate(UbTransport transport)
		{
			var queryStringParams =
				new Dictionary<string, string>
				{
					{"AUTHTYPE", "Negotiate"},
					{"userName", string.Empty}
				};
			var resp = transport.Get<NegotiateAuthResponse>("auth", queryStringParams, null, true);
			return new UbNegotiateSession(resp.LogonName, resp.SessionID);
		}

		public class NegotiateAuthResponse
		{
			[JsonProperty("result")]
			public string SessionID { get; set; }

			[JsonProperty("logonname")]
			public string LogonName { get; set; }
		}
	}

	internal class UbAuthenticationMethod : UbAuthenticationMethodBase
	{
		internal UbAuthenticationMethod(string login, string password, string appName)
		{
			_login = login;
			_password = password;
			_appName = appName;
		}

		internal override UbSession Authenticate(UbTransport transport)
		{
			var firstQueryString =
				new Dictionary<string, string>
				{
					{"AUTHTYPE", "UB"},
					{"userName", _login},
					{"password", string.Empty}
				};
			var firstResponse = transport.Get<UbHandShakeAuthResponse>("auth", firstQueryString, null, false);

			var clientNonce = CryptoHelper.Nsha256(DateTime.UtcNow.ToString("o").Substring(0, 16));
			var serverNonce = firstResponse.Result;
			if (string.IsNullOrEmpty(serverNonce))
				throw new UbException("No server nonce.");

			var pwdHash = CryptoHelper.Nsha256("salt" + _password);
			var secretWord = pwdHash;
			var pwdForAuth = CryptoHelper.Nsha256(_appName + serverNonce + clientNonce + _login + pwdHash);

			var secondQueryString =
				new Dictionary<string, string>
				{
					{"AUTHTYPE", "UB"},
					{"userName", _login},
					{"password", pwdForAuth},
					{"clientNonce", clientNonce}
				};
			if (firstResponse.ConnectionID != null)
				secondQueryString.Add("connectionID", firstResponse.ConnectionID);

			var secondResonse = transport.Get<UbAuthSecondResponse>("auth", secondQueryString, null, true);

			return new UbSession(UbAuthSchema.UB, secondResonse.LogonName, secretWord, secondResonse.SessionID);
		}

		private readonly string _login;
		private readonly string _password;
		private readonly string _appName;

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