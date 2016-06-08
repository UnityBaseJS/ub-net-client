using System.ComponentModel;

namespace Softengi.UbClient
{
	static public class UbFilterCondition
	{
		static public string Equal => "=";

		[DisplayName("Less than or Equal")]
		static public string Le => "<=";

		[DisplayName("Less than")]
		static public string Lt => "<";

		[DisplayName("Greater than or Equal")]
		static public string Ge => ">=";

		[DisplayName("Greater than ")]
		static public string Gt => ">";

		static public string In => "in";
	}

	public class UbFilterClause
	{
		public string Expression { get; set; }
		public string Condition { get; set; }
		public object Value { get; set; }
		public string ClauseName { get; set; }
	}
}
