using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class OscBundle
    {

        public readonly OscMessage[] messages;
        public readonly double timestamp;

        public OscBundle(
            OscMessage[] messages
        ):this(messages, CSEmulator.Commons.UnixEpochMs(DateTime.UtcNow)) { } //UtcNowは実機確認済み(2.35.0.1)


        public OscBundle(
            OscMessage[] messages,
            double timestamp
        )
        {
            this.messages = messages ?? new OscMessage[0];
            this.timestamp = timestamp;
        }

        public object toJSON(string key)
        {
            return this;
        }
        public override string ToString()
        {
            return String.Format("[OscBundle][{0}][{1}]", timestamp, messages.Select(v => v.ToString()).Aggregate((a, b) => a + b));
        }
    }
}
