using CalculatorExample;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace CalculatorTests
{
	public class CalculatorTest
	{
		[Theory]
		[InlineData("3.5*(2+4)", 21.0)]
		[InlineData("11-mass/6.0", 7)]
		[InlineData("effluent*influent^2", 0.03)]
		public void Calculator_WithContext_EvaluateExpression_ShouldReturnCorrect(string expression, double expected)
		{
			var context = CreateContext();
			var calculator = new Calculator(context);
			var result = calculator.Evaluate(expression);
			result.Should().BeApproximately(expected, 0.0000000001);
		}

		private CalculatorContext CreateContext()
		{
			var variables = new Dictionary<string, double>()
			{
				{ "mass", 24.0 },
				{ "influent", Math.Sqrt(3) },
				{ "effluent", 0.01 }
			};
			return new CalculatorContext(variables);
		}
	}
}
