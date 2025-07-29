using System;
using System.Net;

namespace uOSC
{

    public class uOscServer
    {
        public static uOscServer instance = null;

        Udp udp_ = new DotNet.Udp();
        Thread thread_ = new DotNet.Thread();
        Parser parser_ = new Parser();

        public DataReceiveEvent onDataReceived = new DataReceiveEvent();

        bool isStarted_ = false;

        public uOscServer(IPAddress address, int listenPort)
        {
            if(instance != null)
            {
                throw new Exception(String.Format("OSC Serverのインスタンスが既にあります。開発者に連絡してください。"));
            }
            instance = this;
            udp_.StartServer(address, listenPort);
            thread_.Start(UpdateMessage);
            isStarted_ = true;
        }

        public void Dispose()
        {
            if (!isStarted_) return;
            thread_.Stop();
            udp_.Stop();
            onDataReceived.RemoveAllListeners();
            isStarted_ = false;
            instance = null;
        }

        void UpdateMessage()
        {
            while (udp_.messageCount > 0) 
            {
                var buf = udp_.Receive();
                int pos = 0;
                parser_.Parse(buf, ref pos, buf.Length);
            }
            if(parser_.messageCount > 0)
            {
                var messages = parser_.DequeueAll();
                onDataReceived.Invoke(messages);
            }
        }
    }

}