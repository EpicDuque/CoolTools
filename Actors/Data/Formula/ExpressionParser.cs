/* * * * * * * * * * * * * *
* A simple expression parser
* --------------------------
*
* The parser can parse a mathematical expression into a simple custom
* expression tree. It can recognise methods and fields/contants which
* are user extensible. It can also contain expression parameters which
* are registrated automatically. An expression tree can be "converted"
* into a delegate.
*
* Written by Bunny83
* 2014-11-02
*
* Features:
* - Elementary arithmetic [ + - * / ]
* - Power [ ^ ]
* - Brackets ( )
* - Most function from System.Math (abs, sin, round, floor, min, ...)
* - Constants ( e, PI )
* - MultiValue return (quite slow, produce extra garbage each call)
*
* * * * * * * * * * * * * */

using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ZLinq;
using Random = System.Random;

namespace B83.ExpressionParser
{
    public interface IValue
    {
        double Value { get; }
    }
    
    public class Number : IValue
    {
        public double Value { get; set; }

        public Number(double aValue)
        {
            Value = aValue;
        }
        
        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }
    }
    
    public class OperationSum : IValue
    {
        private readonly IValue[] m_Values;
        
        private static readonly List<IValue> operationValues = new();
        private static readonly StringBuilder sb = new();

        public double Value => m_Values.Sum(v => v.Value);

        public OperationSum(params IValue[] aValues)
        {
            operationValues.Clear();
            
            foreach (var I in aValues)
            {
                if (I is not OperationSum sum)
                    operationValues.Add(I);
                else
                    operationValues.AddRange(sum.m_Values);
            }
            
            m_Values = operationValues.ToArray();
        }
        
        public override string ToString()
        {
            sb.Clear();
            sb.Append("( ");

            for(int i = 0; i < m_Values.Length; i++)
            {
                sb.Append(m_Values[i]);
                
                if(i != m_Values.Length - 1)
                    sb.Append(" + ");
            }
            
            sb.Append(" )");
            return sb.ToString();
        }
    }
    
    public class OperationProduct : IValue
    {
        private readonly IValue[] m_Values;
        
        public double Value => m_Values.AsValueEnumerable()
            .Select(v => v.Value).Aggregate((v1, v2) => v1 * v2);

        public OperationProduct(params IValue[] aValues)
        {
            m_Values = aValues;
        }
        
        public override string ToString()
        {
            return "( " + string.Join(" * ", m_Values.Select(v => v.ToString()).ToArray()) + " )";
        }
    }
    
    public class OperationPower : IValue
    {
        private readonly IValue m_Value;
        private readonly IValue m_Power;
        
        public double Value => Math.Pow(m_Value.Value, m_Power.Value);

        public OperationPower(IValue aValue, IValue aPower)
        {
            m_Value = aValue;
            m_Power = aPower;
        }
        
        public override string ToString()
        {
            return "( " + m_Value + "^" + m_Power + " )";
        }
    }
    
    public class OperationNegate : IValue
    {
        private readonly IValue m_Value;
        public double Value => -m_Value.Value;

        public OperationNegate(IValue aValue)
        {
            m_Value = aValue;
        }
        
        public override string ToString()
        {
            return "( -" + m_Value + " )";
        }
    }
    
    public class OperationReciprocal : IValue
    {
        private readonly IValue m_Value;
        public double Value => 1.0 / m_Value.Value;

        public OperationReciprocal(IValue aValue)
        {
            m_Value = aValue;
        }
        
        public override string ToString()
        {
            return "( 1/" + m_Value + " )";
        }
    }
    
    public class MultiParameterList : IValue
    {
        public IValue[] Parameters { get; }

        public double Value => Parameters.Select(v => v.Value).FirstOrDefault();

        public MultiParameterList(params IValue[] aValues)
        {
            Parameters = aValues;
        }
        
        public override string ToString() => string.Join(", ", Parameters.Select(v => v.ToString()).ToArray());
    }
    
    public class CustomFunction : IValue
    {
        private readonly IValue[] m_Params;
        private readonly Func<double[], double> m_Delegate;
        private readonly string m_Name;
        
        public double Value => m_Delegate(m_Params?.Select(p => p.Value).ToArray());

        public CustomFunction(string aName, Func<double[], double> aDelegate, params IValue[] aValues)
        {
            m_Delegate = aDelegate;
            m_Params = aValues;
            m_Name = aName;
        }
        
        public override string ToString()
        {
            if (m_Params == null) return m_Name;
            
            return m_Name + "( " + string.Join(", ", m_Params.Select(v => v.ToString()).ToArray()) + " )";
        }
    }
    
    public class Parameter : Number
    {
        private string Name { get; }
        
        public override string ToString()
        {
            return Name+"["+base.ToString()+"]";
        }
        
        public Parameter(string aName) : base(0)
        {
            Name = aName;
        }
    }
    
    public class Expression : IValue
    {
        public readonly Dictionary<string, Parameter> Parameters = new ();
        private readonly Parameter[] m_ParamList = new Parameter[32];
        
        public IValue ExpressionTree { get; set; }
        
        public double Value => ExpressionTree.Value;

        public override string ToString()
        {
            return ExpressionTree.ToString();
        }
        
        public ExpressionDelegate ToDelegate(string[] aParamOrder)
        {
            RefreshParamOrder(aParamOrder);

            return p => Invoke(p, m_ParamList);
        }

        public void RefreshParamOrder(string[] aParamOrder)
        {
            for (int i = 0; i < m_ParamList.Length; i++)
            {
                m_ParamList[i] = null;
            }

            foreach (var kvp in Parameters)
            {
                kvp.Value.Value = default;
            }

            var index = 0;
            foreach (var str in aParamOrder)
            {
                m_ParamList[index] = Parameters.GetValueOrDefault(str);
                
                if(m_ParamList[index] != null)
                    m_ParamList[index].Value = default;

                index++;
            }
        }

        // public MultiResultDelegate ToMultiResultDelegate(params string[] aParamOrder)
        // {
        //     var parameters = new List<Parameter>(aParamOrder.Length);
        //     
        //     foreach (var t in aParamOrder)
        //     {
        //         parameters.Add(Parameters.ContainsKey(t) ? Parameters[t] : null);
        //     }
        //     
        //     var parameters2 = parameters.ToArray();
        //     
        //     return (p) => InvokeMultiResult(p, parameters2);
        // }
        
        private double Invoke(double[] aParams, Parameter[] aParamList)
        {
            int count = Math.Min(aParamList.Length, aParams.Length);
            
            for (int i = 0; i < count; i++ )
            {
                if (aParamList[i] != null)
                    aParamList[i].Value = aParams[i];
            }
            
            return Value;
        }
        
        // double[] InvokeMultiResult(double[] aParams, Parameter[] aParamList)
        // {
        //     int count = System.Math.Min(aParamList.Length, aParams.Length);
        //     for (int i = 0; i < count; i++)
        //     {
        //         if (aParamList[i] != null)
        //             aParamList[i].Value = aParams[i];
        //     }
        //     return MultiValue;
        // }

        public class ParameterException : Exception { public ParameterException(string aMessage) : base(aMessage) { } }
    }
    
    public delegate double ExpressionDelegate(params double[] aParams);
    
    public delegate double[] MultiResultDelegate(params double[] aParams);
    
    public class ExpressionParser
    {
        private readonly List<string> m_BracketHeap = new ();
        private static readonly Dictionary<string, Func<double>> m_Consts = new ();
        private static readonly Dictionary<string, Func<double[], double>> m_Funcs = new ();

        public static string[] FunctionNames => m_Funcs.Keys.ToArray();
        
        private Expression m_Context;
        
        public ExpressionParser()
        {
            var rnd = new Random();

            if (m_Consts.Count == 0)
            {
                m_Consts.Add("PI", () => Math.PI);
                m_Consts.Add("e", () => Math.E);
            }

            if (m_Funcs.Count > 0) return;
            
            m_Funcs.Add("sqrt", (p) => Math.Sqrt(p.FirstOrDefault()));
            m_Funcs.Add("abs", (p) => Math.Abs(p.FirstOrDefault()));
            m_Funcs.Add("ln", (p) => Math.Log(p.FirstOrDefault()));
            m_Funcs.Add("floor", (p) => Math.Floor(p.FirstOrDefault()));
            m_Funcs.Add("ceiling", (p) => Math.Ceiling(p.FirstOrDefault()));
            m_Funcs.Add("round", (p) => Math.Round(p.FirstOrDefault()));
            m_Funcs.Add("sin", (p) => Math.Sin(p.FirstOrDefault()));
            m_Funcs.Add("cos", (p) => Math.Cos(p.FirstOrDefault()));
            m_Funcs.Add("tan", (p) => Math.Tan(p.FirstOrDefault()));
            m_Funcs.Add("asin", (p) => Math.Asin(p.FirstOrDefault()));
            m_Funcs.Add("acos", (p) => Math.Acos(p.FirstOrDefault()));
            m_Funcs.Add("atan", (p) => Math.Atan(p.FirstOrDefault()));
            m_Funcs.Add("atan2", (p) => Math.Atan2(p.FirstOrDefault(),p.ElementAtOrDefault(1)));
            
            m_Funcs.Add("min", (p) => Math.Min(p.FirstOrDefault(), p.ElementAtOrDefault(1)));
            m_Funcs.Add("max", (p) => Math.Max(p.FirstOrDefault(), p.ElementAtOrDefault(1)));
            m_Funcs.Add("clamp", (p) => Math.Clamp(p.FirstOrDefault(), p.ElementAtOrDefault(1), p.ElementAtOrDefault(2)));
            m_Funcs.Add("greater", (p) => p.FirstOrDefault() > p.ElementAtOrDefault(1) ? 1 : 0);
            m_Funcs.Add("less", (p) => p.FirstOrDefault() < p.ElementAtOrDefault(1) ? 1 : 0);
            m_Funcs.Add("distance", (p) => Math.Sqrt(p.FirstOrDefault() * p.FirstOrDefault() +
                                                     p.ElementAtOrDefault(1) * p.ElementAtOrDefault(1) +
                                                     p.ElementAtOrDefault(2) * p.ElementAtOrDefault(2)));
            
            m_Funcs.Add("select", (p) => p.FirstOrDefault() <= 0 ? p.ElementAtOrDefault(1) : p.ElementAtOrDefault(2));

            m_Funcs.Add("rnd", (p) =>
            {
                if (p.Length == 2)
                    return p[0] + rnd.NextDouble() * (p[1] - p[0]);
                if (p.Length == 1)
                    return rnd.NextDouble() * p[0];
                return rnd.NextDouble();
            });
        }

        private int FindClosingBracket(ref string aText, int aStart, char aOpen, char aClose)
        {
            int counter = 0;
            for (int i = aStart; i < aText.Length; i++)
            {
                if (aText[i] == aOpen)
                    counter++;
                if (aText[i] == aClose)
                    counter--;
                if (counter == 0)
                    return i;
            }
            return -1;
        }
        
        void SubstitudeBracket(ref string aExpression, int aIndex)
        {
            int closing = FindClosingBracket(ref aExpression, aIndex, '(', ')');
            if (closing > aIndex + 1)
            {
                string inner = aExpression.Substring(aIndex + 1, closing - aIndex - 1);
                m_BracketHeap.Add(inner);
                
                string sub = "&" + (m_BracketHeap.Count - 1) + ";";
                
                aExpression = aExpression.Substring(0, aIndex) + sub + aExpression.Substring(closing + 1);
            }
            else throw new ParseException("Bracket not closed!");
        }

        private IValue Parse(string aExpression)
        {
            aExpression = aExpression.Trim();
            int index = aExpression.IndexOf('(');
            while (index >= 0)
            {
                SubstitudeBracket(ref aExpression, index);
                index = aExpression.IndexOf('(');
            }
            
            if (aExpression.Contains(','))
            {
                var parts = aExpression.Split(',');
                
                var exp = new List<IValue>(parts.Length);
                
                for (int i = 0; i < parts.Length; i++)
                {
                    string s = parts[i].Trim();
                    
                    if (!string.IsNullOrEmpty(s))
                        exp.Add(Parse(s));
                }
                
                return new MultiParameterList(exp.ToArray());
            }
            
            if (aExpression.Contains('+'))
            {
                var parts = aExpression.Split('+');
                
                var exp = new List<IValue>(parts.Length);
                
                for (int i = 0; i < parts.Length; i++)
                {
                    string s = parts[i].Trim();
                    
                    if (!string.IsNullOrEmpty(s))
                        exp.Add(Parse(s));
                }
                
                if (exp.Count == 1) return exp[0];
                
                return new OperationSum(exp.ToArray());
            }

            if (aExpression.Contains('-'))
            {
                var parts = aExpression.Split('-');
                
                var exp = new List<IValue>(parts.Length);

                if (!string.IsNullOrEmpty(parts[0].Trim()))
                    exp.Add(Parse(parts[0]));
                
                for (int i = 1; i < parts.Length; i++)
                {
                    string s = parts[i].Trim();
                    
                    if (!string.IsNullOrEmpty(s))
                        exp.Add(new OperationNegate(Parse(s)));
                }
                
                if (exp.Count == 1) return exp[0];
                
                return new OperationSum(exp.ToArray());
            }
            
            if (aExpression.Contains('*'))
            {
                var parts = aExpression.Split('*');
                var exp = new List<IValue>(parts.Length);
                
                for (int i = 0; i < parts.Length; i++)
                {
                    exp.Add(Parse(parts[i]));
                }
                
                if (exp.Count == 1)
                    return exp[0];
                
                return new OperationProduct(exp.ToArray());
            }
            
            if (aExpression.Contains('/'))
            {
                var parts = aExpression.Split('/');
                
                var exp = new List<IValue>(parts.Length);
                
                if (!string.IsNullOrEmpty(parts[0].Trim()))
                    exp.Add(Parse(parts[0]));
                
                for (int i = 1; i < parts.Length; i++)
                {
                    var s = parts[i].Trim();
                    if (!string.IsNullOrEmpty(s))
                        exp.Add(new OperationReciprocal(Parse(s)));
                }
                
                return new OperationProduct(exp.ToArray());
            }
            
            if (aExpression.Contains('^'))
            {
                int pos = aExpression.IndexOf('^');
                var val = Parse(aExpression.Substring(0, pos));
                var pow = Parse(aExpression.Substring(pos + 1));
                return new OperationPower(val, pow);
            }
            
            int pPos = aExpression.IndexOf("&", StringComparison.Ordinal);
            
            if (pPos > 0)
            {
                string fName = aExpression.Substring(0, pPos);
                foreach (var M in m_Funcs)
                {
                    if (fName == M.Key)
                    {
                        var inner = aExpression.Substring(M.Key.Length);
                        var param = Parse(inner);
                        IValue[] parameters;
                        if (param is MultiParameterList multiParams)
                            parameters = multiParams.Parameters;
                        else
                            parameters = new[] { param };
                        return new CustomFunction(M.Key, M.Value, parameters);
                    }
                }
            }
            
            foreach (var C in m_Consts)
            {
                if (aExpression == C.Key)
                {
                    return new CustomFunction(C.Key,(p)=> C.Value(),null);
                }
            }
            
            int index2a = aExpression.IndexOf('&');
            int index2b = aExpression.IndexOf(';');
            
            if (index2a >= 0 && index2b >= 2)
            {
                var inner = aExpression.Substring(index2a + 1, index2b - index2a - 1);
                
                if (int.TryParse(inner, out var bracketIndex) && bracketIndex >= 0 && bracketIndex < m_BracketHeap.Count)
                {
                    return Parse(m_BracketHeap[bracketIndex]);
                }
                
                throw new ParseException("Can't parse substitude token");
            }

            if (double.TryParse(aExpression, out var doubleValue))
            {
                return new Number(doubleValue);
            }
            
            if (ValidIdentifier(aExpression))
            {
                if (m_Context.Parameters.ContainsKey(aExpression))
                    return m_Context.Parameters[aExpression];
                
                var val = new Parameter(aExpression);
                m_Context.Parameters.Add(aExpression, val);
                
                return val;
            }
            
            throw new ParseException($"Expression: {aExpression} Reached unexpected end within the parsing tree.");
        }
        
        private bool ValidIdentifier(string aExpression)
        {
            aExpression = aExpression.Trim();
            if (string.IsNullOrEmpty(aExpression))
                return false;
            if (aExpression.Length < 1)
                return false;
            if (aExpression.Contains(" "))
                return false;
            if (!"abcdefghijklmnopqrstuvwxyz§$".Contains(char.ToLower(aExpression[0])))
                return false;
            if (m_Consts.ContainsKey(aExpression))
                return false;
            if (m_Funcs.ContainsKey(aExpression))
                return false;
            return true;
        }
        
        public Expression GetExpressionFromString(string aExpression)
        {
            var val = new Expression();
            m_Context = val;
            
            val.ExpressionTree = Parse(aExpression);
            
            m_Context = null;
            m_BracketHeap.Clear();
            
            return val;
        }
        
        public class ParseException : System.Exception { public ParseException(string aMessage) : base(aMessage) { } }
    }
}