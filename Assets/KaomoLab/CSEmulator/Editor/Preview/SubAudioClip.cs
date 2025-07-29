using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Assets.KaomoLab.CSEmulator.Editor.Preview
{
    public class SubAudioClip
        : Components.ISubAudioController
    {
        class AudioClipBuffer<T>
        {
            readonly T[] buffer;
            readonly T padding;
            int head = 0;
            int tail = 0;
            public int count { get; private set; } = 0;

            public AudioClipBuffer(int size, T padding)
            {
                this.buffer = new T[size];
                this.padding = padding;
            }

            public void Write(T value)
            {
                if (count == buffer.Length)
                {
                    tail = (tail + 1) >= buffer.Length ? (tail + 1) % buffer.Length : (tail + 1);
                    count--;
                }
                buffer[head] = value;
                head = (head + 1) >= buffer.Length ? (head + 1) % buffer.Length : (head + 1);
                count++;
            }

            public void Read(T[] output)
            {
                var read = Math.Min(output.Length, count);
                var firstPart = Math.Min(read, buffer.Length - tail);
                Array.Copy(buffer, tail, output, 0, firstPart);

                var remaining = read - firstPart;
                if (remaining > 0)
                {
                    Array.Copy(buffer, 0, output, firstPart, remaining);
                }

                tail = (tail + read) % buffer.Length;
                count -= read;

                //念の為ゼロ埋め
                for (var i = read; i < output.Length; i++)
                {
                    output[i] = padding;
                }
            }
        }

        interface ISubAudioFactory : Components.ISubAudioFactory
        {
            void Destory();
        }
        class Standard : ISubAudioFactory
        {
            readonly int sampleRate;
            readonly int bufferSec;

            UnityEngine.AudioClip mix = null;
            UnityEngine.AudioClip left = null;
            UnityEngine.AudioClip right = null;

            readonly List<AudioClipBuffer<float>> mixBuffers = new();
            readonly List<AudioClipBuffer<float>> leftBuffers = new();
            readonly List<AudioClipBuffer<float>> rightBuffers = new();

            public Standard(
                int sampleRate, int bufferSec
            )
            {
                this.sampleRate = sampleRate;
                this.bufferSec = bufferSec;
            }

            public UnityEngine.AudioClip CreateClip(ClusterVR.CreatorKit.World.SpeakerType speakerType)
            {
                if (speakerType == ClusterVR.CreatorKit.World.SpeakerType.Mixed)
                    return CreateMixClip();
                if (speakerType == ClusterVR.CreatorKit.World.SpeakerType.Left)
                    return CreateLeftClip();
                if (speakerType == ClusterVR.CreatorKit.World.SpeakerType.Right)
                    return CreateRightClip();
                throw new Exception("このエラーが出たら開発者に連絡して下さい。");
            }

            public UnityEngine.AudioClip GetReuseClip(ClusterVR.CreatorKit.World.SpeakerType speakerType)
            {
                if (speakerType == ClusterVR.CreatorKit.World.SpeakerType.Mixed)
                {
                    if (mix == null) mix = CreateMixClip();
                    return mix;
                }
                if (speakerType == ClusterVR.CreatorKit.World.SpeakerType.Left)
                {
                    if(left == null) left = CreateLeftClip();
                    return left;
                }
                if (speakerType == ClusterVR.CreatorKit.World.SpeakerType.Right)
                {
                    if(right == null) right = CreateRightClip();
                    return right;
                }
                throw new Exception("このエラーが出たら開発者に連絡して下さい。");
            }

            public void Destory()
            {
                mix = null;
                left = null;
                right = null;
                mixBuffers.Clear();
                leftBuffers.Clear();
                rightBuffers.Clear();
            }

            public void Write(float left, float right)
            {
                foreach (var buffer in mixBuffers)
                {
                    buffer.Write(left);
                    buffer.Write(right);
                }
                foreach (var buffer in leftBuffers)
                {
                    buffer.Write(left);
                }
                foreach (var buffer in rightBuffers)
                {
                    buffer.Write(right);
                }
            }

            public void WaitMixCount(int count)
            {
                if (mixBuffers.Count == 0) return;
                var buffer = mixBuffers[0];
                while (buffer.count < count) { }
            }

            UnityEngine.AudioClip CreateMixClip()
            {
                var buffer = new AudioClipBuffer<float>(sampleRate * bufferSec, 0);
                mixBuffers.Add(buffer);
                var ret = UnityEngine.AudioClip.Create("SubAudio", sampleRate, 2, sampleRate, true, data =>
                {
                    buffer.Read(data);
                });
                return ret;
            }
            UnityEngine.AudioClip CreateLeftClip()
            {
                var buffer = new AudioClipBuffer<float>(sampleRate * bufferSec, 0);
                leftBuffers.Add(buffer);
                var ret = UnityEngine.AudioClip.Create("SubAudio_Left", sampleRate, 1, sampleRate, true, data =>
                {
                    buffer.Read(data);
                });
                return ret;
            }
            UnityEngine.AudioClip CreateRightClip()
            {
                var buffer = new AudioClipBuffer<float>(sampleRate * bufferSec, 0);
                rightBuffers.Add(buffer);
                var ret = UnityEngine.AudioClip.Create("SubAudio_Right", sampleRate, 1, sampleRate, true, data =>
                {
                    buffer.Read(data);
                });
                return ret;
            }
        }
        class ForceReuse : ISubAudioFactory
        {
            readonly UnityEngine.AudioClip mix;
            readonly UnityEngine.AudioClip left;
            readonly UnityEngine.AudioClip right;

            public ForceReuse(
                UnityEngine.AudioClip mix,
                UnityEngine.AudioClip left,
                UnityEngine.AudioClip right
            )
            {
                this.mix = mix;
                this.left = left;
                this.right = right;
            }

            public UnityEngine.AudioClip CreateClip(ClusterVR.CreatorKit.World.SpeakerType speakerType)
            {
                return GetReuseClip(speakerType);
            }

            public UnityEngine.AudioClip GetReuseClip(ClusterVR.CreatorKit.World.SpeakerType speakerType)
            {
                if (speakerType == ClusterVR.CreatorKit.World.SpeakerType.Mixed)
                    return mix;
                if (speakerType == ClusterVR.CreatorKit.World.SpeakerType.Left)
                    return left;
                if (speakerType == ClusterVR.CreatorKit.World.SpeakerType.Right)
                    return right;
                throw new Exception("このエラーが出たら開発者に連絡して下さい。");
            }

            public void Destory()
            {
                //nop
            }
        }

        public event Action<Components.ISubAudioFactory> OnSubAudioStarted = delegate { };
        public event Action OnSubAudioEnded = delegate { };

        readonly string device;

        System.Threading.CancellationTokenSource subAudioClipSplitterCanceller = null;
        WaveInEvent waveInEvent = null;
        ISubAudioFactory subAudioFactory = null;

        public SubAudioClip()
        {
        }

        public void ChangeSubAudioPlaying(bool isPlaying, string device)
        {
            if (isPlaying)
            {
                Shutdown();
                subAudioFactory = GetClips(device);
                OnSubAudioStarted.Invoke(subAudioFactory);
            }
            else
            {
                UnityEngine.Microphone.End(device == "" ? null : device);
                OnSubAudioEnded.Invoke();
                Shutdown();
            }
        }

        ISubAudioFactory GetClips(
            string device
        )
        {
            var rate = UnityEngine.AudioSettings.outputSampleRate;

            //NAudioはWin用
            //https://qiita.com/SousiOmine/items/976ed486803fc70992ad
            if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.WindowsEditor)
            {
                UnityEngine.Debug.Log($"サブ音声：Windows");

                var bufferSec = 2;
                var factory = new Standard(rate, bufferSec);

                waveInEvent = new WaveInEvent();
                var (deviceNumber, waveInDevice) = SelectDevice(device);
                waveInEvent.DeviceNumber = deviceNumber;
                waveInEvent.WaveFormat = new WaveFormat(rate, 16, waveInDevice.Channels);
                waveInEvent.BufferMilliseconds = 100;
                waveInEvent.DataAvailable += (o, e) =>
                {
                    var sampleCount = e.BytesRecorded / 2; //LLRRLLRRLLRRと入ってくる
                    for (var i = 0; i < sampleCount; i += 2)
                    {
                        var left = BitConverter.ToInt16(e.Buffer, i * 2) / 32768f; //LLRRのLLをfloatにする
                        var right = BitConverter.ToInt16(e.Buffer, (i + 1) * 2) / 32768f; //LLRRのRRをfloatにする
                        factory.Write(left, right);
                    }
                };
                waveInEvent.StartRecording();
                //どうやらAudioClipはnewのタイミングで1.5秒分先読みするらしい。1.5秒分待機しておくとブチッとならない。
                factory.WaitMixCount((int)(rate * 1.5));

                return factory;
            }
            else
            {
                var mix = UnityEngine.Microphone.Start(device == "" ? null : device, true, 5, rate);
                //0だとバッファかつかつの為かノイズが入るので0.25秒分待機。もっと短くてもいいかもしれないが。
                while (UnityEngine.Microphone.GetPosition(device) < (rate / 4)) { }

                //Unityのマイクはモノラルになってしまう
                //https://discussions.unity.com/t/how-to-record-stereo-sound-in-unity/694555
                if (mix.channels == 1)
                {
                    UnityEngine.Debug.Log($"サブ音声：モノラル");
                    //Mac対応までの仮
                    var factory = new ForceReuse(mix, mix, mix);
                    return factory;
                }
                else
                { //ステレオにはならないとは思うが念の為
                    UnityEngine.Debug.Log($"サブ音声：ステレオ");
                    subAudioClipSplitterCanceller = new System.Threading.CancellationTokenSource();
                    var left = UnityEngine.AudioClip.Create(
                        mix.name + "_Left", mix.samples, 1, mix.frequency, false
                    );
                    var right = UnityEngine.AudioClip.Create(
                        mix.name + "_Right", mix.samples, 1, mix.frequency, false
                    );
                    var lastPos = SplitClip(UnityEngine.Microphone.GetPosition(device), 0, mix, left, right);

                    Task.Run(() =>
                    {
                        while (!subAudioClipSplitterCanceller.IsCancellationRequested)
                        {
                            System.Threading.Thread.Sleep(100);
                            lastPos = SplitClip(UnityEngine.Microphone.GetPosition(device), lastPos, mix, left, right);
                        }
                    }, subAudioClipSplitterCanceller.Token);

                    //Mac対応までの仮
                    var factory = new ForceReuse(mix, left, right);
                    return factory;
                }
            }
        }
        int SplitClip(int pos, int lastPos, UnityEngine.AudioClip source, UnityEngine.AudioClip left, UnityEngine.AudioClip right)
        {
            if (pos == lastPos) return lastPos;

            //var ch = source.channels;
            var ch = 2;
            var sampleRead = (pos - lastPos + source.samples) % source.samples;
            if (sampleRead == 0) return lastPos;

            var sourceData = new float[sampleRead * ch];
            var leftData = new float[sampleRead];
            var rightData = new float[sampleRead];

            source.GetData(sourceData, lastPos);

            for (var i = 0; i < sampleRead; i++)
            {
                var si = i * ch;
                leftData[i] = sourceData[si];
                rightData[i] = sourceData[si + 1];
            }

            left.SetData(leftData, lastPos);
            right.SetData(rightData, lastPos);

            return pos;
        }
        (int, WaveInCapabilities) SelectDevice(string deviceName)
        {
            for (var i = 0; i < WaveIn.DeviceCount; i++)
            {
                var device = WaveIn.GetCapabilities(i);
                if (deviceName.StartsWith(device.ProductName)) //31文字で切れる
                {
                    return (i, device);
                }
            }
            return (0, WaveIn.GetCapabilities(0));
        }

        public void Shutdown()
        {
            if (subAudioClipSplitterCanceller != null)
            {
                subAudioClipSplitterCanceller.Cancel();
                subAudioClipSplitterCanceller = null;
            }
            if (waveInEvent != null)
            {
                waveInEvent.StopRecording();
                waveInEvent.Dispose();
                waveInEvent = null;
            }
            if (subAudioFactory != null)
            {
                subAudioFactory.Destory();
                subAudioFactory = null;
            }
        }
    }
}
