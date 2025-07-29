using Assets.KaomoLab.CSEmulator.Editor.EmulateClasses;
using Assets.KaomoLab.CSEmulator.Editor.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Preview
{
    public class EngineFacade
    {
        //必要なら別ファイルに
        class SpaceContext
            : ISpaceContext
        {
            public void Update()
            {
            }
        }

        //必要なら別ファイルに。正直ここでやることではない気がする。どうにかならんのか
        class SpawnPointHolderWrapper : Components.ISpawnPointHolder
        {
            readonly ClusterVR.CreatorKit.Editor.Preview.World.SpawnPointManager spawnPointManager;
            public SpawnPointHolderWrapper(
                ClusterVR.CreatorKit.Editor.Preview.World.SpawnPointManager spawnPointManager
            )
            {
                this.spawnPointManager = spawnPointManager;
            }
            public Components.SpawnPoint GetSpawnPoint(Components.PermissionType permissionType)
            {
                var p = spawnPointManager.GetRespawnPoint((ClusterVR.CreatorKit.Editor.Preview.World.PermissionType)permissionType);
                var ret = new Components.SpawnPoint(p.Position, p.YRotation);
                return ret;
            }
        }

        //必要なら別ファイルに。なんだか様子がおかしい。ここに置いてよいのか？
        class HapticsAudioBridge : EmulateClasses.IHapticsAudioController
        {
            Components.CSEmulatorHapticsAudio haptics;

            public HapticsAudioBridge()
            {
            }

            public void Start()
            {
                var gameObject = new UnityEngine.GameObject("CSEmulatorHapticsAudio");
                haptics = gameObject.AddComponent<Components.CSEmulatorHapticsAudio>();
                haptics.Construct();
            }

            public void Shutdown()
            {
                haptics.StopLeft();
                haptics.StopRight();
                UnityEngine.GameObject.Destroy(haptics.gameObject);
                haptics = null;
            }

            public void PlayLeft(float amp, float dur, float freq) { haptics?.PlayLeft(amp, dur, freq); }
            public void PlayRight(float amp, float dur, float freq) { haptics?.PlayRight(amp, dur, freq); }
            public void PlayBoth(float amp, float dur, float freq) { haptics?.PlayBoth(amp, dur, freq); }
            public void StopLeft() { haptics?.StopLeft(); }
            public void StopRight() { haptics?.StopRight(); }
        }

        //必要なら別ファイルに。ファイル増やすと消すに消せなくなるので。
        class OscClientWrapper : EmulateClasses.IOscSender
        {
            uOSC.uOscClient client;
            UnityEngine.GameObject gameObject;

            public OscClientWrapper(){}

            public void Start(string address, int port)
            {
                gameObject = new UnityEngine.GameObject("CSEmulator OSC Client");
                gameObject.SetActive(false);
                client = gameObject.AddComponent<uOSC.uOscClient>();
                client.address = address;
                client.port = port;
                gameObject.SetActive(true);
            }

            public void Shutdown()
            {
                client.StopClient();
                gameObject.SetActive(false);
            }

            public void Send(EmulateClasses.OscBundle payload)
            {
                var bundle = ConvertBundle(payload);
                client.Send(bundle);
            }
            public void Send(EmulateClasses.OscMessage payload)
            {
                var message = ConvertMessage(payload);
                client.Send(message);
            }

            uOSC.Message ConvertMessage(EmulateClasses.OscMessage message)
            {
                var values = message.values.Select(v =>
                {
                    if (v.types == OscValue.Types.@bool) return (object)v.boolValue;
                    if (v.types == OscValue.Types.@float) return (object)v.floatValue;
                    if (v.types == OscValue.Types.@int) return (object)v.intValue;
                    if (v.types == OscValue.Types.@string) return (object)v.stringValue;
                    if (v.types == OscValue.Types.bytes) return (object)v.bytesValue;
                    throw new Exception(String.Format("不明な形式{0}", v.ToString()));
                }).ToArray();
                var ret = new uOSC.Message(message.address, values);
                return ret;
            }
            uOSC.Bundle ConvertBundle(EmulateClasses.OscBundle bundle)
            {
                var ret = new uOSC.Bundle(
                    uOSC.Timestamp.CreateFromDateTime(
                        CSEmulator.Commons.UnixEpochMsDateTime(bundle.timestamp)
                ));
                foreach(var m in bundle.messages){
                    var message = ConvertMessage(m);
                    ret.Add(message);
                }
                return ret;
            }
        }

        //必要なら別ファイルに。
        public class ShutdownNotifier : Components.IShutdownNotifier
        {
            public event Handler OnShutdown = delegate { };
            public void Shutdown()
            {
                OnShutdown.Invoke();
            }
        }


        readonly CckPreviewFinder previewFinder;
        readonly ItemCollector itemCollector;
        readonly DesktopPlayerControllerReflector desktopPlayerControllerReflector;
        readonly VrmPreparer vrmPreparer;

        readonly PrefabItemStore prefabItemStore;
        readonly ItemMessageRouter itemMessageRouter;
        readonly TextInputRouter textInputRouter;
        readonly CommentBridge commentBridge;
        readonly PlayerHandleFactoryBuilder playerHandleFactoryBuilder;
        readonly UserInterfacePreparer userInterfacePreparer;
        readonly ProductPurchaser productPurchaser;
        readonly ProductGranter productGranter;
        readonly CSEmulatorOscServer csEmulatorOscServer;
        readonly OscClientWrapper oscClientWrapper;
        readonly FogSettingsBridge fogSettingsBridge;
        readonly PlayerStorageSerDes playerStorageSerDes;
        readonly GroupStateProxyMapper groupStateProxyMapper;
        readonly SubAudioClip subAudioClip;
        readonly HapticsAudioBridge hapticsAudio;
        readonly OptionBridge optionBridge;
        readonly ShutdownNotifier shutdownNotifier;

        readonly ClusterVR.CreatorKit.Editor.Preview.World.SpawnPointManager spawnPointManager;

        List<CodeRunner> codeRunners = new List<CodeRunner>();
        PlayerCodeRunner playerCodeRunner = null;
        UnityEngine.GameObject vrm = null;
        SpaceContext spaceContext = null;
        //CSETODO マルチ環境につき、CSEmulatorPlayerHandlerは個別に管理させたい(DummyPlayerは個別管理している)
        Components.CSEmulatorPlayerHandler csPlayerHandler = null;

        bool isRunning = false;

        public EngineFacade(
            OptionBridge optionBridge,
            CSEmulatorOscServer csEmulatorOscServer,
            ClusterVR.CreatorKit.Editor.Preview.Item.ItemCreator itemCreator,
            ClusterVR.CreatorKit.Editor.Preview.Item.ItemDestroyer itemDestroyer,
            ClusterVR.CreatorKit.Editor.Preview.World.SpawnPointManager spawnPointManager,
            ClusterVR.CreatorKit.Editor.Preview.World.CommentScreenPresenter commentScreenPresenter
        )
        {
            this.optionBridge = optionBridge;
            this.shutdownNotifier = new ShutdownNotifier();
            this.spawnPointManager = spawnPointManager;
            spaceContext = new SpaceContext(
            );
            previewFinder = new CckPreviewFinder();
            this.itemCollector = new ItemCollector(
                itemCreator
            );

            this.desktopPlayerControllerReflector = new DesktopPlayerControllerReflector(
                previewFinder.controller.GetComponentInParent<ClusterVR.CreatorKit.Preview.PlayerController.DesktopPlayerController>()
            );

            this.vrmPreparer = new VrmPreparer(
                previewFinder,
                desktopPlayerControllerReflector,
                optionBridge.raw.vrm,
                optionBridge,
                optionBridge,
                optionBridge.playerMeasurementsHolder,
                shutdownNotifier
            );

            prefabItemStore = optionBridge.raw.usePrefabItemId ? new PrefabItemStore(
                itemCollector.GetAllItemPrefabs()
            ) : new PrefabItemStore(
                optionBridge.raw.itemTemplateIdValidSets.Select(t => (t.itemTemplateId, t.prefab))
            );
            itemMessageRouter = new ItemMessageRouter(
                spaceContext
            );
            textInputRouter = new TextInputRouter();
            userInterfacePreparer = new UserInterfacePreparer(
                previewFinder
            );
            productPurchaser = new ProductPurchaser(
                optionBridge
            );
            productGranter = new ProductGranter();
            this.csEmulatorOscServer = csEmulatorOscServer;
            oscClientWrapper = new OscClientWrapper();
            fogSettingsBridge = new FogSettingsBridge();
            commentBridge = new CommentBridge(
                optionBridge
            );
            ((IList<ClusterVR.CreatorKit.World.ICommentScreenView>)commentScreenPresenter.GetType().GetField("commentScreenViews", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(commentScreenPresenter))
                .Add(commentBridge);
            hapticsAudio = new HapticsAudioBridge();
            playerHandleFactoryBuilder = new PlayerHandleFactoryBuilder(
                spaceContext,
                userInterfacePreparer,
                textInputRouter,
                userInterfacePreparer,
                productPurchaser,
                optionBridge,
                productGranter,
                optionBridge,
                optionBridge,
                optionBridge,
                optionBridge,
                hapticsAudio,
                itemMessageRouter,
                optionBridge,
                fogSettingsBridge,
                optionBridge,
                spawnPointManager
            );
            groupStateProxyMapper = new GroupStateProxyMapper();
            subAudioClip = new SubAudioClip();
            optionBridge.OnChangedSubAudioPlaying += (isPlaying, device) => subAudioClip.ChangeSubAudioPlaying(isPlaying, device);
            this.optionBridge = optionBridge;

            playerCodeRunner = new PlayerCodeRunner(
                itemMessageRouter, optionBridge,
                new DebugLogFactory(optionBridge.raw)
            );

            itemCollector.OnScriptableItemCreated += i =>
            {
                codeRunners.Add(StartRunner(playerCodeRunner, spaceContext, i));
            };
            itemCollector.OnItemCreated += i =>
            {
                var csItemHandler = CSEmulator.Commons.AddComponent<Components.CSEmulatorItemHandler>(i.gameObject);
                csItemHandler.Construct(optionBridge, true);
                CSEmulator.Commons.AddComponent<Components.CSEmulatorStateWatcher>(i.gameObject);
                csItemHandler.SetOwnerPlayer(csPlayerHandler);
            };
            itemDestroyer.OnDestroy += i =>
            {
                var destoryed = codeRunners
                    .FirstOrDefault(c => c.csItemHandler.item.Id.Value == i.Id.Value);
                if (destoryed == null) return;
                destoryed.Shutdown();
                codeRunners.Remove(destoryed);
            };
        }

        public void Start()
        {
            if (!optionBridge.raw.enable) return;

            foreach(var i in itemCollector.GetAllItems())
            {
                var csItemHandler = CSEmulator.Commons.AddComponent<Components.CSEmulatorItemHandler>(i.gameObject);
                csItemHandler.Construct(optionBridge, false);
                CSEmulator.Commons.AddComponent<Components.CSEmulatorStateWatcher>(i.gameObject);
            }

            //StartRunner前にVRMをInstantinateするのは合ってる。
            //しかし複数プレイヤーを考えるとこれは雑なのでそのうち何とかする。
            vrm = vrmPreparer.InstantiateVrm();
            //CSETODO マルチ環境につき、CSEmulatorPlayerHandlerは個別に管理させたい(DummyPlayerは個別管理している)
            csPlayerHandler = vrm.GetComponent<Components.CSEmulatorPlayerHandler>();
            playerHandleFactoryBuilder.AddPlayer(csPlayerHandler, optionBridge);
            //InstantiateVrmでPointOfViewManagerが作られるので
            var localUIEventBridge = CSEmulator.Commons.AddComponent<Components.CSEmulatorPlayerLocalUIEventBridge>(
                previewFinder.panel
            );
            foreach (var csPlayerLocalUI in Components.CSEmulatorPlayerLocalUI.GetAllPlayerLocalUIs())
            {
                //createItemでPlayerLocalUIを含むものは生成できない(CCKドキュメント)ので、これで十分のはず。
                var c = CSEmulator.Commons.AddComponent<Components.CSEmulatorPlayerLocalUI>(csPlayerLocalUI.RectTransform.gameObject);
                c.Construct(localUIEventBridge, vrmPreparer.csPlayerController.pointOfViewManager);
            }

            var plainItemBridge = new PlainItemBridge(vrmPreparer.csPlayerController);
            foreach (var i in itemCollector.GetAllPlainItems())
            {
                var csItemHandler = i.gameObject.GetComponent<Components.CSEmulatorItemHandler>();
                plainItemBridge.Bind(csItemHandler);
            }
            itemCollector.OnPlainItemCreated += i =>
            {
                var csItemHandler = i.gameObject.GetComponent<Components.CSEmulatorItemHandler>();
                plainItemBridge.Bind(csItemHandler);
            };

            foreach (var csDummyPlayer in Components.CSEmulatorDummyPlayer.GetAllDummyPlayers())
            {
                //このあたりの処理はEngine層で行うもの。必要になったらなんとかする。
                var animationController = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.RuntimeAnimatorController>(
                    "Assets/KaomoLab/CSEmulator/Components/Animations/Player.controller"
                );

                csDummyPlayer.Construct(
                    animationController,
                    o => new PlayerFaceController(o),
                    new SpawnPointHolderWrapper(spawnPointManager),
                    optionBridge,
                    optionBridge.playerMeasurementsHolder,
                    shutdownNotifier
                );
                playerHandleFactoryBuilder.AddDummyPlayer(csDummyPlayer);
            }

            foreach (var i in itemCollector.GetAllItems())
            {
                var csItemHandler = i.gameObject.GetComponent<Components.CSEmulatorItemHandler>();
                //CSETODO Itemオーナー系はマルチ未対応。
                var ownerPlayer = vrm.GetComponent<Components.CSEmulatorPlayerHandler>();
                csItemHandler.SetOwnerPlayer(ownerPlayer);
            }

            foreach (var s in Components.CSEmulatorSubAudioHandler.GetAllSpeakers())
            {
                var handler = s.gameObject.AddComponent<Components.CSEmulatorSubAudioHandler>();
                handler.Construct(subAudioClip);
            }

            oscClientWrapper.Start(optionBridge.raw.oscSendAddress, optionBridge.raw.oscSendPort);
            fogSettingsBridge.Start();
            hapticsAudio.Start();

            //各種コンポーネントを付けてから実行した方がいい気がする。
            var newRunners = itemCollector
                .GetAllScriptableItem()
                .Select(i => StartRunner(playerCodeRunner, spaceContext, i));
            codeRunners.AddRange(newRunners);
            isRunning = true;
        }

        CodeRunner StartRunner(
            PlayerCodeRunner playerCodeRunner,
            SpaceContext spaceContext,
            ClusterVR.CreatorKit.Item.IScriptableItem scriptableItem
        )
        {
            var itemHandler = scriptableItem.Item.gameObject.GetComponent<Components.CSEmulatorItemHandler>();
            var stateWatcher = scriptableItem.Item.gameObject.GetComponent<Components.CSEmulatorStateWatcher>();
            //見やすさ重視で上に持っていく仕組みだったけど、ウィンドウあるしそこまでしなくていいかと
            //while (UnityEditorInternal.ComponentUtility.MoveComponentUp(stateWatcher)) { }
            var loggerFactory = new DebugLogFactory(optionBridge.raw);
            var ret = new CodeRunner(
                scriptableItem,
                itemHandler,
                stateWatcher,
                prefabItemStore,
                itemCollector,
                itemMessageRouter,
                textInputRouter,
                productPurchaser,
                productGranter,
                optionBridge,
                commentBridge,
                csEmulatorOscServer,
                oscClientWrapper,
                optionBridge,
                optionBridge,
                optionBridge,
                playerHandleFactoryBuilder,
                playerCodeRunner,
                groupStateProxyMapper,
                spaceContext,
                optionBridge,
                optionBridge,
                loggerFactory
            );
            ret.Start();
            return ret;
        }

        public void Update()
        {
            //Update中にDestroyされて減ることがあるのでToArray
            foreach (var runner in codeRunners.ToArray())
            {
                runner.Update();
            }
            playerCodeRunner.Update();
            itemMessageRouter.Routing();
            textInputRouter.Routing();
            productPurchaser.Routing();
            productGranter.Routing();
            if(spaceContext != null) spaceContext.Update();
            if (csPlayerHandler != null) csPlayerHandler.Update();
        }

        public void Shutdown()
        {
            if (!isRunning) return;

            foreach (var runner in codeRunners)
            {
                runner.Shutdown();
            }
            codeRunners.Clear();
            oscClientWrapper.Shutdown();
            subAudioClip.Shutdown();
            hapticsAudio.Shutdown();
            shutdownNotifier.Shutdown();

            isRunning = false;
            vrm = null;
        }
    }
}
