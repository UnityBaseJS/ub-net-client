using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Softengi.UbClient.Linq
{
	/// <summary>
	/// Class containst an expression.  When <see cref="GetEnumerator()"/> is called,
	/// the expression is exectued with help of <see cref="Provider"/> and the result is
	/// enumerated.
	/// </summary>
	internal class QueryableUbData<T> : IOrderedQueryable<T>
	{
		internal QueryableUbData()
		{
			Provider = new UbQueryProvider();
			Expression = Expression.Constant(this);
		}

		/// <summary> 
		/// This constructor is called by Provider.CreateQuery(). 
		/// </summary>
		internal QueryableUbData(UbQueryProvider provider, Expression expression)
		{
			if (provider == null)
				throw new ArgumentNullException(nameof(provider));
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));
			if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
				throw new ArgumentOutOfRangeException(nameof(expression));

			Provider = provider;
			Expression = expression;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public Type ElementType => typeof(T);

		public Expression Expression { get; }
		public IQueryProvider Provider { get; }
	}
}