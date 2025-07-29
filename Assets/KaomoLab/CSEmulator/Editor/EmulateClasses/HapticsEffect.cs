using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class HapticsEffect
    {
        float? _amplitude = null;
        float? _duration = null;
        float? _frequency = null;

        public float? amplitude
        {
            get => _amplitude;
            set => _amplitude = value != null ? Math.Clamp(value.Value, 0.0f, 1.0f) : value;
        }
        public float? duration
        {
            get => _duration;
            set => _duration = value != null ? Math.Max(value.Value, 0.0f) : value;
        }
        public float? frequency
        {
            get => _frequency;
            set => _frequency = value != null ? Math.Max(value.Value, 0.0f) : value;
        }

        public HapticsEffect() { }

        public object toJSON(string key)
        {
            return this;
        }
        public override string ToString()
        {
            return String.Format("[HapticsEffect][{0}][{1}][{2}]", amplitude, duration, frequency);
        }
    }
}
