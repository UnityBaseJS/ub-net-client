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
	// TODO: merge session and authentication method into the same class
	// TODO: query constructing syntax
	// TODO: linq to UB

	public class UbConnection
	{
		public UbConnection(Uri baseUri, UbAuthenticationMethodBase authMethod)
		{
			_transport = new UbTransport(baseUri);
			_baseUri = baseUri;
			_authMethod = authMethod;
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
			_headersAuthorization = _authMethod.Authenticate(_transport).AuthHeader();
		}

		private string Get(string url, Dictionary<string, string> queryStringParams, bool sendCredentials,
			bool base64Response = false)
		{
			return Request("GET", url, queryStringParams, sendCredentials, null, base64Response);
		}

		private string Post(string url, Dictionary<string, string> queryStringParams = null, Stream data = null)
		{
			return Request("POST", url, queryStringParams, false, data);
		}

		private string Request(
			string httpMethod, string appMethod,
			Dictionary<string, string> queryStringParams,
			bool sendCredentials, Stream data = null, bool base64Response = false)
		{
			return _transport.Request(httpMethod, appMethod, queryStringParams, GetRequestHeaders(), sendCredentials, data, base64Response);
		}

		private Dictionary<string, string> GetRequestHeaders()
		{
			return !string.IsNullOrEmpty(_headersAuthorization)
				? new Dictionary<string, string> {{"Authorization", _headersAuthorization}}
				: null;
		}

		private readonly Uri _baseUri;
		private readonly UbAuthenticationMethodBase _authMethod;

		private string _headersAuthorization;
		private UbSession _ubSession;
		private readonly UbTransport _transport;

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
	}
}