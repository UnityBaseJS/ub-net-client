using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Softengi.UbClient.Linq
{
	/// <summary>
	/// The class evaluates everything except expressions of type <see cref="ExpressionType.Parameter"/>,
	/// or using a funciton which checks if an expression may be evaluated or not.
	/// </summary>
	static public class Evaluator
	{
		/// <summary>
		/// Performs evaluation & replacement of independent sub-trees
		/// </summary>
		/// <param name="expression">The root of the expression tree.</param>
		/// <param name="fnCanBeEvaluated">
		/// A function that decides whether a given expression node can be part of the local
		/// function.
		/// </param>
		/// <returns>A new tree with sub-trees evaluated and replaced.</returns>
		static public Expression PartialEval(Expression expression, Func<Expression, bool> fnCanBeEvaluated)
		{
			return SubtreeEvaluator.Eval(expression, Nominator.Nominate(fnCanBeEvaluated, expression));
		}

		/// <summary>
		/// Performs evaluation & replacement of independent sub-trees
		/// </summary>
		/// <param name="expression">The root of the expression tree.</param>
		/// <returns>A new tree with sub-trees evaluated and replaced.</returns>
		static public Expression PartialEval(Expression expression)
		{
			return PartialEval(expression, CanBeEvaluatedLocally);
		}

		static private bool CanBeEvaluatedLocally(Expression expression)
		{
			return expression.NodeType != ExpressionType.Parameter;
		}

		/// <summary>
		/// Evaluates & replaces sub-trees when first candidate is reached (top-down)
		/// </summary>
		private class SubtreeEvaluator : ExpressionVisitor
		{
			private SubtreeEvaluator(HashSet<Expression> candidates)
			{
				_candidates = candidates;
			}

			public override Expression Visit(Expression exp)
			{
				if (exp == null)
					return null;

				if (_candidates.Contains(exp))
					return Evaluate(exp);

				return base.Visit(exp);
			}

			/// <summary>
			/// Traverse the expression and evaluate provided subexpressions, substituting them with result of
			/// their evaluation.
			/// </summary>
			/// <param name="exp">Expression to process</param>
			/// <param name="candidates">Sub-expressions which may be evaluated.</param>
			/// <returns></returns>
			static internal Expression Eval(Expression exp, HashSet<Expression> candidates)
			{
				return new SubtreeEvaluator(candidates).Visit(exp);
			}

			static private Expression Evaluate(Expression e)
			{
				if (e.NodeType == ExpressionType.Constant)
					return e;

				var lambda = Expression.Lambda(e);
				var fn = lambda.Compile();
				return Expression.Constant(fn.DynamicInvoke(null), e.Type);
			}

			private readonly HashSet<Expression> _candidates;
		}

		/// <summary>
		/// Performs bottom-up analysis to determine which nodes can possibly
		/// be part of an evaluated sub-tree.
		/// </summary>
		private class Nominator : ExpressionVisitor
		{
			private Nominator(Func<Expression, bool> fnCanBeEvaluated)
			{
				_fnCanBeEvaluated = fnCanBeEvaluated;
			}

			public override Expression Visit(Expression expression)
			{
				if (expression == null)
					return null;

				// Save the current value of "_cannotBeEvaluated", to set it false and see the result of
				// traversing the child nodes.
				var saveCannotBeEvaluated = _cannotBeEvaluated;
				_cannotBeEvaluated = false;

				// Visit all the child nodes, in order to see if they are ok or not to be evaulated
				base.Visit(expression);

				// At this moment "_cannotBeEvaluated" contains value indicating if any of children cannot be evaluated.

				if (!_cannotBeEvaluated)
				{
					// Child nodes are ok, see if the current node is ok.
					if (_fnCanBeEvaluated(expression))
					{
						// If all children nodes ok and the current node is ok - add it as candidate for evaluation
						_candidates.Add(expression);
					}
					else
					{
						// Otherwise, set the flag that the current node, and therefore all the parents cannot be evaluated
						_cannotBeEvaluated = true;
					}
				}

				_cannotBeEvaluated |= saveCannotBeEvaluated;

				return expression;
			}

			static internal HashSet<Expression> Nominate(Func<Expression, bool> fnCanBeEvaluated, Expression expression)
			{
				var nominator = new Nominator(fnCanBeEvaluated);
				nominator.Visit(expression);
				return nominator._candidates;
			}

			private readonly Func<Expression, bool> _fnCanBeEvaluated;
			private readonly HashSet<Expression> _candidates = new HashSet<Expression>();
			private bool _cannotBeEvaluated;
		}
	}
}