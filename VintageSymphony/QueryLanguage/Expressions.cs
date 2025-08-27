using System;
using System.Collections.Generic;
using System.Linq;

namespace VintageSymphony.QueryLanguage;

/// <summary>
/// Base class for binary expressions (AND, OR).
/// </summary>
public abstract class BinaryExpression : IExpression
{
	protected readonly IExpression Left;
	protected readonly IExpression Right;

	protected BinaryExpression(IExpression left, IExpression right)
	{
		Left = left ?? throw new ArgumentNullException(nameof(left));
		Right = right ?? throw new ArgumentNullException(nameof(right));
	}

	public abstract bool Evaluate(IFunctionRegistry functionRegistry);
}

/// <summary>
/// Represents a logical AND expression.
/// </summary>
public class AndExpression : BinaryExpression
{
	public AndExpression(IExpression left, IExpression right) : base(left, right)
	{
	}

	public override bool Evaluate(IFunctionRegistry functionRegistry)
	{
		return Left.Evaluate(functionRegistry) && Right.Evaluate(functionRegistry);
	}
}

/// <summary>
/// Represents a logical OR expression.
/// </summary>
public class OrExpression : BinaryExpression
{
	public OrExpression(IExpression left, IExpression right) : base(left, right)
	{
	}

	public override bool Evaluate(IFunctionRegistry functionRegistry)
	{
		return Left.Evaluate(functionRegistry) || Right.Evaluate(functionRegistry);
	}
}

/// <summary>
/// Represents a logical NOT expression.
/// </summary>
public class NotExpression : IExpression
{
	private readonly IExpression expression;

	public NotExpression(IExpression expression)
	{
		this.expression = expression ?? throw new ArgumentNullException(nameof(expression));
	}

	public bool Evaluate(IFunctionRegistry functionRegistry)
	{
		return !expression.Evaluate(functionRegistry);
	}
}

/// <summary>
/// Represents a function call expression.
/// </summary>
public class FunctionExpression : IExpression
{
	private readonly string functionName;
	private readonly object[] parameters;

	public FunctionExpression(string functionName, params object[] parameters)
	{
		this.functionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
		this.parameters = parameters ?? Array.Empty<object>();
	}

	public bool Evaluate(IFunctionRegistry functionRegistry)
	{
		if (functionRegistry == null)
			throw new ArgumentNullException(nameof(functionRegistry));

		return functionRegistry.CallFunction(functionName, parameters);
	}
}

/// <summary>
/// Represents a literal boolean value.
/// </summary>
public class LiteralExpression : IExpression
{
	private readonly bool value;

	public LiteralExpression(bool value)
	{
		this.value = value;
	}

	public bool Evaluate(IFunctionRegistry functionRegistry)
	{
		return value;
	}
}