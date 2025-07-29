using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class PlayerScriptSetter
    {
        readonly IPlayerStoragerFactory playerStoragerFactory;
        readonly IMessageSender messageSender;
        readonly IPlayerReceiveListenerBinder playerReceiveListenerBinder;
        readonly IOscReceiveListenerBinder oscReceiveListenerBinder;
        readonly IOscSender oscSender;
        readonly IOscContext oscContext;
        readonly IPlayerLooks playerLooks;
        readonly IHapticsSettings hapticsSettings;
        readonly IPlayerSendableSanitizer playerSendableSanitizer;
        readonly IItemExceptionFactory itemExceptionFactory;
        readonly IPlayerScriptRunner playerScriptRunner;
        readonly PlayerScript.ClusterWorldRuntimeSettings clusterWorldRuntimeSettings;
        readonly IRayDrawer rayDrawer;

        public PlayerScriptSetter(
            IPlayerStoragerFactory playerStoragerFactory,
            IMessageSender messageSender,
            IPlayerReceiveListenerBinder playerReceiveListenerBinder,
            IOscReceiveListenerBinder oscReceiveListenerBinder,
            IOscSender oscSender,
            IOscContext oscContext,
            IPlayerLooks playerLooks,
            IHapticsSettings hapticsSettings,
            IPlayerSendableSanitizer playerSendableSanitizer,
            IItemExceptionFactory itemExceptionFactory,
            IPlayerScriptRunner playerScriptRunner,
            IRayDrawer rayDrawer
        )
        {
            this.playerStoragerFactory = playerStoragerFactory;
            this.messageSender = messageSender;
            this.playerReceiveListenerBinder = playerReceiveListenerBinder;
            this.oscReceiveListenerBinder = oscReceiveListenerBinder;
            this.oscSender = oscSender;
            this.oscContext = oscContext;
            this.playerLooks = playerLooks;
            this.hapticsSettings = hapticsSettings;
            this.playerSendableSanitizer = playerSendableSanitizer;
            this.itemExceptionFactory = itemExceptionFactory;
            this.playerScriptRunner = playerScriptRunner;
            this.rayDrawer = rayDrawer;

            var worldRuntimeSettings = ClusterVR.CreatorKit.Editor.Builder.WorldRuntimeSettingGatherer.GatherWorldRuntimeSettings(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene()
            ).FirstOrDefault();
            clusterWorldRuntimeSettings = new PlayerScript.ClusterWorldRuntimeSettings(
                worldRuntimeSettings
            );
        }


        public void Set(
            PlayerHandle playerHandle, ClusterScript clusterScript, string code
        )
        {
            var storager = playerStoragerFactory.Create(
                clusterScript.csItemHandler,
                playerHandle.serializedPlayerStorage
            );
            var ps = new PlayerScript(
                clusterWorldRuntimeSettings,
                messageSender,
                playerReceiveListenerBinder,
                playerHandle.buttonInterfaceHandler,
                playerHandle.playerLocalObjectGatherer,
                oscReceiveListenerBinder,
                oscSender,
                oscContext,
                playerLooks,
                hapticsSettings,
                playerSendableSanitizer,
                storager,
                itemExceptionFactory,
                rayDrawer,
                playerHandle,
                clusterScript,
                code
            );
            playerScriptRunner.Run(ps, playerHandle.id);
        }
    }
}
