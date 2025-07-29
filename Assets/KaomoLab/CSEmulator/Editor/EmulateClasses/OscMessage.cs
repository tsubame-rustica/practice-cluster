using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class OscMessage
    {

        public readonly string address;
        public readonly double timestamp;
        public readonly OscValue[] values;

        public static OscMessage Construct(
            string address,
            double timestamp,
            OscValue[] values
        )
        {
            return new OscMessage(address, timestamp, values);
        }

        public OscMessage(
            string address,
            OscValue[] values
        ) : this(
            address,
            0, //このコンストラクタの場合は送信用なので参照されない
            values
        )
        { }

        OscMessage(
            string address,
            double timestamp,
            OscValue[] values
        )
        {
            this.address = address;
            this.timestamp = timestamp;
            this.values = values;
        }

        public object toJSON(string key)
        {
            return this;
        }
        public override string ToString()
        {
            return String.Format("[OscMessage][{0}][{1}][{2}]", address, timestamp, values.Select(v => v.ToString()).Aggregate((a, b) => a + b));
        }
    }
}
