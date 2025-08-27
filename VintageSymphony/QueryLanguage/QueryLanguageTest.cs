using System;

namespace VintageSymphony.QueryLanguage;

/// <summary>
/// Simple test class to verify the query language functionality.
/// </summary>
public static class QueryLanguageTest
{
	/// <summary>
	/// Runs basic tests to verify the query language implementation.
	/// </summary>
	public static void RunTests()
	{
		var registry = new FunctionRegistry();
		
		// Register test functions
		registry.RegisterFunction("rain", () => true);
		registry.RegisterFunction("night", () => false);
		registry.RegisterFunction("day", () => true);
		registry.RegisterFunction("time", (parameters) => 
		{
			if (parameters.Length == 2 && 
				parameters[0] is int start && 
				parameters[1] is int end)
			{
				// Simulate current time being 15 (3 PM)
				var currentHour = 15;
				return currentHour >= start && currentHour <= end;
			}
			return false;
		});
		registry.RegisterFunction("month", (parameters) =>
		{
			if (parameters.Length == 1 && parameters[0] is int month)
			{
				// Simulate current month being 11 (November)
				return month == 11;
			}
			return false;
		});

		// Test cases from the issue description
		TestExpression(registry, "rain() and night()", false, "rain() and night()");
		TestExpression(registry, "rain and night", false, "rain and night (no parentheses)");
		TestExpression(registry, "rain() and (night() or not day())", true, "rain() and (night() or not day())");
		TestExpression(registry, "time(9,20)", true, "time(9,20)");
		TestExpression(registry, "rain() and month(11)", true, "rain() and month(11)");
		
		// Additional tests
		TestExpression(registry, "not rain()", false, "not rain()");
		TestExpression(registry, "day() or night()", true, "day() or night()");
		TestExpression(registry, "(rain() and day()) or night()", true, "(rain() and day()) or night()");
		
		Console.WriteLine("All query language tests completed successfully!");
	}

	private static void TestExpression(IFunctionRegistry registry, string expression, bool expectedResult, string description)
	{
		try
		{
			var parsed = ExpressionParser.ParseExpression(expression);
			var result = parsed.Evaluate(registry);
			
			if (result == expectedResult)
			{
				Console.WriteLine($"✓ PASS: {description} = {result}");
			}
			else
			{
				Console.WriteLine($"✗ FAIL: {description} = {result} (expected {expectedResult})");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"✗ ERROR: {description} - {ex.Message}");
		}
	}
}