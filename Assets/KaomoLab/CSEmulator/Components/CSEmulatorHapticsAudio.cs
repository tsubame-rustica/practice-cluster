using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Components
{
    [DisallowMultipleComponent, AddComponentMenu("")]
    public class CSEmulatorHapticsAudio
         : MonoBehaviour
    {

        AudioSource leftSource;
        AudioSource rightSource;

        private void OnEnable()
        {
            var leftObject = new GameObject("CSEmulator Haptics Left");
            leftObject.transform.SetParent(transform, false);
            var rightObject = new GameObject("CSEmulator Haptics Right");
            rightObject.transform.SetParent(transform, false);

            leftSource = leftObject.AddComponent<AudioSource>();
            rightSource = rightObject.AddComponent<AudioSource>();
            InitializeAudioSource(leftSource, -1.0f);
            InitializeAudioSource(rightSource, 1.0f);
        }
        void InitializeAudioSource(AudioSource source, float panStereo)
        {
            source.loop = false;
            source.playOnAwake = true;
            source.panStereo = panStereo;
        }

        public void Construct(
        )
        {

        }

        AudioClip CreateClip(float amp, float dur, float freq)
        {
            var rate = 44100;
            var sample = Mathf.CeilToInt(rate * dur);
            var samples = new float[sample];

            var samplesPerPeriod = rate / freq;
            for (var i = 0; i < sample; i++)
            {
                var t = i % samplesPerPeriod;
                samples[i] = t < samplesPerPeriod / 2f ? amp : -amp;
            }

            var clip = AudioClip.Create(String.Format("{0}:{1}:{2}", amp, dur, freq), sample, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public void PlayLeft(float amp, float dur, float freq)
        {
            var clip = CreateClip(amp, dur, freq);
            leftSource.clip = clip;
            leftSource.Play();
        }

        public void PlayRight(float amp, float dur, float freq)
        {
            var clip = CreateClip(amp, dur, freq);
            rightSource.clip = clip;
            rightSource.Play();
        }

        public void PlayBoth(float amp, float dur, float freq)
        {
            var clip = CreateClip(amp, dur, freq);
            leftSource.clip = clip;
            leftSource.Play();
            rightSource.clip = clip;
            rightSource.Play();
        }

        public void StopLeft()
        {
            leftSource.Stop();
            leftSource.clip = null;
        }

        public void StopRight()
        {
            rightSource.Stop();
            rightSource.clip = null;
        }
    }
}
