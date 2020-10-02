using System;

namespace CalculatorExample.Operators
{
	public class Power : IOperator
	{
		public int Precedence { get; } = 4;

		public double Apply(double x, double y)
		{
			return Math.Pow(x, y);
		}
	}
}
