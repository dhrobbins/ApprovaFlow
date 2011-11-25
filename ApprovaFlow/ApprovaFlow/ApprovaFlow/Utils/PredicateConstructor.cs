using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ApprovaFlow.Utils
{
    /// <summary>
    /// General exception class for the <see cref="PredicateConstructor{TArg}"/>.
    /// </summary>
    public class PredicateConstructorException : Exception {
        /// <summary>
        /// Create a PredicateBuilderException that contains no message.
        /// </summary>
        public PredicateConstructorException() {
        }

        /// <summary>
        /// Create a PredicateBuilderException object with a syntax error message, see
        /// Message in the base class to get the syntax error back.
        /// </summary>
        /// <param name="message">Syntax error that was encountered.</param>
        public PredicateConstructorException(string message)
            : base(message) {
        }
    }

    /// <summary>
    /// Expression parser for a super simple grammar, see http://www.willamette.edu/~fruehr/348/lab3.html for more on
    /// grammars and parsing.
    /// 
    /// Code created by Scott Garland - http://scottgarland.com/Post/Display/Dynamically_Creating_LINQ_Expression_Predicates_From_A_String
    /// </summary>
    /// <typeparam name="TArg">The type of the arg.</typeparam>
    public class PredicateConstructor<TArg> {
        private static readonly Regex RegexBooleanNot = new Regex(@"^(\!)");
        private static readonly Regex RegexBooleanOp = new Regex(@"^(\|\||\&\&|\!)");
        private static readonly Regex RegexKeyword = new Regex(@"^(?<value>[A-Za-z0-9_]+)");
        private static readonly Regex RegexQoute = new Regex(@"^[""']");
        private static readonly Regex RegexQuotedValue = new Regex(@"([""'])(?:(?=(\\?))\2.)*?\1");
        private static readonly Regex RegexRelationalOp = new Regex(@"^(==|<>|!=|<=?|>=?)");
        private static readonly Regex RegexValue = new Regex(@"^(?<value>[^""',\s\)]+)");
        private TokenizerString _filterSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="PredicateConstructor&lt;TArg&gt;"/> class.
        /// </summary>
        public PredicateConstructor() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PredicateConstructor&lt;TArg&gt;"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public PredicateConstructor(string expression) {
            Predicate = Compile(expression);
        }

        /// <summary>
        /// Gets the compiled predicate, null if an expression has not been successfully compiled.
        /// </summary>
        /// <value>The predicate.</value>
        public Func<TArg, bool> Predicate { get; private set; }

        /// <summary>
        /// Compiles the specified expression and creates a predicate that can be accessed by
        /// the <see cref="Predicate">Predicate</see> property. Exceptions are thrown for syntax
        /// errors and illegal type operations.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public Func<TArg, bool> Compile(string expression) {
            ParameterExpression param = Expression.Parameter(typeof (TArg), "o");

            ParsedExpressionNode root = ParseExpression(expression);
            Expression condition = ConvertToExpression(param, root);
            Expression<Func<TArg, bool>> myLambda = Expression.Lambda<Func<TArg, bool>>(condition, param);

            Func<TArg, bool> z = myLambda.Compile();

            Predicate = z.Invoke;
            return Predicate;
        }

        public Expression<Func<TArg, bool>> CompileToExpression(string expression)
        {
            ParameterExpression param = Expression.Parameter(typeof(TArg), "o");

            ParsedExpressionNode root = ParseExpression(expression);
            Expression condition = ConvertToExpression(param, root);
            Expression<Func<TArg, bool>> myLambda = Expression.Lambda<Func<TArg, bool>>(condition, param);

            return myLambda;
        }

        /// <summary>
        /// Parses the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>Parse tree root</returns>
        private ParsedExpressionNode ParseExpression(string expression) {
            if (string.IsNullOrEmpty(expression)) {
                throw new InvalidOperationException();
            }
            _filterSource = new TokenizerString(expression);
            ParsedExpressionNode expressionTree = ParseExpression();
            return expressionTree;
        }

        /// <summary>
        /// Checks for valid propery or field in the type we are building an predicate function for.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>True if the name is a property or field.</returns>
        private static bool CheckForValidProperyOrField(string name) {
            try {
                return GetProperyOrFieldType(name) != null;
            }
            catch (PredicateConstructorException) {
                return false;
            }
        }

        /// <summary>
        /// Gets the type of the propery or field.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Type object.</returns>
        private static Type GetProperyOrFieldType(string name) {
            PropertyInfo pi = (from p in typeof (TArg).GetProperties() where p.Name.Equals(name) select p).FirstOrDefault();
            if (pi == null) {
                FieldInfo fi = (from p in typeof (TArg).GetFields() where p.Name.Equals(name) select p).FirstOrDefault();
                if (fi != null) {
                    return fi.FieldType;
                }
                throw new PredicateConstructorException("Can only build expression using Properties or Fields of objects");
            }
            return pi.PropertyType;
        }

        /// <summary>
        /// Determines whether if the name is a field in the TArg type.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// 	<c>true</c> if name is a field; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsFieldType(string name) {
            return (from p in typeof (TArg).GetFields() where p.Name.Equals(name) select p).FirstOrDefault() != null;
        }

        /// <summary>
        /// Determines whether name is a property in the TArg type.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// 	<c>true</c> if name is a property; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsPropertyType(string name) {
            return (from p in typeof (TArg).GetProperties() where p.Name.Equals(name) select p).FirstOrDefault() != null;
        }

        /// <summary>
        /// Determines whether type is a Nullable&lt;&gt; type.
        /// </summary>
        /// <param name="theType">The type.</param>
        /// <returns>
        /// 	<c>true</c> if theType is a Nullable&lt;&gt; type; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsNullableGenericType(Type theType) {
            return (theType.IsGenericType && theType.GetGenericTypeDefinition().Equals(typeof (Nullable<>)));
        }

        /// <summary>
        /// Converts the parse tree to an <see cref="Expression" />.
        /// </summary>
        /// <param name="parameter">The base object Expression.Parameter.</param>
        /// <param name="root">The root of the parse tree.</param>
        /// <returns>An <see cref="Expression{TDelegate}"/> that can be used to create runtime predicate.</returns>
        private static Expression ConvertToExpression(Expression parameter, ParsedExpressionNode root) {
            var binaryRoot = root as ParsedExpressionBinaryOperatorNode;
            if (binaryRoot != null && binaryRoot.Left is ParsedExpressionNameNode) {
                var left = (ParsedExpressionNameNode) binaryRoot.Left;
                var right = (ParsedExpressionValueNode) binaryRoot.Right;
                var op = (ParsedExpressionRelationalOperatorNode) binaryRoot;

                Expression target;
                if (IsFieldType(left.Name)) {
                    target = Expression.Field(parameter, left.Name);
                }
                else if (IsPropertyType(left.Name)) {
                    target = Expression.Property(parameter, left.Name);
                }
                else {
                    throw new PredicateConstructorException("Can only build expression using Properties or Fields of objects");
                }

                Type targetType = GetProperyOrFieldType(left.Name);

                if (!targetType.IsValueType && !right.Quoted && right.Value.Equals("null")) {
                    if (op.Operator == ParsedExpressionRelationalOperatorNode.RelationalOperator.EqualsOp) {
                        return Expression.Equal(target, Expression.Constant(null));
                    }
                    if (op.Operator == ParsedExpressionRelationalOperatorNode.RelationalOperator.NotEqualsOp) {
                        return Expression.NotEqual(target, Expression.Constant(null));
                    }
                    throw new PredicateConstructorException("Can only use equality operators when comparing NULL values");
                }

                Expression val;
                if (IsNullableGenericType(targetType)) {
                    var c = new NullableConverter(targetType);
                    if (right.Value.Equals("null")) {
                        val = Expression.Constant(null);
                    }
                    else {
                        object value = c.ConvertFrom(right.Value);
                        val = Expression.Constant(value);
                    }
                }
                else {
                    TypeConverter converter = TypeDescriptor.GetConverter(targetType);
                    if (!converter.CanConvertFrom(typeof (string))) {
                        throw new PredicateConstructorException("Can not convert expression value to property/field type");
                    }
                    object value = converter.ConvertFrom(right.Value);
                    val = Expression.Constant(value);
                }

                switch (op.Operator) {
                    case ParsedExpressionRelationalOperatorNode.RelationalOperator.EqualsOp:
                        return Expression.Equal(target, val);
                    case ParsedExpressionRelationalOperatorNode.RelationalOperator.NotEqualsOp:
                        return Expression.NotEqual(target, val);
                    case ParsedExpressionRelationalOperatorNode.RelationalOperator.LessThanOp:
                        return Expression.LessThan(target, val);
                    case ParsedExpressionRelationalOperatorNode.RelationalOperator.LessThanEqualsOp:
                        return Expression.LessThanOrEqual(target, val);
                    case ParsedExpressionRelationalOperatorNode.RelationalOperator.GreaterThanOp:
                        return Expression.GreaterThan(target, val);
                    case ParsedExpressionRelationalOperatorNode.RelationalOperator.GreaterThanEqualsOp:
                        return Expression.GreaterThanOrEqual(target, val);
                }
            }
            if (root.GetType() == typeof (ParsedExpressionBooleanOperatorNode)) {
                var op = root as ParsedExpressionBooleanOperatorNode;
                switch (op.Operator) {
                    case ParsedExpressionBooleanOperatorNode.BooleanOperator.AndOp:
                        return Expression.And(ConvertToExpression(parameter, op.Left), ConvertToExpression(parameter, op.Right));
                    case ParsedExpressionBooleanOperatorNode.BooleanOperator.OrOp:
                        return Expression.Or(ConvertToExpression(parameter, op.Left), ConvertToExpression(parameter, op.Right));
                    case ParsedExpressionBooleanOperatorNode.BooleanOperator.NotOp:
                        return Expression.Not(ConvertToExpression(parameter, op.Left));
                    case ParsedExpressionBooleanOperatorNode.BooleanOperator.OrElse:
                        return Expression.OrElse(ConvertToExpression(parameter, op.Left), ConvertToExpression(parameter, op.Right));
                    case ParsedExpressionBooleanOperatorNode.BooleanOperator.AndAlso:
                        return Expression.AndAlso(ConvertToExpression(parameter, op.Left), ConvertToExpression(parameter, op.Right));
                }
            }
            throw new PredicateConstructorException("Unknown parse tree structure");
        }

        /// <summary>
        /// Parses the expression.
        /// </summary>
        /// <returns>Root of the parse tree.</returns>
        private ParsedExpressionNode ParseExpression() {
            _filterSource.SkipWhiteSpace();
            ParsedExpressionNode n = ParseBooleanExpression();
            _filterSource.SkipWhiteSpace();
            if (_filterSource.TestFor(RegexBooleanOp)) {
                string sop = _filterSource.Accept(RegexBooleanOp);
                var o = new ParsedExpressionBooleanOperatorNode(sop) {Left = n, Right = ParseExpression()};
                return o;
            }
            return n;
        }

        /// <summary>
        /// Parses the boolean expression.
        /// </summary>
        /// <returns>The boolean root.</returns>
        private ParsedExpressionNode ParseBooleanExpression() {
            _filterSource.SkipWhiteSpace();
            if (_filterSource.TestFor("(")) {
                _filterSource.Accept("(");
                ParsedExpressionNode n = ParseExpression();
                if (!_filterSource.TestFor(")")) {
                    throw new PredicateConstructorException("Missing Closing ')'");
                }
                _filterSource.Accept(")");
                return n;
            }
            if (_filterSource.TestFor(RegexBooleanNot)) {
                string bop = _filterSource.Accept(RegexBooleanNot);
                var n = new ParsedExpressionBooleanOperatorNode(bop) {Left = ParseBooleanExpression()};
                return n;
            }
            return ParseSimpleQuery();
        }

        /// <summary>
        /// Parses the simple query.
        /// </summary>
        /// <returns>The simple query root object.</returns>
        private ParsedExpressionNode ParseSimpleQuery() {
            if (!_filterSource.TestFor(RegexKeyword)) {
                throw new PredicateConstructorException("Expected A Keyword");
            }

            string keyword = _filterSource.Accept(RegexKeyword);

            if (!CheckForValidProperyOrField(keyword)) {
                throw new PredicateConstructorException("Can only build expression using Properties or Fields of objects [" + keyword + "]");
            }

            var keywordNode = new ParsedExpressionNameNode(keyword);

            if (!_filterSource.TestFor(RegexRelationalOp)) {
                throw new PredicateConstructorException("Expected A Relational Operator After Keyword" + "[ " + _filterSource.OriginalString + "]");
            }

            string sop = _filterSource.Accept(RegexRelationalOp);
            ParsedExpressionNode termNode = ParseTerm();
            return new ParsedExpressionRelationalOperatorNode(sop) {Left = keywordNode, Right = termNode};
        }

        /// <summary>
        /// Parses the term.
        /// </summary>
        /// <returns>The parsed term tree root.</returns>
        private ParsedExpressionNode ParseTerm() {
            return ParseValue();
        }

        /// <summary>
        /// Parses the value. Can handle unquoted and quoted values. If you want <code>null</code> as a comparision
        /// value don't use quotes.
        /// </summary>
        /// <returns>The parse value node.</returns>
        private ParsedExpressionValueNode ParseValue() {
            if (_filterSource.TestFor(RegexQoute)) {
                // We have to do more work for a quoted value, including dealing with nested quotes.
                char quoteCharacter = _filterSource[0];

                // There will be >1 matches for something like 'foo''bar' (2 in this case).
                MatchCollection ms = RegexQuotedValue.Matches(_filterSource.CurrentInput);
                int entireLength = (from Match m in ms select m.Value.Length).Sum();
                _filterSource.Accept(_filterSource.CurrentInput.Substring(0, entireLength));

                IEnumerable<string> q = from Match m in ms select m.Value.Substring(1, m.Value.Length - 2);
                // Not sure this is right, not test b/c I don't know if I need this at all.
                string v = string.Join(quoteCharacter.ToString(), q.ToArray());
                var n = new ParsedExpressionValueNode(v) {Quoted = true, QuoteCharacter = quoteCharacter};
                _filterSource.SkipWhiteSpace();
                return n;
            }
            if (_filterSource.TestFor(RegexValue)) {
                string v = _filterSource.Accept(RegexValue);
                var n = new ParsedExpressionValueNode(v);
                _filterSource.SkipWhiteSpace();
                return n;
            }
            throw new PredicateConstructorException("Expected A Value In Expression [ " + _filterSource.OriginalString + "]");
        }

        #region Nested type: ParsedExpressionBinaryOperatorNode

        /// <summary>
        /// Base class for all operator nodes, including the ! operator node (in that case the right
        /// child is always null).
        /// </summary>
        private class ParsedExpressionBinaryOperatorNode : ParsedExpressionNode {
            /// <summary>
            /// Gets or sets the left.
            /// </summary>
            /// <value>The left.</value>
            public ParsedExpressionNode Left { get; set; }

            /// <summary>
            /// Gets or sets the right.
            /// </summary>
            /// <value>The right.</value>
            public ParsedExpressionNode Right { get; set; }
        }

        #endregion

        #region Nested type: ParsedExpressionBooleanOperatorNode

        /// <summary>
        /// The parent of a boolean expression. Left and right children for <code>and</code> and <code>or</code>
        /// operators, left children only for <code>not</code>.
        /// </summary>
        private class ParsedExpressionBooleanOperatorNode : ParsedExpressionBinaryOperatorNode {
            #region BooleanOperator enum

            /// <summary>
            /// The symbollic constants for the boolean operators.
            /// </summary>
            public enum BooleanOperator {
                AndOp,
                AndAlso,
                OrOp,
                OrElse,
                NotOp
            }

            #endregion

            /// <summary>
            /// Construct a BooleanOperatorNode from a string representation of the operator.
            /// Note:  For Lucene, || (OrElse) yields the appropriate results for an Or,
            ///                    && (AndAlso) yields the appropriate results for an And
            /// </summary>
            /// <param name="booleanOperator">A string boolean operator</param>
            public ParsedExpressionBooleanOperatorNode(string booleanOperator) {
                booleanOperator = booleanOperator.ToLower().Trim();
                if (booleanOperator.Equals("&&")) {
                    Operator = BooleanOperator.AndAlso;
                }
                else if (booleanOperator.Equals("|"))
                {
                    Operator = BooleanOperator.AndOp;
                }
                else if (booleanOperator.Equals("||")) {
                    Operator = BooleanOperator.OrElse;
                }
                else if (booleanOperator.Equals("|"))
                {
                    Operator = BooleanOperator.OrOp;
                }
                else if (booleanOperator.Equals("!")) {
                    Operator = BooleanOperator.NotOp;
                }
            }

            /// <summary>
            /// Gets or sets the operator.
            /// </summary>
            /// <value>The operator.</value>
            public BooleanOperator Operator { get; private set; }
        }

        #endregion

        #region Nested type: ParsedExpressionNameNode

        /// <summary>
        /// The field or property name of a object that is always the left child of a relational node.
        /// </summary>
        private class ParsedExpressionNameNode : ParsedExpressionNode {
            /// <summary>
            /// Initializes a new instance of the <see cref="PredicateConstructor&lt;TArg&gt;.ParsedExpressionNameNode"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            public ParsedExpressionNameNode(string name) {
                Name = name;
            }

            /// <summary>
            /// Gets the field or property name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; private set; }
        }

        #endregion

        #region Nested type: ParsedExpressionNode

        /// <summary>
        /// Base class for all nodes in the generated parse tree.
        /// </summary>
        private class ParsedExpressionNode {
        }

        #endregion

        #region Nested type: ParsedExpressionRelationalOperatorNode

        /// <summary>
        /// A parse not that represents a relational operator. Will have two children, relational
        /// associativity is not supported.
        /// </summary>
        private class ParsedExpressionRelationalOperatorNode : ParsedExpressionBinaryOperatorNode {
            #region RelationalOperator enum

            public enum RelationalOperator {
                LessThanOp,
                LessThanEqualsOp,
                GreaterThanOp,
                GreaterThanEqualsOp,
                EqualsOp,
                NotEqualsOp
            }

            #endregion

            /// <summary>
            /// Construct a relational operator node using a string description of the operator.
            /// </summary>
            /// <param name="relationalOperator">The string version of the operator</param>
            public ParsedExpressionRelationalOperatorNode(string relationalOperator) {
                relationalOperator = relationalOperator.Trim().ToLower();
                if (relationalOperator.Equals("=") || relationalOperator.Equals("==")) {
                    Operator = RelationalOperator.EqualsOp;
                }
                else if (relationalOperator.Equals("!=") || relationalOperator.Equals("<>")) {
                    Operator = RelationalOperator.NotEqualsOp;
                }
                else if (relationalOperator.Equals("<")) {
                    Operator = RelationalOperator.LessThanOp;
                }
                else if (relationalOperator.Equals("<=")) {
                    Operator = RelationalOperator.LessThanEqualsOp;
                }
                else if (relationalOperator.Equals(">")) {
                    Operator = RelationalOperator.GreaterThanOp;
                }
                else if (relationalOperator.Equals(">=")) {
                    Operator = RelationalOperator.GreaterThanEqualsOp;
                }
            }

            /// <summary>
            /// Gets or sets the operator.
            /// </summary>
            /// <value>The operator.</value>
            public RelationalOperator Operator { get; private set; }
        }

        #endregion

        #region Nested type: ParsedExressionValueNode

        /// <summary>
        /// The value side of a relation, in all cases the right child of a relational node.
        /// </summary>
        private class ParsedExpressionValueNode : ParsedExpressionNode {
            /// <summary>
            /// Initializes a new instance of the <see cref="PredicateConstructor&lt;TArg&gt;.ParsedExpressionValueNode"/> class.
            /// </summary>
            /// <param name="value">The value.</param>
            public ParsedExpressionValueNode(string value) {
                Value = value;
            }

            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            /// <value>The value.</value>
            public string Value { get; private set; }

            /// <summary>
            /// Gets or sets a value indicating whether this input string was quoted.
            /// </summary>
            /// <value><c>true</c> if quoted; otherwise, <c>false</c>.</value>
            public bool Quoted { get; set; }

            /// <summary>
            /// Gets or sets the quote character.
            /// </summary>
            /// <value>The quote character.</value>
            public char QuoteCharacter { get; set; }
        }

        #endregion

        #region Nested type: TokenizerString

        /// <summary>
        /// String wrapper class that provides the parser with a simple lexical tokenizer
        /// for a string.
        /// </summary>
        private class TokenizerString {
            private readonly string _originalInput;
            private string _input;

            /// <ummary>
            /// Construct a TokenizerString object and pass a string expression. The string
            /// parameter is saved and a copy is created that will be used for parsing.
            /// </summary>
            /// <param name="s">Original string expression</param>
            public TokenizerString(string s) {
                _originalInput = s;
                _input = s.Trim();
            }

            /// <summary>
            /// Gets the <see cref="System.Char"/> at the specified index.
            /// </summary>
            /// <value></value>
            public char this[int index] {
                get { return _input[index]; }
            }

            /// <summary>
            /// The original string that was passed to the constructor is saved and
            /// can be found using this property.
            /// </summary>
            public string OriginalString {
                get { return _originalInput; }
            }

            /// <summary>
            /// Gets the current input.
            /// </summary>
            /// <value>The current input.</value>
            public string CurrentInput {
                get { return _input; }
            }

            /// <summary>
            /// Trim the white space from both the front and rear of the string.
            /// </summary>
            public void SkipWhiteSpace() {
                _input = _input.Trim();
            }

            /// <summary>
            /// Test for a leading string in the query string and return true if found.
            /// </summary>
            /// <param name="s">Test string</param>
            /// <returns>True if the query string starts with s, otherwise false</returns>
            public bool TestFor(string s) {
                return _input.StartsWith(s);
            }

            /// <summary>
            /// Test for a regular expression in the query string and return if found. One
            /// should be careful to anchor the regular expression such that it doesn't match
            /// somewhere other than the start (like the middle of the query string).
            /// </summary>
            /// <param name="testRegex">A regular expression object to test</param>
            /// <returns>True if the regular expression if found and matched, otherwise false</returns>
            public bool TestFor(Regex testRegex) {
                return testRegex.Match(_input).Success;
            }

            /// <summary>
            /// Test for a string at the start of the query string, and if the test is
            /// successful then the query string is stripped of the test string (removed
            /// from the front) and we return true.
            /// </summary>
            /// <param name="acceptString">Testing string</param>
            /// <returns>True if the string was accepted, otherwise false</returns>
            public void Accept(string acceptString) {
                if (TestFor(acceptString)) {
                    _input = _input.Substring(acceptString.Length);
                    SkipWhiteSpace();
                    return;
                }
                throw new PredicateConstructorException();
            }

            /// <summary>
            /// Accepts input characters based upon the specified regex.
            /// </summary>
            /// <param name="acceptRegex">The accept regex.</param>
            /// <returns>The matched string or null</returns>
            public string Accept(Regex acceptRegex) {
                Match m = acceptRegex.Match(_input);
                if (m.Success) {
                    _input = _input.Substring(m.Value.Length);
                    SkipWhiteSpace();
                    Group gv = m.Groups["value"];
                    if (gv == null || gv.Length == 0) {
                        return m.Value;
                    }
                    return gv.Value;
                }
                return null;
            }

            /// <sumary>
            /// Return the current query string.
            /// </summary>
            /// <returns>The current query string.</returns>
            public override string ToString() {
                return _input;
            }
        }

        #endregion
    }
}