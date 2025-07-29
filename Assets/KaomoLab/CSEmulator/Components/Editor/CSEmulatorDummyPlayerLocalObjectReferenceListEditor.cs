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
    [CustomEditor(typeof(CSEmulatorDummyPlayerLocalObjectReferenceList), isFallback = true), CanEditMultipleObjects]
    public class CSEmulatorDummyPlayerLocalObjectReferenceListEditor : ClusterVR.CreatorKit.Editor.Custom.VisualElementEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var container = base.CreateInspectorGUI();
            container.Insert(0, new IMGUIContainer(() => {
                EditorGUILayout.HelpBox("DummyPlayerはここの要素を優先して参照します。\nここに指定がない場合は、元のリストを参照します。", MessageType.Info);
            }));
            return container;
        }

    }

}
