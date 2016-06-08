using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Softengi.UbClient;

namespace UbClient.UnitTests
{
	[TestFixture]
	public class UbLinqTest
	{
		public class Sample
		{
			public string Name;
		}

		[Test]
		public void TestLinq()
		{
			var cn = new UbConnection(new Uri("http://localhost:888"), UbAuthSchema.UB, "admin", "admin");
			var queryable = cn.Query<Sample>("smp_sample");
			var expr = from s in queryable
					   where s.Name == "ttt1"
					   where s.Name == "ttt2"
					   select s.Name;

			var result = expr.ToList();

			var i = 0;
		}
	}
}
