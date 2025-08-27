using System;
using Xunit;
using VintageSymphony.QueryLanguage;

namespace VintageSymphony.Test.QueryLanguage;

public class ExpressionEvaluationTests
{
	private IFunctionRegistry CreateTestRegistry()
	{
		var registry = new FunctionRegistry();
		registry.RegisterFunction("true_func", () => true);
		registry.RegisterFunction("false_func", () => false);
		registry.RegisterFunction("time", (parameters) =>
		{
			if (parameters.Length == 2)
			{
				// Convert parameters to integers more robustly
				if (int.TryParse(parameters[0].ToString(), out int start) &&
					int.TryParse(parameters[1].ToString(), out int end))
				{
					var currentHour = 15; // Simulate 3 PM
					return currentHour >= start && currentHour <= end;
				}
			}
			return false;
		});
		registry.RegisterFunction("equals", (parameters) =>
		{
			if (parameters.Length == 2)
			{
				return parameters[0].Equals(parameters[1]);
			}
			return false;
		});
		return registry;
	}

	[Fact]
	public void AndExpression_BothTrue_ReturnsTrue()
	{
		// Arrange
		var registry = CreateTestRegistry();
		var left = new FunctionExpression("true_func");
		var right = new FunctionExpression("true_func");
		var expression = new AndExpression(left, right);

		// Act
		var result = expression.Evaluate(registry);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void AndExpression_LeftFalse_ReturnsFalse()
	{
		// Arrange
		var registry = CreateTestRegistry();
		var left = new FunctionExpression("false_func");
		var right = new FunctionExpression("true_func");
		var expression = new AndExpression(left, right);

		// Act
		var result = expression.Evaluate(registry);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void AndExpression_RightFalse_ReturnsFalse()
	{
		// Arrange
		var registry = CreateTestRegistry();
		var left = new FunctionExpression("true_func");
		var right = new FunctionExpression("false_func");
		var expression = new AndExpression(left, right);

		// Act
		var result = expression.Evaluate(registry);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void AndExpression_BothFalse_ReturnsFalse()
	{
		// Arrange
		var registry = CreateTestRegistry();
		var left = new FunctionExpression("false_func");
		var right = new FunctionExpression("false_func");
		var expression = new AndExpression(left, right);

		// Act
		var result = expression.Evaluate(registry);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OrExpression_BothTrue_ReturnsTrue()
	{
		// Arrange
		var registry = CreateTestRegistry();
		var left = new FunctionExpression("true_func");
		var right = new FunctionExpression("true_func");
		var expression = new OrExpression(left, right);

		// Act
		var result = expression.Evaluate(registry);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void OrExpression_LeftTrue_ReturnsTrue()
	{
		// Arrange
		var registry = CreateTestRegistry();
		var left = new FunctionExpression("true_func");
		var right = new FunctionExpression("false_func");
		var expression = new OrExpression(left, right);

		// Act
		var result = expression.Evaluate(registry);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void OrExpression_RightTrue_ReturnsTrue()
	{
		// Arrange
		var registry = CreateTestRegistry();
		var left = new FunctionExpression("false_func");
		var right = new FunctionExpression("true_func");
		var expression = new OrExpression(left, right);

		// Act
		var result = expression.Evaluate(registry);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void OrExpression_BothFalse_ReturnsFalse()
	{
		// Arrange
		var registry = CreateTestRegistry();
		var left = new FunctionExpression("false_func");
		var right = new FunctionExpression("false_func");
		var expression = new OrExpression(left, right);

		// Act
		var result = expression.Evaluate(registry);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void NotExpression_True_ReturnsFalse()
	{
		// Arrange
		var registry = CreateTestRegistry();
		var inner = new FunctionExpression("true_func");
		var expression = new NotExpression(inner);

		// Act
		var result = expression.Evaluate(registry);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void NotExpression_False_ReturnsTrue()
	{
		// Arrange
		var registry = CreateTestRegistry();
		var inner = new FunctionExpression("false_func");
		var expression = new NotExpression(inner);

		// Act
		var result = expression.Evaluate(registry);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void FunctionExpression_NoParameters_Success()
	{
		// Arrange
		var registry = CreateTestRegistry();
		var expression = new FunctionExpression("true_func");

		// Act
		var result = expression.Evaluate(registry);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void FunctionExpression_WithParameters_Success()
	{
		// Arrange
		var registry = CreateTestRegistry();
		var expression = new FunctionExpression("time", 10, 20);

		// Act
		var result = expression.Evaluate(registry);

		// Assert
		Assert.True(result); // Current simulated time is 15, which is between 10 and 20
	}

	[Fact]
	public void FunctionExpression_NonExistentFunction_ThrowsException()
	{
		// Arrange
		var registry = CreateTestRegistry();
		var expression = new FunctionExpression("nonexistent");

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => expression.Evaluate(registry));
	}

	[Fact]
	public void FunctionExpression_NullRegistry_ThrowsException()
	{
		// Arrange
		var expression = new FunctionExpression("test");

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => expression.Evaluate(null));
	}

	[Fact]
	public void LiteralExpression_True_ReturnsTrue()
	{
		// Arrange
		var registry = CreateTestRegistry();
		var expression = new LiteralExpression(true);

		// Act
		var result = expression.Evaluate(registry);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void LiteralExpression_False_ReturnsFalse()
	{
		// Arrange
		var registry = CreateTestRegistry();
		var expression = new LiteralExpression(false);

		// Act
		var result = expression.Evaluate(registry);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void ComplexExpression_MultipleOperators_EvaluatesCorrectly()
	{
		// Arrange - (true_func and false_func) or not false_func
		// Expected: (true and false) or not false = false or true = true
		var registry = CreateTestRegistry();
		var andLeft = new FunctionExpression("true_func");
		var andRight = new FunctionExpression("false_func");
		var andExpr = new AndExpression(andLeft, andRight);
		
		var notInner = new FunctionExpression("false_func");
		var notExpr = new NotExpression(notInner);
		
		var orExpr = new OrExpression(andExpr, notExpr);

		// Act
		var result = orExpr.Evaluate(registry);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void IntegrationTest_ParseAndEvaluate_Success()
	{
		// Arrange
		var registry = CreateTestRegistry();
		var expression = ExpressionParser.ParseExpression("true_func() and not false_func()");

		// Act
		var result = expression.Evaluate(registry);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void IntegrationTest_ComplexExpression_Success()
	{
		// Arrange
		var registry = CreateTestRegistry();
		var expression = ExpressionParser.ParseExpression("time(10,20) and (true_func() or false_func())");

		// Act
		var result = expression.Evaluate(registry);

		// Assert
		Assert.True(result); // time(10,20) = true (15 is between 10-20), true_func or false_func = true
	}

	[Fact]
	public void IntegrationTest_WithStringParameters_Success()
	{
		// Arrange
		var registry = CreateTestRegistry();
		var expression = ExpressionParser.ParseExpression("equals(\"hello\", \"hello\")");

		// Act
		var result = expression.Evaluate(registry);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void BinaryExpression_NullLeftOperand_ThrowsException()
	{
		// Arrange
		var right = new LiteralExpression(true);

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => new AndExpression(null, right));
		Assert.Throws<ArgumentNullException>(() => new OrExpression(null, right));
	}

	[Fact]
	public void BinaryExpression_NullRightOperand_ThrowsException()
	{
		// Arrange
		var left = new LiteralExpression(true);

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => new AndExpression(left, null));
		Assert.Throws<ArgumentNullException>(() => new OrExpression(left, null));
	}

	[Fact]
	public void NotExpression_NullOperand_ThrowsException()
	{
		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => new NotExpression(null));
	}

	[Fact]
	public void FunctionExpression_NullFunctionName_ThrowsException()
	{
		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => new FunctionExpression(null));
	}

	[Fact]
	public void FunctionExpression_NullParameters_TreatedAsEmpty()
	{
		// Arrange
		var expression = new FunctionExpression("test", null);
		var registry = new FunctionRegistry();
		registry.RegisterFunction("test", () => true);

		// Act
		var result = expression.Evaluate(registry);

		// Assert
		Assert.True(result);
	}
}