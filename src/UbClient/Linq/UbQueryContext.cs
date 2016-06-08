using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Softengi.UbClient.Linq
{
	static internal class UbQueryContext
	{
		/// <summary>
		/// Executes the expression tree that is passed to it. 
		/// </summary>
		static internal object Execute(Expression expression, bool isEnumerable)
		{
			// The expression must represent a query over the data source. 
			if (!IsQueryOverDataSource(expression))
				throw new InvalidProgramException("No query over the data source was specified.");

			// Find the call to Where() and get the lambda expression predicate.
			var whereExpression = InnermostWhereFinder.GetInnermostWhere(expression);
			var lambdaExpression = (LambdaExpression) ((UnaryExpression) whereExpression.Arguments[1]).Operand;

			// Send the lambda expression through the partial evaluator.
			lambdaExpression = (LambdaExpression) Evaluator.PartialEval(lambdaExpression);
/*
			// Get the place name(s) to query the Web service with.
			var lf = new LocationFinder(lambdaExpression.Body);
			var locations = lf.Locations;
			if (locations.Count == 0)
				throw new InvalidQueryException("You must specify at least one place name in your query.");

			// Call the Web service and get the results.
			Place[] places = WebServiceHelper.GetPlacesFromTerraServer(locations);

			// Copy the IEnumerable places to an IQueryable.
			IQueryable<Place> queryablePlaces = places.AsQueryable();

			var newExpressionTree = CopyTree(expression, queryablePlaces);
			
			// This step creates an IQueryable that executes by replacing Queryable methods with Enumerable methods. 
			if (isEnumerable)
				return queryablePlaces.Provider.CreateQuery(newExpressionTree);
			return queryablePlaces.Provider.Execute(newExpressionTree);
			*/

			// TODO: temp
			return null;
		}

		/*
		/// <summary>
		/// Copy the expression tree that was passed in, changing only the first 
		/// argument of the innermost MethodCallExpression.
		/// </summary>
		static private Expression CopyTree(Expression expression, IQueryable<Place> queryablePlaces)
		{
			return new ExpressionTreeModifier<Place>(queryablePlaces).Visit(expression);
		}*/

		static private bool IsQueryOverDataSource(Expression expression)
		{
			// If expression represents an unqueried IQueryable data source instance, 
			// expression is of type ConstantExpression, not MethodCallExpression. 
			return expression is MethodCallExpression;
		}
	}

	/*
	internal class LocationFinder : ExpressionVisitor
	{
		public LocationFinder(Expression exp)
		{
			_expression = exp;
		}

		public List<string> Locations
		{
			get
			{
				if (_locations == null)
				{
					_locations = new List<string>();
					Visit(_expression);
				}
				return _locations;
			}
		}

		protected override Expression VisitBinary(BinaryExpression be)
		{
			if (be.NodeType != ExpressionType.Equal)
				return base.VisitBinary(be);

			if (ExpressionTreeHelpers.IsMemberEqualsValueExpression(be, typeof(Place), "Name"))
			{
				_locations.Add(ExpressionTreeHelpers.GetValueFromEqualsExpression(be, typeof(Place), "Name"));
				return be;
			}

			if (ExpressionTreeHelpers.IsMemberEqualsValueExpression(be, typeof(Place), "State"))
			{
				_locations.Add(ExpressionTreeHelpers.GetValueFromEqualsExpression(be, typeof(Place), "State"));
				return be;
			}

			return base.VisitBinary(be);
		}

		private readonly Expression _expression;
		private List<string> _locations;
	}*/
}
