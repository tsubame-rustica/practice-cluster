using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Assets.KaomoLab.CSEmulator.Editor.Preview;

namespace Assets.KaomoLab.CSEmulator.Editor.Window
{
    public class EmulatorOptionsWindow : EditorWindow
    {
        const string UNIVRAM_PACKAGE = "Assets/VRM/package.json";
        bool isValidUniVrm = false;

        private Vector2 scroll = Vector2.zero;


        [System.Serializable]
        public class UniVrmPackage
        {
            public string version;
        }


        [MenuItem("Window/かおもラボ/CSEmulator")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<EmulatorOptionsWindow>(false, "CSEmulator");
        }
        const string PrefsKeyFoldoutPlayerControl = "KaomoCSEmulator_OptionsWindow_FoldoutPlayerControl";
        const string PrefsKeyFoldoutPlayerHandle = "KaomoCSEmulator_OptionsWindow_FoldoutPlayerHandle";
        const string PrefsKeyFoldoutPlayerScript = "KaomoCSEmulator_OptionsWindow_FoldoutPlayerScript";
        const string PrefsKeyFoldoutClusterScript = "KaomoCSEmulator_OptionsWindow_FoldoutClusterScript";
        const string PrefsKeyFoldoutOscHandle = "KaomoCSEmulator_OptionsWindow_FoldoutOscHandle";
        const string PrefsKeyFoldoutItemTemplateId = "KaomoCSEmulator_OptionsWindow_FoldoutItemTemplateId";
        const string PrefsKeyFoldoutCallExternal = "KaomoCSEmulator_OptionsWindow_FoldoutCallExternal";
        const string PrefsKeyFoldoutProductInfo = "KaomoCSEmulator_OptionsWindow_FoldoutProductInfo";
        const string PrefsKeyFoldoutGift = "KaomoCSEmulator_OptionsWindow_FoldoutGift";
        const string PrefsKeyFoldoutBelongingAvatar = "KaomoCSEmulator_OptionsWindow_FoldoutBelongingAvatar";
        const string PrefsKeyFoldoutBelongingAccessory = "KaomoCSEmulator_OptionsWindow_FoldoutBelongingAccessory";
        const string PrefsKeyFoldoutBelongingCraftItem = "KaomoCSEmulator_OptionsWindow_FoldoutBelongingCraftItem";
        const string PrefsKeyFoldoutStoredProduct = "KaomoCSEmulator_OptionsWindow_FoldoutStoredProduct";
        const string PrefsKeyFoldoutComment = "KaomoCSEmulator_OptionsWindow_FoldoutComment";
        const string PrefsKeyFoldoutHaptics = "KaomoCSEmulator_OptionsWindow_FoldoutHaptics";
        const string PrefsKeyFoldoutPlayerStorage = "KaomoCSEmulator_OptionsWindow_PlayerStorage";
        const string PrefsKeyFoldoutDummyPlayer = "KaomoCSEmulator_OptionsWindow_DummyPlayer";
        const string PrefsKeyFoldoutCSEmulator = "KaomoCSEmulator_OptionsWindow_CSEmulator";

        class BlockIndent : IDisposable
        {
            public BlockIndent() : this(1) { }
            public BlockIndent(int indent)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(indent * 10);
                EditorGUILayout.BeginVertical();
            }

            public void Dispose()
            {
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }
        class EditorModeOnly : IDisposable
        {
            public EditorModeOnly()
            {
                EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
            }

            public void HelpBox(string message = "プレビュー中は操作できません")
            {
                if (!EditorApplication.isPlaying) return;
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox(
                    message, MessageType.Info
                );
                EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
            }

            public void Dispose()
            {
                EditorGUI.EndDisabledGroup();
            }
        }

        GUIStyle BoldFontStyle(int fontSizeDelta)
        {
            return new GUIStyle() {
                fontStyle = FontStyle.Bold,
                fontSize = EditorStyles.boldFont.fontSize + fontSizeDelta,
                normal = new GUIStyleState() {
                    textColor = GUI.skin.label.normal.textColor
                }
            };
        }
        GUIStyle FoldoutStyle(int fontSizeDelta = 1)
        {
            var style = new GUIStyle(EditorStyles.foldout)
            {
                fontSize = EditorStyles.foldout.fontSize + fontSizeDelta,
                fontStyle = FontStyle.Bold,
                contentOffset = EditorStyles.foldout.contentOffset + new Vector2(5, 0),
                margin = new RectOffset(
                    EditorStyles.foldout.margin.left + 1,
                    EditorStyles.foldout.margin.right,
                    EditorStyles.foldout.margin.top,
                    EditorStyles.foldout.margin.bottom
                ),
            };
            return style;
        }
        bool Foldout(string label, string tooltip, string prefsKey)
        {
            var guiContent = (tooltip == null || tooltip == "")
                ? new GUIContent(label)
                : new GUIContent(label, tooltip);
            var foldout = EditorGUILayout.Foldout(
                (PlayerPrefs.GetInt(prefsKey, 1) == 1), guiContent, true, FoldoutStyle()
            );
            PlayerPrefs.SetInt(prefsKey, foldout ? 1: 0);
            return foldout;
        }

        void Indent(Action Field, int indent = 1)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(10 * indent);
                Field();
            }
        }

        void OnGUI()
        {
            var logo = AssetDatabase.LoadAssetAtPath<Texture>("Assets/KaomoLab/CSEmulator/Editor/Window/logo.png");
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(new GUIContent(logo), GUILayout.Height(30));
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndHorizontal();

            scroll = EditorGUILayout.BeginScrollView(scroll);


            CheckUniVrmVersion();

            var op = Bootstrap.options;

            if (Foldout("プレイヤーの操作・カスタマイズ", "", PrefsKeyFoldoutPlayerControl))
            {
                EditorGUILayout.Separator();
                EditorGUI.BeginDisabledGroup(!op.canChangePerspective);
                op.perspective = EditorGUILayout.Toggle("　一人称視点", op.perspective);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Separator();

                EditorGUILayout.LabelField("　サブ音声");
                {
                    //デフォルトのマイクを指定している場合は、空文字列を保存するという挙動。
                    var defaultMicrophone = "(デフォルトのマイク)";
                    var devices = new string[] { "" }.Concat(UnityEngine.Microphone.devices).ToArray();
                    var index = devices.Select((d, i) => (d, i)).Where(t => t.d == op.subAudioDevice).Select(t => t.i).DefaultIfEmpty(0).First();
                    var selected = EditorGUILayout.Popup(new GUIContent("　　デバイス"), index, devices.Select(d => {
                        if (d == "") return defaultMicrophone;
                        return d;
                    }).ToArray());
                    op.subAudioDevice = devices[selected];
                }
                if (UnityEditor.EditorApplication.isPlaying)
                {
                    op.isSubAudioPlaying = EditorGUILayout.Toggle(new GUIContent("　　サブ音声を有効にする"), op.isSubAudioPlaying);
                    if (UnityEngine.Application.platform != UnityEngine.RuntimePlatform.WindowsEditor && op.isSubAudioPlaying)
                    {
                        EditorGUILayout.HelpBox(
                            "Windows以外の環境ではモノラル扱いになります。", MessageType.Warning
                        );
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "サブ音声はプレビュー時に有効にできます。", MessageType.Info
                    );
                }
                EditorGUILayout.Separator();

                op.overwriteMouseSensitivity = EditorGUILayout.Slider(new GUIContent("　マウス操作感度", "clusterでの感度3(デフォルト値)は2.15ぐらいです"), op.overwriteMouseSensitivity, 0, 4f);
                EditorGUILayout.Separator();

                using (var editorOnly = new EditorModeOnly())
                {
                    editorOnly.HelpBox();
                    op.vrm = (GameObject)EditorGUILayout.ObjectField("　動作確認用のVRM", op.vrm, typeof(GameObject), false);
                    if (!Components.Commons.IsVrmPrefab(op.vrm))
                    {
                        EditorGUILayout.HelpBox(
                            "VRMのPrefabを指定してください。", MessageType.Error
                        );
                    }
                    EditorGUILayout.Separator();

                    EditorGUILayout.LabelField(new GUIContent("　アバターのコライダーサイズ", "アバターのカプセル状のコライダーが以下の設定に合わせリサイズされます。"));
                    EditorGUILayout.LabelField(new GUIContent("　※基本的には変更不要です。"));
                    op.playerColliderRadius = EditorGUILayout.FloatField(new GUIContent("　　半径", "CapsuleColliderの半径です。"), op.playerColliderRadius);
                    if (op.playerColliderRadius <= 0)
                    {
                        op.playerColliderRadius = op.DefaultPlayerColliderRadius;
                    }
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField(new GUIContent("　　高さ", "CapsuleColliderの高さです。\nPreviewSettingsの視点の高さ(立ち)を参照します。"), GUILayout.Width(148));
                        EditorGUILayout.TextField(
                            ClusterVR.CreatorKit.Preview.PlayerController.CameraControlSettings.StandingEyeHeight.ToString(),
                            EditorStyles.textField
                        );
                        if (GUILayout.Button("変更はこちら"))
                        {
                            ClusterVR.CreatorKit.Editor.Preview.EditorUI.SettingsWindow.ShowWindow();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
            }
            EditorGUILayout.Separator();

            if (Foldout("PlayerHandleの値", "", PrefsKeyFoldoutPlayerHandle))
            {
                EditorGUILayout.Separator();
                //using var _ = new BlockIndent(); //チェックボックスの位置までズレるのが気に入らなかったのでやめた。
                op.exists = EditorGUILayout.Toggle(new GUIContent("　.exists", "PlayerHandle.existsの値を指定できます。"), op.exists);
                op.userIdfc = EditorGUILayout.TextField(new GUIContent("　.idfc", "PlayerHandle.idfcの値を指定できます。"), op.userIdfc);
                if (!op.userIdfcPattern.IsMatch(op.userIdfc))
                {
                    EditorGUILayout.HelpBox(
                        "idfcの形式ではありません。", MessageType.Warning
                    );
                }
                if (op.userIdfc == "")
                    op.userIdfc = op.DefaultUserIdfc;
                op.userId = EditorGUILayout.TextField(new GUIContent("　.userId", "PlayerHandle.userIdの値を指定できます。"), op.userId);
                if (!op.userIdPattern.IsMatch(op.userId))
                {
                    EditorGUILayout.HelpBox(
                        "userIdの形式ではありません。", MessageType.Warning
                    );
                }
                if (op.userId == "")
                    op.userId = op.DefaultUserId;
                op.userName = EditorGUILayout.TextField(new GUIContent("　.userDisplayName", "PlayerHandle.userDisplayNameの値を指定できます。"), op.userName);
                if (op.userName == "")
                    op.userName = op.DefaultUserName;
                op.playerEventRole = (CSEmulator.EventRole)EditorGUILayout.EnumPopup(new GUIContent("　.getEventRole()", "PlayerHandle.getEventRoleの値を指定できます。"), op.playerEventRole);
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
            }
            EditorGUILayout.Separator();

            if (Foldout("PlayerScriptの値", "", PrefsKeyFoldoutPlayerScript))
            {
                EditorGUILayout.Separator();
                op.playerDevice = (CSEmulator.PlayerDevice)EditorGUILayout.EnumPopup(new GUIContent("　デバイス", "PlayerScript.isDesktop/isVr/isMobileの値を指定できます。"), op.playerDevice);
                op.playerOperatingSystem = (CSEmulator.PlayerOperatingSystem)EditorGUILayout.EnumPopup(new GUIContent("　OS", "PlayerScript.isWindows/isMacOs/isIos/isAndroidの値を指定できます。"), op.playerOperatingSystem);
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
            }
            EditorGUILayout.Separator();

            if (Foldout("ClusterScriptの値", "", PrefsKeyFoldoutClusterScript))
            {
                EditorGUILayout.Separator();
                op.isEvent = EditorGUILayout.Toggle(new GUIContent("　.isEvent"), op.isEvent);
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
            }
            EditorGUILayout.Separator();

            if (Foldout("OscHandle・OSC 送受信設定", "", PrefsKeyFoldoutOscHandle))
            {
                EditorGUILayout.Separator();
                op.oscIsReceiveEnabled = EditorGUILayout.Toggle(new GUIContent("　.isReceiveEnabled", "OscHandle.isReceiveEnabledの値を指定できます。"), op.oscIsReceiveEnabled);
                op.oscIsSendEnabled = EditorGUILayout.Toggle(new GUIContent("　.isSendEnabled", "OscHandle.isSendEnabledの値を指定できます。"), op.oscIsSendEnabled);
                using (var editorOnly = new EditorModeOnly())
                {
                    editorOnly.HelpBox();
                    EditorGUILayout.LabelField("　OscHandle.onReceive 設定(UDP)");
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField("アドレス", GUILayout.Width(40));
                    op.oscAddress = EditorGUILayout.TextField(op.oscAddress, GUILayout.Width(120));
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("ポート", GUILayout.Width(35));
                    op.oscPort = EditorGUILayout.IntField(op.oscPort, GUILayout.Width(50));
                    EditorGUILayout.EndHorizontal();
                    if (!EditorApplication.isPlaying && !EditorApplication.isPaused)
                    {
                        var ipp = IPGlobalProperties.GetIPGlobalProperties();
                        var udps = ipp.GetActiveUdpListeners();
                        if (udps.Where(udp => udp.Port == op.oscPort).Any())
                        {
                            EditorGUILayout.HelpBox(
                                "このポートは使用されています。別のポートを指定してください。", MessageType.Error
                            );
                        }
                    }

                    EditorGUILayout.LabelField("　OscHandle.send 設定(UDP)");
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField("アドレス", GUILayout.Width(40));
                    op.oscSendAddress = EditorGUILayout.TextField(op.oscSendAddress, GUILayout.Width(120));
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("ポート", GUILayout.Width(35));
                    op.oscSendPort = EditorGUILayout.IntField(op.oscSendPort, GUILayout.Width(50));
                    EditorGUILayout.EndHorizontal();

                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);
                    if (GUILayout.Button("OSCMonitorを表示する"))
                    {
                        OscMonitorWindow.ShowWindow();
                    }
                }
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
            }
            EditorGUILayout.Separator();

            if (Foldout("ItemTemplateIdの参照先", "", PrefsKeyFoldoutItemTemplateId))
            {
                EditorGUILayout.Separator();
                using (var editorOnly = new EditorModeOnly())
                {
                    Indent(() => op.usePrefabItemId = EditorGUILayout.ToggleLeft(new GUIContent("CSEmulatorPrefabItemを参照する"), op.usePrefabItemId));
                    if (op.usePrefabItemId)
                    {
                        Indent(() => EditorGUILayout.HelpBox(
                            "missingなスクリプトなどを持つPrefab(AudioLink等)からwarningが発生している場合、このオプションをOFFにして運用することで発生しなくなります。", MessageType.Info
                        ));
                    }
                    else
                    {
                        Indent(() => EditorGUILayout.HelpBox(
                            "CSEmulatorPrefabItemは参照されません。\n以下のリストにItemTemplateIdの登録が必要です。", MessageType.Info
                        ));
                        Indent(() => {
                            EditorGUILayout.LabelField("ItemTemplateId", GUILayout.Width(120));
                            EditorGUILayout.LabelField("Prefab", GUILayout.Width(40));
                        });
                        op.itemTemplateIdSets = op.itemTemplateIdSets.Select(idSet =>
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(10);
                            var itemTemplateId = EditorGUILayout.TextField("", idSet.itemTemplateId, GUILayout.Width(120));
                            var prefab = (GameObject)EditorGUILayout.ObjectField(idSet.prefab, typeof(GameObject), false, GUILayout.MinWidth(30));
                            if (GUILayout.Button("削除", GUILayout.Width(40)))
                            {
                                EditorGUILayout.EndHorizontal();
                                return null;
                            }
                            EditorGUILayout.EndHorizontal();

                            if (
                                idSet.itemTemplateId != ""
                                && !op.itemTemplateIdPattern.IsMatch(idSet.itemTemplateId)
                            )
                            {
                                Indent(() => EditorGUILayout.HelpBox(
                                    "ItemTemplateIdの形式ではありません。", MessageType.Warning
                                ));
                            }
                            if (
                                idSet.prefab != null
                                && !idSet.prefab.TryGetComponent<ClusterVR.CreatorKit.Item.IItem>(out var _)
                            )
                            {
                                Indent(() => EditorGUILayout.HelpBox(
                                    "ItemではないPrefabです", MessageType.Warning
                                ));
                            }

                            return new EmulatorOptions.ItemTemplateIdSet()
                            {
                                itemTemplateId = itemTemplateId,
                                prefab = prefab,
                            };
                        }).Where(info => info != null).ToArray();
                        Indent(() => {
                            if (GUILayout.Button("ItemTemplateIdを追加する"))
                            {
                                var added = op.itemTemplateIdSets.ToList();
                                added.Add(new EmulatorOptions.ItemTemplateIdSet());
                                op.itemTemplateIdSets = added.ToArray();
                            }
                        });
                    }
                }

                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
            }
            EditorGUILayout.Separator();

            if (Foldout("callExternal用Endpoint", "", PrefsKeyFoldoutCallExternal))
            {
                EditorGUILayout.Separator();
                using (var productHeader = new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("ID", GUILayout.Width(100));
                    EditorGUILayout.LabelField("URL", GUILayout.Width(40));
                }
                op.callExternalUrl = op.callExternalUrl.Select(endpoint =>
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    var id = EditorGUILayout.TextField("", endpoint.id, GUILayout.Width(100));
                    var url = EditorGUILayout.TextField("", endpoint.url, GUILayout.MinWidth(30));
                    if (GUILayout.Button("削除", GUILayout.Width(40)))
                    {
                        EditorGUILayout.EndHorizontal();
                        return null;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (
                        endpoint.id != ""
                        && !op.externalEndpointIdPattern.IsMatch(endpoint.id)
                        && endpoint.id != "_legacy_single_endpoint"
                    )
                    {
                        EditorGUILayout.HelpBox(
                            "Endpoint IDの形式ではありません。", MessageType.Warning
                        );
                    }

                    return new EmulatorOptions.ExternalEndpoint()
                    {
                        id = id,
                        url = url,
                    };
                }).Where(info => info != null).ToArray();
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);
                    if (GUILayout.Button("外部通信Endpointを追加する"))
                    {
                        var added = op.callExternalUrl.ToList();
                        added.Add(new EmulatorOptions.ExternalEndpoint());
                        op.callExternalUrl = added.ToArray();
                    }
                }
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
            }
            EditorGUILayout.Separator();

            if (Foldout("ワールド内課金商品", "", PrefsKeyFoldoutProductInfo))
            {
                EditorGUILayout.Separator();
                using (var productHeader = new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("名前", GUILayout.Width(100));
                    EditorGUILayout.LabelField("商品ID", GUILayout.Width(40));
                }
                op.productInfos = op.productInfos.Select(info =>
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    var name = EditorGUILayout.TextField("", info.productName, GUILayout.Width(100));
                    var id = EditorGUILayout.TextField("", info.productId, GUILayout.MinWidth(30));
                    if (GUILayout.Button("削除", GUILayout.Width(40)))
                    {
                        EditorGUILayout.EndHorizontal();
                        return null;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (info.productId != "" && !op.productIdPattern.IsMatch(info.productId))
                    {
                        EditorGUILayout.HelpBox(
                            "商品IDの形式ではありません。", MessageType.Warning
                        );
                    }

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("公開", GUILayout.Width(25));
                    var enable = EditorGUILayout.Toggle("", info.isPublic, GUILayout.Width(25));
                    EditorGUILayout.LabelField(String.Format("所持状況：{0}＝", info.plus - info.minus), GUILayout.Width(95));
                    var plus = EditorGUILayout.IntField("", info.plus, GUILayout.Width(30));
                    EditorGUILayout.LabelField("－", GUILayout.Width(10));
                    var minus = EditorGUILayout.IntField("", info.minus, GUILayout.Width(30));
                    EditorGUILayout.EndHorizontal();

                    return new EmulatorOptions.ProductInfo()
                    {
                        productId = id,
                        productName = name,
                        plus = plus,
                        minus = minus,
                        isPublic = enable
                    };
                }).Where(info => info != null).ToArray();
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);
                    if (GUILayout.Button("商品を追加する"))
                    {
                        var added = op.productInfos.ToList();
                        added.Add(new EmulatorOptions.ProductInfo());
                        op.productInfos = added.ToArray();
                    }
                }
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
            }
            EditorGUILayout.Separator();

            if (Foldout("ギフト", "", PrefsKeyFoldoutGift))
            {
                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.LabelField("giftType", GUILayout.Width(60));
                op.giftType = EditorGUILayout.TextField(op.giftType, GUILayout.Width(60));
                GUILayout.Space(20);
                EditorGUILayout.LabelField("price", GUILayout.Width(35));
                op.giftPrice = EditorGUILayout.IntField(op.giftPrice, GUILayout.Width(40));
                EditorGUILayout.EndHorizontal();
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);
                    if (GUILayout.Button("ギフトを投げる"))
                    {
                        op.SendGift();
                    }
                }
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
            }
            EditorGUILayout.Separator();

            if (Foldout("所持しているアバター", "", PrefsKeyFoldoutBelongingAvatar))
            {
                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.LabelField("使用中", GUILayout.Width(40));
                GUILayout.Space(10);
                EditorGUILayout.LabelField("ID", GUILayout.Width(35));
                EditorGUILayout.EndHorizontal();
                var avaterEnabledId = "";
                (op.avatarProductIds, op.isAvatarProductIdsEnabled)
                    = op.avatarProductIds
                    .Zip(op.isAvatarProductIdsEnabled, (id, enabled) => new EmulatorOptions.AvatarInfo(id, enabled))
                    .Select(info =>
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(10);
                        if (GUILayout.Button(info.enabled ? "✓" : "－", GUILayout.Width(40), GUILayout.Height(18)))
                        {
                            if (info.enabled)
                            {
                                info.enabled = false;
                            }
                            else
                            {
                                avaterEnabledId = info.productId;
                            }
                        }
                        GUILayout.Space(10);
                        info.productId = EditorGUILayout.TextField("", info.productId, GUILayout.MinWidth(100));
                        if (GUILayout.Button("削除", GUILayout.Width(40)))
                        {
                            info = null;
                        }
                        EditorGUILayout.EndHorizontal();
                        if (info != null && info.productId != "" && !op.productIdPattern.IsMatch(info.productId))
                        {
                            EditorGUILayout.HelpBox(
                                "アバターIDの形式ではありません。", MessageType.Warning
                            );
                        }
                        return info;
                    })
                    .Where(info => info != null)
                    .ToArray() //Aggreagetでenable処理を行うためにArrayにして一旦全部回す
                    .Aggregate((new string[0], new bool[0]), (acc, cur) =>
                    {
                        var enabled = avaterEnabledId == "" ? cur.enabled : (avaterEnabledId == cur.productId);
                        acc.Item1 = acc.Item1.Append(cur.productId).ToArray();
                        acc.Item2 = acc.Item2.Append(enabled).ToArray();
                        return acc;
                    });
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);
                    if (GUILayout.Button("アバターを追加する"))
                    {
                        op.avatarProductIds = op.avatarProductIds.Append("").ToArray();
                        op.isAvatarProductIdsEnabled = op.isAvatarProductIdsEnabled.Append(false).ToArray();
                    }
                }
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
            }
            EditorGUILayout.Separator();

            if (Foldout("所持しているアクセサリー", "", PrefsKeyFoldoutBelongingAccessory))
            {
                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.LabelField("装着中", GUILayout.Width(40));
                GUILayout.Space(10);
                EditorGUILayout.LabelField("ID", GUILayout.Width(35));
                EditorGUILayout.EndHorizontal();
                var isAccessoryFull = op.isAccessoryProductIdsEnabled.Count(e => e) >= 3;
                (op.accessoryProductIds, op.isAccessoryProductIdsEnabled)
                    = op.accessoryProductIds
                    .Zip(op.isAccessoryProductIdsEnabled, (id, enabled) => new EmulatorOptions.AccessoryInfo(id, enabled))
                    .Select(info =>
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(10);
                        if (GUILayout.Button(info.enabled ? "✓" : "－", GUILayout.Width(40), GUILayout.Height(18)))
                        {
                            if (info.enabled)
                            {
                                info.enabled = false;
                            }
                            else if (!isAccessoryFull)
                            {
                                info.enabled = true;
                            }
                        }
                        GUILayout.Space(10);
                        info.productId = EditorGUILayout.TextField("", info.productId, GUILayout.MinWidth(100));
                        if (GUILayout.Button("削除", GUILayout.Width(40)))
                        {
                            info = null;
                        }
                        EditorGUILayout.EndHorizontal();
                        if (info != null && info.productId != "" && !op.productIdPattern.IsMatch(info.productId))
                        {
                            EditorGUILayout.HelpBox(
                                "アクセサリーIDの形式ではありません。", MessageType.Warning
                            );
                        }
                        return info;
                    })
                    .Where(info => info != null)
                    .Aggregate((new string[0], new bool[0]), (acc, cur) =>
                    {
                        acc.Item1 = acc.Item1.Append(cur.productId).ToArray();
                        acc.Item2 = acc.Item2.Append(cur.enabled).ToArray();
                        return acc;
                    });
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);
                    if (GUILayout.Button("アクセサリーを追加する"))
                    {
                        op.accessoryProductIds = op.accessoryProductIds.Append("").ToArray();
                        op.isAccessoryProductIdsEnabled = op.isAccessoryProductIdsEnabled.Append(false).ToArray();
                    }
                }
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
            }
            EditorGUILayout.Separator();

            if (Foldout("所持しているアクセサリー", "", PrefsKeyFoldoutBelongingCraftItem))
            {
                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.LabelField("ID", GUILayout.Width(35));
                EditorGUILayout.EndHorizontal();
                op.craftItemProductIds = op.craftItemProductIds
                    .Select(id => new EmulatorOptions.CraftItemInfo(id))
                    .Select(info =>
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(10);
                        info.productId = EditorGUILayout.TextField("", info.productId, GUILayout.MinWidth(100));
                        if (GUILayout.Button("削除", GUILayout.Width(40)))
                        {
                            info = null;
                        }
                        EditorGUILayout.EndHorizontal();
                        if (info != null && info.productId != "" && !op.productIdPattern.IsMatch(info.productId))
                        {
                            EditorGUILayout.HelpBox(
                                "クラフトアイテムIDの形式ではありません。", MessageType.Warning
                            );
                        }
                        return info;
                    })
                    .Where(info => info != null)
                    .Select(info => info.productId)
                    .ToArray();
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);
                    if (GUILayout.Button("クラフトアイテムを追加する"))
                    {
                        op.craftItemProductIds = op.craftItemProductIds.Append("").ToArray();
                    }
                }
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
            }
            EditorGUILayout.Separator();

            if (Foldout("販売中の商品", "", PrefsKeyFoldoutStoredProduct))
            {
                EditorGUILayout.Separator();
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField(new GUIContent("共通商品名", "アバター、アクセサリー、クラフトアイテムで商品名を取得するときに使用されます。"), GUILayout.Width(70));
                    op.productDummyName = EditorGUILayout.TextField("", op.productDummyName, GUILayout.MinWidth(80));
                    if (op.productDummyName == "") op.productDummyName = "テストプロダクト";
                }
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.LabelField("種類", GUILayout.Width(80));
                EditorGUILayout.LabelField("ID", GUILayout.Width(35));
                EditorGUILayout.EndHorizontal();
                (op.storedProductIds, op.storedProductIdsType)
                    = op.storedProductIds
                    .Zip(op.storedProductIdsType, (id, type) => new EmulatorOptions.StoredProductInfo(id, type))
                    .Select(info =>
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(10);
                        info.type = (EmulateClasses.StoredProductType)EditorGUILayout.EnumPopup("", info.type, GUILayout.Width(80));
                        info.productId = EditorGUILayout.TextField("", info.productId, GUILayout.MinWidth(100));
                        if (GUILayout.Button("削除", GUILayout.Width(40)))
                        {
                            info = null;
                        }
                        EditorGUILayout.EndHorizontal();
                        if (info != null && info.productId != "" && !op.productIdPattern.IsMatch(info.productId))
                        {
                            EditorGUILayout.HelpBox(
                                "商品IDの形式ではありません。", MessageType.Warning
                            );
                        }
                        return info;
                    })
                    .Where(info => info != null)
                    .Aggregate((new string[0], new EmulateClasses.StoredProductType[0]), (acc, cur) =>
                    {
                        acc.Item1 = acc.Item1.Append(cur.productId).ToArray();
                        acc.Item2 = acc.Item2.Append(cur.type).ToArray();
                        return acc;
                    });
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);
                    if (GUILayout.Button("商品を追加する"))
                    {
                        op.storedProductIds = op.storedProductIds.Append("").ToArray();
                        op.storedProductIdsType = op.storedProductIdsType.Append(EmulateClasses.StoredProductType.Accessory).ToArray();
                    }
                }
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
            }
            EditorGUILayout.Separator();

            if (Foldout("コメント", "", PrefsKeyFoldoutComment))
            {
                EditorGUILayout.Separator();
                op.commentVia = (EmulateClasses.CommentVia)EditorGUILayout.EnumPopup("　送信元", op.commentVia);
                op.commentNextId = EditorGUILayout.IntField("　次のid", op.commentNextId);
                EditorGUILayout.LabelField("　.displayNameはコメント送信画面の「表示名」が使われます。");
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);
                    if (GUILayout.Button("コメント送信画面を表示する"))
                    {
                        ClusterVR.CreatorKit.Editor.Preview.EditorUI.PreviewControlWindow.ShowWindow();
                    }
                }
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
            }
            EditorGUILayout.Separator();

            if (Foldout("Haptics", "", PrefsKeyFoldoutHaptics))
            {
                EditorGUILayout.Separator();
                op.isHapticsAvailable = EditorGUILayout.Toggle("　.isAvailable()", op.isHapticsAvailable);
                op.isHapticsFrequencyAvailable = EditorGUILayout.Toggle("　振動数をサポート", op.isHapticsFrequencyAvailable);
                EditorGUI.BeginDisabledGroup(!op.isHapticsFrequencyAvailable);
                op.hapticsMinFrequency = EditorGUILayout.FloatField(new GUIContent("　.minFrequencyHz"), op.hapticsMinFrequency);
                op.hapticsMaxFrequency = EditorGUILayout.FloatField(new GUIContent("　.maxFrequencyHz"), op.hapticsMaxFrequency);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("　HapticsEffect　デフォルト値");
                op.hapticsDefaultAmplitude = Math.Clamp(EditorGUILayout.FloatField(new GUIContent("　　.amplitude"), op.hapticsDefaultAmplitude), 0, 1);
                op.hapticsDefaultDuration = Math.Max(EditorGUILayout.FloatField(new GUIContent("　　.duration"), op.hapticsDefaultDuration), 0);
                op.hapticsDefaultFrequency = Math.Clamp(EditorGUILayout.FloatField(new GUIContent("　　.frequency"), op.hapticsDefaultFrequency), 0, 1);
                EditorGUILayout.Separator();
                op.hapticsAudioVolume = EditorGUILayout.FloatField(new GUIContent("　音声化ボリューム", "振動を音として確認できます。その時の音量です。"), op.hapticsAudioVolume);
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
            }
            EditorGUILayout.Separator();


            if (Foldout("PlayerStorage", "", PrefsKeyFoldoutPlayerStorage))
            {
                EditorGUILayout.Separator();
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);
                    if (GUILayout.Button("保存している内容を削除する"))
                    {
                        op.playerStorage = "";
                    }
                }
                op.playerStorage = EditorGUILayout.TextField("　内容(コピペ用)", op.playerStorage);
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
            }
            EditorGUILayout.Separator();

            if (Foldout("ダミープレイヤー初期値", "", PrefsKeyFoldoutDummyPlayer))
            {
                EditorGUILayout.Separator();
                op.dummyPlayerDefaults_isMute = EditorGUILayout.Toggle(new GUIContent("　ミュート"), op.dummyPlayerDefaults_isMute);
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
            }
            EditorGUILayout.Separator();

            if (Foldout("CSEmulatorのカスタマイズ", "", PrefsKeyFoldoutCSEmulator))
            {
                EditorGUILayout.Separator();
                op.fps = (EmulatorOptions.FpsLimit)EditorGUILayout.EnumPopup("　FPSを制限する。", op.fps);
                EditorGUILayout.Separator();

                op.isRayDraw = EditorGUILayout.Toggle("　raycastを可視化する。", op.isRayDraw);
                EditorGUILayout.Separator();

                EditorGUILayout.LabelField("　$.onReceive失敗時にログ出力する。");
                op.loggingOnReceiveFail = EditorGUILayout.Toggle("　　", op.loggingOnReceiveFail);
                EditorGUILayout.Separator();

                EditorGUILayout.LabelField("　ログ出力元のアイテムを選択する。");
                op.selectHierarchyLoggingItem = EditorGUILayout.Toggle("　　", op.selectHierarchyLoggingItem);
                EditorGUILayout.Separator();

                using (var editorOnly = new EditorModeOnly())
                {
                    editorOnly.HelpBox();
                    op.debug = EditorGUILayout.Toggle("　デバッグモードで実行する。", op.debug);
                    EditorGUILayout.LabelField("　　動作が遅くなりますが、ログ出力が詳細になります。");
                    EditorGUILayout.Separator();

                    op.pauseFrameKey = EditorGUILayout.TextField(new GUIContent("　一時停止キー", "$.stateに値を入れた時に、プレビューを一時停止させるキー名"), op.pauseFrameKey);
                    EditorGUILayout.Separator();

                    op.ignorePackageListUpdate = EditorGUILayout.Toggle("　プレビュー起動を早くする。", op.ignorePackageListUpdate);
                    EditorGUILayout.LabelField("　　以下の処理を抑制します。");
                    EditorGUILayout.LabelField(String.Format(
                        "　　・「{0}」の処理",
                        ClusterVR.CreatorKit.Translation.TranslationTable.cck_package_list_fetch_success
                    ));
                    EditorGUILayout.Separator();

                    op.enable = EditorGUILayout.Toggle("　ClusterScriptを実行する。", op.enable);
                    EditorGUILayout.Separator();
                }
            }
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            EditorGUILayout.EndScrollView();
        }

        void CheckUniVrmVersion()
        {
            if (!isValidUniVrm)
            {
                if (!System.IO.File.Exists(UNIVRAM_PACKAGE))
                {
                    //Debug.Logで出すと出すぎる。出すなら工夫が必要。
                    EditorGUILayout.HelpBox(
                        "UniVRM 0.61.1が必要です。",
                        MessageType.Error
                    );
                }
                else
                {
                    var packageJson = System.IO.File.ReadAllText("Assets/VRM/package.json");
                    var package = JsonUtility.FromJson<UniVrmPackage>(packageJson);

                    if (package.version != "0.61.1")
                    {
                        //Debug.Logで出すと出すぎる。出すなら工夫が必要。
                        EditorGUILayout.HelpBox(
                            "UniVRM 0.61.1が必要です。",
                            MessageType.Error
                        );
                    }
                    else
                    {
                        isValidUniVrm = true;
                    }
                }
            }
        }

    }
}
