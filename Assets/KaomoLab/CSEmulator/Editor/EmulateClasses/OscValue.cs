using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class OscValue
    {
        public enum Types
        {
            @bool, @float, @int, @string, bytes
        }

        public readonly bool? boolValue = null;
        public readonly float? floatValue = null;
        public readonly int? intValue = null;
        public readonly string stringValue = null;
        public readonly byte[] bytesValue = null;

        public readonly Types types;

        public OscValue(bool boolValue) { this.boolValue = boolValue; types = Types.@bool; }
        public OscValue(float floatValue) { this.floatValue = floatValue; types = Types.@float; }
        public OscValue(int intValue) { this.intValue = intValue; types = Types.@int; }
        public OscValue(string stringValue) { this.stringValue = stringValue; types = Types.@string; }
        public OscValue(IReadOnlyList<byte> bytesValue) { this.bytesValue = bytesValue.ToArray(); types = Types.bytes; }

        public string getAsciiString()
        {
            return stringValue;
        }

        public int[] getBlobAsUint8Array()
        {
            if (bytesValue == null) return null;
            return bytesValue.Select(x => (int)x).ToArray();
        }

        public string getBlobAsUtf8String()
        {
            if (bytesValue == null) return null;
            try
            {
                return System.Text.Encoding.UTF8.GetString(bytesValue);
            }
            catch(ArgumentException)
            {
                return null;
            }
        }

        public object getBool()
        {
            if (boolValue == null) return null;
            return boolValue.Value;
        }

        public object getFloat()
        {
            if (floatValue == null) return null;
            return floatValue.Value;
        }

        public object getInt()
        {
            if (intValue == null) return null;
            return intValue.Value;
        }

        public static OscValue @int(int value)
        {
            return new OscValue(value);
        }
        public static OscValue @float(float value)
        {
            return new OscValue(value);
        }
        public static OscValue asciiString(string value)
        {
            var ascii = Encoding.ASCII.GetBytes(value);
            var check = Encoding.ASCII.GetString(ascii);
            if (value != check) throw new Jint.Runtime.JavaScriptException(String.Format("ASCII文字以外の文字が含まれています。[{0}]", value));
            return new OscValue(value);
        }
        public static OscValue blob(Jint.Native.JsValue value)
        {
            //CCK2.35.0.1時点でUint8Arrayがstring解釈されているようだけど、多分こういう実装で正のように思うので一旦これで。
            if(value is Jint.Native.JsTypedArray jsTypedArray && jsTypedArray.Prototype.GetType().Name == "Uint8ArrayPrototype")
            {
                var bytes = (byte[])jsTypedArray.ToObject();
                return new OscValue(bytes);
            }
            else
            {
                var str = value.ToString();
                var bytes = Encoding.UTF8.GetBytes(str);
                return new OscValue(bytes);
            }
        }
        public static OscValue @bool(bool value)
        {
            return new OscValue(value);
        }

        public object toJSON(string key)
        {
            return this;
        }
        public override string ToString()
        {
            return String.Format("[OscValue][{0}][{1}][{2}][{3}][{4}]", boolValue, floatValue, intValue, stringValue, bytesValue?.Select(b => b.ToString("X2")).Aggregate((a, b) => a + "," + b));
        }
    }
}
