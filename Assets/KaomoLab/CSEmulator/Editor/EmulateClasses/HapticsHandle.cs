using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class HapticsHandle
    {
        readonly IHapticsSettings hapticsSettings;
        readonly IHapticsAudioController hapticsAudio;
        readonly IItemExceptionFactory itemExceptionFactory;

        public float? maxFrequencyHz => hapticsSettings.isFrequencySupported ? hapticsSettings.maxFrequencyHz : null;
        public float? minFrequencyHz => hapticsSettings.isFrequencySupported ? hapticsSettings.minFrequencyHz : null;

        string lastTarget = null;

        public HapticsHandle(
            IHapticsSettings hapticsSettings,
            IHapticsAudioController hapticsAudio,
            IItemExceptionFactory itemExceptionFactory
        )
        {
            this.hapticsSettings = hapticsSettings;
            this.hapticsAudio = hapticsAudio;
            this.itemExceptionFactory = itemExceptionFactory;
        }

        public bool isAvailable()
        {
            return hapticsSettings.isAvailable;
        }

        float GetFreq(HapticsEffect effect)
        {
            if (!hapticsSettings.isFrequencySupported)
            {
                return hapticsSettings.defaultFrequency;
            }
            return effect.frequency ?? hapticsSettings.defaultFrequency;
        }

        public void playEffect(HapticsEffect effect, string target)
        {
            if (!hapticsSettings.isAvailable) return;

            if (lastTarget == null)
            {
                //25.05.13 CCK2.33.0.2 直前がbothの場合はleft指定でもrightも止まる
                hapticsAudio.StopLeft();
                hapticsAudio.StopRight();
            }

            var amp = (effect.amplitude ?? hapticsSettings.defaultAmplitude) * hapticsSettings.volume;
            var dur = effect.duration ?? hapticsSettings.defaultDuration;
            var freq = GetFreq(effect) * (hapticsSettings.maxFrequencyHz - hapticsSettings.minFrequencyHz) + hapticsSettings.minFrequencyHz;

            if (target == "left")
            {
                hapticsAudio.PlayLeft(amp, dur, freq);
                lastTarget = target;
            }
            else if (target == "right")
            {
                hapticsAudio.PlayRight(amp, dur, freq);
                lastTarget = target;
            }
            else
            {
                hapticsAudio.PlayBoth(amp, dur, freq);
                lastTarget = null;
            }
        }

        public void Shutdown()
        {
            //25.05.13 CCK2.33.0.2 PSが再設定されても振動は残る
            //hapticsAudio.StopLeft();
            //hapticsAudio.StopRight();
        }

        public object toJSON(string key)
        {
            dynamic o = new System.Dynamic.ExpandoObject();
            return o;
        }
        public override string ToString()
        {
            return String.Format("[AudioLinkHandle]");
        }
    }
}
