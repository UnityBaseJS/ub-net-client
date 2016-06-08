using System.Collections.Generic;
using System.Linq;

namespace Softengi.UbClient
{
	public class UbFilter
	{
		private readonly List<UbFilterClause> _clauses = new List<UbFilterClause>();

		public UbFilter(string expresson, string condition, object value = null)
		{
			Add(expresson, condition, value);
		}

		public UbFilter Add(string expresson, string condition, object value = null, string clauseName = null)
		{
			_clauses.Add(new UbFilterClause
			{
				Expression = expresson,
				Condition = condition,
				Value = value,
				ClauseName = clauseName
			});
			return this;
		}

		public Dictionary<string, object> ToDictionary()
		{
			return _clauses.ToDictionary(
				clause => clause.ClauseName ?? "by" + clause.Expression,
				clause =>
					(object) new {expression = clause.Expression, condition = clause.Condition, values = new {v1 = clause.Value}});
		}
	}
}