using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Web;

using Newtonsoft.Json;

namespace Softengi.UbClient.Sessions
{
	internal class UbTransport
	{
		internal UbTransport(Uri uri)
		{
			_uri = uri;
		}

		internal T Get<T>(
			string appMethod,
			Dictionary<string, string> queryStringParams,
			Dictionary<string, string> requestHeaders,
			bool sendCredentials, bool base64Response = false)
		{
			return JsonConvert.DeserializeObject<T>(Get(appMethod, queryStringParams, requestHeaders, sendCredentials, base64Response));
		}

		internal string Get(string appMethod, Dictionary<string, string> queryStringParams, Dictionary<string, string> requestHeaders, bool sendCredentials,
			bool base64Response = false)
		{
			return Request("GET", appMethod, queryStringParams, requestHeaders, sendCredentials, null, base64Response);
		}

		internal string Request(
			string httpMethod, string appMethod,
			Dictionary<string, string> queryStringParams,
			Dictionary<string, string> requestHeaders,
			bool sendCredentials, Stream data, bool base64Response = false)
		{
			if (httpMethod != "GET" && httpMethod != "POST")
				throw new ApplicationException($"HTTP method '{httpMethod}' is not supported.");
			Argument.NotNullOrEmpty(nameof(httpMethod), httpMethod);

			var uri = BuildUri(_uri, appMethod, queryStringParams);
			var request = WebRequest.Create(uri);
			request.Method = httpMethod;

			if (sendCredentials)
			{
				request.Credentials = CredentialCache.DefaultNetworkCredentials;
				request.ImpersonationLevel = TokenImpersonationLevel.Delegation;
			}

			if (requestHeaders != null)
				foreach (var p in requestHeaders)
					request.Headers[p.Key] = p.Value;

			if (data != null && data.Length > 0)
				using (var requestStream = request.GetRequestStream())
					data.CopyTo(requestStream);

			using (var response = request.GetResponse())
			{
				using (var responseStream = response.GetResponseStream())
				{
					if (responseStream == null)
						return null;

					if (base64Response)
						using (var ms = new MemoryStream())
						{
							responseStream.CopyTo(ms);
							return Convert.ToBase64String(ms.ToArray());
						}

					using (var reader = new StreamReader(responseStream))
						return reader.ReadToEnd();
				}
			}
		}

		static internal Uri BuildUri(Uri baseUri, string relativeUri, Dictionary<string, string> queryStringParams)
		{
			return new Uri(baseUri, AppendQueryParamsToUri(relativeUri, queryStringParams));
		}

		static private string AppendQueryParamsToUri(string uri, Dictionary<string, string> queryStringParams)
		{
			return queryStringParams != null
				? uri + "?" + QueryParamsToString(queryStringParams)
				: uri;
		}

		static private string QueryParamsToString(Dictionary<string, string> queryStringParams)
		{
			return queryStringParams
				.Select(p => HttpUtility.UrlEncode(p.Key) + "=" + HttpUtility.UrlEncode(p.Value))
				.Aggregate((current, next) => current + "&" + next);
		}

		private readonly Uri _uri;
	}
}