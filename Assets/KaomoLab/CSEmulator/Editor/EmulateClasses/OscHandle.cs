using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JintFunction = Jint.Native.Function.Function;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class OscHandle
    {

        readonly ICallbackExecutor callbackExecutor;
        readonly IOscReceiveListenerBinder oscReceiveListenerBinder;
        readonly IOscSender oscSender;
        readonly IOscContext oscContext;
        readonly IItemExceptionFactory itemExceptionFactory;
        readonly string playerId;

        public OscHandle(
            ICallbackExecutor callbackExecutor,
            IOscReceiveListenerBinder oscReceiveListenerBinder,
            IOscSender oscSender,
            IOscContext oscContext,
            IItemExceptionFactory itemExceptionFactory,
            string playerId
        )
        {
            this.callbackExecutor = callbackExecutor;
            this.oscReceiveListenerBinder = oscReceiveListenerBinder;
            this.oscSender = oscSender;
            this.oscContext = oscContext;
            this.itemExceptionFactory = itemExceptionFactory;
            this.playerId = playerId;
        }

        public bool isReceiveEnabled()
        {
            return oscContext.isReceiveEnabled;
        }

        public void onReceive(JintFunction Callback)
        {
            var jintCallback = new JintCallback<EmulateClasses.OscMessage[]>(callbackExecutor, Callback);
            //複数のhandleから複数回登録しても最後の登録のみが有効の模様。
            //PlayerScript系は同時に複数存在しないのでplayerのIDでいいはず
            oscReceiveListenerBinder.SetOscReceiveCallback(playerId, jintCallback);
        }

        public void send(OscMessage payload)
        {
            CheckSend(payload);
            oscSender.Send(payload);
        }
        public void send(OscBundle payload)
        {
            foreach(var m in payload.messages)
            {
                CheckSend(m);
            }
            oscSender.Send(payload);
        }
        void CheckSend(OscMessage target)
        {
            if (!oscContext.isSendEnabled) throw itemExceptionFactory.CreateGeneral("isSendEnabledがfalseです。");
            if (!target.address.StartsWith("/")) throw itemExceptionFactory.CreateGeneral("addressが/から始まっていません。");
            if (target.address.StartsWith("/cluster/")) throw itemExceptionFactory.CreateGeneral("addressの/cluster/は予約済みです。");
            //CSETODO 「送信するOSCメッセージが不正なメッセージや OscValue を含む場合」という状況がわからないので一旦ノーチェック
        }

        public object toJSON(string key)
        {
            return this;
        }
        public override string ToString()
        {
            return String.Format("[OscHandle][{0}]", isReceiveEnabled());
        }
    }
}
