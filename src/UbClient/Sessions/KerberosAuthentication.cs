using System.Collections.Generic;

using Newtonsoft.Json;

namespace Softengi.UbClient.Sessions
{
	internal class KerberosAuthentication : AuthenticationBase
	{
		internal KerberosAuthentication(string password)
		{
			_passwordHash = Crypto.Nsha256("salt" + password);
		}

		internal override void Authenticate(UbTransport transport)
		{
			var queryStringParams =
				new Dictionary<string, string>
				{
					{"AUTHTYPE", "Negotiate"},
					{"userName", string.Empty}
				};
			var resp = transport.Get<NegotiateAuthResponse>("auth", queryStringParams, null, true);
			_sessionID = Crypto.Hexa8(resp.SessionID.Split('+')[0]);
		}

		internal override string AuthHeader()
		{
			return "Negotiate " + Crypto.Signature(_sessionID, _passwordHash);
		}

		private string _sessionID;
		private readonly string _passwordHash;

		public class NegotiateAuthResponse
		{
			[JsonProperty("result")]
			public string SessionID { get; set; }

			[JsonProperty("logonname")]
			public string LogonName { get; set; }
		}
	}
}