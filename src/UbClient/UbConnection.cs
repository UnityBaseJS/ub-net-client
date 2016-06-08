using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Softengi.UbClient.Dto;
using Softengi.UbClient.Sessions;

namespace Softengi.UbClient
{
	// TODO: handshake logic into the session classes
	// TODO: query constructing syntax
	// TODO: linq to UB

	public class UbConnection
	{
		public UbConnection(Uri baseUri, UbAuthSchema authSchema, string login, string password)
		{
			_baseUri = baseUri;
			_authSchema = authSchema;
			_login = login;
			_password = password;

			var localPath = baseUri.LocalPath;
			_appName = localPath == "/" ? "/" : localPath.Trim('/').ToLower();
		}

		public IOrderedQueryable<T> Query<T>(string entityName)
		{
			return new Linq.QueryableUbData<T>();
		}

		// TODO: remove method from here - parsing JSON is a different task
		public string GetDocument(string entityName, string attributeName, long id, string documentInfoStr,
			bool base64Response = false)
		{
			var documentInfo = JsonConvert.DeserializeObject<UbDocumentInfo>(documentInfoStr);
			return GetDocument(entityName, attributeName, id, documentInfo, base64Response);
		}

		public string GetDocument(string entityName, string attributeName, long id, UbDocumentInfo documentInfo,
			bool base64Response = false)
		{
			if (string.IsNullOrEmpty(_headersAuthorization))
				Auth();

			var documentQueryParams = new Dictionary<string, string>
			{
				{"entity", entityName},
				{"attribute", attributeName},
				{"ID", id.ToString()},
				{"store", documentInfo.Store},
				{"origName", documentInfo.OriginalName},
				{"filename", documentInfo.FileName}
			};

			return Get("getDocument", documentQueryParams, false, base64Response);
		}

		public List<Dictionary<string, object>> Select(
			string entity,
			string[] fieldList,
			UbFilter filter = null,
			object orderByList = null)
		{
			return
				RunList<RunListResponse>(
					new {entity, method = "select", fieldList, whereList = filter?.ToDictionary(), orderByList})[0]
					.ToDictionary();
		}

		public T SelectScalar<T>(string entity, string field, UbFilter filter = null, object orderByList = null)
		{
			return (T) Select(entity, new[] {field}, filter, orderByList)?[0][field];
		}

		public Dictionary<string, object> SelectByID(string entity, string[] fieldList, long id)
		{
			return Select(entity, fieldList, new UbFilter("ID", "=", id))?[0];
		}

		public long GetId(string entity)
		{
			var response = AddNew(entity, new[] {"ID"});
			return (long) response.ResultData.Data[0][0];
		}

		public RunListResponse AddNew(string entity, string[] fieldList)
		{
			return RunList<RunListResponse>(
				new {entity, method = "addnew", fieldList})[0];
		}

		public RunListResponse Insert(string entity, object entityInstance, string[] fieldsToReturn = null)
		{
			var responses = RunList<RunListResponse>(
				new
				{
					entity,
					method = "insert",
					fieldList = fieldsToReturn,
					execParams = entityInstance
				});
			return responses[0];
		}

		public RunListResponse Update(string entity, object entityInstance, bool skipOptimisticLock = false)
		{
			var responses = RunList<RunListResponse>(
				new
				{
					entity,
					method = "update",
					execParams = entityInstance,
					// TODO: support for other options, have flags object?
					__skipOptimisticLock = skipOptimisticLock
				});
			return responses[0];
		}

		public RunListResponse Update(string entity, object entityInstance, string[] fieldsToReturn,
			bool skipOptimisticLock = false)
		{
			var responses = RunList<RunListResponse>(
				new
				{
					entity,
					method = "update",
					fieldList = fieldsToReturn,
					execParams = entityInstance,
					__skipOptimisticLock = skipOptimisticLock
				});
			return responses[0];
		}

		public RunListResponse Delete(string entity, long id)
		{
			var responses = RunList<RunListResponse>(new {entity, method = "delete", execParams = new {ID = id}});
			return responses[0];
		}

		public T[] RunList<T>(object data) where T : RunListResponse
		{
			return RunList<T>(new[] {data});
		}

		public T[] RunList<T>(object[] data) where T : RunListResponse
		{
			var requestBody = JArray.FromObject(data).ToString();
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(requestBody)))
				return JsonConvert.DeserializeObject<T[]>(Run("runList", null, stream));
		}

		public string Run(string ubAppMethod, Dictionary<string, string> queryStringParams, Stream data)
		{
			if (string.IsNullOrEmpty(_headersAuthorization))
				Auth();

			try
			{
				return Post(ubAppMethod, queryStringParams, data);
			}
			catch (WebException ex) when (ex.Status == WebExceptionStatus.ConnectFailure)
			{
				const int retries = 3;
				for (var i = 0; i < retries; i++)
				{
					Thread.Sleep(1000);
					try
					{
						Auth();
						return Post(ubAppMethod, queryStringParams, data);
					}
					catch (WebException excf)
					{
						if (excf.Status != WebExceptionStatus.ConnectFailure)
							break;
					}
				}
				throw;
			}
			catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.Unauthorized)
			{
				try
				{
					Auth();
					return Post(ubAppMethod, queryStringParams, data);
				}
				catch (WebException authRetryException)
				{
					throw new UbException(_baseUri, "Authentication error", authRetryException);
				}
			}
			catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.Forbidden)
			{
				throw new UbException(_baseUri, $"UnityBase does not support method '{ubAppMethod}'", ex);
			}
			catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.InternalServerError)
			{
				throw new UbException(_baseUri, "Error", ex);
			}
		}

		public RunListSetDocumentResponse SetDocument(string entity, string attribute, string fileName, long id, Stream data)
		{
			var urlParams = new Dictionary<string, string>
			{
				{"ID", id.ToString()},
				{"ENTITY", entity},
				{"ATTRIBUTE", attribute},
				{"filename", fileName},
				{"origName", fileName}
			};
			var response = Run("setDocument", urlParams, data);
			return JsonConvert.DeserializeObject<RunListSetDocumentResponse>(response);
		}

		private void Auth()
		{
			_headersAuthorization = null;
			_ubSession = null;

			switch (_authSchema)
			{
				case UbAuthSchema.UB:
					_ubSession = AuthHandshakeUb();
					break;
				case UbAuthSchema.Negotiate:
					_ubSession = AuthHandshakeNegotiate();
					break;
				case UbAuthSchema.UBIP:
					_ubSession = AuthHandshakeUbip();
					break;
				default:
					throw new InvalidOperationException("Unknown authentication schema.");
			}

			if (_ubSession != null)
				_headersAuthorization = _ubSession.AuthHeader();
		}

		private UbSession AuthHandshakeUb()
		{
			var firstQueryString =
				new Dictionary<string, string>
				{
					{"AUTHTYPE", "UB"},
					{"userName", _login},
					{"password", string.Empty}
				};
			var firstResponse = Get<UbHandShakeAuthResponse>("auth", firstQueryString, false);

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

			var secondResonse = Get<AuthResponse>("auth", secondQueryString, true);

			return new UbSession(UbAuthSchema.UB, secondResonse.LogonName, secretWord, secondResonse.SessionID);
		}

		private UbSession AuthHandshakeNegotiate()
		{
			var queryStringParams =
				new Dictionary<string, string>
				{
					{"AUTHTYPE", "Negotiate"},
					{"userName", string.Empty}
				};
			var resp = Get<AuthResponse>("auth", queryStringParams, true);
			return new UbNegotiateSession(resp.LogonName, resp.SessionID);
		}

		private UbSession AuthHandshakeUbip()
		{
			_headersAuthorization = $"{_authSchema} {_login}";
			var resp = Get<AuthResponse>("auth", null, false);
			return new UbIpSession(resp.LogonName, resp.SecretWord, resp.SessionID);
		}

		private T Get<T>(string url, Dictionary<string, string> queryStringParams, bool sendCredentials,
			bool base64Response = false)
		{
			return JsonConvert.DeserializeObject<T>(Get(url, queryStringParams, sendCredentials, base64Response));
		}

		private string Get(string url, Dictionary<string, string> queryStringParams, bool sendCredentials,
			bool base64Response = false)
		{
			return HttpHelper.Xhr(_baseUri, "GET", url, queryStringParams, GetRequestHeaders(), sendCredentials, null,
				base64Response);
		}

		private string Post(string url, Dictionary<string, string> queryStringParams = null, Stream data = null)
		{
			return HttpHelper.Xhr(_baseUri, "POST", url, queryStringParams, GetRequestHeaders(), false, data);
		}

		private Dictionary<string, string> GetRequestHeaders()
		{
			return !string.IsNullOrEmpty(_headersAuthorization)
				? new Dictionary<string, string> {{"Authorization", _headersAuthorization}}
				: null;
		}

		/// <summary>
		/// Name of UB app, or "/", if UB hosted at root.
		/// </summary>
		private readonly string _appName;

		private readonly UbAuthSchema _authSchema;
		private readonly Uri _baseUri;
		private readonly string _login;
		private readonly string _password;

		private string _headersAuthorization;
		private UbSession _ubSession;

		public class AuthResponse
		{
			[JsonProperty("result")]
			public string SessionID { get; set; }

			[JsonProperty("logonname")]
			public string LogonName { get; set; }

			[JsonProperty("uData")]
			public string UserData { get; set; }

			[JsonProperty("data")]
			public string Data { get; set; }

			[JsonProperty("nonce")]
			public string Nonce { get; set; }

			[JsonProperty("connectionID")]
			public string ConnectionID { get; set; }

			[JsonProperty("realm")]
			public string Realm { get; set; }

			[JsonProperty("secretWord")]
			public string SecretWord { get; set; }
		}

		public class UbHandShakeAuthResponse
		{
			[JsonProperty("result")]
			public string Result { get; set; }

			// TODO: obsolete?  used by cert method only?
			[JsonProperty("connectionID")]
			public string ConnectionID { get; set; }
		}
	}
}