using Newtonsoft.Json;

namespace Softengi.UbClient.Dto
{
	public class UbDocumentInfo
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

		// TODO: why the properties below are nullable?
		[JsonProperty("size")]
		public long? Size { get; set; }

		[JsonProperty("md5")]
		public string Md5 { get; set; }

		[JsonProperty("revision")]
		public int? Revision { get; set; }

		[JsonProperty("isDirty")]
		public bool? IsDirty { get; set; }
	}
}