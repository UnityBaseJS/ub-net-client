using System.Collections.Generic;

using Newtonsoft.Json;

namespace Softengi.UbClient.Sessions
{
	internal class KerberosAuthentication : AuthenticationBase
	{
		internal override void Authenticate(UbTransport transport)
		{
			var queryStringParams = new Dictionary<string, string> {{"AUTHTYPE", "Negotiate"}};
			var resp = transport.Get<NegotiateAuthResponse>("auth", queryStringParams, null, sendCredentials: true);
			_sessionWord = resp.SessionID;
			_sessionID = Crypto.Hexa8(resp.SessionID.Split('+')[0]);
			_sessionPasswordHash = resp.LogonName;
		}

		internal override string AuthHeader()
		{
			return "Negotiate " + Crypto.Signature(_sessionID, _sessionWord, _sessionPasswordHash);
		}

		private string _sessionID;
		private string _sessionPasswordHash;
		private string _sessionWord;

		public class NegotiateAuthResponse
		{
			[JsonProperty("result")]
			public string SessionID { get; set; }

			[JsonProperty("logonname")]
			public string LogonName { get; set; }
		}
	}
}