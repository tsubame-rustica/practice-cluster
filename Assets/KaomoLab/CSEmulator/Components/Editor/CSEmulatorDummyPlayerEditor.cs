using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Assets.KaomoLab.CSEmulator.Components.Editor
{
    [CustomEditor(typeof(CSEmulatorDummyPlayer))]
    public class CSEmulatorDummyPlayerEditor
         : UnityEditor.Editor
    {
        const string UUID_CAPTION = @"ItemTemplateIdとは「7865f52c-1305-4489-b780-c3562109e5e8」というような文字列です。
クラフトアイテムの情報取得Windowで取得できます。";

        public override void OnInspectorGUI()
        {
            var dummyPlayer = target as CSEmulatorDummyPlayer;

            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "プレビュー中の変更は破棄されます。", MessageType.Warning
            );

            EditorGUILayout.LabelField("PlayerHandle");

            var exists = serializedObject.FindProperty("_exists");
            exists.boolValue = EditorGUILayout.Toggle(new GUIContent("　.exists", "PlayerHandle.existsの値を指定できます。"), exists.boolValue);

            var idfc = serializedObject.FindProperty("_idfc");
            idfc.stringValue = EditorGUILayout.TextField(new GUIContent("　.idfc", "PlayerHandle.idfcの値を指定できます。"), idfc.stringValue);
            if (!CSEmulator.Commons.playerHandleIdfcPattern.IsMatch(idfc.stringValue))
            {
                EditorGUILayout.HelpBox(
                    "idfcの形式ではありません。", MessageType.Warning
                );
            }
            if (idfc.stringValue == "")
                idfc.stringValue = CSEmulator.Commons.CreateRandomPlayerHandleIdfc();

            var userId = serializedObject.FindProperty("_userId");
            userId.stringValue = EditorGUILayout.TextField(new GUIContent("　.userId", "PlayerHandle.userIdの値を指定できます。"), userId.stringValue);
            if (!CSEmulator.Commons.playerHandleUserIdPattern.IsMatch(userId.stringValue))
            {
                EditorGUILayout.HelpBox(
                    "userIdの形式ではありません。", MessageType.Warning
                );
            }
            if (userId.stringValue == "")
                userId.stringValue = CSEmulator.Commons.CreateRandomPlayerHandleUserId();

            var userDisplayName = serializedObject.FindProperty("_userDisplayName");
            userDisplayName.stringValue = EditorGUILayout.TextField(new GUIContent("　.userDisplayName", "PlayerHandle.userDisplayNameの値を指定できます。"), userDisplayName.stringValue);
            if (userDisplayName.stringValue == "")
                userDisplayName.stringValue = "ダミーユーザー";

            var eventRole = serializedObject.FindProperty("_eventRole");
            eventRole.intValue = (int)(CSEmulator.EventRole)EditorGUILayout.EnumPopup(new GUIContent("　.getEventRole()", "PlayerHandle.getEventRoleの値を指定できます。"), (CSEmulator.EventRole)eventRole.intValue);
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("PlayerScript");
            var playerDevice = serializedObject.FindProperty("_playerDevice");
            playerDevice.intValue = (int)(CSEmulator.PlayerDevice)EditorGUILayout.EnumPopup(new GUIContent("　デバイス", "PlayerScript.isDesktop/isVr/isMobileの値を指定できます。"), (CSEmulator.PlayerDevice)playerDevice.intValue);
            var playerOperatingSystem = serializedObject.FindProperty("_playerOperatingSystem");
            playerOperatingSystem.intValue = (int)(CSEmulator.PlayerOperatingSystem)EditorGUILayout.EnumPopup(new GUIContent("　OS", "PlayerScript.isWindows/isMacOs/isIos/isAndroidの値を指定できます。"), (CSEmulator.PlayerOperatingSystem)playerOperatingSystem.intValue);
            EditorGUILayout.Separator();


            EditorGUILayout.LabelField("プレビュー中に発生した変更は破棄されます。");
            var avatarProductIds = serializedObject.FindProperty("_avatarProductIds");
            EditorGUILayout.PropertyField(avatarProductIds, new GUIContent("所持しているアバターのID"), true);
            var accessoryProductIds = serializedObject.FindProperty("_accessoryProductIds");
            EditorGUILayout.PropertyField(accessoryProductIds, new GUIContent("所持しているアクセサリーのID"), true);
            var craftItemProductIds = serializedObject.FindProperty("_craftItemProductIds");
            EditorGUILayout.PropertyField(craftItemProductIds, new GUIContent("所持しているクラフトアイテムのID"), true);
            EditorGUILayout.Separator();


            EditorGUILayout.LabelField("PlayerStorage");
            var playerStorage = serializedObject.FindProperty("_playerStorage");
            EditorGUILayout.LabelField("　プレビュー中の変更は破棄されます。");
            EditorGUILayout.LabelField("　必要に応じて「プレビュー中に」コピペして保持して下さい。");
            playerStorage.stringValue = EditorGUILayout.TextField("　内容(コピペ用)", playerStorage.stringValue);
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("サンプルボイス");
            dummyPlayer.GetComponent<AudioSource>().mute = EditorGUILayout.Toggle(new GUIContent("　ミュート"), dummyPlayer.GetComponent<AudioSource>().mute);
            EditorGUILayout.Separator();

            var vrm = serializedObject.FindProperty("_vrm");
            vrm.objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("VRM", vrm.objectReferenceValue, typeof(GameObject), false);
            if (!Commons.IsVrmPrefab((GameObject)vrm.objectReferenceValue))
            {
                vrm.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(
                    CSEmulator.Editor.Commons.DefaultVrmPath
                );
            }

            serializedObject.ApplyModifiedProperties();

        }
    }
}
