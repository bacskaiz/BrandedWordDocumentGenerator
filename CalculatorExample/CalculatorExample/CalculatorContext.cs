using System;
using System.Collections.Generic;

namespace CalculatorExample
{
	public class CalculatorContext
	{
		public Dictionary<string, double> Variables;

		public CalculatorContext()
		{
			Variables = new Dictionary<string, double>();
		}

		public CalculatorContext(Dictionary<string, double> variables)
		{
			Variables = variables;
		}

		public void SetVariable(string name, double value)
		{
			if(Variables.ContainsKey(name))
			{
				Variables[name] = value;
			}
			else
			{
				Variables.Add(name, value);
			}
		}
	}
}
