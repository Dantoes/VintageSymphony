using System;

namespace VintageSymphony.QueryLanguage;

/// <summary>
/// Registry for functions that can be called from expressions.
/// </summary>
public interface IFunctionRegistry
{
	/// <summary>
	/// Registers a function with no parameters.
	/// </summary>
	/// <param name="name">Function name</param>
	/// <param name="callback">Function callback that returns a boolean result</param>
	void RegisterFunction(string name, Func<bool> callback);

	/// <summary>
	/// Registers a function with parameters.
	/// </summary>
	/// <param name="name">Function name</param>
	/// <param name="callback">Function callback that takes parameters and returns a boolean result</param>
	void RegisterFunction(string name, Func<object[], bool> callback);

	/// <summary>
	/// Calls a function with the given name and parameters.
	/// </summary>
	/// <param name="name">Function name</param>
	/// <param name="parameters">Function parameters (can be empty)</param>
	/// <returns>Function result</returns>
	bool CallFunction(string name, params object[] parameters);

	/// <summary>
	/// Checks if a function is registered.
	/// </summary>
	/// <param name="name">Function name</param>
	/// <returns>True if function exists, false otherwise</returns>
	bool HasFunction(string name);
}