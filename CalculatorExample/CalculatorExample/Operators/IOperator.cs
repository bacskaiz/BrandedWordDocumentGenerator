namespace CalculatorExample.Operators
{
	public interface IOperator
	{
		int Precedence { get; }
		double Apply(double x, double y);
	}
}
