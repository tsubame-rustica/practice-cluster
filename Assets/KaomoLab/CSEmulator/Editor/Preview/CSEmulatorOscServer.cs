using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Preview
{
    public class CSEmulatorOscServer
        : EmulateClasses.IOscReceiveListenerBinder
    {
        SynchronizationContext main = null;
        uOSC.uOscServer oscServer = null;
        readonly Dictionary<string, EmulateClasses.IJintCallback<EmulateClasses.OscMessage[]>> callbacks = new();

        readonly EmulateClasses.IOscContext oscContext;

        public CSEmulatorOscServer(
            EmulateClasses.IOscContext oscContext
        )
        {
            this.oscContext = oscContext;
        }

        public void Start(string address, int port)
        {
            if(oscServer != null) Shutdown();

            var ipAddress = Parse(address);
            if (ipAddress == null) return;

            main = SynchronizationContext.Current;
            oscServer = new uOSC.uOscServer(ipAddress, port);
            oscServer.onDataReceived.AddListener(
                (uOSC.Message[] _messages) => {
                    main.Post((_) => {
                        if (!oscContext.isReceiveEnabled) return;

                        var messages = new List<EmulateClasses.OscMessage>();
                        foreach (var m in _messages)
                        {
                            if (m.address.StartsWith("/cluster/")) continue;

                            var values = new List<EmulateClasses.OscValue>();
                            var count = m.values.Length;
                            foreach (var v in m.values)
                            {
                                if (v is int intValue) values.Add(new EmulateClasses.OscValue(intValue));
                                if (v is float floatValue) values.Add(new EmulateClasses.OscValue(floatValue));
                                if (v is bool boolValue) values.Add(new EmulateClasses.OscValue(boolValue));
                                if (v is string stringValue) values.Add(new EmulateClasses.OscValue(stringValue));
                                if (v is byte[] bytesValue) values.Add(new EmulateClasses.OscValue(bytesValue));
                            }

                            var message = EmulateClasses.OscMessage.Construct(
                                m.address,
                                CSEmulator.Commons.UnixEpochMs(m.timestamp.ToUtcTime()),
                                values.ToArray()

                            );
                            messages.Add(message);
                        }

                        foreach (var id in callbacks.Keys)
                        {
                            callbacks[id].Execute(messages.ToArray());
                        }
                    }, null);
                }
            );
        }
        System.Net.IPAddress Parse(string address)
        {
            try
            {
                var ipAddress = System.Net.IPAddress.Parse(address);
                return ipAddress;
            }
            catch(FormatException)
            {
                UnityEngine.Debug.LogError(String.Format("IPアドレスの書式が正しくありません。[{0}]", address));
                return null;
            }
        }

        public void Shutdown()
        {
            oscServer?.Dispose();
            oscServer = null;
            main = null;
        }

        public void SetOscReceiveCallback(string id, EmulateClasses.IJintCallback<EmulateClasses.OscMessage[]> Callback)
        {
            callbacks[id] = Callback;
        }

        public void DeleteOscReceiveCallback(string id)
        {
            callbacks.Remove(id);
        }
    }
}
