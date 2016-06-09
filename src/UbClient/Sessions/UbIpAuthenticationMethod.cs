using System.Collections.Generic;

using Newtonsoft.Json;

namespace Softengi.UbClient.Sessions
{
	internal class UbIpAuthenticationMethod : UbAuthenticationMethodBase
	{
		internal UbIpAuthenticationMethod(string login)
		{
			_login = login;
		}

		internal override UbSession Authenticate(UbTransport transport)
		{
			var requestHeaders = new Dictionary<string, string> {{"Authorization", $"UBIP {_login}"}};
			var resp = transport.Get<UbIpAuthResponse>("auth", null, requestHeaders, false);
			return new UbIpSession(resp.LogonName, resp.SecretWord, resp.SessionID);
		}

		private readonly string _login;

		public class UbIpAuthResponse
		{
			[JsonProperty("result")]
			public string SessionID { get; set; }

			[JsonProperty("logonname")]
			public string LogonName { get; set; }

			[JsonProperty("secretWord")]
			public string SecretWord { get; set; }
		}
	}
}