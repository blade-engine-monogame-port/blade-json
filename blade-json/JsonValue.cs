using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using static Bladecoder.Utils.JsonWriter;

namespace Bladecoder.Utils
{
    public class JsonValue: IEnumerable<JsonValue>
    {
        private ValueType type;

        /** May be null. */
        private string stringValue;
        private double doubleValue;
        private long longValue;

        public string name;

        /** May be null. */
        public JsonValue child, next, prev, parent;
        public int size;

        public JsonValue(ValueType type)
        {
            this.type = type;
        }

        /** @param value May be null. */
        public JsonValue(string value)
        {
            set(value);
        }

        public JsonValue(double value)
        {
            set(value, null);
        }

        public JsonValue(long value)
        {
            set(value, null);
        }

        public JsonValue(double value, string stringValue)
        {
            set(value, stringValue);
        }

        public JsonValue(long value, string stringValue)
        {
            set(value, stringValue);
        }

        public JsonValue(bool value)
        {
            set(value);
        }

        /** Returns the child at the specified index. This requires walking the linked list to the specified entry, see
         * {@link JsonValue} for how to iterate efficiently.
         * @return May be null. */
        public JsonValue get(int index)
        {
            JsonValue current = child;
            while (current != null && index > 0)
            {
                index--;
                current = current.next;
            }
            return current;
        }

        // IEnumerable Member  
        public IEnumerator<JsonValue> GetEnumerator()
        {
            JsonValue n = child;

            while(n != null)
            {
                yield return n;
                n = n.next;
            }
        }

        /** Returns the child with the specified name.
         * @return May be null. */
        public JsonValue get(String name)
        {
            JsonValue current = child;
            while (current != null && (current.name == null ||
                !current.name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                current = current.next;
            return current;
        }

        /** Returns true if a child with the specified name exists. */
        public bool has(String name)
        {
            return get(name) != null;
        }

        /** Returns the child at the specified index. This requires walking the linked list to the specified entry, see
         * {@link JsonValue} for how to iterate efficiently.
         * @throws ArgumentException if the child was not found. */
        public JsonValue require(int index)
        {
            JsonValue current = child;
            while (current != null && index > 0)
            {
                index--;
                current = current.next;
            }
            if (current == null) throw new ArgumentException("Child not found with index: " + index);
            return current;
        }

        /** Returns the child with the specified name.
         * @throws ArgumentException if the child was not found. */
        public JsonValue require(String name)
        {
            JsonValue current = child;
            while (current != null && (current.name == null ||
                !current.name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                current = current.next;
            if (current == null) throw new ArgumentException("Child not found with name: " + name);
            return current;
        }

        /** Removes the child with the specified index. This requires walking the linked list to the specified entry, see
         * {@link JsonValue} for how to iterate efficiently.
         * @return May be null. */
        public JsonValue remove(int index)
        {
            JsonValue child = get(index);
            if (child == null) return null;
            if (child.prev == null)
            {
                this.child = child.next;
                if (this.child != null) this.child.prev = null;
            }
            else
            {
                child.prev.next = child.next;
                if (child.next != null) child.next.prev = child.prev;
            }
            size--;
            return child;
        }

        /** Removes the child with the specified name.
         * @return May be null. */
        public JsonValue remove(String name)
        {
            JsonValue child = get(name);
            if (child == null) return null;
            if (child.prev == null)
            {
                this.child = child.next;
                if (this.child != null) this.child.prev = null;
            }
            else
            {
                child.prev.next = child.next;
                if (child.next != null) child.next.prev = child.prev;
            }
            size--;
            return child;
        }

        /** Returns true if there are one or more children in the array or object. */
        public bool notEmpty()
        {
            return size > 0;
        }

        /** Returns true if there are not children in the array or object. */
        public bool isEmpty()
        {
            return size == 0;
        }

        /** @deprecated Use {@link #size} instead. Returns this number of children in the array or object. */
        public int Size()
        {
            return size;
        }

        /** Returns this value as a string.
         * @return May be null if this value is null.
         * @throws InvalidOperationException if this an array or object. */
        public string asString()
        {
            switch (type)
            {
                case ValueType.stringValue:
                    return stringValue;
                case ValueType.doubleValue:
                    return stringValue != null ? stringValue : doubleValue.ToString(CultureInfo.InvariantCulture);
                case ValueType.longValue:
                    return stringValue != null ? stringValue : longValue.ToString();
                case ValueType.booleanValue:
                    return longValue != 0 ? "true" : "false";
                case ValueType.nullValue:
                    return null;
            }
            throw new InvalidOperationException("Value cannot be converted to string: " + type);
        }

        /** Returns this value as a float.
         * @throws InvalidOperationException if this an array or object. */
        public float asFloat()
        {
            switch (type)
            {
                case ValueType.stringValue:
                    return float.Parse(stringValue, CultureInfo.InvariantCulture);
                case ValueType.doubleValue:
                    return (float)doubleValue;
                case ValueType.longValue:
                    return longValue;
                case ValueType.booleanValue:
                    return longValue != 0 ? 1 : 0;
            }
            throw new InvalidOperationException("Value cannot be converted to float: " + type);
        }

        /** Returns this value as a double.
         * @throws InvalidOperationException if this an array or object. */
        public double asDouble()
        {
            switch (type)
            {
                case ValueType.stringValue:
                    return double.Parse(stringValue, CultureInfo.InvariantCulture);
                case ValueType.doubleValue:
                    return doubleValue;
                case ValueType.longValue:
                    return longValue;
                case ValueType.booleanValue:
                    return longValue != 0 ? 1 : 0;
            }
            throw new InvalidOperationException("Value cannot be converted to double: " + type);
        }

        /** Returns this value as a long.
         * @throws InvalidOperationException if this an array or object. */
        public long asLong()
        {
            switch (type)
            {
                case ValueType.stringValue:
                    return long.Parse(stringValue);
                case ValueType.doubleValue:
                    return (long)doubleValue;
                case ValueType.longValue:
                    return longValue;
                case ValueType.booleanValue:
                    return longValue != 0 ? 1 : 0;
            }
            throw new InvalidOperationException("Value cannot be converted to long: " + type);
        }

        /** Returns this value as an int.
         * @throws InvalidOperationException if this an array or object. */
        public int asInt()
        {
            switch (type)
            {
                case ValueType.stringValue:
                    return int.Parse(stringValue);
                case ValueType.doubleValue:
                    return (int)doubleValue;
                case ValueType.longValue:
                    return (int)longValue;
                case ValueType.booleanValue:
                    return longValue != 0 ? 1 : 0;
            }
            throw new InvalidOperationException("Value cannot be converted to int: " + type);
        }

        /** Returns this value as a boolean.
         * @throws InvalidOperationException if this an array or object. */
        public bool asBoolean()
        {
            switch (type)
            {
                case ValueType.stringValue:
                    return stringValue.Equals("true", StringComparison.InvariantCultureIgnoreCase);
                case ValueType.doubleValue:
                    return doubleValue != 0;
                case ValueType.longValue:
                    return longValue != 0;
                case ValueType.booleanValue:
                    return longValue != 0;
            }
            throw new InvalidOperationException("Value cannot be converted to boolean: " + type);
        }

        /** Returns this value as a byte.
         * @throws InvalidOperationException if this an array or object. */
        public byte asByte()
        {
            switch (type)
            {
                case ValueType.stringValue:
                    return byte.Parse(stringValue);
                case ValueType.doubleValue:
                    return (byte)doubleValue;
                case ValueType.longValue:
                    return (byte)longValue;
                case ValueType.booleanValue:
                    return longValue != 0 ? (byte)1 : (byte)0;
            }
            throw new InvalidOperationException("Value cannot be converted to byte: " + type);
        }

        /** Returns this value as a short.
         * @throws InvalidOperationException if this an array or object. */
        public short asShort()
        {
            switch (type)
            {
                case ValueType.stringValue:
                    return short.Parse(stringValue);
                case ValueType.doubleValue:
                    return (short)doubleValue;
                case ValueType.longValue:
                    return (short)longValue;
                case ValueType.booleanValue:
                    return longValue != 0 ? (short)1 : (short)0;
            }
            throw new InvalidOperationException("Value cannot be converted to short: " + type);
        }

        /** Returns this value as a char.
         * @throws InvalidOperationException if this an array or object. */
        public char asChar()
        {
            switch (type)
            {
                case ValueType.stringValue:
                    return stringValue.Length == 0 ? (char)0 : stringValue[0];
                case ValueType.doubleValue:
                    return (char)doubleValue;
                case ValueType.longValue:
                    return (char)longValue;
                case ValueType.booleanValue:
                    return longValue != 0 ? (char)1 : (char)0;
            }
            throw new InvalidOperationException("Value cannot be converted to char: " + type);
        }

        /** Returns the children of this value as a newly allocated String array.
         * @throws InvalidOperationException if this is not an array. */
        public string[] asStringArray()
        {
            if (type != ValueType.array) throw new InvalidOperationException("Value is not an array: " + type);
            string[] array = new string[size];
            int i = 0;
            for (JsonValue value = child; value != null; value = value.next, i++)
            {
                string v;
                switch (value.type)
                {
                    case ValueType.stringValue:
                        v = value.stringValue;
                        break;
                    case ValueType.doubleValue:
                        v = stringValue != null ? stringValue : value.doubleValue.ToString(CultureInfo.InvariantCulture);
                        break;
                    case ValueType.longValue:
                        v = stringValue != null ? stringValue : value.longValue.ToString();
                        break;
                    case ValueType.booleanValue:
                        v = value.longValue != 0 ? "true" : "false";
                        break;
                    case ValueType.nullValue:
                        v = null;
                        break;
                    default:
                        throw new InvalidOperationException("Value cannot be converted to string: " + value.type);
                }
                array[i] = v;
            }
            return array;
        }

        /** Returns the children of this value as a newly allocated float array.
         * @throws InvalidOperationException if this is not an array. */
        public float[] asFloatArray()
        {
            if (type != ValueType.array) throw new InvalidOperationException("Value is not an array: " + type);
            float[] array = new float[size];
            int i = 0;
            for (JsonValue value = child; value != null; value = value.next, i++)
            {
                float v;
                switch (value.type)
                {
                    case ValueType.stringValue:
                        v = float.Parse(value.stringValue, CultureInfo.InvariantCulture);
                        break;
                    case ValueType.doubleValue:
                        v = (float)value.doubleValue;
                        break;
                    case ValueType.longValue:
                        v = value.longValue;
                        break;
                    case ValueType.booleanValue:
                        v = value.longValue != 0 ? 1 : 0;
                        break;
                    default:
                        throw new InvalidOperationException("Value cannot be converted to float: " + value.type);
                }
                array[i] = v;
            }
            return array;
        }

        /** Returns the children of this value as a newly allocated double array.
         * @throws InvalidOperationException if this is not an array. */
        public double[] asDoubleArray()
        {
            if (type != ValueType.array) throw new InvalidOperationException("Value is not an array: " + type);
            double[] array = new double[size];
            int i = 0;
            for (JsonValue value = child; value != null; value = value.next, i++)
            {
                double v;
                switch (value.type)
                {
                    case ValueType.stringValue:
                        v = double.Parse(value.stringValue, CultureInfo.InvariantCulture);
                        break;
                    case ValueType.doubleValue:
                        v = value.doubleValue;
                        break;
                    case ValueType.longValue:
                        v = value.longValue;
                        break;
                    case ValueType.booleanValue:
                        v = value.longValue != 0 ? 1 : 0;
                        break;
                    default:
                        throw new InvalidOperationException("Value cannot be converted to double: " + value.type);
                }
                array[i] = v;
            }
            return array;
        }

        /** Returns the children of this value as a newly allocated long array.
         * @throws InvalidOperationException if this is not an array. */
        public long[] asLongArray()
        {
            if (type != ValueType.array) throw new InvalidOperationException("Value is not an array: " + type);
            long[] array = new long[size];
            int i = 0;
            for (JsonValue value = child; value != null; value = value.next, i++)
            {
                long v;
                switch (value.type)
                {
                    case ValueType.stringValue:
                        v = long.Parse(value.stringValue);
                        break;
                    case ValueType.doubleValue:
                        v = (long)value.doubleValue;
                        break;
                    case ValueType.longValue:
                        v = value.longValue;
                        break;
                    case ValueType.booleanValue:
                        v = value.longValue != 0 ? 1 : 0;
                        break;
                    default:
                        throw new InvalidOperationException("Value cannot be converted to long: " + value.type);
                }
                array[i] = v;
            }
            return array;
        }

        /** Returns the children of this value as a newly allocated int array.
         * @throws InvalidOperationException if this is not an array. */
        public int[] asIntArray()
        {
            if (type != ValueType.array) throw new InvalidOperationException("Value is not an array: " + type);
            int[] array = new int[size];
            int i = 0;
            for (JsonValue value = child; value != null; value = value.next, i++)
            {
                int v;
                switch (value.type)
                {
                    case ValueType.stringValue:
                        v = int.Parse(value.stringValue);
                        break;
                    case ValueType.doubleValue:
                        v = (int)value.doubleValue;
                        break;
                    case ValueType.longValue:
                        v = (int)value.longValue;
                        break;
                    case ValueType.booleanValue:
                        v = value.longValue != 0 ? 1 : 0;
                        break;
                    default:
                        throw new InvalidOperationException("Value cannot be converted to int: " + value.type);
                }
                array[i] = v;
            }
            return array;
        }

        /** Returns the children of this value as a newly allocated bool array.
         * @throws InvalidOperationException if this is not an array. */
        public bool[] asBooleanArray()
        {
            if (type != ValueType.array) throw new InvalidOperationException("Value is not an array: " + type);
            bool[] array = new bool[size];
            int i = 0;
            for (JsonValue value = child; value != null; value = value.next, i++)
            {
                bool v;
                switch (value.type)
                {
                    case ValueType.stringValue:
                        v = bool.Parse(value.stringValue);
                        break;
                    case ValueType.doubleValue:
                        v = value.doubleValue == 0;
                        break;
                    case ValueType.longValue:
                        v = value.longValue == 0;
                        break;
                    case ValueType.booleanValue:
                        v = value.longValue != 0;
                        break;
                    default:
                        throw new InvalidOperationException("Value cannot be converted to boolean: " + value.type);
                }
                array[i] = v;
            }
            return array;
        }

        /** Returns the children of this value as a newly allocated byte array.
         * @throws InvalidOperationException if this is not an array. */
        public byte[] asByteArray()
        {
            if (type != ValueType.array) throw new InvalidOperationException("Value is not an array: " + type);
            byte[] array = new byte[size];
            int i = 0;
            for (JsonValue value = child; value != null; value = value.next, i++)
            {
                byte v;
                switch (value.type)
                {
                    case ValueType.stringValue:
                        v = byte.Parse(value.stringValue);
                        break;
                    case ValueType.doubleValue:
                        v = (byte)value.doubleValue;
                        break;
                    case ValueType.longValue:
                        v = (byte)value.longValue;
                        break;
                    case ValueType.booleanValue:
                        v = value.longValue != 0 ? (byte)1 : (byte)0;
                        break;
                    default:
                        throw new InvalidOperationException("Value cannot be converted to byte: " + value.type);
                }
                array[i] = v;
            }
            return array;
        }

        /** Returns the children of this value as a newly allocated short array.
         * @throws InvalidOperationException if this is not an array. */
        public short[] asShortArray()
        {
            if (type != ValueType.array) throw new InvalidOperationException("Value is not an array: " + type);
            short[] array = new short[size];
            int i = 0;
            for (JsonValue value = child; value != null; value = value.next, i++)
            {
                short v;
                switch (value.type)
                {
                    case ValueType.stringValue:
                        v = short.Parse(value.stringValue);
                        break;
                    case ValueType.doubleValue:
                        v = (short)value.doubleValue;
                        break;
                    case ValueType.longValue:
                        v = (short)value.longValue;
                        break;
                    case ValueType.booleanValue:
                        v = value.longValue != 0 ? (short)1 : (short)0;
                        break;
                    default:
                        throw new InvalidOperationException("Value cannot be converted to short: " + value.type);
                }
                array[i] = v;
            }
            return array;
        }

        /** Returns the children of this value as a newly allocated char array.
         * @throws InvalidOperationException if this is not an array. */
        public char[] asCharArray()
        {
            if (type != ValueType.array) throw new InvalidOperationException("Value is not an array: " + type);
            char[] array = new char[size];
            int i = 0;
            for (JsonValue value = child; value != null; value = value.next, i++)
            {
                char v;
                switch (value.type)
                {
                    case ValueType.stringValue:
                        v = value.stringValue.Length == 0 ? (char)0 : value.stringValue[0];
                        break;
                    case ValueType.doubleValue:
                        v = (char)value.doubleValue;
                        break;
                    case ValueType.longValue:
                        v = (char)value.longValue;
                        break;
                    case ValueType.booleanValue:
                        v = value.longValue != 0 ? (char)1 : (char)0;
                        break;
                    default:
                        throw new InvalidOperationException("Value cannot be converted to char: " + value.type);
                }
                array[i] = v;
            }
            return array;
        }

        /** Returns true if a child with the specified name exists and has a child. */
        public bool hasChild(String name)
        {
            return getChild(name) != null;
        }

        /** Finds the child with the specified name and returns its first child.
         * @return May be null. */
        public JsonValue getChild(String name)
        {
            JsonValue child = get(name);
            return child == null ? null : child.child;
        }

        /** Finds the child with the specified name and returns it as a string. Returns defaultValue if not found.
         * @param defaultValue May be null. */
        public String getString(String name, String defaultValue)
        {
            JsonValue child = get(name);
            return (child == null || !child.isValue() || child.isNull()) ? defaultValue : child.asString();
        }

        /** Finds the child with the specified name and returns it as a float. Returns defaultValue if not found. */
        public float getFloat(String name, float defaultValue)
        {
            JsonValue child = get(name);
            return (child == null || !child.isValue() || child.isNull()) ? defaultValue : child.asFloat();
        }

        /** Finds the child with the specified name and returns it as a double. Returns defaultValue if not found. */
        public double getDouble(String name, double defaultValue)
        {
            JsonValue child = get(name);
            return (child == null || !child.isValue() || child.isNull()) ? defaultValue : child.asDouble();
        }

        /** Finds the child with the specified name and returns it as a long. Returns defaultValue if not found. */
        public long getLong(String name, long defaultValue)
        {
            JsonValue child = get(name);
            return (child == null || !child.isValue() || child.isNull()) ? defaultValue : child.asLong();
        }

        /** Finds the child with the specified name and returns it as an int. Returns defaultValue if not found. */
        public int getInt(String name, int defaultValue)
        {
            JsonValue child = get(name);
            return (child == null || !child.isValue() || child.isNull()) ? defaultValue : child.asInt();
        }

        /** Finds the child with the specified name and returns it as a boolean. Returns defaultValue if not found. */
        public bool getBoolean(String name, bool defaultValue)
        {
            JsonValue child = get(name);
            return (child == null || !child.isValue() || child.isNull()) ? defaultValue : child.asBoolean();
        }

        /** Finds the child with the specified name and returns it as a byte. Returns defaultValue if not found. */
        public byte getByte(String name, byte defaultValue)
        {
            JsonValue child = get(name);
            return (child == null || !child.isValue() || child.isNull()) ? defaultValue : child.asByte();
        }

        /** Finds the child with the specified name and returns it as a short. Returns defaultValue if not found. */
        public short getShort(String name, short defaultValue)
        {
            JsonValue child = get(name);
            return (child == null || !child.isValue() || child.isNull()) ? defaultValue : child.asShort();
        }

        /** Finds the child with the specified name and returns it as a char. Returns defaultValue if not found. */
        public char getChar(String name, char defaultValue)
        {
            JsonValue child = get(name);
            return (child == null || !child.isValue() || child.isNull()) ? defaultValue : child.asChar();
        }

        /** Finds the child with the specified name and returns it as a string.
         * @throws ArgumentException if the child was not found. */
        public String getString(String name)
        {
            JsonValue child = get(name);
            if (child == null) throw new ArgumentException("Named value not found: " + name);
            return child.asString();
        }

        /** Finds the child with the specified name and returns it as a float.
         * @throws ArgumentException if the child was not found. */
        public float getFloat(String name)
        {
            JsonValue child = get(name);
            if (child == null) throw new ArgumentException("Named value not found: " + name);
            return child.asFloat();
        }

        /** Finds the child with the specified name and returns it as a double.
         * @throws ArgumentException if the child was not found. */
        public double getDouble(String name)
        {
            JsonValue child = get(name);
            if (child == null) throw new ArgumentException("Named value not found: " + name);
            return child.asDouble();
        }

        /** Finds the child with the specified name and returns it as a long.
         * @throws ArgumentException if the child was not found. */
        public long getLong(String name)
        {
            JsonValue child = get(name);
            if (child == null) throw new ArgumentException("Named value not found: " + name);
            return child.asLong();
        }

        /** Finds the child with the specified name and returns it as an int.
         * @throws ArgumentException if the child was not found. */
        public int getInt(String name)
        {
            JsonValue child = get(name);
            if (child == null) throw new ArgumentException("Named value not found: " + name);
            return child.asInt();
        }

        /** Finds the child with the specified name and returns it as a boolean.
         * @throws ArgumentException if the child was not found. */
        public bool getBoolean(String name)
        {
            JsonValue child = get(name);
            if (child == null) throw new ArgumentException("Named value not found: " + name);
            return child.asBoolean();
        }

        /** Finds the child with the specified name and returns it as a byte.
         * @throws ArgumentException if the child was not found. */
        public byte getByte(String name)
        {
            JsonValue child = get(name);
            if (child == null) throw new ArgumentException("Named value not found: " + name);
            return child.asByte();
        }

        /** Finds the child with the specified name and returns it as a short.
         * @throws ArgumentException if the child was not found. */
        public short getShort(String name)
        {
            JsonValue child = get(name);
            if (child == null) throw new ArgumentException("Named value not found: " + name);
            return child.asShort();
        }

        /** Finds the child with the specified name and returns it as a char.
         * @throws ArgumentException if the child was not found. */
        public char getChar(String name)
        {
            JsonValue child = get(name);
            if (child == null) throw new ArgumentException("Named value not found: " + name);
            return child.asChar();
        }

        /** Finds the child with the specified index and returns it as a string.
         * @throws ArgumentException if the child was not found. */
        public String getString(int index)
        {
            JsonValue child = get(index);
            if (child == null) throw new ArgumentException("Indexed value not found: " + name);
            return child.asString();
        }

        /** Finds the child with the specified index and returns it as a float.
         * @throws ArgumentException if the child was not found. */
        public float getFloat(int index)
        {
            JsonValue child = get(index);
            if (child == null) throw new ArgumentException("Indexed value not found: " + name);
            return child.asFloat();
        }

        /** Finds the child with the specified index and returns it as a double.
         * @throws ArgumentException if the child was not found. */
        public double getDouble(int index)
        {
            JsonValue child = get(index);
            if (child == null) throw new ArgumentException("Indexed value not found: " + name);
            return child.asDouble();
        }

        /** Finds the child with the specified index and returns it as a long.
         * @throws ArgumentException if the child was not found. */
        public long getLong(int index)
        {
            JsonValue child = get(index);
            if (child == null) throw new ArgumentException("Indexed value not found: " + name);
            return child.asLong();
        }

        /** Finds the child with the specified index and returns it as an int.
         * @throws ArgumentException if the child was not found. */
        public int getInt(int index)
        {
            JsonValue child = get(index);
            if (child == null) throw new ArgumentException("Indexed value not found: " + name);
            return child.asInt();
        }

        /** Finds the child with the specified index and returns it as a boolean.
         * @throws ArgumentException if the child was not found. */
        public bool getBoolean(int index)
        {
            JsonValue child = get(index);
            if (child == null) throw new ArgumentException("Indexed value not found: " + name);
            return child.asBoolean();
        }

        /** Finds the child with the specified index and returns it as a byte.
         * @throws ArgumentException if the child was not found. */
        public byte getByte(int index)
        {
            JsonValue child = get(index);
            if (child == null) throw new ArgumentException("Indexed value not found: " + name);
            return child.asByte();
        }

        /** Finds the child with the specified index and returns it as a short.
         * @throws ArgumentException if the child was not found. */
        public short getShort(int index)
        {
            JsonValue child = get(index);
            if (child == null) throw new ArgumentException("Indexed value not found: " + name);
            return child.asShort();
        }

        /** Finds the child with the specified index and returns it as a char.
         * @throws ArgumentException if the child was not found. */
        public char getChar(int index)
        {
            JsonValue child = get(index);
            if (child == null) throw new ArgumentException("Indexed value not found: " + name);
            return child.asChar();
        }

        public ValueType Type()
        {
            return type;
        }

        public void setType(ValueType type)
        {
            if (type == null) throw new ArgumentException("type cannot be null.");
            this.type = type;
        }

        public bool isArray()
        {
            return type == ValueType.array;
        }

        public bool isObject()
        {
            return type == ValueType.objectValue;
        }

        public bool isString()
        {
            return type == ValueType.stringValue;
        }

        /** Returns true if this is a double or long value. */
        public bool isNumber()
        {
            return type == ValueType.doubleValue || type == ValueType.longValue;
        }

        public bool isDouble()
        {
            return type == ValueType.doubleValue;
        }

        public bool isLong()
        {
            return type == ValueType.longValue;
        }

        public bool isBoolean()
        {
            return type == ValueType.booleanValue;
        }

        public bool isNull()
        {
            return type == ValueType.nullValue;
        }

        /** Returns true if this is not an array or object. */
        public bool isValue()
        {
            switch (type)
            {
                case ValueType.stringValue:
                case ValueType.doubleValue:
                case ValueType.longValue:
                case ValueType.booleanValue:
                case ValueType.nullValue:
                    return true;
            }
            return false;
        }

        /** Returns the name for this object value.
         * @return May be null. */
        public String Name()
        {
            return name;
        }

        /** @param name May be null. */
        public void setName(String name)
        {
            this.name = name;
        }

        /** Returns the parent for this value.
         * @return May be null. */
        public JsonValue Parent()
        {
            return parent;
        }

        /** Returns the first child for this object or array.
         * @return May be null. */
        public JsonValue Child()
        {
            return child;
        }

        /** Sets the name of the specified value and adds it after the last child. */
        public void addChild(String name, JsonValue value)
        {
            if (name == null) throw new ArgumentException("name cannot be null.");
            value.name = name;
            addChild(value);
        }

        /** Adds the specified value after the last child. */
        public void addChild(JsonValue value)
        {
            value.parent = this;
            JsonValue current = child;
            if (current == null)
                child = value;
            else
            {
                while (true)
                {
                    if (current.next == null)
                    {
                        current.next = value;
                        return;
                    }
                    current = current.next;
                }
            }
        }

        /** Returns the next sibling of this value.
         * @return May be null. */
        public JsonValue Next()
        {
            return next;
        }

        public void setNext(JsonValue next)
        {
            this.next = next;
        }

        /** Returns the previous sibling of this value.
         * @return May be null. */
        public JsonValue Prev()
        {
            return prev;
        }

        public void setPrev(JsonValue prev)
        {
            this.prev = prev;
        }

        /** @param value May be null. */
        public void set(string value)
        {
            stringValue = value;
            type = value == null ? ValueType.nullValue : ValueType.stringValue;
        }

        /** @param stringValue May be null if the string representation is the string value of the double (eg, no leading zeros). */
        public void set(double value, string stringValue)
        {
            doubleValue = value;
            longValue = (long)value;
            this.stringValue = stringValue;
            type = ValueType.doubleValue;
        }

        /** @param stringValue May be null if the string representation is the string value of the long (eg, no leading zeros). */
        public void set(long value, string stringValue)
        {
            longValue = value;
            doubleValue = value;
            this.stringValue = stringValue;
            type = ValueType.longValue;
        }

        public void set(bool value)
        {
            longValue = value ? 1 : 0;
            type = ValueType.booleanValue;
        }

        public String toJson(OutputType outputType)
        {
            if (isValue()) return asString();
            StringBuilder buffer = new StringBuilder(512);
            json(this, buffer, outputType);
            return buffer.ToString();
        }

        private void json(JsonValue obj, StringBuilder buffer, OutputType outputType)
        {
            if (obj.isObject())
            {
                if (obj.child == null)
                    buffer.Append("{}");
                else
                {
                    int start = buffer.Length;
                    while (true)
                    {
                        buffer.Append('{');
                        int i = 0;
                        for (JsonValue child = obj.child; child != null; child = child.next)
                        {
                            buffer.Append(OutputTypeQuote.quoteName(outputType, child.name));
                            buffer.Append(':');
                            json(child, buffer, outputType);
                            if (child.next != null) buffer.Append(',');
                        }
                        break;
                    }
                    buffer.Append('}');
                }
            }
            else if (obj.isArray())
            {
                if (obj.child == null)
                    buffer.Append("[]");
                else
                {
                    int start = buffer.Length;
                    while (true)
                    {
                        buffer.Append('[');
                        for (JsonValue child = obj.child; child != null; child = child.next)
                        {
                            json(child, buffer, outputType);
                            if (child.next != null) buffer.Append(',');
                        }
                        break;
                    }
                    buffer.Append(']');
                }
            }
            else if (obj.isString())
            {
                buffer.Append(OutputTypeQuote.quoteValue(outputType, obj.asString()));
            }
            else if (obj.isDouble())
            {
                double doubleValue = obj.asDouble();
                long longValue = obj.asLong();
                buffer.Append(doubleValue == longValue ? longValue : doubleValue);
            }
            else if (obj.isLong())
            {
                buffer.Append(obj.asLong());
            }
            else if (obj.isBoolean())
            {
                buffer.Append(obj.asBoolean());
            }
            else if (obj.isNull())
            {
                buffer.Append("null");
            }
            else
                throw new SerializationException("Unknown obj type: " + obj);
        }

        public override string ToString()
        {
            if (isValue()) return name == null ? asString() : name + ": " + asString();
            return (name == null ? "" : name + ": ") + prettyPrint(OutputType.minimal, 0);
        }

        public string prettyPrint(OutputType outputType, int singleLineColumns)
        {
            PrettyPrintSettings settings = new PrettyPrintSettings();
            settings.outputType = outputType;
            settings.singleLineColumns = singleLineColumns;
            return prettyPrint(settings);
        }

        public string prettyPrint(PrettyPrintSettings settings)
        {
            StringBuilder buffer = new StringBuilder(512);
            prettyPrint(this, buffer, 0, settings);
            return buffer.ToString();
        }

        private void prettyPrint(JsonValue jobject, StringBuilder buffer, int indent, PrettyPrintSettings settings)
        {
            OutputType outputType = settings.outputType;
            if (jobject.isObject())
            {
                if (jobject.child == null)
                    buffer.Append("{}");
                else
                {
                    bool newLines = !isFlat(jobject);
                    int start = buffer.Length;
                outer:
                    while (true)
                    {
                        buffer.Append(newLines ? "{\n" : "{ ");
                        int i = 0;
                        for (JsonValue child = jobject.child; child != null; child = child.next)
                        {
                            if (newLines) Indent(indent, buffer);
                            buffer.Append(OutputTypeQuote.quoteName(outputType, child.name));
                            buffer.Append(": ");
                            prettyPrint(child, buffer, indent + 1, settings);
                            if ((!newLines || outputType != OutputType.minimal) && child.next != null) buffer.Append(',');
                            buffer.Append(newLines ? '\n' : ' ');
                            if (!newLines && buffer.Length - start > settings.singleLineColumns)
                            {
                                buffer.Length = start;
                                newLines = true;
                                goto outer;
                            }
                        }
                        break;
                    }
                    if (newLines) Indent(indent - 1, buffer);
                    buffer.Append('}');
                }
            }
            else if (jobject.isArray())
            {
                if (jobject.child == null)
                    buffer.Append("[]");
                else
                {
                    bool newLines = !isFlat(jobject);
                    bool wrap = settings.wrapNumericArrays || !isNumeric(jobject);
                    int start = buffer.Length;
                outer:
                    while (true)
                    {
                        buffer.Append(newLines ? "[\n" : "[ ");
                        for (JsonValue child = jobject.child; child != null; child = child.next)
                        {
                            if (newLines) Indent(indent, buffer);
                            prettyPrint(child, buffer, indent + 1, settings);
                            if ((!newLines || outputType != OutputType.minimal) && child.next != null) buffer.Append(',');
                            buffer.Append(newLines ? '\n' : ' ');
                            if (wrap && !newLines && buffer.Length - start > settings.singleLineColumns)
                            {
                                buffer.Length = start;
                                newLines = true;
                                goto outer;
                            }
                        }
                        break;
                    }
                    if (newLines) Indent(indent - 1, buffer);
                    buffer.Append(']');
                }
            }
            else if (jobject.isString())
            {
                buffer.Append(OutputTypeQuote.quoteValue(outputType, jobject.asString()));
            }
            else if (jobject.isDouble())
            {
                double doubleValue = jobject.asDouble();
                long longValue = jobject.asLong();
                buffer.Append(doubleValue == longValue ? longValue : doubleValue);
            }
            else if (jobject.isLong())
            {
                buffer.Append(jobject.asLong());
            }
            else if (jobject.isBoolean())
            {
                buffer.Append(jobject.asBoolean());
            }
            else if (jobject.isNull())
            {
                buffer.Append("null");
            }
            else
                throw new SerializationException("Unknown obj type: " + jobject);
        }

        /** More efficient than {@link #prettyPrint(PrettyPrintSettings)} but {@link PrettyPrintSettings#singleLineColumns} and
         * {@link PrettyPrintSettings#wrapNumericArrays} are not supported. */
        public void prettyPrint(OutputType outputType, StreamWriter writer)
        {
            PrettyPrintSettings settings = new PrettyPrintSettings();
            settings.outputType = outputType;
            prettyPrint(this, writer, 0, settings);
        }

        private void prettyPrint(JsonValue jobject, StreamWriter writer, int indent, PrettyPrintSettings settings)
        {
            OutputType outputType = settings.outputType;
            if (jobject.isObject())
            {
                if (jobject.child == null)
                    writer.Write("{}");
                else
                {
                    bool newLines = !isFlat(jobject) || jobject.size > 6;
                    writer.Write(newLines ? "{\n" : "{ ");
                    int i = 0;
                    for (JsonValue child = jobject.child; child != null; child = child.next)
                    {
                        if (newLines) Indent(indent, writer);
                        writer.Write(OutputTypeQuote.quoteName(outputType, child.name));
                        writer.Write(": ");
                        prettyPrint(child, writer, indent + 1, settings);
                        if ((!newLines || outputType != OutputType.minimal) && child.next != null) writer.Write(',');
                        writer.Write(newLines ? '\n' : ' ');
                    }
                    if (newLines) Indent(indent - 1, writer);
                    writer.Write('}');
                }
            }
            else if (jobject.isArray())
            {
                if (jobject.child == null)
                    writer.Write("[]");
                else
                {
                    bool newLines = !isFlat(jobject);
                    writer.Write(newLines ? "[\n" : "[ ");
                    int i = 0;
                    for (JsonValue child = jobject.child; child != null; child = child.next)
                    {
                        if (newLines) Indent(indent, writer);
                        prettyPrint(child, writer, indent + 1, settings);
                        if ((!newLines || outputType != OutputType.minimal) && child.next != null) writer.Write(',');
                        writer.Write(newLines ? '\n' : ' ');
                    }
                    if (newLines) Indent(indent - 1, writer);
                    writer.Write(']');
                }
            }
            else if (jobject.isString())
            {
                writer.Write(OutputTypeQuote.quoteValue(outputType, jobject.asString()));
            }
            else if (jobject.isDouble())
            {
                double doubleValue = jobject.asDouble();
                long longValue = jobject.asLong();
                writer.Write(doubleValue == longValue ? longValue.ToString() : doubleValue.ToString(CultureInfo.InvariantCulture));
            }
            else if (jobject.isLong())
            {
                writer.Write(jobject.asLong().ToString());
            }
            else if (jobject.isBoolean())
            {
                writer.Write(jobject.asBoolean().ToString());
            }
            else if (jobject.isNull())
            {
                writer.Write("null");
            }
            else
                throw new SerializationException("Unknown jobject type: " + jobject);
        }

        private static bool isFlat(JsonValue jobject)
        {
            for (JsonValue child = jobject.child; child != null; child = child.next)
                if (child.isObject() || child.isArray()) return false;
            return true;
        }

        private static bool isNumeric(JsonValue jobject)
        {
            for (JsonValue child = jobject.child; child != null; child = child.next)
                if (!child.isNumber()) return false;
            return true;
        }

        private static void Indent(int count, StringBuilder buffer)
        {
            for (int i = 0; i < count; i++)
                buffer.Append('\t');
        }

        private static void Indent(int count, StreamWriter buffer)
        {
            for (int i = 0; i < count; i++)
                buffer.Write('\t');
        }

        public enum ValueType
        {
            objectValue, array, stringValue, doubleValue, longValue, booleanValue, nullValue
        }

        /** Returns a human readable string representing the path from the root of the JSON object graph to this value. */
        public String trace()
        {
            if (parent == null)
            {
                if (type == ValueType.array) return "[]";
                if (type == ValueType.objectValue) return "{}";
                return "";
            }
            String trace;
            if (parent.type == ValueType.array)
            {
                trace = "[]";
                int i = 0;
                for (JsonValue child = parent.child; child != null; child = child.next, i++)
                {
                    if (child == this)
                    {
                        trace = "[" + i + "]";
                        break;
                    }
                }
            }
            else if (name.IndexOf('.') != -1)
                trace = ".\"" + name.Replace("\"", "\\\"") + "\"";
            else
                trace = '.' + name;
            return parent.trace() + trace;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public sealed class PrettyPrintSettings
        {
            public OutputType outputType;

            /** If an object on a single line fits this many columns, it won't wrap. */
            public int singleLineColumns;

            /** Arrays of floats won't wrap. */
            public bool wrapNumericArrays;
        }
    }
}
