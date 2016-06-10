using Newtonsoft.Json;

namespace Softengi.UbClient.Dto
{
	public class SetDocumentResponse
	{
		[JsonProperty("success")]
		public bool Success { get; set; }

		[JsonProperty("errMsg")]
		public string ErrrorMessage { get; set; }

		[JsonProperty("errCode")]
		public string ErrorCode { get; set; }

		[JsonProperty("result")]
		public UbDocumentInfo Result { get; set; }
	}
}