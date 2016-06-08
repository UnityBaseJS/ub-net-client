using System.Linq.Expressions;

namespace Softengi.UbClient.Linq
{
	/// <summary>
	/// Finds the last method call for method with name "Where".
	/// </summary>
	internal class InnermostWhereFinder : ExpressionVisitor
	{
		private InnermostWhereFinder()
		{}

		protected override Expression VisitMethodCall(MethodCallExpression expression)
		{
			if (expression.Method.Name == "Where")
				_innermostWhereExpression = expression;

			// TODO: and what if there is more than one argument for method..?
			Visit(expression.Arguments[0]);

			return expression;
		}

		static internal MethodCallExpression GetInnermostWhere(Expression expression)
		{
			var finder = new InnermostWhereFinder();
			finder.Visit(expression);
			return finder._innermostWhereExpression;
		}

		private MethodCallExpression _innermostWhereExpression;
	}
}