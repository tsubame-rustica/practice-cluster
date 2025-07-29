using Assets.KaomoLab.CSEmulator.Components;
using Assets.KaomoLab.CSEmulator.Editor.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Preview
{
    public class OptionBridge
        : Engine.IRunnerOptions,
        Engine.IPlayerViewOptions,
        Engine.IProductOptions,
        EmulateClasses.IProductAmount,
        Components.IPerspectiveChangeNotifier,
        Components.IPlayerMeta,
        EmulateClasses.ISerializedPlayerStorage,
        EmulateClasses.IClusterEvent,
        Engine.ICommentOptions,
        EmulateClasses.IOscContext,
        EmulateClasses.IPlayerLooks,
        EmulateClasses.IHapticsSettings,
        EmulateClasses.IPlayerBelongings,
        EmulateClasses.IStoredProducts,
        EmulateClasses.ILoggingOptions
    {
        public event Handler<string, int> OnGiftSent = delegate{};
        public event Handler<bool, string> OnChangedSubAudioPlaying = delegate { };

        public bool isDebug => raw.debug;
        public IExternalCallerOptions externalCallerOptions { get; private set; }
        public string pauseFrameKey => raw.pauseFrameKey;

        public EmulatorOptions raw { get; private set; }

        public string userIdfc => raw.userIdfc;
        public string userId => raw.userId;
        public string userDisplayName => raw.userName;
        public CSEmulator.EventRole eventRole => raw.playerEventRole;

        public bool exists => raw.exists;

        public IReadOnlyList<string> accessoryProductIds
            => raw.accessoryProductIds.Zip(raw.isAccessoryProductIdsEnabled, (id, enable) => enable ? id: "").ToArray();
        public string avatarProductId
            => raw.avatarProductIds
            .Zip(raw.isAvatarProductIdsEnabled, (id, enable) => enable ? id : "")
            .Where(id => id != "")
            .DefaultIfEmpty("")
            .FirstOrDefault();

        public bool isAndroid => raw.playerOperatingSystem == CSEmulator.PlayerOperatingSystem.Android;
        public bool isDesktop => raw.playerDevice == CSEmulator.PlayerDevice.Desktop;
        public bool isIos => raw.playerOperatingSystem == CSEmulator.PlayerOperatingSystem.iOS;
        public bool isMacOs => raw.playerOperatingSystem == CSEmulator.PlayerOperatingSystem.macOS;
        public bool isMobile => raw.playerDevice == CSEmulator.PlayerDevice.Mobile;
        public bool isVr => raw.playerDevice == CSEmulator.PlayerDevice.VR;
        public bool isWindows => raw.playerOperatingSystem == CSEmulator.PlayerOperatingSystem.Windows;

        public bool isFirstPersonView
        {
            get => raw.perspective;
            set => raw.perspective = value;
        }

        public bool isRayDraw => raw.isRayDraw;

        public bool ignorePackageListUpdate => raw.ignorePackageListUpdate;

        public IPlayerMeasurementsHolder playerMeasurementsHolder { get; private set; }

        bool EmulateClasses.IClusterEvent.isEvent => raw.isEvent;

        public bool loggingOnReceiveFail => raw.loggingOnReceiveFail;

        EmulateClasses.CommentVia ICommentOptions.via => raw.commentVia;

        bool EmulateClasses.IOscContext.isReceiveEnabled => raw.oscIsReceiveEnabled;
        bool EmulateClasses.IOscContext.isSendEnabled => raw.oscIsSendEnabled;

        IReadOnlyList<string> EmulateClasses.IPlayerBelongings.accessoryProductIds
            => raw.accessoryProductIds.Where(id => id != "").ToList();
        IReadOnlyList<string> EmulateClasses.IPlayerBelongings.avatarProductIds
            => raw.avatarProductIds.Where(id => id != "").ToList();
        IReadOnlyList<string> EmulateClasses.IPlayerBelongings.craftItemProductIds
            => raw.craftItemProductIds.Where(id => id != "").ToList();

        bool EmulateClasses.IHapticsSettings.isAvailable => raw.isHapticsAvailable;
        float EmulateClasses.IHapticsSettings.maxFrequencyHz => raw.hapticsMaxFrequency;
        float EmulateClasses.IHapticsSettings.minFrequencyHz => raw.hapticsMinFrequency;
        bool EmulateClasses.IHapticsSettings.isFrequencySupported => raw.isHapticsFrequencyAvailable;
        float EmulateClasses.IHapticsSettings.volume => raw.hapticsAudioVolume;
        float EmulateClasses.IHapticsSettings.defaultAmplitude => raw.hapticsDefaultAmplitude;
        float EmulateClasses.IHapticsSettings.defaultDuration => raw.hapticsDefaultDuration;
        float EmulateClasses.IHapticsSettings.defaultFrequency => raw.hapticsDefaultFrequency;

        public class ExternalCallerOptions : IExternalCallerOptions
        {
            public event Handler OnChangeLimit = delegate { };
            readonly EmulatorOptions options;
            public ExternalCallerOptions(EmulatorOptions options)
            {
                this.options = options;
                this.options.OnChangedExternalCallLimit += () => {
                    OnChangeLimit.Invoke();
                };
            }
            public string GetUrl(string id)
            {
                var endpoints = options.callExternalUrl;
                foreach(var p in endpoints)
                {
                    if(p.id == id) return p.url;
                }
                return null;
            }
        }

        public class PlayerMeasurementsHolder : IPlayerMeasurementsHolder
        {
            public float height => ClusterVR.CreatorKit.Preview.PlayerController.CameraControlSettings.StandingEyeHeight;
            public float radius => options.playerColliderRadius;

            readonly EmulatorOptions options;
            public PlayerMeasurementsHolder(
                EmulatorOptions options
            )
            {
                this.options = options;
            }
        }

        public OptionBridge(
            EmulatorOptions options
        )
        {
            this.raw = options;
            this.externalCallerOptions = new ExternalCallerOptions(options);
            this.playerMeasurementsHolder = new PlayerMeasurementsHolder(options);
            options.OnChangedPerspective += Options_OnChangedPerspective;
            options.OnGiftSent += Options_OnGiftSent;
            options.OnChangedSubAudioPlaying += Options_OnChangedSubAudioPlaying;
        }

        private void Options_OnChangedPerspective()
        {
            foreach (var l in perspectiveChangeListeners)
            {
                l.Invoke(raw.perspective);
            }
        }

        private void Options_OnGiftSent(string giftType, int price)
        {
            OnGiftSent.Invoke(giftType, price);
        }

        private void Options_OnChangedSubAudioPlaying(bool isPlaying, string device)
        {
            OnChangedSubAudioPlaying.Invoke(isPlaying, device);
        }

        readonly List<Handler<bool>> perspectiveChangeListeners = new List<Handler<bool>>();
        event Handler<bool> IPerspectiveChangeNotifier.OnChanged
        {
            add => perspectiveChangeListeners.Add(value);
            remove => perspectiveChangeListeners.Remove(value);
        }

        public void SetPersonViewChangeable(bool canChange)
        {
            raw.canChangePerspective = canChange;
        }

        void IPerspectiveChangeNotifier.RequestNotify()
        {
            Options_OnChangedPerspective();
        }

        public bool IsPublicProduct(string productId)
        {
            foreach (var info in raw.productInfos)
            {
                if (info.productId == productId) return info.isPublic;
            }
            return false;
        }
        public string GetProductName(string productId)
        {
            foreach(var info in raw.productInfos)
            {
                if(info.productId == productId) return info.productName;
            }
            return null; //nullで判定する。限界きたらちゃんとする
        }
        public (int, int) GetProductAmount(string productId)
        {
            foreach (var info in raw.productInfos)
            {
                if (info.productId == productId) return (info.plus, info.minus);
            }
            return (0, 0);
        }
        public void SetProductAmount(string productId, int plus, int minus)
        {
            var infos = raw.productInfos.ToArray();
            foreach (var info in infos)
            {
                if (info.productId == productId)
                {
                    info.plus = plus;
                    info.minus = minus;
                }
            }
            raw.productInfos = infos;
        }

        public void SavePlayerStorage(string serialized)
        {
            raw.playerStorage = serialized;
        }
        public string LoadPlayerStorage()
        {
            return raw.playerStorage;
        }

        int ICommentOptions.GetNextId()
        {
            var ret = raw.commentNextId;
            raw.commentNextId++;
            return ret;
        }

        void EmulateClasses.IPlayerBelongings.AddAccessory(string productId)
        {
            raw.accessoryProductIds = raw.accessoryProductIds.Append(productId).ToArray();
            raw.isAccessoryProductIdsEnabled = raw.isAccessoryProductIdsEnabled.Append(false).ToArray();
        }

        void EmulateClasses.IPlayerBelongings.AddAvater(string productId)
        {
            raw.avatarProductIds = raw.avatarProductIds.Append(productId).ToArray();
            raw.isAvatarProductIdsEnabled = raw.isAvatarProductIdsEnabled.Append(false).ToArray();
        }

        void EmulateClasses.IPlayerBelongings.AddCraftItem(string productId)
        {
            raw.craftItemProductIds = raw.craftItemProductIds.Append(productId).ToArray();
        }

        string EmulateClasses.IStoredProducts.GetProductName(string productId)
        {
            var id = raw.storedProductIds.Where(id => productId == id).DefaultIfEmpty("").FirstOrDefault();
            if (id == "") return "";
            return raw.productDummyName;
        }

        EmulateClasses.StoredProductType EmulateClasses.IStoredProducts.GetProductType(string productId)
        {
            var (_, type) = raw.storedProductIds.Zip(raw.storedProductIdsType, (id, type) => (id, type))
                .Where(t => t.id == productId)
                .DefaultIfEmpty(("", EmulateClasses.StoredProductType.Disable))
                .FirstOrDefault();
            return type;
        }

        public void Shutdown()
        {
        }
    }
}
