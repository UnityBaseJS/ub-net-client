using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Web;

namespace Softengi.UbClient
{
	static internal class HttpHelper
	{
		static internal string Xhr(
			Uri baseUri, string httpMethod, string url,
			Dictionary<string, string> queryStringParams,
			Dictionary<string, string> requestHeaders,
			bool sendCredentials, Stream data = null, bool base64Response = false)
		{
			var uri = BuildUri(baseUri, url, queryStringParams);
			return Xhr(uri, httpMethod, requestHeaders, sendCredentials, data, base64Response);
		}

		static internal string Xhr(Uri uri, string httpMethod, Dictionary<string, string> requestHeaders, bool sendCredentials,
			Stream data, bool base64Response = false)
		{
			var request = WebRequest.Create(uri);

			request.Method = Argument.NotNullOrEmpty(nameof(httpMethod), httpMethod);
			if (httpMethod != "GET" && httpMethod != "POST")
				throw new ApplicationException($"HTTP method '{httpMethod}' is not supported.");

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
					{
						using (var ms = new MemoryStream())
						{
							var chunk = new byte[4096];
							int bytesRead;
							while ((bytesRead = responseStream.Read(chunk, 0, chunk.Length)) > 0)
							{
								ms.Write(chunk, 0, bytesRead);
							}
							return Convert.ToBase64String(ms.ToArray());
						}
					}

					using (var reader = new StreamReader(responseStream))
						return reader.ReadToEnd();
				}
			}
		}

		static private Uri BuildUri(Uri baseUri, string uri, Dictionary<string, string> queryStringParams)
		{
			var relativeUri = queryStringParams != null
				? uri + "?" + QueryParamsToString(queryStringParams)
				: uri;
			return new Uri(baseUri, relativeUri);
		}

		static private string QueryParamsToString(Dictionary<string, string> queryStringParams)
		{
			return queryStringParams
				.Select(p => HttpUtility.UrlEncode(p.Key) + "=" + HttpUtility.UrlEncode(p.Value))
				.Aggregate((current, next) => current + "&" + next);
		}
	}
}