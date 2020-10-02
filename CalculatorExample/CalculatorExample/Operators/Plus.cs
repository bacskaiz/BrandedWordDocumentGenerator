namespace CalculatorExample.Operators
{
	public class Plus : IOperator
	{
		public int Precedence { get; } = 2;

		public double Apply(double x, double y)
		{
			return x + y;
		}
	}
}
