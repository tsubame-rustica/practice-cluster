using Assets.KaomoLab.CSEmulator.Editor.EmulateClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class PlayerHandleFactoryBuilder
        : IItemOwnerHandler, IEngineApplyBuilder
    {
        public interface IBuilder
        {
            PlayerHandle Create(Components.CSEmulatorItemHandler csOwnerItemHandler);
        }
        public class PlayerHandleBuilder : IBuilder
        {
            class NopVoiceHandler : IVoiceHandler
            {
                public void SetVoiceVolumeRate(float rate)
                {
                    //メインプレイヤーのボイスを聞く人は今のところいない想定。
                }
            }

            public readonly string idfc;
            readonly PlayerHandleFactoryBuilder parent;
            readonly Components.CSEmulatorPlayerHandler csPlayerHandler;
            readonly Components.IPlayerMeta playerMeta;

            public PlayerHandleBuilder(
                PlayerHandleFactoryBuilder parent,
                Components.CSEmulatorPlayerHandler csPlayerHandler,
                Components.IPlayerMeta playerMeta,
                string idfc
            )
            {
                this.parent = parent;
                this.csPlayerHandler = csPlayerHandler;
                this.playerMeta = playerMeta;
                this.idfc = idfc;
            }

            public PlayerHandle Create(Components.CSEmulatorItemHandler csOwnerItemHandler)
            {
                if (csPlayerHandler == null) return null;
                var desktopPlayerController = csPlayerHandler.GetComponentInParent<ClusterVR.CreatorKit.Preview.PlayerController.DesktopPlayerController>();
                if (desktopPlayerController == null) return null;

                var csPlayerController = desktopPlayerController.gameObject.GetComponent<Components.CSEmulatorPlayerController>();
                var playerController = new CCKPlayerController(
                    csPlayerHandler, csPlayerController, desktopPlayerController, parent.playerOptions,
                    csPlayerController.pointOfViewManager,
                    csPlayerController.poseManager,
                    csPlayerController.faceConstraintManager,
                    parent.spawnPointManager
                );

                var playerLocalObjectGatherer = new PlayerScript.DefaultGatherer();

                //VrmPrepare側での設定をここで意識する必要があるのはどうにもよくないと思うけど
                //いいアイデアが出るか、困った事になるまではこれで行く
                var postProcessApplier = new PostProcessApplier(
                    csPlayerHandler.gameObject.transform.parent.parent.parent.gameObject,
                    parent.fogSettingsBridge
                );

                var voiceHandler = new NopVoiceHandler();

                var handle = new PlayerHandle(
                    playerMeta,
                    parent.spaceContext,
                    playerController,
                    playerController,
                    parent.userInterfaceHandler,
                    parent.textInputSender,
                    parent.buttonInterfaceHandler,
                    playerLocalObjectGatherer,
                    parent.productPurchaser,
                    parent.productAmount,
                    parent.productGranter,
                    parent.clusterEvent,
                    parent.playerLooks,
                    parent.playerBelongings,
                    parent.storedProducts,
                    postProcessApplier,
                    voiceHandler,
                    parent.hapticsAudioController,
                    parent.serializedPlayerStorage,
                    parent.messageSender,
                    csOwnerItemHandler
                );
                return handle;
            }
        }
        public class DummyPlayerHandleBuilder : IBuilder
        {
            class DummyPostProcessApplier : IPostProcessApplier
            {
                //Get系がないのでこれでOK
                public void Apply(BloomSettings settings) { }
                public void Apply(ChromaticAberrationSettings settings) { }
                public void Apply(ColorGradingSettings settings) { }
                public void Apply(DepthOfFieldSettings settings) { }
                public void Apply(FogSettings settings) { }
                public void Apply(GrainSettings settings) { }
                public void Apply(LensDistortionSettings settings) { }
                public void Apply(MotionBlurSettings settings) { }
                public void Apply(VignetteSettings settings) { }
            }
            class PlayerLooksDummyBridge : IPlayerLooks
            {
                public bool exists => parent._exists;
                public IReadOnlyList<string> accessoryProductIds => new List<string>();
                public string avatarProductId => null;

                readonly Components.CSEmulatorDummyPlayer parent;

                public PlayerLooksDummyBridge(
                    Components.CSEmulatorDummyPlayer parent
                )
                {
                    this.parent = parent;
                }
            }
            class PlayerBelongingsDummyBridge : IPlayerBelongings
            {
                public IReadOnlyList<string> accessoryProductIds => parent._accessoryProductIds;
                public IReadOnlyList<string> avatarProductIds => parent._avatarProductIds;
                public IReadOnlyList<string> craftItemProductIds => parent._craftItemProductIds;

                readonly Components.CSEmulatorDummyPlayer parent;

                public PlayerBelongingsDummyBridge(
                    Components.CSEmulatorDummyPlayer parent
                )
                {
                    this.parent = parent;
                }

                public void AddAccessory(string productId)
                {
                    if (parent._accessoryProductIds.Contains(productId)) return;
                    parent._accessoryProductIds.Add(productId);
                }

                public void AddAvater(string productId)
                {
                    if (parent._avatarProductIds.Contains(productId)) return;
                    parent._avatarProductIds.Add(productId);
                }

                public void AddCraftItem(string productId)
                {
                    if (parent._craftItemProductIds.Contains(productId)) return;
                    parent._craftItemProductIds.Add(productId);
                }
            }
            class SerializedPlayerStorageDummyBridge : ISerializedPlayerStorage
            {
                readonly Components.CSEmulatorDummyPlayer parent;

                public SerializedPlayerStorageDummyBridge(
                    Components.CSEmulatorDummyPlayer parent
                )
                {
                    this.parent = parent;
                }

                public string LoadPlayerStorage()
                {
                    return parent._playerStorage;
                }

                public void SavePlayerStorage(string serialized)
                {
                    parent._playerStorage = serialized;
                }
            }
            class BusyUserInputInterfaceHandler : IUserInputInterfaceHandler
            {
                public bool isUserInputting => true;
                public void StartDialog(string caption, string[] buttons, Action<int> Callback) { }
                public void StartPurchase(string productName, string productId, string meta, Action<PurchaseRequestStatus> Callback)
                {
                    Callback(PurchaseRequestStatus.Busy);
                }

                public void StartTextInput(string caption, Action<string> SendCallback, Action CancelCallback, Action BusyCallback)
                {
                    BusyCallback();
                }
            }
            class DummyProductAmount : IProductAmount
            {
                public (int, int) GetProductAmount(string productId)
                {
                    return (0, 0);
                }

                public void SetProductAmount(string productId, int plus, int minus)
                {
                    //常にBuzyのため、実質Setされないので処理を行わない
                }
            }
            class VoiceHandlerDummyBridge : IVoiceHandler
            {
                readonly Components.CSEmulatorDummyPlayer parent;

                public VoiceHandlerDummyBridge(
                    Components.CSEmulatorDummyPlayer parent
                )
                {
                    this.parent = parent;
                }

                public void SetVoiceVolumeRate(float rate)
                {
                    parent.SetVolumeRate(rate);
                }
            }
            class DummyHapticsAudio : IHapticsAudioController
            {
                public void PlayBoth(float amp, float dur, float freq) { }
                public void PlayLeft(float amp, float dur, float freq) { }
                public void PlayRight(float amp, float dur, float freq) { }
                public void StopLeft() { }
                public void StopRight() { }
            }
            class DummyButtonInterfaceHandler : IButtonInterfaceHandler
            {
                //Get系がないためこれでOK
                public void DeleteAllButtonCallbacks() { }
                public void HideAllButtons() { }
                public void HideButton(int index) { }
                public void SetButtonCallback(int index, IJintCallback<bool> Callback) { }
                public void ShowButton(int index, IconAsset icon) { }
            }
            class OverwritePlayerLocalObjectGatherer : IPlayerLocalObjectGatherer
            {
                readonly IPlayerLocalObjectGatherer defaultList;

                public OverwritePlayerLocalObjectGatherer(
                    IPlayerLocalObjectGatherer defaultList
                )
                {
                    this.defaultList = defaultList;
                }

                public PlayerLocalObject GetPlayerLocalObject(string playerId, string id, GameObject listObject, IItemExceptionFactory itemExceptionFactory, ILogger logger)
                {
                    var overwrite = GetOverwritePlayerLocalObject(playerId, id, listObject, itemExceptionFactory, logger);
                    if(overwrite != null) return overwrite;

                    var original = defaultList.GetPlayerLocalObject(playerId, id, listObject, itemExceptionFactory, logger);
                    return original;
                }
                PlayerLocalObject GetOverwritePlayerLocalObject(string playerId, string id, GameObject listObject, IItemExceptionFactory itemExceptionFactory, ILogger logger)
                {
                    var list = listObject.GetComponent<Components.CSEmulatorDummyPlayerLocalObjectReferenceList>();
                    if (list == null)
                        return null;

                    var entry = list.references.FirstOrDefault(i => i.Id == id);
                    if (entry == null)
                        return null;
                    //ダミー系はテスト要素なのでチェック不要。
                    var ret = new PlayerLocalObject(playerId, entry.GameObject, itemExceptionFactory, logger);
                    return ret;

                }
            }

            readonly PlayerHandleFactoryBuilder parent;
            readonly Components.CSEmulatorPlayerHandler csPlayerHandler;
            readonly Components.CSEmulatorDummyPlayer csDummyPlayer;

            public DummyPlayerHandleBuilder(
                PlayerHandleFactoryBuilder parent,
                Components.CSEmulatorPlayerHandler csPlayerHandler,
                Components.CSEmulatorDummyPlayer csDummyPlayer
            )
            {
                this.parent = parent;
                this.csPlayerHandler = csPlayerHandler;
                this.csDummyPlayer = csDummyPlayer;
            }

            public PlayerHandle Create(Components.CSEmulatorItemHandler csOwnerItemHandler)
            {
                if (csDummyPlayer == null) return null;

                var csPlayerController = csDummyPlayer.dummyPlayerController.gameObject.GetComponent<Components.CSEmulatorPlayerController>();
                var playerController = new CCKPlayerController(
                    csPlayerHandler, csPlayerController, csDummyPlayer.dummyPlayerController, parent.playerOptions,
                    csPlayerController.pointOfViewManager,
                    csPlayerController.poseManager,
                    csPlayerController.faceConstraintManager,
                    parent.spawnPointManager
                );
                var postProcessApplier = new DummyPostProcessApplier();
                var playerLooks = new PlayerLooksDummyBridge(csDummyPlayer);
                var playerBelongings = new PlayerBelongingsDummyBridge(csDummyPlayer);
                var serializedPlayerStorage = new SerializedPlayerStorageDummyBridge(csDummyPlayer);
                var userInputInterfaceHandler = new BusyUserInputInterfaceHandler();
                var productAmount = new DummyProductAmount();
                var voiceHandler = new VoiceHandlerDummyBridge(csDummyPlayer);
                var hapticsAudio = new DummyHapticsAudio();
                var buttonInterfaceHandler = new DummyButtonInterfaceHandler();
                var playerLocalObjectGatherer = new OverwritePlayerLocalObjectGatherer(
                    new PlayerScript.DefaultGatherer()
                );

                var handle = new PlayerHandle(
                    csDummyPlayer,
                    parent.spaceContext,
                    playerController,
                    playerController,
                    userInputInterfaceHandler,
                    parent.textInputSender,
                    buttonInterfaceHandler,
                    playerLocalObjectGatherer,
                    parent.productPurchaser,
                    productAmount,
                    parent.productGranter,
                    parent.clusterEvent,
                    playerLooks,
                    playerBelongings,
                    parent.storedProducts,
                    postProcessApplier,
                    voiceHandler,
                    hapticsAudio,
                    serializedPlayerStorage,
                    parent.messageSender,
                    csOwnerItemHandler
                );
                return handle;
            }
        }



        public class PlayerHandleFactory
            : IPlayerHandleFactory
        {
            readonly Dictionary<string, IBuilder> builders;

            //Engine層で渡したい時がある時はここ
            public PlayerHandleFactory(
                Dictionary<string, IBuilder> builders
            )
            {
                this.builders = builders;
            }

            public PlayerHandle CreateByIdfc(string idfc, Components.CSEmulatorItemHandler csOwnerItemHandler)
            {
                var b = builders[idfc];
                var ret = b.Create(csOwnerItemHandler);
                return ret;
            }
        }

        readonly Dictionary<string, IBuilder> players = new ();

        readonly ISpaceContext spaceContext;
        readonly IUserInputInterfaceHandler userInterfaceHandler;
        readonly ITextInputSender textInputSender;
        readonly IButtonInterfaceHandler buttonInterfaceHandler;
        readonly IProductPurchaser productPurchaser;
        readonly IProductAmount productAmount;
        readonly IProductGranter productGranter;
        readonly IClusterEvent clusterEvent;
        readonly IPlayerLooks playerLooks;
        readonly IPlayerBelongings playerBelongings;
        readonly IStoredProducts storedProducts;
        readonly IHapticsAudioController hapticsAudioController;
        readonly IMessageSender messageSender;
        readonly IPlayerViewOptions playerOptions;
        readonly FogSettingsBridge fogSettingsBridge;
        readonly ISerializedPlayerStorage serializedPlayerStorage;
        readonly ClusterVR.CreatorKit.Editor.Preview.World.SpawnPointManager spawnPointManager;

        public PlayerHandleFactoryBuilder(
            ISpaceContext spaceContext,
            IUserInputInterfaceHandler userInterfaceHandler,
            ITextInputSender textInputSender,
            IButtonInterfaceHandler buttonInterfaceHandler,
            IProductPurchaser productPurchaser,
            IProductAmount productAmount,
            IProductGranter productGranter,
            IClusterEvent clusterEvent,
            IPlayerLooks playerLooks,
            IPlayerBelongings playerBelongings,
            IStoredProducts storedProducts,
            IHapticsAudioController hapticsAudioController,
            IMessageSender messageSender,
            IPlayerViewOptions playerOptions,
            FogSettingsBridge fogSettingsBridge,
            ISerializedPlayerStorage serializedPlayerStorage,
            ClusterVR.CreatorKit.Editor.Preview.World.SpawnPointManager spawnPointManager
        )
        {
            this.spaceContext = spaceContext;
            this.userInterfaceHandler = userInterfaceHandler;
            this.textInputSender = textInputSender;
            this.buttonInterfaceHandler = buttonInterfaceHandler;
            this.productPurchaser = productPurchaser;
            this.productAmount = productAmount;
            this.productGranter = productGranter;
            this.clusterEvent = clusterEvent;
            this.playerLooks = playerLooks;
            this.playerBelongings = playerBelongings;
            this.storedProducts = storedProducts;
            this.hapticsAudioController = hapticsAudioController;
            this.messageSender = messageSender;
            this.playerOptions = playerOptions;
            this.fogSettingsBridge = fogSettingsBridge;
            this.serializedPlayerStorage = serializedPlayerStorage;
            this.spawnPointManager = spawnPointManager;
        }

        public void AddPlayer(Components.CSEmulatorPlayerHandler playerHandler, Components.IPlayerMeta playerMeta)
        {
            var b = new PlayerHandleBuilder(this, playerHandler, playerMeta, playerHandler.idfc);
            players.Add(playerHandler.idfc, b);
        }

        public void AddDummyPlayer(Components.CSEmulatorDummyPlayer dummyPlayer)
        {
            var b = new DummyPlayerHandleBuilder(this, dummyPlayer.csPlayerHandler, dummyPlayer);
            players.Add(dummyPlayer._idfc, b);
        }

        public IPlayerHandleFactory BuildFactory(
            Components.CSEmulatorItemHandler csItemHandler,
            Jint.Engine engine
        )
        {
            var ret = new PlayerHandleFactory(
                players
            );
            return ret;
        }

        public string GetOwnerIdfc()
        {
            //CSETODO Itemオーナー系はマルチ未対応。
            return players.Values.OfType<PlayerHandleBuilder>().First().idfc;
        }

    }
}
