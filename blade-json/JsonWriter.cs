using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Bladecoder.Utils
{
    public class JsonWriter
    {
        readonly TextWriter writer;
        private readonly Stack<JsonObject> stack = new Stack<JsonObject>();
        private JsonObject current;
        private bool named;
        private OutputType outputType = OutputType.json;
        private bool quoteLongValues = false;

        public JsonWriter(TextWriter writer)
        {
            this.writer = writer;
        }

        public TextWriter getWriter()
        {
            return writer;
        }

        /** Sets the type of JSON output. Default is {@link OutputType#minimal}. */
        public void setOutputType(OutputType outputType)
        {
            this.outputType = outputType;
        }

        /** When true, quotes long, double, BigInteger, BigDecimal types to prevent truncation in languages like JavaScript and PHP.
         * This is not necessary when using libgdx, which handles these types without truncation. Default is false. */
        public void setQuoteLongValues(bool quoteLongValues)
        {
            this.quoteLongValues = quoteLongValues;
        }

        public JsonWriter Name(String name)
        {
            if (current == null || current.array) throw new InvalidOperationException("Current item must be an object.");
            if (!current.needsComma)
                current.needsComma = true;
            else
                writer.Write(',');
            writer.Write(OutputTypeQuote.quoteName(outputType, name));
            writer.Write(':');
            named = true;
            return this;
        }

        public JsonWriter Obj()
        {
            requireCommaOrName();
            stack.Push(current = new JsonObject(this, false));
            return this;

        }

        public JsonWriter Array()
        {
            requireCommaOrName();
            stack.Push(current = new JsonObject(this, true));
            return this;
        }


        public JsonWriter Value(object value)
        {
            if (quoteLongValues
                && (value is long || value is double))
            {
                if(value is long)
                    value = ((long)value).ToString(CultureInfo.InvariantCulture);
                else
                    value = ((double)value).ToString(CultureInfo.InvariantCulture);
            }
            else if (value != null && IsNumeric(value))
            {
                long longValue = Convert.ToInt64(value);
                if (Convert.ToDouble(value) == longValue) value = longValue;
            }

            requireCommaOrName();
            writer.Write(OutputTypeQuote.quoteValue(outputType, value));
            return this;
        }

        /** Writes the specified JSON value, without quoting or escaping. */
        public JsonWriter json(String json)
        {
            requireCommaOrName();
            writer.Write(json);
            return this;
        }

        private void requireCommaOrName()
        {
            if (current == null) return;
            if (current.array)
            {
                if (!current.needsComma)
                    current.needsComma = true;
                else
                    writer.Write(',');
            }
            else
            {
                if (!named) throw new InvalidOperationException("Name must be set.");
                named = false;
            }
        }

        public JsonWriter GetObject(String name)
        {
            return Name(name).Obj();
        }

        public JsonWriter array(String name)
        {
            return Name(name).Array();
        }

        public JsonWriter set(String name, Object value)
        {
            return Name(name).Value(value);
        }

        /** Writes the specified JSON value, without quoting or escaping. */
        public JsonWriter json(String name, String json)
        {
            return Name(name).json(json);
        }

        public JsonWriter pop()
        {
            if (named) throw new InvalidOperationException("Expected an object, array, or value since a name was set.");
            stack.Pop().close();
            current = stack.Count == 0 ? null : stack.Peek();
            return this;

        }

        public void write(char[] cbuf, int off, int len)
        {
            writer.Write(cbuf, off, len);
        }

        public void Flush()
        {
            writer.Flush();
        }

        public void Close()
        {
            while (stack.Count > 0)
                pop();
            writer.Close();
        }

        class JsonObject
        {
            private JsonWriter _jsonWriter;
            public readonly bool array;
            public bool needsComma;

            public JsonObject(JsonWriter jsonWriter, bool array)
            {
                this.array = array;
                _jsonWriter = jsonWriter;
                _jsonWriter.writer.Write(array ? '[' : '{');
            }

            public void close()
            {
                _jsonWriter.writer.Write(array ? ']' : '}');
            }
        }

        public enum OutputType
        {
            /** Normal JSON, with all its double quotes. */
            json,
            /** Like JSON, but names are only double quoted if necessary. */
            javascript,
            /** Like JSON, but:
             * <ul>
             * <li>Names only require double quotes if they start with <code>space</code> or any of <code>":,}/</code> or they contain
             * <code>//</code> or <code>/*</code> or <code>:</code>.
             * <li>Values only require double quotes if they start with <code>space</code> or any of <code>":,{[]/</code> or they
             * contain <code>//</code> or <code>/*</code> or any of <code>}],</code> or they are equal to <code>true</code>,
             * <code>false</code> , or <code>null</code>.
             * <li>Newlines are treated as commas, making commas optional in many cases.
             * <li>C style comments may be used: <code>//...</code> or <code>/*...*<b></b>/</code>
             * </ul> */
            minimal
        }

        public static class OutputTypeQuote
        {
            private static Regex javascriptPattern = new Regex("^[a-zA-Z_$][a-zA-Z_$0-9]*$");
            private static Regex minimalNamePattern = new Regex("^[^\":,}/ ][^:]*$");
            private static Regex minimalValuePattern = new Regex("^[^\":,{\\[\\]/ ][^}\\],]*$");

            public static string quoteValue(OutputType ot, object value)
            {
                if (value == null) return "null";

                if (IsNumeric(value))
                { 
                    if(value is float f)
                        return f.ToString(CultureInfo.InvariantCulture);

                    if (value is double d)
                        return d.ToString(CultureInfo.InvariantCulture);

                    if (value is int i)
                        return i.ToString(CultureInfo.InvariantCulture);

                    return value.ToString();
                }

                string str = value.ToString();

                if (value is bool) return str.ToLower();

                StringBuilder buffer = new StringBuilder(str);
                buffer.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
                if (ot == OutputType.minimal && !str.Equals("true") && !str.Equals("false") && !str.Equals("null")
                    && !str.Contains("//") && !str.Contains("/*"))
                {
                    int length = buffer.Length;
                    if (length > 0 && buffer[length - 1] != ' ' && minimalValuePattern.IsMatch(buffer.ToString()))
                        return buffer.ToString();
                }
                return '"' + buffer.Replace("\"", "\\\"").ToString() + "\"";
            }

            public static string quoteName(OutputType ot, string value)
            {
                StringBuilder buffer = new StringBuilder(value);
                buffer.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");

                if (ot == OutputType.minimal && !value.Contains("//") && !value.Contains("/*") && minimalNamePattern.IsMatch(buffer.ToString()))
                    return buffer.ToString();

                if (ot == OutputType.javascript && javascriptPattern.IsMatch(buffer.ToString()))
                    return buffer.ToString();

                return '"' + buffer.Replace("\"", "\\\"").ToString() + '"';
            }
        }

        private static bool IsNumeric(object o)
        {
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
}
