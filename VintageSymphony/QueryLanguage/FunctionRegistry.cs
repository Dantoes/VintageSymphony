using System;
using System.Collections.Generic;

namespace VintageSymphony.QueryLanguage;

/// <summary>
/// Default implementation of IFunctionRegistry that manages function callbacks.
/// </summary>
public class FunctionRegistry : IFunctionRegistry
{
	private readonly Dictionary<string, Func<bool>> parameterlessFunctions = new();
	private readonly Dictionary<string, Func<object[], bool>> parameteredFunctions = new();

	/// <inheritdoc />
	public void RegisterFunction(string name, Func<bool> callback)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Function name cannot be null or empty", nameof(name));
		
		parameterlessFunctions[name.ToLowerInvariant()] = callback ?? throw new ArgumentNullException(nameof(callback));
	}

	/// <inheritdoc />
	public void RegisterFunction(string name, Func<object[], bool> callback)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Function name cannot be null or empty", nameof(name));
		
		parameteredFunctions[name.ToLowerInvariant()] = callback ?? throw new ArgumentNullException(nameof(callback));
	}

	/// <inheritdoc />
	public bool CallFunction(string name, params object[] parameters)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Function name cannot be null or empty", nameof(name));

		var lowerName = name.ToLowerInvariant();
		
		// If no parameters provided, try parameterless function first
		if (parameters == null || parameters.Length == 0)
		{
			if (parameterlessFunctions.TryGetValue(lowerName, out var parameterlessFunc))
				return parameterlessFunc();
			
			if (parameteredFunctions.TryGetValue(lowerName, out var parameteredFunc))
				return parameteredFunc(Array.Empty<object>());
		}
		else
		{
			// Parameters provided, use parametered function
			if (parameteredFunctions.TryGetValue(lowerName, out var parameteredFunc))
				return parameteredFunc(parameters);
		}

		throw new InvalidOperationException($"Function '{name}' is not registered");
	}

	/// <inheritdoc />
	public bool HasFunction(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			return false;
		
		var lowerName = name.ToLowerInvariant();
		return parameterlessFunctions.ContainsKey(lowerName) || parameteredFunctions.ContainsKey(lowerName);
	}
}