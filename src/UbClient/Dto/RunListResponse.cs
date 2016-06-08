using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;

namespace Softengi.UbClient.Dto
{
	public class RunListResponse
	{
		[JsonProperty("entity")]
		public string Entity { get; set; }

		[JsonProperty("method")]
		public string Method { get; set; }

		[JsonProperty("fieldList")]
		public string[] FieldList { get; set; }

		[JsonProperty("resultData")]
		public ResultDataResponse ResultData { get; set; }

		public DataTable ToDataTable()
		{
			var table = new DataTable(Entity);
			foreach (var field in ResultData.Fields)
				table.Columns.Add(field);
			foreach (var dataRow in ResultData.Data)
				table.Rows.Add(dataRow);
			return table;
		}

		public List<Dictionary<string, object>> ToDictionary()
		{
			return ResultData.Data
				.ToList()
				.Select(data =>
					data.Select((value, index) => new {key = ResultData.Fields[index], value})
						.ToDictionary(row => row.key, row => row.value))
				.ToList();
		}
	}
}