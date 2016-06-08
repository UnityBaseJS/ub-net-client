using System;
using System.IO;
using System.Net;

using Newtonsoft.Json;

namespace Softengi.UbClient
{
	public class UbException : Exception
	{
		public UbException(string message) : this(default(Uri), message, null)
		{}

		public UbException(Uri appUri, string message, Exception innerException) : base(message, innerException)
		{
			this.AppUri = appUri;
			var webException = innerException as WebException;
			if (webException != null)
			{
				this.ResponseMsg = GetResponse(webException);
				this.ResponseObj = GetResponseObj(this.ResponseMsg);
				if (this.ResponseObj != null)
				{
					this.ErrorMessage = this.ResponseObj.ErrorMessage;
					this.ErrorMessageCode = this.ResponseObj.ErrorCode;
				}
			}

			if (!string.IsNullOrEmpty(this.ErrorMessage))
			{
				var indexBo = this.ErrorMessage.IndexOf("<<<", StringComparison.Ordinal);
				var indexBc = this.ErrorMessage.IndexOf(">>>", StringComparison.Ordinal);
				if (indexBo >= 0 && indexBo < indexBc)
					this.ErrorMessageUser = this.ErrorMessage.Substring(indexBo, indexBc - indexBo).TrimStart('<');
			}
		}

		public RunListExceptionResponse ResponseObj { get; }
		public string ResponseMsg { get; }
		public string ErrorMessage { get; }
		public string ErrorMessageCode { get; }
		public string ErrorMessageUser { get; }
		public Uri AppUri { get; }

		public override string Message
		{
			get
			{
				if (!string.IsNullOrEmpty(this.ErrorMessageUser))
					return $"{base.Message} {this.Source} Code:{this.ErrorMessageCode}, {this.ErrorMessageUser}";

				if (!string.IsNullOrEmpty(this.ErrorMessage))
					return $"{base.Message} {this.Source}\nInner:{this.ErrorMessage}";

				if (!string.IsNullOrEmpty(this.ErrorMessageCode))
					return $"{base.Message} {this.Source} Code:{this.ErrorMessageCode}";

				return $"{base.Message} {this.Source}";
			}
		}

		public override string Source => Convert.ToString(this.AppUri);

		static private string GetResponse(WebException ex)
		{
			var responseStream = ex.Response?.GetResponseStream();
			if (responseStream == null)
				return null;

			using (var reader = new StreamReader(responseStream))
				return reader.ReadToEnd();
		}

		static private RunListExceptionResponse GetResponseObj(string response)
		{
			return response != null ? JsonConvert.DeserializeObject<RunListExceptionResponse>(response) : null;
		}

		#region Nested type: RunListExceptionResponse
		public class RunListExceptionResponse
		{
			[JsonProperty("success")]
			public bool Success { get; set; }

			[JsonProperty("errMsg")]
			public string ErrorMessage { get; set; }

			[JsonProperty("errCode")]
			public string ErrorCode { get; set; }

			[JsonProperty("result")]
			public RunListSetDocumentResultResponse Result { get; set; }
		}
		#endregion

		#region Nested type: RunListSetDocumentResultResponse
		public class RunListSetDocumentResultResponse
		{
			[JsonProperty("store")]
			public string Store { get; set; }

			[JsonProperty("fName")]
			public string FileName { get; set; }

			[JsonProperty("origName")]
			public string OriginalName { get; set; }

			[JsonProperty("relPath")]
			public string RelativePath { get; set; }

			[JsonProperty("ct")]
			public string ContentType { get; set; }

			[JsonProperty("size")]
			public long Size { get; set; }

			[JsonProperty("md5")]
			public string Md5 { get; set; }

			[JsonProperty("revision")]
			public int Revision { get; set; }

			[JsonProperty("isDirty")]
			public bool IsDirty { get; set; }
		}
		#endregion
	}
}