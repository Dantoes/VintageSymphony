using System;

namespace VintageSymphony.QueryLanguage;

/// <summary>
/// Represents an expression that can be evaluated to a boolean result.
/// </summary>
public interface IExpression
{
	/// <summary>
	/// Evaluates the expression using the provided function registry.
	/// </summary>
	/// <param name="functionRegistry">Registry containing available functions</param>
	/// <returns>True if the expression evaluates to true, false otherwise</returns>
	bool Evaluate(IFunctionRegistry functionRegistry);
}