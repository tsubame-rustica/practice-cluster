using ClusterVR.CreatorKit.World.Implements.Speaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Components
{
    [DisallowMultipleComponent, AddComponentMenu(""), RequireComponent(typeof(Speaker))]
    public class CSEmulatorSubAudioHandler
        : MonoBehaviour
    {
        public static IEnumerable<Speaker> GetAllSpeakers()
        {
            var speakers = UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                .GetRootGameObjects()
                .SelectMany(
                    //inactiveでも把握する必要がある
                    o => o.GetComponentsInChildren<Speaker>(true)
                );

            return speakers;
        }


        ISubAudioController subAudioController;
        AudioClip prevClip;
        AudioClip monoClip;

        public void Construct(
            ISubAudioController subAudioController
        )
        {
            this.subAudioController = subAudioController;
            var audioSource = GetComponent<AudioSource>();
            var speaker = GetComponent<Speaker>();

            prevClip = audioSource.clip;

            subAudioController.OnSubAudioStarted += factory =>
            {
                prevClip = audioSource.clip;
                //同じAudioClipをAudioLinkやSteamAudioで同時に使うと、排他っぽい挙動をするのでAudioClipを別に用意することにした。
                //AudioClipをAudioLinkが利用しているかどうか等を確認するのが思ったより面倒なので全部Createにする
                audioSource.clip = factory.CreateClip(speaker.SpeakerType);
                audioSource.Play();
            };
            subAudioController.OnSubAudioEnded += () =>
            {
                audioSource.Stop();
                audioSource.clip = prevClip;
            };
        }
    }
}
