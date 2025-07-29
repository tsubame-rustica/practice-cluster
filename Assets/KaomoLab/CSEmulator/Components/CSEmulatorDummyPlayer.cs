#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using ICckPlayerController = ClusterVR.CreatorKit.Preview.PlayerController.IPlayerController;

namespace Assets.KaomoLab.CSEmulator.Components
{
    [DisallowMultipleComponent,
        RequireComponent(typeof(CharacterController)),
        RequireComponent(typeof(AudioSource)),
        RequireComponent(typeof(AudioLowPassFilter)),
        //Requireではあるが、Constructと同時にAddする前提になっている。
        //RequireComponent(typeof(CSEmulatorDummyPlayerController)),
        //RequireComponent(typeof(CSEmulatorPlayerController)),
        //RequireComponent(typeof(CSEmulatorPlayerHandler))
    ]
    public class CSEmulatorDummyPlayer
        : MonoBehaviour, IPlayerMeta
    {
        class NopGrabController
            : IGrabController
        {
            public bool isGrab => false;
            public Vector3 grabPoint => throw new Exception("開発者に連絡して下さい。"); //来ない想定
            public void ApplyUpdate() { }
        }
        class NopRawInput
            : IRawInput
        {
            public Func<bool> IsForwardKey => () => false;
            public Func<bool> IsBackKey => () => false;
            public Func<bool> IsRightKey => () => false;
            public Func<bool> IsLeftKey => () => false;
            public Func<bool> IsRightHandKeyDown => () => false;
            public Func<bool> IsRightHandKeyUp => () => false;
            public Func<bool> IsLeftHandKeyDown => () => false;
            public Func<bool> IsLeftHandKeyUp => () => false;
            public Func<bool> IsWalkKey => () => false;
            public Func<bool> IsDashKey => () => false;
            public Func<bool> IsCrouchKey => () => false;
            public Func<bool> IsJumpKey => () => false;
        }
        class EmptyHumanoidAnimationCreator : IHumanoidAnimationCreator
        {
            public ClusterVR.CreatorKit.Item.Implements.HumanoidAnimation Create(AnimationClip clip)
            {
                var humanoidAnimation = ScriptableObject.CreateInstance<ClusterVR.CreatorKit.Item.Implements.HumanoidAnimation>();
                humanoidAnimation.Construct(0, false, new List<ClusterVR.CreatorKit.Item.Implements.HumanoidAnimationCurve>());
                return humanoidAnimation;

            }
        }
        class InactiveController : IControlActivator
        {
            public event Handler<bool> OnActivated = delegate { };
            public void RequestNotify()
            {
                OnActivated.Invoke(false);
            }
        }

        public static IEnumerable<CSEmulatorDummyPlayer> GetAllDummyPlayers()
        {
            var ret = UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                .GetRootGameObjects()
                .SelectMany(
                    o => o.GetComponentsInChildren<CSEmulator.Components.CSEmulatorDummyPlayer>(true)
                );
            return ret;
        }

        //デフォルト値
        public static bool isMute = false;

        [SerializeField] public bool _exists = true;
        [SerializeField] public string _id;
        [SerializeField] public string _idfc;
        [SerializeField] public string _userDisplayName;
        [SerializeField] public string _userId;
        [SerializeField] public CSEmulator.EventRole _eventRole = EventRole.Guest;
        [SerializeField] public CSEmulator.PlayerDevice _playerDevice = PlayerDevice.Desktop;
        [SerializeField] public CSEmulator.PlayerOperatingSystem _playerOperatingSystem = PlayerOperatingSystem.Windows;
        [SerializeField] public GameObject _vrm;
        [SerializeField] public List<string> _accessoryProductIds = new List<string>();
        [SerializeField] public List<string> _avatarProductIds = new List<string>();
        [SerializeField] public List<string> _craftItemProductIds = new List<string>();
        [SerializeField] public string _playerStorage = "";

        string IPlayerMeta.userIdfc => _idfc;
        string IPlayerMeta.userId => _userId;
        string IPlayerMeta.userDisplayName => _userDisplayName;
        EventRole IPlayerMeta.eventRole => _eventRole;
        bool IPlayerMeta.exists => _exists;
        bool IPlayerMeta.isAndroid => _playerOperatingSystem == PlayerOperatingSystem.Android;
        bool IPlayerMeta.isIos => _playerOperatingSystem == PlayerOperatingSystem.iOS;
        bool IPlayerMeta.isMacOs => _playerOperatingSystem == PlayerOperatingSystem.macOS;
        bool IPlayerMeta.isWindows => _playerOperatingSystem == PlayerOperatingSystem.Windows;
        bool IPlayerMeta.isDesktop => _playerDevice == PlayerDevice.Desktop;
        bool IPlayerMeta.isMobile => _playerDevice == PlayerDevice.Mobile;
        bool IPlayerMeta.isVr => _playerDevice == PlayerDevice.VR;

        float initialVolume;
        float despawnHeight;
        bool constructed = false;

        public CharacterController characterController { get; private set; } = null;
        public AudioSource audioSource { get; private set; } = null;
        public AudioLowPassFilter audioLowPassFilter { get; private set; } = null;
        public CSEmulatorPlayerHandler csPlayerHandler { get; private set; } = null;
        public CSEmulatorDummyPlayerController dummyPlayerController { get; private set; } = null;
        public CSEmulatorPlayerController csPlayerController { get; private set; } = null;
        ISpawnPointHolder spawnPointHolder = null;

        void PresetSelf()
        {
            gameObject.tag = "EditorOnly";
        }

        void PresetCharacterController(CharacterController cc)
        {
            characterController = cc;
            //PreviewOnlyを参考にした。
            //動く床対応等により、実際の挙動もおおよそこの通りと考えている。
            //厳密な調査は行っていない。
            cc.slopeLimit = 45;
            cc.stepOffset = 0.38f;
            cc.skinWidth = 0.02f;
            cc.minMoveDistance = 0.001f;
            cc.center = new Vector3(0, 0.52f, 0);
            cc.radius = 0.2f;
            cc.height = 1;
            cc.gameObject.layer = 24; //Audience

        }

        void PresetAudioSource(AudioSource a)
        {
            audioSource = a;
            //実際に曲を流して聞いて調整してみた 25.04.03
            a.volume = 0.3f;
            a.spatialBlend = 1.0f;
            a.dopplerLevel = 0;
            a.maxDistance = 20;
            a.rolloffMode = AudioRolloffMode.Custom;
            var c = new AnimationCurve(
                new Keyframe(0.05f,  1,      -20.0f,  -20.0f,  0, 0) { weightedMode = WeightedMode.None },
                new Keyframe(0.1f,   0.5f,   -5.0f,   -5.0f,   0, 0) { weightedMode = WeightedMode.None },
                new Keyframe(0.2f,   0.25f,  -1.25f,  -1.25f,  0, 0) { weightedMode = WeightedMode.None },
                new Keyframe(0.4f,   0.125f, -0.313f, -0.313f, 0, 0) { weightedMode = WeightedMode.None },
                new Keyframe(0.672f, 0.048f, -0.204f, -0.204f, 0, 0.708f) { weightedMode = WeightedMode.None },
                new Keyframe(1.0f,   0.0f,   -0.005f, -0.005f, 0, 0) { weightedMode = WeightedMode.None }
            );
            for(var i = 0; i < c.length; i++)
            {
                UnityEditor.AnimationUtility.SetKeyLeftTangentMode(c, i, UnityEditor.AnimationUtility.TangentMode.Free);
                UnityEditor.AnimationUtility.SetKeyRightTangentMode(c, i, UnityEditor.AnimationUtility.TangentMode.Free);
            }
            a.SetCustomCurve(AudioSourceCurveType.CustomRolloff, c);

            //サンプル音声用
            a.loop = true;
            a.clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(
                "Assets/KaomoLab/CSEmulator/Components/Sounds/DummyPlayerVoice.ogg"
            );

            //デフォルト値
            a.mute = isMute;
        }

        void PresetAudioLowPassFilter(AudioLowPassFilter lowPassFilter)
        {
            this.audioLowPassFilter = lowPassFilter;
            //実際に曲を流して聞いて調整してみた 25.04.03
            lowPassFilter.lowpassResonanceQ = 1;
            var c = new AnimationCurve(
                new Keyframe(0,      1.001f, -2.424f, -2.424f, 0.333f, 0.385f) { weightedMode = WeightedMode.None },
                new Keyframe(0.602f, 0.118f, 0,       0,       0.333f, 0.333f) { weightedMode = WeightedMode.None }
            );
            for (var i = 0; i < c.length; i++)
            {
                UnityEditor.AnimationUtility.SetKeyLeftTangentMode(c, i, UnityEditor.AnimationUtility.TangentMode.Free);
                UnityEditor.AnimationUtility.SetKeyRightTangentMode(c, i, UnityEditor.AnimationUtility.TangentMode.Free);
            }
            lowPassFilter.customCutoffCurve = c;
        }

        void ConstructPlayerHandler(
            VRM.VRMMeta vrmMeta
        )
        {
            this.csPlayerHandler = vrmMeta.gameObject.AddComponent<CSEmulatorPlayerHandler>();
            this.csPlayerHandler.Construct(_idfc);
        }

        void ConstructDummyPlayerController(
            GameObject vrmPrefab,
            CharacterController characterController
        )
        {
            this.dummyPlayerController = gameObject.AddComponent<CSEmulatorDummyPlayerController>();
            this.dummyPlayerController.Construct(
                vrmPrefab,
                characterController,
                new CSEmulatorDummyPlayerController.DummyDesktopPointerEventListener(),
                new CSEmulatorDummyPlayerController.DummyMoveInputController()
            );
            var avatarCollider = this.dummyPlayerController.vrm.AddComponent<CapsuleCollider>();
            this.dummyPlayerController.vrm.layer = 15; //OtherAvatar
            //VrmPrepareのコピペ
            avatarCollider.center = characterController.center;
            avatarCollider.height = characterController.height - 0.02f;
            avatarCollider.radius = characterController.radius - 0.02f;

        }

        void ConstructPlayerController(
            CSEmulatorPlayerHandler csPlayerHandler,
            CSEmulatorDummyPlayerController dummyPlayerController,
            CharacterController characterController,
            Animator animator,
            RuntimeAnimatorController runtimeAnimatorController,
            IPlayerFaceController playerFaceController,
            IPerspectiveChangeNotifier perspectiveChangeNotifier,
            IPlayerMeasurementsHolder playerMeasurementsHolder,
            IShutdownNotifier shutdownNotifier
        )
        {
            this.csPlayerController = gameObject.AddComponent<CSEmulatorPlayerController>();

            //WorldRuntimeSettingGathererからのコピペ。Editorだったので仕方なく。IFにするまでもないと判断。
            var worldRuntimeSettings = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(o => o.GetComponentsInChildren<ClusterVR.CreatorKit.World.Implements.WorldRuntimeSetting.WorldRuntimeSetting>(true))
                .FirstOrDefault();

            var grabController = new NopGrabController();
            var rawInput = new NopRawInput();
            var humanoidAnimationCreator = new EmptyHumanoidAnimationCreator();
            var inactiveController = new InactiveController();
            animator.runtimeAnimatorController = runtimeAnimatorController;

            this.csPlayerController.Construct(
                new MovingPlatformSettings(worldRuntimeSettings),
                new MantlingSettings(worldRuntimeSettings),
                new HudSettings(worldRuntimeSettings),
                new ClippingPlanesSettings(worldRuntimeSettings),
                characterController,
                animator,
                dummyPlayerController,
                dummyPlayerController,
                dummyPlayerController,
                playerFaceController,
                dummyPlayerController,
                dummyPlayerController,
                perspectiveChangeNotifier,
                playerMeasurementsHolder,
                grabController,
                csPlayerHandler,
                humanoidAnimationCreator,
                inactiveController,
                shutdownNotifier,
                rawInput
            );
        }

        public void Construct(
            RuntimeAnimatorController runtimeAnimatorController,
            Func<GameObject, IPlayerFaceController> playerFaceControllerFactory,
            ISpawnPointHolder spawnPointHolder,
            IPerspectiveChangeNotifier perspectiveChangeNotifier,
            IPlayerMeasurementsHolder playerMeasurementsHolder,
            IShutdownNotifier shutdownNotifier
        )
        {
            if (constructed) return;

            this.spawnPointHolder = spawnPointHolder;

            //引数で依存を指定しているので、順番を変えるときは確認する。
            ConstructDummyPlayerController(
                _vrm,
                GetComponent<CharacterController>()
            );
            ConstructPlayerHandler(
                GetComponentInChildren<VRM.VRMMeta>()
            );
            var playerFaceController = playerFaceControllerFactory(this.dummyPlayerController.avaterParent);
            ConstructPlayerController(
                GetComponentInChildren<CSEmulatorPlayerHandler>(),
                GetComponent<CSEmulatorDummyPlayerController>(),
                GetComponent<CharacterController>(),
                GetComponentInChildren<Animator>(),
                runtimeAnimatorController,
                playerFaceController,
                perspectiveChangeNotifier,
                playerMeasurementsHolder,
                shutdownNotifier
            );

            constructed = true;
        }

        //アタッチした時＝ユーザに設定を変更して貰う可能性があるものなどを初期化する
        void Reset()
        {
            PresetSelf();
            PresetCharacterController(GetComponent<CharacterController>());
            PresetAudioSource(GetComponent<AudioSource>());
            PresetAudioLowPassFilter(GetComponent<AudioLowPassFilter>());
        }

        void Start()
        {
            initialVolume = GetComponent<AudioSource>().volume;
            var rootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            despawnHeight = rootGameObjects
                .Select(o => o.GetComponentInChildren<ClusterVR.CreatorKit.World.IDespawnHeight>(true))
                .First(o => o != null).Height;

            CheckDespawn();
        }

        async void CheckDespawn()
        {
            while (this != null)
            {
                if (gameObject.transform.position.y < despawnHeight)
                {
                    Respawn();
                }
                await Task.Delay(300);
            }
        }
        void Respawn()
        {
            var spawnPoint = spawnPointHolder.GetSpawnPoint(PermissionType.Audience);
            //CCKPlayerControllerのコピペ。
            //CCKのAvatarRespawner(Editor系)の代替処理。
            //ClusterScriptでのRespawnはCCKPlayerController側の処理で行われる。
            var playerController = (ICckPlayerController)dummyPlayerController;
            playerController.WarpTo(spawnPoint.position);
            var yawOnlyRotation = Quaternion.Euler(0f, spawnPoint.yRotation, 0f);
            playerController.SetRotationKeepingHeadPitch(yawOnlyRotation);
            playerController.ResetCameraRotation(yawOnlyRotation);

            csPlayerController.ForceForward();
            playerController.SetRotationKeepingHeadPitch(yawOnlyRotation);
            csPlayerController.ForceRotate(yawOnlyRotation.eulerAngles.y);
        }

        public void SetVolumeRate(float rate)
        {
            var audio = GetComponent<AudioSource>();
            audio.volume = initialVolume * rate;
        }
    }
}
#endif
