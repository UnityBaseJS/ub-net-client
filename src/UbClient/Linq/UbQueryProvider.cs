using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Softengi.UbClient.Linq
{
	internal class UbQueryProvider : IQueryProvider
	{
		public IQueryable CreateQuery(Expression expression)
		{
			var elementType = TypeSystem.GetElementType(expression.Type);
			try
			{
				var genericType = typeof(QueryableUbData<>).MakeGenericType(elementType);
				return (IQueryable) Activator.CreateInstance(genericType, this, expression);
			}
			catch (TargetInvocationException tie)
			{
				throw tie.InnerException;
			}
		}

		/// <summary>
		/// Queryable's collection-returning standard query operators call this method.
		/// </summary>
		public IQueryable<TResult> CreateQuery<TResult>(Expression expression)
		{
			return new QueryableUbData<TResult>(this, expression);
		}

		public object Execute(Expression expression)
		{
			return UbQueryContext.Execute(expression, false);
		}

		/// <summary>
		/// Queryable's "single value" standard query operators call this method.
		/// It is also called from <see cref="QueryableUbData{T}.GetEnumerator"/>.
		/// </summary>
		public TResult Execute<TResult>(Expression expression)
		{
			var isEnumerable = typeof(TResult).Name == "IEnumerable`1";
			return (TResult) UbQueryContext.Execute(expression, isEnumerable);
		}
	}
}