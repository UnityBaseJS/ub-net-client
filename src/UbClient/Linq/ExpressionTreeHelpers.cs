using System;
using System.Linq.Expressions;

using Softengi.UbClient.Linq;

namespace Softengi.UbClient.Linq
{
	static internal class ExpressionTreeHelpers
	{
		static internal bool IsMemberEqualsValueExpression(Expression exp, Type declaringType, string memberName)
		{
			if (exp.NodeType != ExpressionType.Equal)
				return false;

			var be = (BinaryExpression) exp;

			if (IsSpecificMemberExpression(be.Left, declaringType, memberName) &&
			    IsSpecificMemberExpression(be.Right, declaringType, memberName))
				throw new ArgumentException("Cannot have 'member' == 'member' in an expression!", nameof(be));

			return IsSpecificMemberExpression(be.Left, declaringType, memberName) ||
			       IsSpecificMemberExpression(be.Right, declaringType, memberName);
		}

		static internal bool IsSpecificMemberExpression(Expression exp, Type declaringType, string memberName)
		{
			var memberExpression = exp as MemberExpression;
			if (memberExpression == null) return false;

			var member = memberExpression.Member;
			return member.DeclaringType == declaringType && member.Name == memberName;
		}

		static internal string GetValueFromEqualsExpression(BinaryExpression be, Type memberDeclaringType, string memberName)
		{
			if (be.NodeType != ExpressionType.Equal)
				throw new ArgumentException("Expected equal expression", nameof(be));

			if (be.Left.NodeType == ExpressionType.MemberAccess)
			{
				if (IsSpecificMemberExpression(be.Left, memberDeclaringType, memberName))
					return GetValueFromExpression(be.Right);
			}
			else if (be.Right.NodeType == ExpressionType.MemberAccess)
			{
				if (IsSpecificMemberExpression(be.Right, memberDeclaringType, memberName))
					return GetValueFromExpression(be.Left);
			}

			// We should have returned by now. 
			throw new InvalidOperationException();
		}

		static internal string GetValueFromExpression(Expression expression)
		{
			if (expression.NodeType == ExpressionType.Constant)
				return (string) ((ConstantExpression) expression).Value;
			throw new InvalidQueryException($"The expression type {expression.NodeType} is not supported to obtain a value.");
		}
	}
}