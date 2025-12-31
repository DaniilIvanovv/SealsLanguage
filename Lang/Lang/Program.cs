using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MiniLang
{
    enum TokenType
    {
        Number, String, Identifier,
        Plus, Minus, Star, Slash, Caret,
        LParen, RParen, Comma, Semicolon,
        Equal, DoubleEqual, NotEqual, Less, LessEqual, Greater, GreaterEqual,
        And, Or, Not,
        Assign, Let,
        EOF
    }

    class Token
    {
        public TokenType Type;
        public string Text;
        public Token(TokenType t, string txt) { Type = t; Text = txt; }
        public override string ToString() => $"{Type}:{Text}";
    }

    class Lexer
    {
        private readonly string _s;
        private int _i = 0;
        public Lexer(string s) { _s = s; }

        private char Current => _i < _s.Length ? _s[_i] : '\0';
        private void Next() => _i++;

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();
            while (_i < _s.Length)
            {
                if (char.IsWhiteSpace(Current)) { Next(); continue; }
                if (Current == '/' && Peek() == '/')
                {
                    while (Current != '\n' && Current != '\0') Next();
                    continue;
                }
                if (char.IsDigit(Current) || (Current == '.' && char.IsDigit(Peek())))
                {
                    var start = _i;
                    while (char.IsDigit(Current) || Current == '.') Next();
                    tokens.Add(new Token(TokenType.Number, _s.Substring(start, _i - start)));
                    continue;
                }
                if (Current == '"' || Current == '\'')
                {
                    var quote = Current; Next();
                    var sb = new StringBuilder();
                    while (Current != quote && Current != '\0')
                    {
                        if (Current == '\\')
                        {
                            Next();
                            if (Current == 'n') { sb.Append('\n'); Next(); }
                            else if (Current == 't') { sb.Append('\t'); Next(); }
                            else { sb.Append(Current); Next(); }
                        }
                        else { sb.Append(Current); Next(); }
                    }
                    if (Current == quote) Next();
                    tokens.Add(new Token(TokenType.String, sb.ToString()));
                    continue;
                }
                if (char.IsLetter(Current) || Current == '_')
                {
                    var start = _i;
                    while (char.IsLetterOrDigit(Current) || Current == '_') Next();
                    var word = _s.Substring(start, _i - start);
                    switch (word)
                    {
                        case "let": tokens.Add(new Token(TokenType.Let, word)); break;
                        case "true": tokens.Add(new Token(TokenType.Identifier, "true")); break;
                        case "false": tokens.Add(new Token(TokenType.Identifier, "false")); break;
                        case "and": tokens.Add(new Token(TokenType.And, "and")); break;
                        case "or": tokens.Add(new Token(TokenType.Or, "or")); break;
                        case "not": tokens.Add(new Token(TokenType.Not, "not")); break;
                        default: tokens.Add(new Token(TokenType.Identifier, word)); break;
                    }
                    continue;
                }
                switch (Current)
                {
                    case '+': tokens.Add(new Token(TokenType.Plus, "+")); Next(); break;
                    case '-': tokens.Add(new Token(TokenType.Minus, "-")); Next(); break;
                    case '*': tokens.Add(new Token(TokenType.Star, "*")); Next(); break;
                    case '/': tokens.Add(new Token(TokenType.Slash, "/")); Next(); break;
                    case '^': tokens.Add(new Token(TokenType.Caret, "^")); Next(); break;
                    case '(': tokens.Add(new Token(TokenType.LParen, "(")); Next(); break;
                    case ')': tokens.Add(new Token(TokenType.RParen, ")")); Next(); break;
                    case ',': tokens.Add(new Token(TokenType.Comma, ",")); Next(); break;
                    case ';': tokens.Add(new Token(TokenType.Semicolon, ";")); Next(); break;
                    case '!':
                        Next();
                        if (Current == '=') { Next(); tokens.Add(new Token(TokenType.NotEqual, "!=")); }
                        else tokens.Add(new Token(TokenType.Not, "!"));
                        break;
                    case '=':
                        Next();
                        if (Current == '=') { Next(); tokens.Add(new Token(TokenType.DoubleEqual, "==")); }
                        else tokens.Add(new Token(TokenType.Assign, "="));
                        break;
                    case '<':
                        Next();
                        if (Current == '=') { Next(); tokens.Add(new Token(TokenType.LessEqual, "<=")); }
                        else tokens.Add(new Token(TokenType.Less, "<"));
                        break;
                    case '>':
                        Next();
                        if (Current == '=') { Next(); tokens.Add(new Token(TokenType.GreaterEqual, ">=")); }
                        else tokens.Add(new Token(TokenType.Greater, ">"));
                        break;
                    case '&':
                        Next();
                        if (Current == '&') { Next(); tokens.Add(new Token(TokenType.And, "&&")); }
                        else tokens.Add(new Token(TokenType.And, "&"));
                        break;
                    case '|':
                        Next();
                        if (Current == '|') { Next(); tokens.Add(new Token(TokenType.Or, "||")); }
                        else tokens.Add(new Token(TokenType.Or, "|"));
                        break;
                    default:
                        throw new Exception($"Unrecognized char: {Current}");
                }
            }
            tokens.Add(new Token(TokenType.EOF, ""));
            return tokens;
        }

        private char Peek() => _i + 1 < _s.Length ? _s[_i + 1] : '\0';
    }

    abstract class Expr { }
    class NumberExpr : Expr { public double Value; public NumberExpr(double v) { Value = v; } }
    class StringExpr : Expr { public string Value; public StringExpr(string v) { Value = v; } }
    class BoolExpr : Expr { public bool Value; public BoolExpr(bool v) { Value = v; } }
    class VariableExpr : Expr { public string Name; public VariableExpr(string n) { Name = n; } }
    class BinaryExpr : Expr { public string Op; public Expr Left, Right; public BinaryExpr(string op, Expr l, Expr r) { Op = op; Left = l; Right = r; } }
    class UnaryExpr : Expr { public string Op; public Expr Right; public UnaryExpr(string op, Expr r) { Op = op; Right = r; } }
    class CallExpr : Expr { public string Name; public List<Expr> Args; public CallExpr(string name, List<Expr> args) { Name = name; Args = args; } }
    class AssignStmt : Expr { public string Name; public Expr Value; public AssignStmt(string name, Expr value) { Name = name; Value = value; } }

    class Parser
    {
        private readonly List<Token> _tokens;
        private int _i = 0;
        private Token Cur => _tokens[_i];
        public Parser(List<Token> tokens) { _tokens = tokens; }

        private Token Eat(TokenType t)
        {
            if (Cur.Type == t) { var tmp = Cur; _i++; return tmp; }
            throw new Exception($"Expected {t}, got {Cur.Type} ({Cur.Text})");
        }

        private bool Match(TokenType t) { if (Cur.Type == t) { _i++; return true; } return false; }

        public Expr ParseTopLevel()
        {
            Expr last = null;
            while (Cur.Type != TokenType.EOF)
            {
                last = ParseStatement();
                if (Cur.Type == TokenType.Semicolon) Match(TokenType.Semicolon);
                else if (Cur.Type != TokenType.EOF) { }
            }
            return last;
        }

        private Expr ParseStatement()
        {
            if (Cur.Type == TokenType.Let)
            {
                Eat(TokenType.Let);
                var id = Eat(TokenType.Identifier).Text;
                Eat(TokenType.Assign);
                var val = ParseExpr();
                return new AssignStmt(id, val);
            }
            return ParseExpr();
        }

        private Expr ParseExpr() => ParseOr();

        private Expr ParseOr()
        {
            var left = ParseAnd();
            while (Cur.Type == TokenType.Or)
            {
                _i++;
                var right = ParseAnd();
                left = new BinaryExpr("or", left, right);
            }
            return left;
        }

        private Expr ParseAnd()
        {
            var left = ParseEquality();
            while (Cur.Type == TokenType.And)
            {
                _i++;
                var right = ParseEquality();
                left = new BinaryExpr("and", left, right);
            }
            return left;
        }

        private Expr ParseEquality()
        {
            var left = ParseComparison();
            while (Cur.Type == TokenType.DoubleEqual || Cur.Type == TokenType.NotEqual)
            {
                _i++;
                var right = ParseComparison();
                left = new BinaryExpr(_tokens[_i-1].Text, left, right);
            }
            return left;
        }

        private Expr ParseComparison()
        {
            var left = ParseTerm();
            while (Cur.Type == TokenType.Less || Cur.Type == TokenType.LessEqual || Cur.Type == TokenType.Greater || Cur.Type == TokenType.GreaterEqual)
            {
                var op = Cur.Text; _i++;
                var right = ParseTerm();
                left = new BinaryExpr(op, left, right);
            }
            return left;
        }

        private Expr ParseTerm()
        {
            var left = ParseFactor();
            while (Cur.Type == TokenType.Plus || Cur.Type == TokenType.Minus)
            {
                var op = Cur.Text; _i++;
                var right = ParseFactor();
                left = new BinaryExpr(op, left, right);
            }
            return left;
        }

        private Expr ParseFactor()
        {
            var left = ParsePower();
            while (Cur.Type == TokenType.Star || Cur.Type == TokenType.Slash)
            {
                var op = Cur.Text; _i++;
                var right = ParsePower();
                left = new BinaryExpr(op, left, right);
            }
            return left;
        }

        private Expr ParsePower()
        {
            var left = ParseUnary();
            while (Cur.Type == TokenType.Caret)
            {
                _i++;
                var right = ParseUnary();
                left = new BinaryExpr("^", left, right);
            }
            return left;
        }

        private Expr ParseUnary()
        {
            if (Cur.Type == TokenType.Minus) { _i++; var r = ParseUnary(); return new UnaryExpr("-", r); }
            if (Cur.Type == TokenType.Not) { _i++; var r = ParseUnary(); return new UnaryExpr("!", r); }
            return ParsePrimary();
        }

        private Expr ParsePrimary()
        {
            if (Cur.Type == TokenType.Number)
            {
                var n = double.Parse(Cur.Text, CultureInfo.InvariantCulture);
                _i++; return new NumberExpr(n);
            }
            if (Cur.Type == TokenType.String)
            {
                var s = Cur.Text; _i++; return new StringExpr(s);
            }
            if (Cur.Type == TokenType.Identifier)
            {
                var name = Cur.Text; _i++;
                if (Cur.Type == TokenType.LParen)
                {
                    _i++;
                    var args = new List<Expr>();
                    if (Cur.Type != TokenType.RParen)
                    {
                        args.Add(ParseExpr());
                        while (Cur.Type == TokenType.Comma) { _i++; args.Add(ParseExpr()); }
                    }
                    Eat(TokenType.RParen);
                    return new CallExpr(name, args);
                }
                if (name == "true") return new BoolExpr(true);
                if (name == "false") return new BoolExpr(false);
                return new VariableExpr(name);
            }
            if (Cur.Type == TokenType.LParen)
            {
                Eat(TokenType.LParen);
                var e = ParseExpr();
                Eat(TokenType.RParen);
                return e;
            }
            throw new Exception($"Unexpected token {Cur.Type} ({Cur.Text})");
        }
    }

    class EvalContext
    {
        public Dictionary<string, object> Variables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, Func<List<object>, object>> Builtins = new Dictionary<string, Func<List<object>, object>>(StringComparer.OrdinalIgnoreCase);

        public EvalContext()
        {
            RegisterBuiltins();
        }

        private void RegisterBuiltins()
        {
            Builtins["sin"] = args => Math.Sin(ToNumber(args, 0));
            Builtins["cos"] = args => Math.Cos(ToNumber(args, 0));
            Builtins["tan"] = args => Math.Tan(ToNumber(args, 0));
            Builtins["sqrt"] = args => Math.Sqrt(ToNumber(args, 0));
            Builtins["pow"] = args => Math.Pow(ToNumber(args, 0), ToNumber(args, 1));
            Builtins["abs"] = args => Math.Abs(ToNumber(args, 0));
            Builtins["max"] = args => Math.Max(ToNumber(args, 0), ToNumber(args, 1));
            Builtins["min"] = args => Math.Min(ToNumber(args, 0), ToNumber(args, 1));
            Builtins["round"] = args => Math.Round(ToNumber(args, 0));
            Builtins["concat"] = args => string.Concat(args.Select(a => ToStringVal(a)));
            Builtins["len"] = args => (double)ToStringVal(args[0]).Length;
            Builtins["substr"] = args =>
            {
                var s = ToStringVal(args[0]);
                var start = (int)ToNumber(args, 1);
                if (args.Count == 2) return s.Substring(Math.Clamp(start, 0, s.Length));
                var len = (int)ToNumber(args, 2);
                start = Math.Clamp(start, 0, s.Length);
                var endLen = Math.Clamp(len, 0, s.Length - start);
                return s.Substring(start, endLen);
            };
            Builtins["upper"] = args => ToStringVal(args[0]).ToUpperInvariant();
            Builtins["lower"] = args => ToStringVal(args[0]).ToLowerInvariant();
            Builtins["trim"] = args => ToStringVal(args[0]).Trim();
            Builtins["if"] = args =>
            {
                var cond = ToBool(args[0]);
                return cond ? args[1] : (args.Count > 2 ? args[2] : null);
            };
            Builtins["print"] = args =>
            {
                foreach (var a in args) Console.Write(ToStringVal(a) + " ");
                Console.WriteLine();
                return null;
            };
        }

        private static string ToStringVal(object o) => o switch
        {
            null => "",
            double d when d % 1 == 0 => ((long)d).ToString(),
            double d => d.ToString(CultureInfo.InvariantCulture),
            bool b => b ? "true" : "false",
            string s => s,
            _ => o.ToString()
        };

        private static double ToNumber(List<object> args, int idx)
        {
            if (idx >= args.Count) throw new Exception("Not enough arguments");
            return ConvertToNumber(args[idx]);
        }

        private static double ConvertToNumber(object o)
        {
            if (o is double d) return d;
            if (o is bool b) return b ? 1.0 : 0.0;
            if (o is string s && double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)) return v;
            throw new Exception($"Cannot convert to number: {o}");
        }

        private static bool ToBool(object o)
        {
            if (o is bool b) return b;
            if (o is double d) return Math.Abs(d) > 1e-12;
            if (o is string s) return !string.IsNullOrEmpty(s);
            return o != null;
        }

        public object Evaluate(Expr node)
        {
            switch (node)
            {
                case NumberExpr n: return n.Value;
                case StringExpr s: return s.Value;
                case BoolExpr b: return b.Value;
                case VariableExpr v:
                    if (Variables.TryGetValue(v.Name, out var valVar)) return valVar;
                    throw new Exception($"Undefined variable '{v.Name}'");
                case AssignStmt asn:
                    var assignedValue = Evaluate(asn.Value);
                    Variables[asn.Name] = assignedValue;
                    return assignedValue;
                case UnaryExpr u:
                    var r = Evaluate(u.Right);
                    if (u.Op == "-") return -ConvertToNumber(r);
                    if (u.Op == "!") return !ToBool(r);
                    throw new Exception($"Unknown unary op {u.Op}");
                case BinaryExpr b:
                    return EvalBinary(b);
                case CallExpr c:
                    return EvalCall(c);
                default:
                    throw new Exception("Unknown node");
            }
        }


        private object EvalCall(CallExpr c)
        {
            var args = c.Args.Select(a => Evaluate(a)).ToList();
            if (Builtins.TryGetValue(c.Name, out var fn))
            {
                return fn(args);
            }
            throw new Exception($"Unknown function '{c.Name}'");
        }

        private object EvalBinary(BinaryExpr b)
        {
            if (b.Op == "and")
            {
                var left = Evaluate(b.Left);
                if (!ToBool(left)) return false;
                var right = Evaluate(b.Right);
                return ToBool(right);
            }
            if (b.Op == "or")
            {
                var left = Evaluate(b.Left);
                if (ToBool(left)) return true;
                var right = Evaluate(b.Right);
                return ToBool(right);
            }

            var l = Evaluate(b.Left);
            var r = Evaluate(b.Right);

            if (b.Op == "==") return AreEqual(l, r);
            if (b.Op == "!=") return !AreEqual(l, r);
            if (b.Op == "<" || b.Op == ">" || b.Op == "<=" || b.Op == ">=")
            {
                try
                {
                    var ln = ConvertToNumber(l);
                    var rn = ConvertToNumber(r);
                    return b.Op switch
                    {
                        "<" => ln < rn,
                        "<=" => ln <= rn,
                        ">" => ln > rn,
                        ">=" => ln >= rn,
                        _ => throw new Exception("Unreachable")
                    };
                }
                catch
                {
                    var ls = l?.ToString() ?? "";
                    var rs = r?.ToString() ?? "";
                    var cmp = string.Compare(ls, rs, StringComparison.Ordinal);
                    return b.Op switch
                    {
                        "<" => cmp < 0,
                        "<=" => cmp <= 0,
                        ">" => cmp > 0,
                        ">=" => cmp >= 0,
                        _ => throw new Exception("Unreachable")
                    };
                }
            }

            if (b.Op == "+")
            {
                if (l is string || r is string) return (l?.ToString() ?? "") + (r?.ToString() ?? "");
                return ConvertToNumber(l) + ConvertToNumber(r);
            }
            if (b.Op == "-") return ConvertToNumber(l) - ConvertToNumber(r);
            if (b.Op == "*") return ConvertToNumber(l) * ConvertToNumber(r);
            if (b.Op == "/") return ConvertToNumber(l) / ConvertToNumber(r);
            if (b.Op == "^") return Math.Pow(ConvertToNumber(l), ConvertToNumber(r));

            throw new Exception($"Unknown binary op {b.Op}");
        }

        private static bool AreEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            if (a is double da && b is double db) return Math.Abs(da - db) < 1e-12;
            if (a is bool ba && b is bool bb) return ba == bb;
            return a.ToString() == b.ToString();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.ForegroundColor = ConsoleColor.Magenta;

            Console.WriteLine("########################\n##   SEALS LANGUAGE   ##\n########################");

            Console.ResetColor();

            Console.WriteLine("SealsLanguage REPL — выражения с math, logic, text functions.");
            Console.WriteLine("Автор - Ahitle");
            Console.WriteLine("Версия 1.0");

            var ctx = new EvalContext();
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (line == null) break;
                if (line.Trim().ToLowerInvariant() == "exit") break;
                if (line.Trim() == "") continue;

                try
                {
                    var lex = new Lexer(line);
                    var tokens = lex.Tokenize();
                    var parser = new Parser(tokens);
                    var expr = parser.ParseTopLevel();
                    var res = ctx.Evaluate(expr);
                    if (res != null)
                    {
                        if (res is double d) Console.WriteLine(d % 1 == 0 ? ((long)d).ToString() : d.ToString(CultureInfo.InvariantCulture));
                        else Console.WriteLine(res);
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: " + ex.Message);
                    Console.ResetColor();
                }
            }
        }
    }
}
