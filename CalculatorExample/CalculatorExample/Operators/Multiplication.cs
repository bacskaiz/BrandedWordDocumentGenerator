﻿namespace CalculatorExample.Operators
{
	public class Multiplication : IOperator
	{
		public int Precedence { get; } = 3;

		public double Apply(double x, double y)
		{
			return x * y;
		}
	}
}
