using System;
using Xunit;
using VintageSymphony.QueryLanguage;
using Sprache;

namespace VintageSymphony.Test.QueryLanguage;

public class ExpressionParserTests
{
	[Fact]
	public void ParseExpression_SimpleFunction_Success()
	{
		// Arrange
		var input = "rain()";

		// Act
		var result = ExpressionParser.ParseExpression(input);

		// Assert
		Assert.NotNull(result);
		Assert.IsType<FunctionExpression>(result);
	}

	[Fact]
	public void ParseExpression_FunctionWithoutParentheses_Success()
	{
		// Arrange
		var input = "rain";

		// Act
		var result = ExpressionParser.ParseExpression(input);

		// Assert
		Assert.NotNull(result);
		Assert.IsType<FunctionExpression>(result);
	}

	[Fact]
	public void ParseExpression_AndExpression_Success()
	{
		// Arrange
		var input = "rain() and night()";

		// Act
		var result = ExpressionParser.ParseExpression(input);

		// Assert
		Assert.NotNull(result);
		Assert.IsType<AndExpression>(result);
	}

	[Fact]
	public void ParseExpression_OrExpression_Success()
	{
		// Arrange
		var input = "rain() or night()";

		// Act
		var result = ExpressionParser.ParseExpression(input);

		// Assert
		Assert.NotNull(result);
		Assert.IsType<OrExpression>(result);
	}

	[Fact]
	public void ParseExpression_NotExpression_Success()
	{
		// Arrange
		var input = "not rain()";

		// Act
		var result = ExpressionParser.ParseExpression(input);

		// Assert
		Assert.NotNull(result);
		Assert.IsType<NotExpression>(result);
	}

	[Fact]
	public void ParseExpression_ParenthesesGrouping_Success()
	{
		// Arrange
		var input = "(rain() and night())";

		// Act
		var result = ExpressionParser.ParseExpression(input);

		// Assert
		Assert.NotNull(result);
		Assert.IsType<AndExpression>(result);
	}

	[Fact]
	public void ParseExpression_ComplexExpression_Success()
	{
		// Arrange
		var input = "rain() and (night() or not day())";

		// Act
		var result = ExpressionParser.ParseExpression(input);

		// Assert
		Assert.NotNull(result);
		Assert.IsType<AndExpression>(result);
	}

	[Fact]
	public void ParseExpression_FunctionWithParameters_Success()
	{
		// Arrange
		var input = "time(9,20)";

		// Act
		var result = ExpressionParser.ParseExpression(input);

		// Assert
		Assert.NotNull(result);
		Assert.IsType<FunctionExpression>(result);
	}

	[Fact]
	public void ParseExpression_FunctionWithStringParameter_Success()
	{
		// Arrange
		var input = "weather(\"sunny\")";

		// Act
		var result = ExpressionParser.ParseExpression(input);

		// Assert
		Assert.NotNull(result);
		Assert.IsType<FunctionExpression>(result);
	}

	[Fact]
	public void ParseExpression_BooleanLiterals_Success()
	{
		// Arrange & Act & Assert
		var trueResult = ExpressionParser.ParseExpression("true");
		Assert.NotNull(trueResult);
		Assert.IsType<LiteralExpression>(trueResult);

		var falseResult = ExpressionParser.ParseExpression("false");
		Assert.NotNull(falseResult);
		Assert.IsType<LiteralExpression>(falseResult);
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public void ParseExpression_NullOrEmptyInput_ThrowsException(string input)
	{
		// Act & Assert
		Assert.Throws<ArgumentException>(() => ExpressionParser.ParseExpression(input));
	}

	[Theory]
	[InlineData("rain() and")]
	[InlineData("and rain()")]
	[InlineData("rain() and and night()")]
	[InlineData("rain() &")]
	[InlineData("(rain()")]
	[InlineData("rain())")]
	public void ParseExpression_MalformedExpression_ThrowsParseException(string input)
	{
		// Act & Assert
		Assert.Throws<ParseException>(() => ExpressionParser.ParseExpression(input));
	}

	[Fact]
	public void TryParseExpression_ValidExpression_ReturnsTrue()
	{
		// Arrange
		var input = "rain() and night()";

		// Act
		var success = ExpressionParser.TryParseExpression(input, out var result);

		// Assert
		Assert.True(success);
		Assert.NotNull(result);
		Assert.IsType<AndExpression>(result);
	}

	[Fact]
	public void TryParseExpression_InvalidExpression_ReturnsFalse()
	{
		// Arrange
		var input = "rain() and";

		// Act
		var success = ExpressionParser.TryParseExpression(input, out var result);

		// Assert
		Assert.False(success);
		Assert.Null(result);
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public void TryParseExpression_NullOrEmptyInput_ReturnsFalse(string input)
	{
		// Act
		var success = ExpressionParser.TryParseExpression(input, out var result);

		// Assert
		Assert.False(success);
		Assert.Null(result);
	}

	[Fact]
	public void ParseExpression_OperatorPrecedence_WorksCorrectly()
	{
		// Arrange - AND should have higher precedence than OR
		var input = "a or b and c";

		// Act
		var result = ExpressionParser.ParseExpression(input);

		// Assert
		Assert.NotNull(result);
		Assert.IsType<OrExpression>(result);
	}

	[Fact]
	public void ParseExpression_WhitespaceHandling_Success()
	{
		// Arrange
		var input = "  rain()   and   night()  ";

		// Act
		var result = ExpressionParser.ParseExpression(input);

		// Assert
		Assert.NotNull(result);
		Assert.IsType<AndExpression>(result);
	}
}