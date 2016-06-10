using Newtonsoft.Json;

namespace Softengi.UbClient.Dto
{
	public class ResultDataResponse
	{
		[JsonProperty("fields")]
		public string[] Fields { get; set; }

		[JsonProperty("rowCount")]
		public int RowCount { get; set; }

		[JsonProperty("data")]
		public object[][] Data { get; set; }

		// TODO: remove
		[JsonProperty("dataString")]
		public string DataString { get; set; }

		// TODO: remove
		[JsonProperty("dataJson")]
		public string DataJson { get; set; }
	}
}