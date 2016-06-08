using System.Linq;
using System.Linq.Expressions;

namespace Softengi.UbClient.Linq
{
	/// <summary>
	/// Substitutes every <see cref="QueryableUbData{T}"/> it finds to the <see cref="IQueryable{T}"/> value passed in contructor.
	/// </summary>
	internal class ExpressionTreeModifier<T> : ExpressionVisitor
	{
		internal ExpressionTreeModifier(IQueryable<T> values)
		{
			_values = values;
		}

		protected override Expression VisitConstant(ConstantExpression c)
		{
			return c.Type == typeof(QueryableUbData<T>)
				? Expression.Constant(_values)
				: c;
		}

		private readonly IQueryable<T> _values;
	}
}