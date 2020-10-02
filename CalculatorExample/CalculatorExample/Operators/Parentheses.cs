using System;

namespace CalculatorExample.Operators
{
	public class Parentheses : IOperator
	{
		public int Precedence { get; } = 0;

		public double Apply(double x, double y)
		{
			throw new InvalidOperationException();
		}
	}
}
