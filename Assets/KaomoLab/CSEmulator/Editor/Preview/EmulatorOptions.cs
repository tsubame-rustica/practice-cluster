using ClusterVR.CreatorKit.Preview.PlayerController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Editor.Preview
{
    public class EmulatorOptions
    {
        public event Handler OnChangedFps = delegate { };
        public event Handler OnChangedExternalCallLimit = delegate { };
        public event Handler OnChangedPerspective = delegate { };
        public event Handler<string, int> OnGiftSent = delegate { };
        public event Handler<bool, string> OnChangedSubAudioPlaying = delegate { };

        const string PrefsKeyEnable = "KaomoCSEmulator_enable";
        public bool enable {
            get => PlayerPrefs.GetInt(PrefsKeyEnable, 1) == 1;
            set => PlayerPrefs.SetInt(PrefsKeyEnable, value ? 1 : 0);
        }


        const string PrefsKeyFps = "KaomoCSEmulator_fps";
        public enum FpsLimit : int
        {
            unlimited,
            limit90,
            limit30
        };
        public FpsLimit fps
        {
            get => (FpsLimit)PlayerPrefs.GetInt(PrefsKeyFps, (int)FpsLimit.limit90);
            set {
                PlayerPrefs.SetInt(PrefsKeyFps, (int)value);
                OnChangedFps.Invoke();
            }
        }

        //一人称視点開始だと三人称視点に変更してもItemHighligherが正常に稼働する（原因調査しても沼りそうなので未調査）
        const string PrefsKeyFirstPersonPerspective = "KaomoCSEmulator_firstPersonPerspective";
        public bool perspective
        {
            get => PlayerPrefs.GetInt(PrefsKeyFirstPersonPerspective, 1) == 1;
            set {
                PlayerPrefs.SetInt(PrefsKeyFirstPersonPerspective, value ? 1 : 0);
                OnChangedPerspective.Invoke();
            }
        }
        //実行時のみの想定なのでprefを使用しない
        public bool canChangePerspective = true;

        const string PrefsKeyVrm = "KaomoCSEmulator_vrm";
        public GameObject vrm
        {
            get
            {
                var path = PlayerPrefs.GetString(PrefsKeyVrm, Editor.Commons.DefaultVrmPath);
                var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                return prefab;
            }
            set
            {
                var path = UnityEditor.AssetDatabase.GetAssetPath(value);
                if (path == "") path = Editor.Commons.DefaultVrmPath;
                PlayerPrefs.SetString(PrefsKeyVrm, path);
            }
        }

        const string PrefsKeyUserIdfc = "KaomoCSEmulator_userIdfc";
        public readonly string DefaultUserIdfc = new String(Enumerable.Repeat(0, 4).SelectMany(_ => new System.Random().Next().ToString("X")).ToArray()).ToLower();
        public readonly Regex userIdfcPattern = new Regex("^[0-9a-z]{32}$");
        public string userIdfc
        {
            get => PlayerPrefs.GetString(PrefsKeyUserIdfc, DefaultUserIdfc);
            set => PlayerPrefs.SetString(PrefsKeyUserIdfc, value);
        }
        const string PrefsKeyUserId = "KaomoCSEmulator_userId";
        public readonly string DefaultUserId = new String(Enumerable.Repeat('a', 16).Select(c => (char)(c + (new System.Random().Next() % 26))).ToArray());
        public readonly Regex userIdPattern = new Regex(".*");
        public string userId
        {
            get => PlayerPrefs.GetString(PrefsKeyUserId, DefaultUserId);
            set => PlayerPrefs.SetString(PrefsKeyUserId, value);
        }
        const string PrefsKeyUserName = "KaomoCSEmulator_userName";
        public readonly string DefaultUserName = "テストユーザー";
        public string userName
        {
            get => PlayerPrefs.GetString(PrefsKeyUserName, DefaultUserName);
            set => PlayerPrefs.SetString(PrefsKeyUserName, value);
        }
        const string PrefsKeyExists = "KaomoCSEmulator_exists";
        public bool exists
        {
            get => PlayerPrefs.GetInt(PrefsKeyExists, 1) == 1;
            set => PlayerPrefs.SetInt(PrefsKeyExists, value ? 1 : 0);
        }
        const string PrefsKeyPlayerEventRole = "KaomoCSEmulator_playerEventRole";
        public CSEmulator.EventRole playerEventRole
        {
            get => (CSEmulator.EventRole)PlayerPrefs.GetInt(PrefsKeyPlayerEventRole, (int)CSEmulator.EventRole.Audience);
            set => PlayerPrefs.SetInt(PrefsKeyPlayerEventRole, (int)value);
        }

        const string PrefsKeyIsVr = "KaomoCSEmulator_isVr"; //非使用
        const string PrefsKeyPlayerDevice = "KaomoCSEmulator_playerDevice";
        public PlayerDevice playerDevice
        {
            get => (PlayerDevice)PlayerPrefs.GetInt(PrefsKeyPlayerDevice, (int)PlayerDevice.Desktop);
            set => PlayerPrefs.SetInt(PrefsKeyPlayerDevice, (int)value);
        }
        const string PrefsKeyPlayerOperatingSystem = "KaomoCSEmulator_playerOperatingSystem";
        public PlayerOperatingSystem playerOperatingSystem
        {
            get => (PlayerOperatingSystem)PlayerPrefs.GetInt(PrefsKeyPlayerOperatingSystem, (int)PlayerOperatingSystem.Windows);
            set => PlayerPrefs.SetInt(PrefsKeyPlayerOperatingSystem, (int)value);
        }
        const string PrefsKeyPlayerColliderRadius = "KaomoCSEmulator_playerColliderRadius";
        public readonly float DefaultPlayerColliderRadius = 0.2f; //PreviewOnlyの値
        public float playerColliderRadius
        {
            get => PlayerPrefs.GetFloat(PrefsKeyPlayerColliderRadius, DefaultPlayerColliderRadius);
            set => PlayerPrefs.SetFloat(PrefsKeyPlayerColliderRadius, value);
        }

        public class ExternalEndpoint
        {
            public string id = "";
            public string url = "";
            public static ExternalEndpoint[] Deserialize(string serialized)
            {
                var ret = serialized.Split('\r')
                    .Where(endpoint => endpoint != "")
                    .Select(endpoint => {
                        var p = endpoint.Split('\n');
                        if (p.Length == 1)
                        {
                            return new ExternalEndpoint
                            {
                                id = "_legacy_single_endpoint",
                                url = p[0],
                            };
                        }
                        else
                        {
                            return new ExternalEndpoint
                            {
                                id = p[1],
                                url = p[2],
                            };
                        }
                    }).ToArray();
                return ret;
            }
            public static string Serialize(ExternalEndpoint[] externalEndpoints)
            {
                var sb = new StringBuilder();
                foreach (var p in externalEndpoints)
                {
                    //空行判定されないようにdummyを入れている
                    sb.AppendFormat("dummy\n{0}\n{1}\r", p.id.Trim(), p.url.Trim());
                }
                return sb.ToString();
            }
        }
        public readonly Regex externalEndpointIdPattern = new Regex("^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$");
        const string PrefsKeyCallExternalUrl = "KaomoCSEmulator_callExternalUrl";
        public ExternalEndpoint[] callExternalUrl
        {
            get => ExternalEndpoint.Deserialize(PlayerPrefs.GetString(PrefsKeyCallExternalUrl, ""));
            set => PlayerPrefs.SetString(PrefsKeyCallExternalUrl, ExternalEndpoint.Serialize(value));
        }

        const string PrefsKeyIsEvent = "KaomoCSEmulator_isEvent";
        public bool isEvent
        {
            get => PlayerPrefs.GetInt(PrefsKeyIsEvent, 0) == 1;
            set => PlayerPrefs.SetInt(PrefsKeyIsEvent, value ? 1 : 0);
        }

        //const string PrefsKeyOverwriteMouseSensitivity = "KaomoCSEmulator_overwriteMouseSensitivity";
        //独自の値を保持してもあまり意味がなさそうなので直操作
        public float overwriteMouseSensitivity
        {
            get => CameraControlSettings.Sensitivity;
            set => CameraControlSettings.Sensitivity = value;
        }

        const string PrefsKeyDebug = "KaomoCSEmulator_debug";
        public bool debug
        {
            get => PlayerPrefs.GetInt(PrefsKeyDebug, 0) == 1;
            set => PlayerPrefs.SetInt(PrefsKeyDebug, value ? 1 : 0);
        }

        const string PrefsKeyIgnoreCckPackageListUpdate = "KaomoCSEmulator_ignoreCckPackageListUpdate";
        public bool ignorePackageListUpdate
        {
            get => PlayerPrefs.GetInt(PrefsKeyIgnoreCckPackageListUpdate, 0) == 1;
            set => PlayerPrefs.SetInt(PrefsKeyIgnoreCckPackageListUpdate, value ? 1 : 0);
        }

        const string PrefsKeyPauseFrameKey = "KaomoCSEmulator_pauseFrameKey";
        public string pauseFrameKey
        {
            get => PlayerPrefs.GetString(PrefsKeyPauseFrameKey, "_pauseFrame");
            set => PlayerPrefs.SetString(PrefsKeyPauseFrameKey, value);
        }

        const string PrefsKeyIsRayDraw = "KaomoCSEmulator_isRayDraw";
        public bool isRayDraw
        {
            get => PlayerPrefs.GetInt(PrefsKeyIsRayDraw, 1) == 1;
            set => PlayerPrefs.SetInt(PrefsKeyIsRayDraw, value ? 1 : 0);
        }

        const string PrefsKeySubAudioDevice = "KaomoCSEmulator_subAudioDevice";
        public string subAudioDevice
        {
            get => PlayerPrefs.GetString(PrefsKeySubAudioDevice, "");
            set
            {
                if (subAudioDevice == value) return;
                var needRestart = _isSubAudioPlaying;
                if (needRestart) isSubAudioPlaying = false;
                PlayerPrefs.SetString(PrefsKeySubAudioDevice, value);
                if (needRestart) isSubAudioPlaying = true;
            }
        }
        //プレビュー中に保持しておけば良いのでPlayerPrefsは使わない。
        bool _isSubAudioPlaying;
        public bool isSubAudioPlaying
        {
            get => _isSubAudioPlaying;
            set
            {
                if (_isSubAudioPlaying == value) return;
                _isSubAudioPlaying = value;
                OnChangedSubAudioPlaying.Invoke(_isSubAudioPlaying, subAudioDevice);
            }
        }

        const string PrefsKeyDummyPlayerDefaultsIsMute = "KaomoCSEmulator_DummyPlayerDefaults_isMute";
        public bool dummyPlayerDefaults_isMute
        {
            get => PlayerPrefs.GetInt(PrefsKeyDummyPlayerDefaultsIsMute, Components.CSEmulatorDummyPlayer.isMute ? 1 : 0) == 1;
            set {
                PlayerPrefs.SetInt(PrefsKeyDummyPlayerDefaultsIsMute, value ? 1 : 0);
                Components.CSEmulatorDummyPlayer.isMute = value;
            }
        }

        public class ProductInfo
        {
            public string productName = "";
            public string productId = "";
            public int plus;
            public int minus;
            public bool isPublic = true;
            public static ProductInfo[] Deserialize(string serialized)
            {
                var ret = serialized.Split('\r')
                    .Where(info => info != "")
                    .Select(info => {
                        var p = info.Split('\n');
                        return new ProductInfo {
                            productId = p[0], productName = p[1],
                            plus = int.Parse(p[2]),
                            minus = int.Parse(p[3]),
                            isPublic = int.Parse(p[4]) == 1,
                        };
                    }).ToArray();
                return ret;
            }
            public static string Serialize(ProductInfo[] productInfos)
            {
                var sb = new StringBuilder();
                foreach(var p in productInfos)
                {
                    sb.AppendFormat("{0}\n{1}\n{2}\n{3}\n{4}\r", p.productId.Trim(), p.productName.Trim(), p.plus, p.minus, p.isPublic ? 1 : 0);
                }
                return sb.ToString();
            }
        }
        const string PrefsKeyProductInfos = "KaomoCSEmulator_productInfos";
        public readonly Regex productIdPattern = new Regex("^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$");
        public ProductInfo[] productInfos
        {
            get => ProductInfo.Deserialize(PlayerPrefs.GetString(PrefsKeyProductInfos, ""));
            set => PlayerPrefs.SetString(PrefsKeyProductInfos, ProductInfo.Serialize(value));
        }

        const string PrefsKeyPlayerStorage = "KaomoCSEmulator_playerStorage";
        public string playerStorage
        {
            get => PlayerPrefs.GetString(PrefsKeyPlayerStorage, "");
            set => PlayerPrefs.SetString(PrefsKeyPlayerStorage, value);
        }

        const string PrefsKeyGiftType = "KaomoCSEmulator_giftType";
        public string giftType
        {
            get => PlayerPrefs.GetString(PrefsKeyGiftType, "");
            set => PlayerPrefs.SetString(PrefsKeyGiftType, value);
        }
        const string PrefsKeyGiftPrice = "KaomoCSEmulator_giftPrice";
        public int giftPrice
        {
            get => PlayerPrefs.GetInt(PrefsKeyGiftPrice, 0);
            set => PlayerPrefs.SetInt(PrefsKeyGiftPrice, value);
        }

        const string PrefsKeyCommentVia = "KaomoCSEmulator_commentVia";
        public EmulateClasses.CommentVia commentVia
        {
            get => (EmulateClasses.CommentVia)PlayerPrefs.GetInt(PrefsKeyCommentVia, (int)EmulateClasses.CommentVia.cluster);
            set => PlayerPrefs.SetInt(PrefsKeyCommentVia, (int)value);
        }

        const string PrefsKeyCommentNextId = "KaomoCSEmulator_commentNextId";
        public int commentNextId
        {
            get => PlayerPrefs.GetInt(PrefsKeyCommentNextId, 0);
            set => PlayerPrefs.SetInt(PrefsKeyCommentNextId, Math.Max(value, 0));
        }

        const string PrefsKeyLoggingOnReceiveFail = "KaomoCSEmulator_loggingOnReceiveFail";
        public bool loggingOnReceiveFail
        {
            get => PlayerPrefs.GetInt(PrefsKeyLoggingOnReceiveFail, 1) == 1;
            set => PlayerPrefs.SetInt(PrefsKeyLoggingOnReceiveFail, value ? 1 : 0);
        }

        const string PrefsKeyOscAddress = "KaomoCSEmulator_oscAddress";
        public string oscAddress
        {
            get => PlayerPrefs.GetString(PrefsKeyOscAddress, "0.0.0.0");
            set => PlayerPrefs.SetString(PrefsKeyOscAddress, value);
        }
        const string PrefsKeyOscPort = "KaomoCSEmulator_oscPort";
        public int oscPort
        {
            get => PlayerPrefs.GetInt(PrefsKeyOscPort, 9001);
            set => PlayerPrefs.SetInt(PrefsKeyOscPort, value);
        }
        const string PrefsKeyOscIsReceiveEnabled = "KaomoCSEmulator_isReceiveEnabled";
        public bool oscIsReceiveEnabled
        {
            get => PlayerPrefs.GetInt(PrefsKeyOscIsReceiveEnabled, 1) == 1;
            set => PlayerPrefs.SetInt(PrefsKeyOscIsReceiveEnabled, value ? 1 : 0);
        }

        const string PrefsKeyOscSendAddress = "KaomoCSEmulator_oscSendAddress";
        public string oscSendAddress
        {
            get => PlayerPrefs.GetString(PrefsKeyOscSendAddress, "127.0.0.1");
            set => PlayerPrefs.SetString(PrefsKeyOscSendAddress, value);
        }
        const string PrefsKeyOscSendPort = "KaomoCSEmulator_oscSendPort";
        public int oscSendPort
        {
            get => PlayerPrefs.GetInt(PrefsKeyOscSendPort, 9002);
            set => PlayerPrefs.SetInt(PrefsKeyOscSendPort, value);
        }
        const string PrefsKeyOscIsSendEnabled = "KaomoCSEmulator_isSendEnabled";
        public bool oscIsSendEnabled
        {
            get => PlayerPrefs.GetInt(PrefsKeyOscIsSendEnabled, 1) == 1;
            set => PlayerPrefs.SetInt(PrefsKeyOscIsSendEnabled, value ? 1 : 0);
        }

        const string PrefsKeyProductDuumyName = "KaomoCSEmulator_productDummyName";
        public string productDummyName
        {
            get => PlayerPrefs.GetString(PrefsKeyProductDuumyName, "テストプロダクト");
            set => PlayerPrefs.SetString(PrefsKeyProductDuumyName, value);
        }

        public class StoredProductInfo
        {
            public string productId { get; set; }
            public EmulateClasses.StoredProductType type { get; set; }
            public StoredProductInfo(string productId, EmulateClasses.StoredProductType type)
            {
                this.productId = productId;
                this.type = type;
            }
        }
        const string PrefsKeyStoredProductIdsType = "KaomoCSEmulator_storedProductsType";
        public EmulateClasses.StoredProductType[] storedProductIdsType
        {
            get => PlayerPrefs.GetString(PrefsKeyStoredProductIdsType, ((int)EmulateClasses.StoredProductType.Accessory).ToString())
                .Split(",").Select(v => (EmulateClasses.StoredProductType)int.Parse(v == "" ? "0" : v))
                .ToArray();
            set => PlayerPrefs.SetString(PrefsKeyStoredProductIdsType, string.Join(",", value.Select(v => ((int)v).ToString())));
        }
        const string PrefsKeyStoredProductIds = "KaomoCSEmulator_storedProductIds";
        public string[] storedProductIds
        {
            get => PlayerPrefs.GetString(PrefsKeyStoredProductIds, "").Split(",");
            set => PlayerPrefs.SetString(PrefsKeyStoredProductIds, string.Join(",", value));
        }


        public class AvatarInfo
        {
            public string productId { get; set; }
            public bool enabled { get; set; }
            public AvatarInfo(string productId, bool enabled)
            {
                this.enabled = enabled;
                this.productId = productId;
            }
        }
        const string PrefsKeyIsAvatarProductIdsEnabled = "KaomoCSEmulator_isAvatarProductIdEnabled"; //後方互換の為に複数型のsが付いていない
        public bool[] isAvatarProductIdsEnabled
        {
            get => PlayerPrefs.GetString(PrefsKeyIsAvatarProductIdsEnabled, "1").Split(",").Select(v => v == "1").ToArray();
            set => PlayerPrefs.SetString(PrefsKeyIsAvatarProductIdsEnabled, string.Join(",", value.Select(v => v ? "1" : "0")));
        }
        const string PrefsKeyAvatarProductIds = "KaomoCSEmulator_avatarProductId"; //後方互換の為に複数型のsが付いていない
        public string[] avatarProductIds
        {
            get => PlayerPrefs.GetString(PrefsKeyAvatarProductIds, "00000000-abcd-1111-abcd-2222abcdabcd").Split(",");
            set => PlayerPrefs.SetString(PrefsKeyAvatarProductIds, string.Join(",", value));
        }

        public class AccessoryInfo
        {
            public string productId { get; set; }
            public bool enabled { get; set; }
            public AccessoryInfo(string productId, bool enabled)
            {
                this.enabled = enabled;
                this.productId = productId;
            }
        }
        const string PrefsKeyIsAccessoryProductIdsEnabled = "KaomoCSEmulator_isAccessoryProductIdsEnabled";
        public bool[] isAccessoryProductIdsEnabled
        {
            get => PlayerPrefs.GetString(PrefsKeyIsAccessoryProductIdsEnabled, "1,1,1").Split(",").Select(v => v == "1").ToArray();
            set => PlayerPrefs.SetString(PrefsKeyIsAccessoryProductIdsEnabled, string.Join(",", value.Select(v => v ? "1": "0")));
        }
        const string PrefsKeyAccessoryProductIds = "KaomoCSEmulator_accessoryProductIds";
        public string[] accessoryProductIds
        {
            get => PlayerPrefs.GetString(PrefsKeyAccessoryProductIds,
                "11111111-abcd-1111-abcd-2222abcdabcd,22222222-abcd-1111-abcd-2222abcdabcd,33333333-abcd-1111-abcd-2222abcdabcd"
            ).Split(",");
            set => PlayerPrefs.SetString(PrefsKeyAccessoryProductIds, string.Join(",", value));
        }

        public class CraftItemInfo
        {
            public string productId { get; set; }
            public CraftItemInfo(string productId)
           {
                this.productId = productId;
            }
        }
        const string PrefsKeyCraftItemProductIds = "KaomoCSEmulator_craftItemProductIds";
        public string[] craftItemProductIds
        {
            get => PlayerPrefs.GetString(PrefsKeyCraftItemProductIds, "").Split(",");
            set => PlayerPrefs.SetString(PrefsKeyCraftItemProductIds, string.Join(",", value));
        }

        const string PrefsKeyIsHapticsAvailable = "KaomoCSEmulator_isHapticsAvailable";
        public bool isHapticsAvailable
        {
            get => PlayerPrefs.GetInt(PrefsKeyIsHapticsAvailable, 1) == 1;
            set => PlayerPrefs.SetInt(PrefsKeyIsHapticsAvailable, value ? 1 : 0);
        }
        const string PrefsKeyIsHapticsFrequencyAvailable = "KaomoCSEmulator_isHapticsFrequencyAvailable";
        public bool isHapticsFrequencyAvailable
        {
            get => PlayerPrefs.GetInt(PrefsKeyIsHapticsFrequencyAvailable, 1) == 1;
            set => PlayerPrefs.SetInt(PrefsKeyIsHapticsFrequencyAvailable, value ? 1 : 0);
        }
        const string PrefsKeyHapticsMaxFrequency = "KaomoCSEmulator_hapticsMaxFrequency";
        public float hapticsMaxFrequency
        {
            get => PlayerPrefs.GetFloat(PrefsKeyHapticsMaxFrequency, 500);
            set => PlayerPrefs.SetFloat(PrefsKeyHapticsMaxFrequency, value);
        }
        const string PrefsKeyHapticsMinFrequency = "KaomoCSEmulator_hapticsMinFrequency";
        public float hapticsMinFrequency
        {
            get => PlayerPrefs.GetFloat(PrefsKeyHapticsMinFrequency, 160);
            set => PlayerPrefs.SetFloat(PrefsKeyHapticsMinFrequency, value);
        }
        const string PrefsKeyHapticsAudioVolume = "KaomoCSEmulator_hapticsAudioVolume";
        public float hapticsAudioVolume
        {
            get => PlayerPrefs.GetFloat(PrefsKeyHapticsAudioVolume, 0.002f);
            set => PlayerPrefs.SetFloat(PrefsKeyHapticsAudioVolume, value);
        }
        const string PrefsKeyHapticsDefaultAmplitude = "KaomoCSEmulator_hapticsDefaultAmplitude";
        public float hapticsDefaultAmplitude
        {
            get => PlayerPrefs.GetFloat(PrefsKeyHapticsDefaultAmplitude, 0.5f);
            set => PlayerPrefs.SetFloat(PrefsKeyHapticsDefaultAmplitude, value);
        }
        const string PrefsKeyHapticsDefaultDuration = "KaomoCSEmulator_hapticsDefaultDuration";
        public float hapticsDefaultDuration
        {
            get => PlayerPrefs.GetFloat(PrefsKeyHapticsDefaultDuration, 1.0f);
            set => PlayerPrefs.SetFloat(PrefsKeyHapticsDefaultDuration, value);
        }
        const string PrefsKeyHapticsDefaultFrequency = "KaomoCSEmulator_hapticsDefaultFrequency";
        public float hapticsDefaultFrequency
        {
            get => PlayerPrefs.GetFloat(PrefsKeyHapticsDefaultFrequency, 0.5f);
            set => PlayerPrefs.SetFloat(PrefsKeyHapticsDefaultFrequency, value);
        }
        const string PrefsKeySelectHierarchyLoggingItem = "KaomoCSEmulator_selectHierarchyLoggingItem";
        public bool selectHierarchyLoggingItem
        {
            get => PlayerPrefs.GetInt(PrefsKeySelectHierarchyLoggingItem, 1) == 1;
            set => PlayerPrefs.SetInt(PrefsKeySelectHierarchyLoggingItem, value ? 1 : 0);
        }

        const string PrefsKeyUsePrefabItemId = "KaomoCSEmulator_usePrefabItemId";
        public bool usePrefabItemId
        {
            get => PlayerPrefs.GetInt(PrefsKeyUsePrefabItemId, 1) == 1;
            set => PlayerPrefs.SetInt(PrefsKeyUsePrefabItemId, value ? 1 : 0);
        }
        public class ItemTemplateIdSet
        {
            public string itemTemplateId = "";
            public GameObject prefab = null;
            public static ItemTemplateIdSet[] Deserialize(string serialized)
            {
                var ret = serialized.Split('\r')
                    .Where(idSet => idSet != "")
                    .Select(idSet => {
                        var p = idSet.Split('\n');
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(p[2]);
                        var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        return new ItemTemplateIdSet
                        {
                            itemTemplateId = p[1],
                            prefab = prefab,
                        };
                    }).ToArray();
                return ret;
            }
            public static string Serialize(ItemTemplateIdSet[] itemTemplateIdSets)
            {
                var sb = new StringBuilder();
                foreach (var p in itemTemplateIdSets)
                {
                    var path = UnityEditor.AssetDatabase.GetAssetPath(p.prefab);
                    var guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
                    //空行判定されないようにdummyを入れている
                    sb.AppendFormat("dummy\n{0}\n{1}\r", p.itemTemplateId.Trim(), guid);
                }
                return sb.ToString();
            }
        }
        public readonly Regex itemTemplateIdPattern = new Regex("^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$");
        const string PrefsKeyItemTemplateIdSet = "KaomoCSEmulator_itemTemplateIdSets";
        public ItemTemplateIdSet[] itemTemplateIdSets
        {
            get => ItemTemplateIdSet.Deserialize(PlayerPrefs.GetString(PrefsKeyItemTemplateIdSet, ""));
            set => PlayerPrefs.SetString(PrefsKeyItemTemplateIdSet, ItemTemplateIdSet.Serialize(value));
        }
        public ItemTemplateIdSet[] itemTemplateIdValidSets 
        {
            get
            {
                var ret = itemTemplateIdSets
                .Where(s =>
                    s.itemTemplateId != ""
                    && itemTemplateIdPattern.IsMatch(s.itemTemplateId)
                    && s.prefab != null
                    && s.prefab.TryGetComponent<ClusterVR.CreatorKit.Item.IItem>(out var _)
                );
                return ret.ToArray();
            }
        }

        public EmulatorOptions()
        {
            //PlayerPrefs.DeleteKey(PrefsKeyCraftItemProductIds);
            //PlayerPrefs.DeleteKey(PrefsKeyIsAccessoryProductIdsEnabled);
            //PlayerPrefs.DeleteKey(PrefsKeyAccessoryProductIds);
            //PlayerPrefs.DeleteKey(PrefsKeyIsAvatarProductIdsEnabled);
            //PlayerPrefs.DeleteKey(PrefsKeyAvatarProductIds);
        }

        public void InitializePlayMode()
        {
            _isSubAudioPlaying = false;
        }

        public void SendGift()
        {
            OnGiftSent.Invoke(giftType, giftPrice);
        }

        public void Shutdown()
        {
            canChangePerspective = true;
        }
    }
}
