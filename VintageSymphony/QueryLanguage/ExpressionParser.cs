using System;
using System.Globalization;
using System.Linq;
using Sprache;

namespace VintageSymphony.QueryLanguage;

/// <summary>
/// Parser for boolean expressions using Sprache library.
/// </summary>
public static class ExpressionParser
{
	// Token helper for whitespace handling
	private static Parser<T> Token<T>(Parser<T> parser) => 
		from leading in Sprache.Parse.WhiteSpace.Many()
		from item in parser
		from trailing in Sprache.Parse.WhiteSpace.Many()
		select item;

	private static Parser<string> TokenString(string text) =>
		from leading in Sprache.Parse.WhiteSpace.Many()
		from item in Sprache.Parse.String(text).Text()
		from trailing in Sprache.Parse.WhiteSpace.Many()
		select item;

	// Identifiers (function names)
	private static readonly Parser<string> Identifier =
		from first in Sprache.Parse.Letter.Or(Sprache.Parse.Char('_'))
		from rest in Sprache.Parse.LetterOrDigit.Or(Sprache.Parse.Char('_')).Many()
		select new string(new[] { first }.Concat(rest).ToArray());

	// Numbers (for function parameters)
	private static readonly Parser<object> Number =
		from sign in Sprache.Parse.Char('-').Optional()
		from integer in Sprache.Parse.Number
		from fraction in Sprache.Parse.Char('.').Then(_ => Sprache.Parse.Number).Optional()
		select (object)((sign.IsDefined && sign.Get() == '-' ? -1 : 1) *
			(fraction.IsDefined ? 
				double.Parse(integer + "." + fraction.Get(), CultureInfo.InvariantCulture) :
				int.Parse(integer)));

	// String literals (for function parameters)
	private static readonly Parser<object> StringLiteral =
		from openQuote in Sprache.Parse.Char('"')
		from content in Sprache.Parse.CharExcept('"').Many().Text()
		from closeQuote in Sprache.Parse.Char('"')
		select (object)content;

	// Parameter values
	private static readonly Parser<object> Parameter = Number.Or(StringLiteral);

	// Parameter list
	private static readonly Parser<object[]> ParameterList =
		from openParen in Token(Sprache.Parse.Char('('))
		from parameters in Parameter.DelimitedBy(Token(Sprache.Parse.Char(','))).Optional()
		from closeParen in Token(Sprache.Parse.Char(')'))
		select parameters.IsDefined ? parameters.Get().ToArray() : Array.Empty<object>();

	// Function call with optional parentheses
	private static readonly Parser<IExpression> FunctionCall =
		from name in Token(Identifier)
		from parameters in ParameterList.Optional()
		select (IExpression)new FunctionExpression(name, 
			parameters.IsDefined ? parameters.Get() : Array.Empty<object>());

	// Boolean literals - must be exact keywords, not part of larger identifiers
	private static readonly Parser<IExpression> BooleanLiteral =
		from leading in Sprache.Parse.WhiteSpace.Many()
		from keyword in (Sprache.Parse.String("true").Or(Sprache.Parse.String("false")))
		from notFollowedBy in Sprache.Parse.LetterOrDigit.Or(Sprache.Parse.Char('_')).Not()
		from trailing in Sprache.Parse.WhiteSpace.Many()
		select (IExpression)new LiteralExpression(keyword == "true");

	// Primary expressions (function calls, literals, or parenthesized expressions)
	private static readonly Parser<IExpression> Primary =
		BooleanLiteral
		.Or(FunctionCall)
		.Or(from openParen in TokenString("(")
			from expr in Sprache.Parse.Ref(() => OrExpression)
			from closeParen in TokenString(")")
			select expr);

	// NOT expression
	private static readonly Parser<IExpression> NotExpression =
		from not in TokenString("not")
		from expr in Primary
		select (IExpression)new NotExpression(expr);

	// Factor (NOT or Primary)
	private static readonly Parser<IExpression> Factor = NotExpression.Or(Primary);

	// AND expression
	private static readonly Parser<IExpression> AndExpression =
		from first in Factor
		from rest in (from op in TokenString("and")
					  from expr in Factor
					  select expr).Many()
		select rest.Aggregate(first, (acc, expr) => new AndExpression(acc, expr));

	// OR expression
	private static readonly Parser<IExpression> OrExpression =
		from first in AndExpression
		from rest in (from op in TokenString("or")
					  from expr in AndExpression
					  select expr).Many()
		select rest.Aggregate(first, (acc, expr) => new OrExpression(acc, expr));

	// Root expression parser
	private static readonly Parser<IExpression> ExpressionRoot =
		OrExpression.End();

	/// <summary>
	/// Parses a string expression into an IExpression tree.
	/// </summary>
	/// <param name="input">The expression string to parse</param>
	/// <returns>Parsed expression tree</returns>
	/// <exception cref="ParseException">Thrown when parsing fails</exception>
	public static IExpression ParseExpression(string input)
	{
		if (string.IsNullOrWhiteSpace(input))
			throw new ArgumentException("Expression cannot be null or empty", nameof(input));

		try
		{
			return ExpressionRoot.Parse(input);
		}
		catch (ParseException ex)
		{
			throw new ParseException($"Failed to parse expression '{input}': {ex.Message}", ex);
		}
	}

	/// <summary>
	/// Tries to parse a string expression into an IExpression tree.
	/// </summary>
	/// <param name="input">The expression string to parse</param>
	/// <param name="expression">The parsed expression if successful</param>
	/// <returns>True if parsing succeeded, false otherwise</returns>
	public static bool TryParseExpression(string input, out IExpression? expression)
	{
		expression = null;
		
		if (string.IsNullOrWhiteSpace(input))
			return false;

		try
		{
			expression = ExpressionRoot.Parse(input);
			return true;
		}
		catch
		{
			return false;
		}
	}
}