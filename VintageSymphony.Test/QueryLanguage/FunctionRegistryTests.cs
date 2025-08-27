using System;
using Xunit;
using VintageSymphony.QueryLanguage;

namespace VintageSymphony.Test.QueryLanguage;

public class FunctionRegistryTests
{
	[Fact]
	public void RegisterFunction_ParameterlessFunction_Success()
	{
		// Arrange
		var registry = new FunctionRegistry();
		var called = false;
		Func<bool> callback = () => { called = true; return true; };

		// Act
		registry.RegisterFunction("test", callback);

		// Assert
		Assert.True(registry.HasFunction("test"));
		
		// Verify callback is called
		var result = registry.CallFunction("test");
		Assert.True(result);
		Assert.True(called);
	}

	[Fact]
	public void RegisterFunction_ParameteredFunction_Success()
	{
		// Arrange
		var registry = new FunctionRegistry();
		var receivedParams = Array.Empty<object>();
		Func<object[], bool> callback = (params_) => { receivedParams = params_; return true; };

		// Act
		registry.RegisterFunction("test", callback);

		// Assert
		Assert.True(registry.HasFunction("test"));
		
		// Verify callback is called with parameters
		var result = registry.CallFunction("test", 1, "hello");
		Assert.True(result);
		Assert.Equal(2, receivedParams.Length);
		Assert.Equal(1, receivedParams[0]);
		Assert.Equal("hello", receivedParams[1]);
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public void RegisterFunction_NullOrEmptyName_ThrowsArgumentException(string functionName)
	{
		// Arrange
		var registry = new FunctionRegistry();
		Func<bool> callback = () => true;

		// Act & Assert
		Assert.Throws<ArgumentException>(() => registry.RegisterFunction(functionName, callback));
	}

	[Fact]
	public void RegisterFunction_NullParameterlessCallback_ThrowsArgumentNullException()
	{
		// Arrange
		var registry = new FunctionRegistry();

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => registry.RegisterFunction("test", (Func<bool>)null));
	}

	[Fact]
	public void RegisterFunction_NullParameteredCallback_ThrowsArgumentNullException()
	{
		// Arrange
		var registry = new FunctionRegistry();

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => registry.RegisterFunction("test", (Func<object[], bool>)null));
	}

	[Fact]
	public void RegisterFunction_CaseInsensitive_Success()
	{
		// Arrange
		var registry = new FunctionRegistry();
		registry.RegisterFunction("Test", () => true);

		// Act & Assert
		Assert.True(registry.HasFunction("test"));
		Assert.True(registry.HasFunction("TEST"));
		Assert.True(registry.HasFunction("Test"));
		
		Assert.True(registry.CallFunction("test"));
		Assert.True(registry.CallFunction("TEST"));
		Assert.True(registry.CallFunction("Test"));
	}

	[Fact]
	public void RegisterFunction_OverwriteExisting_Success()
	{
		// Arrange
		var registry = new FunctionRegistry();
		registry.RegisterFunction("test", () => false);

		// Act
		registry.RegisterFunction("test", () => true);

		// Assert
		Assert.True(registry.CallFunction("test"));
	}

	[Fact]
	public void CallFunction_ParameterlessWithNoParams_Success()
	{
		// Arrange
		var registry = new FunctionRegistry();
		registry.RegisterFunction("test", () => true);

		// Act & Assert
		Assert.True(registry.CallFunction("test"));
		Assert.True(registry.CallFunction("test", Array.Empty<object>()));
	}

	[Fact]
	public void CallFunction_ParameteredWithNoParams_Success()
	{
		// Arrange
		var registry = new FunctionRegistry();
		registry.RegisterFunction("test", (params_) => params_.Length == 0);

		// Act & Assert
		Assert.True(registry.CallFunction("test"));
		Assert.True(registry.CallFunction("test", Array.Empty<object>()));
	}

	[Fact]
	public void CallFunction_ParameteredWithParams_Success()
	{
		// Arrange
		var registry = new FunctionRegistry();
		registry.RegisterFunction("test", (params_) => params_.Length > 0);

		// Act & Assert
		Assert.True(registry.CallFunction("test", 1, 2, 3));
	}

	[Fact]
	public void CallFunction_PreferParameterlessWhenNoParams_Success()
	{
		// Arrange
		var registry = new FunctionRegistry();
		var parameterlessCalled = false;
		var parameteredCalled = false;
		
		registry.RegisterFunction("test", () => { parameterlessCalled = true; return true; });
		registry.RegisterFunction("test", (params_) => { parameteredCalled = true; return false; });

		// Act
		var result = registry.CallFunction("test");

		// Assert - Should prefer parameterless version when no parameters are provided
		Assert.True(result);
		Assert.True(parameterlessCalled);
		Assert.False(parameteredCalled);
	}

	[Fact]
	public void CallFunction_UseParameteredWhenParamsProvided_Success()
	{
		// Arrange
		var registry = new FunctionRegistry();
		registry.RegisterFunction("test", () => false);
		registry.RegisterFunction("test", (params_) => true);

		// Act
		var result = registry.CallFunction("test", 1);

		// Assert - Should use parametered version when parameters are provided
		Assert.True(result);
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public void CallFunction_NullOrEmptyName_ThrowsArgumentException(string functionName)
	{
		// Arrange
		var registry = new FunctionRegistry();

		// Act & Assert
		Assert.Throws<ArgumentException>(() => registry.CallFunction(functionName));
	}

	[Fact]
	public void CallFunction_UnregisteredFunction_ThrowsInvalidOperationException()
	{
		// Arrange
		var registry = new FunctionRegistry();

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => registry.CallFunction("nonexistent"));
	}

	[Fact]
	public void HasFunction_ExistingFunction_ReturnsTrue()
	{
		// Arrange
		var registry = new FunctionRegistry();
		registry.RegisterFunction("test", () => true);

		// Act & Assert
		Assert.True(registry.HasFunction("test"));
	}

	[Fact]
	public void HasFunction_NonExistingFunction_ReturnsFalse()
	{
		// Arrange
		var registry = new FunctionRegistry();

		// Act & Assert
		Assert.False(registry.HasFunction("nonexistent"));
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public void HasFunction_NullOrEmptyName_ReturnsFalse(string functionName)
	{
		// Arrange
		var registry = new FunctionRegistry();

		// Act & Assert
		Assert.False(registry.HasFunction(functionName));
	}

	[Fact]
	public void CallFunction_NullParameters_TreatedAsEmpty()
	{
		// Arrange
		var registry = new FunctionRegistry();
		registry.RegisterFunction("test", () => true);

		// Act & Assert
		Assert.True(registry.CallFunction("test", null));
	}

	[Fact]
	public void FunctionCallback_ExceptionHandling_Propagates()
	{
		// Arrange
		var registry = new FunctionRegistry();
		registry.RegisterFunction("test", () => throw new InvalidOperationException("Test exception"));

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => registry.CallFunction("test"));
	}
}