using System.Collections.Generic;

using Newtonsoft.Json;

namespace Softengi.UbClient.Sessions
{
	internal class UbIpAuthentication : AuthenticationBase
	{
		internal UbIpAuthentication(string login)
		{
			_login = login;
		}

		internal override void Authenticate(UbTransport transport)
		{
			var requestHeaders = new Dictionary<string, string> {{"Authorization", $"UBIP {_login}"}};
			var resp = transport.Get<UbIpAuthResponse>("auth", null, requestHeaders, false);
			_login = resp.LogonName;
		}

		internal override string AuthHeader()
		{
			return "UBIP " + _login;
		}

		private string _login;

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