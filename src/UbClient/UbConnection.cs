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
using Softengi.UbClient.Linq;
using Softengi.UbClient.Sessions;

namespace Softengi.UbClient
{
	// TODO: query constructing syntax
	// TODO: linq to UB

	public class UbConnection
	{
		public UbConnection(Uri baseUri, AuthenticationBase auth)
		{
			_transport = new UbTransport(baseUri);
			_baseUri = baseUri;
			_auth = auth;
		}

		public IOrderedQueryable<T> Query<T>(string entityName)
		{
			return new QueryableUbData<T>();
		}

		// TODO: remove method from here - parsing JSON is a different task
		public string GetDocument(string entityName, string attributeName, long id, string documentInfoStr,
			bool base64Response = false)
		{
			var documentInfo = JsonConvert.DeserializeObject<UbDocumentInfo>(documentInfoStr);
			return GetDocument(entityName, attributeName, id, documentInfo, base64Response);
		}

		public string GetDocument(string entityName, string attributeName, long id, UbDocumentInfo documentInfo, bool base64Response = false)
		{
			if (!IsAuthenticated)
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

			return Get("getDocument", documentQueryParams, base64Response);
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
				return JsonConvert.DeserializeObject<T[]>(Run("runList", stream));
		}

		// TODO: push handling of authentication (depending on endpoint) and handling of exceptions in some central place
		public string Run(string endPoint, Stream data, Dictionary<string, string> queryStringParams = null)
		{
			if (!IsAuthenticated)
				Auth();

			try
			{
				return Post(endPoint, data, queryStringParams);
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
						return Post(endPoint, data, queryStringParams);
					}
					catch (WebException excf) when (excf.Status == WebExceptionStatus.ConnectFailure)
					{}
				}
				throw;
			}
			catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.Unauthorized)
			{
				try
				{
					IsAuthenticated = false;
					Auth();
					return Post(endPoint, data, queryStringParams);
				}
				catch (WebException authRetryException)
				{
					throw new UbException(_baseUri, "Authentication error", authRetryException);
				}
			}
			catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.Forbidden)
			{
				throw new UbException(_baseUri, $"UnityBase does not support method '{endPoint}'", ex);
			}
			catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.InternalServerError)
			{
				throw new UbException(_baseUri, "Error", ex);
			}
		}

		public SetDocumentResponse SetDocument(string entity, string attribute, string fileName, long id, Stream data)
		{
			var response = Run("setDocument", data, CreateSetDocumentParams(entity, attribute, id, fileName));
			return JsonConvert.DeserializeObject<SetDocumentResponse>(response);
		}

		static private Dictionary<string, string> CreateSetDocumentParams(string entity, string attribute, long id, string fileName)
		{
			return new Dictionary<string, string>
			{
				{"ID", id.ToString()},
				{"ENTITY", entity},
				{"ATTRIBUTE", attribute},
				{"filename", fileName},
				{"origName", fileName}
			};
		}

		/// <summary>
		/// Returns <c>true</c>, if user is authenticated.
		/// </summary>
		/// <remarks>
		/// The class uses lazy authentication, it authenticates user when any method which requires authentication is called.
		/// This property allows to know the current state of the connection.
		/// </remarks>
		public bool IsAuthenticated { get; private set; }

		private void Auth()
		{
			_auth.Authenticate(_transport);
			IsAuthenticated = true;
		}

		private string Get(string endPoint, Dictionary<string, string> queryStringParams, bool base64Response = false)
		{
			return Request("GET", endPoint, queryStringParams, null, base64Response);
		}

		private string Post(string endPoint, Stream data = null, Dictionary<string, string> queryStringParams = null)
		{
			return Request("POST", endPoint, queryStringParams, data);
		}

		private string Request(
			string httpMethod, string endPoint,
			Dictionary<string, string> queryStringParams,
			Stream data = null, bool base64Response = false)
		{
			return _transport.Request(httpMethod, endPoint, queryStringParams, GetRequestHeaders(endPoint), data, base64Response);
		}

		private Dictionary<string, string> GetRequestHeaders(string endPoint)
		{
			if (!_authenticatedEndpoints.Contains(endPoint))
				return null;
			return new Dictionary<string, string> {{"Authorization", _auth.AuthHeader()}};
		}

		private readonly Uri _baseUri;
		private readonly AuthenticationBase _auth;
		private readonly UbTransport _transport;

		private readonly HashSet<string> _authenticatedEndpoints = new HashSet<string>
		{
			"runList",
			"setDocument",
			"getDocument",
			"getDomainInfo"
		};

		/*
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
		}*/
	}
}