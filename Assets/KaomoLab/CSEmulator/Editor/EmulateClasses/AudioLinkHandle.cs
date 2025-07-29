using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class AudioLinkHandle
    {
        readonly Type audioLinkType = null;
        readonly Component audioLink = null;
        readonly IItemExceptionFactory itemExceptionFactory;
        readonly ILogger logger;

        public AudioLinkHandle(
            GameObject gameObject,
            IItemExceptionFactory itemExceptionFactory,
            ILogger logger
        )
        {
            audioLinkType = Type.GetType("AudioLink.AudioLink,AudioLink");
            if(audioLinkType != null)
            {
                audioLink = gameObject.GetComponent(audioLinkType);
            }
            this.itemExceptionFactory = itemExceptionFactory;
            this.logger = logger;
        }

        bool CheckAudioLink()
        {
            if (audioLinkType != null) return true;
            UnityEngine.Debug.LogWarning("AudioLinkが導入されていません。");
            return false;
        }
        bool CheckFloat(float value)
        {
            //NaNはExceptionでInfinityは無視(CCK2.33.0.1現在)
            if (float.IsNaN(value)) throw itemExceptionFactory.CreateJsError("NaNは指定できません。");
            if (float.IsInfinity(value))
            {
                logger.Warning("AudioLinkのパラメータにInfinityが指定されています");
                return false;
            }
            return true;
        }
        void SetFloat(string key, float value)
        {
            audioLinkType.GetField(key).SetValue(audioLink, value);
        }
        void SetBool(string key, bool value)
        {
            audioLinkType.GetField(key).SetValue(audioLink, value);
        }

        public void applySettings()
        {
            if (!CheckAudioLink()) return;
            audioLinkType.GetMethod("UpdateSettings").Invoke(audioLink, null);
        }

        public void setAutogainDerate(float derate)
        {
            if (!CheckAudioLink()) return;
            if (!CheckFloat(derate)) return;
            derate = Mathf.Clamp(derate, 0.001f, 1.0f);
            SetFloat("autogainDerate", derate);
        }
        public void setBass(float bass)
        {
            if (!CheckAudioLink()) return;
            if (!CheckFloat(bass)) return;
            bass = Mathf.Clamp(bass, 0.0f, 2.0f);
            SetFloat("bass", bass);
        }
        public void setCrossover0(float crossoverPoint)
        {
            if (!CheckAudioLink()) return;
            if (!CheckFloat(crossoverPoint)) return;
            crossoverPoint = Mathf.Clamp(crossoverPoint, 0.0f, 0.168f);
            SetFloat("x0", crossoverPoint);
        }
        public void setCrossover1(float crossoverPoint)
        {
            if (!CheckAudioLink()) return;
            if (!CheckFloat(crossoverPoint)) return;
            crossoverPoint = Mathf.Clamp(crossoverPoint, 0.242f, 0.387f);
            SetFloat("x1", crossoverPoint);
        }
        public void setCrossover2(float crossoverPoint)
        {
            if (!CheckAudioLink()) return;
            if (!CheckFloat(crossoverPoint)) return;
            crossoverPoint = Mathf.Clamp(crossoverPoint, 0.461f, 0.628f);
            SetFloat("x2", crossoverPoint);
        }
        public void setCrossover3(float crossoverPoint)
        {
            if (!CheckAudioLink()) return;
            if (!CheckFloat(crossoverPoint)) return;
            crossoverPoint = Mathf.Clamp(crossoverPoint, 0.704f, 0.953f);
            SetFloat("x3", crossoverPoint);
        }
        public void setEnableAutogain(bool enableAutogain)
        {
            if (!CheckAudioLink()) return;
            SetBool("autogain", enableAutogain);
        }
        public void setFadeExpFalloff(float fadeExpFalloff)
        {
            if (!CheckAudioLink()) return;
            if (!CheckFloat(fadeExpFalloff)) return;
            fadeExpFalloff = Mathf.Clamp(fadeExpFalloff, 0.0f, 1.0f);
            SetFloat("fadeExpFalloff", fadeExpFalloff);
        }
        public void setFadeLength(float fadeLength)
        {
            if (!CheckAudioLink()) return;
            if (!CheckFloat(fadeLength)) return;
            fadeLength = Mathf.Clamp(fadeLength, 0.0f, 1.0f);
            SetFloat("fadeLength", fadeLength);
        }
        public void setGain(float gain)
        {
            if (!CheckAudioLink()) return;
            if (!CheckFloat(gain)) return;
            gain = Mathf.Clamp(gain, 0.0f, 2.0f);
            SetFloat("gain", gain);
        }
        public void setThreshold0(float threshold)
        {
            if (!CheckAudioLink()) return;
            if (!CheckFloat(threshold)) return;
            threshold = Mathf.Clamp(threshold, 0.0f, 1.0f);
            SetFloat("threshold0", threshold);
        }
        public void setThreshold1(float threshold)
        {
            if (!CheckAudioLink()) return;
            if (!CheckFloat(threshold)) return;
            threshold = Mathf.Clamp(threshold, 0.0f, 1.0f);
            SetFloat("threshold1", threshold);
        }
        public void setThreshold2(float threshold)
        {
            if (!CheckAudioLink()) return;
            if (!CheckFloat(threshold)) return;
            threshold = Mathf.Clamp(threshold, 0.0f, 1.0f);
            SetFloat("threshold2", threshold);
        }
        public void setThreshold3(float threshold)
        {
            if (!CheckAudioLink()) return;
            if (!CheckFloat(threshold)) return;
            threshold = Mathf.Clamp(threshold, 0.0f, 1.0f);
            SetFloat("threshold3", threshold);
        }
        public void setTreble(float treble)
        {
            if (!CheckAudioLink()) return;
            if (!CheckFloat(treble)) return;
            treble = Mathf.Clamp(treble, 0.0f, 2.0f);
            SetFloat("treble", treble);
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
