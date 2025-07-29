using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Components
{
    [DisallowMultipleComponent, AddComponentMenu(""), RequireComponent(typeof(AudioSource))]
    public class CSEmulatorAudioAmplifier
         : MonoBehaviour
    {
        public float gain;

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (gain < 1.0) return; //1.0以下の場合はAudioSourceのvolumeでの制御とする。
            for (var i = 0; i < data.Length; i++)
            {
                data[i] *= gain;
            }
        }
    }
}
