using CalculatorExample.Operators;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CalculatorExample
{
    /// <summary>
    /// Basic stack-based mathematical calculator
    /// </summary>
    public class Calculator
    {
        private static readonly Dictionary<string, IOperator> _operators = new Dictionary<string, IOperator>()
        {
            { "(", new Parentheses() },
            { ")", new Parentheses() },
            { "+", new Plus() },
            { "-", new Minus() },
            { "*", new Multiplication() },
            { "/", new Division() },
            { "%", new Modulo() },
            { "^", new Power() }
        };

        private readonly CalculatorContext _context;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="context">The context of the calculations (values of the variables)</param>
        public Calculator(CalculatorContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Evaluate the expression with the given context
        /// </summary>
        /// <param name="expression">Mathematical expression</param>
        /// <returns>Result of the expression</returns>
        public double Evaluate(string expression)
        {
            string[] infixTokens = Parse(expression);
            string[] postfixTokens = ReorderInfixToRPNPostfix(infixTokens);
            var result = EvaluateRPNTokens(postfixTokens);
            return result;
        }

        /// <summary>
        /// Parsing (split) a string expression to array of tokens (numbers, variable names, symbols...)
        /// </summary>
        /// <param name="expression">The expression to parse</param>
        /// <returns>The tokens in the expression</returns>
        private string[] Parse(string expression)
        {
            List<string> infixTokens = new List<string>();
            StringBuilder builder = new StringBuilder();

            foreach (char c in expression.ToCharArray())
            {
                if (Char.IsWhiteSpace(c))
                {
                    if (builder.Length > 0)
                    {
                        infixTokens.Add(builder.ToString());
                        builder.Clear();
                    }
                }
                else if (_operators.ContainsKey(c.ToString()))
                {
                    if (builder.Length > 0)
                    {
                        infixTokens.Add(builder.ToString());
                        builder.Clear();
                    }
                    infixTokens.Add(c.ToString());
                }
                else
                {
                    builder.Append(c);
                }
            }

            if (builder.Length > 0)
            {
                infixTokens.Add(builder.ToString());
                builder.Clear();
            }

            return infixTokens.ToArray();
        }

        /// <summary>
        /// Shunting-yard algorithm by Dijsktra: reorder the tokens to reverse (postfix) polish notation
        /// </summary>
        /// <param name="infixTokens">The tokens in infix order</param>
        /// <returns>The tokens in postfix order</returns>
        private string[] ReorderInfixToRPNPostfix(string[] infixTokens)
        {
            var stack = new Stack<string>();
            var postfixTokens = new Stack<string>();

            string st;
            string c;
            for (int i = 0; i < infixTokens.Length; i++)
            {
                c = infixTokens[i];
                if (!(_operators.ContainsKey(c)))
                {
                    postfixTokens.Push(c);
                }
                else
                {
                    if (c.Equals("("))
                    {
                        stack.Push("(");
                    }
                    else if (c.Equals(")"))
                    {
                        st = stack.Pop();
                        while (!(st.Equals("(")))
                        {
                            postfixTokens.Push(st);
                            st = stack.Pop();
                        }
                    }
                    else
                    {
                        while (stack.Count > 0)
                        {
                            st = stack.Pop();
                            if (IsPredecessor(st, c))
                            {
                                postfixTokens.Push(st);
                            }
                            else
                            {
                                stack.Push(st);
                                break;
                            }
                        }
                        stack.Push(c);
                    }
                }
            }
            while (stack.Count > 0)
            {
                postfixTokens.Push(stack.Pop());
            }

            return postfixTokens.Reverse().ToArray();
        }

        private bool IsPredecessor(string firstOperator, string secondOperator)
        {
            return (_operators[firstOperator].Precedence >= _operators[secondOperator].Precedence);
        }

        /// <summary>
        /// Evaluating the expression with stack
        /// </summary>
        /// <param name="postfixArray">The tokens in postfix order</param>
        /// <returns>The result of the expression</returns>
        private double EvaluateRPNTokens(string[] postfixArray)
        {
            Stack<double> stack = new Stack<double>();

            for (int i = 0; i < postfixArray.Length; i++)
            {
                string token = postfixArray[i];

                if(double.TryParse(token, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double value))
                {
                    stack.Push(value);
                }
                else if (_operators.ContainsKey(token))
                {
                    double second = stack.Pop();
                    double first = stack.Pop();

                    var subResult = _operators[token].Apply(first, second);

                    stack.Push(subResult);
                }
                else if (_context.Variables.ContainsKey(token))
                {
                    stack.Push(_context.Variables[token]);
                }
            }

            var result = stack.Pop();
            return result;
        }
    }
}
