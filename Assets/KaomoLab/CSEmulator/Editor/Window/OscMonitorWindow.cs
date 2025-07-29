// OSC Jack - Open Sound Control plugin for Unity
// https://github.com/keijiro/OscJack

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.Net;
using uOSC;

namespace Assets.KaomoLab.CSEmulator.Editor.Window
{
    public class OscMonitorWindow : EditorWindow
    {
        [MenuItem("Window/かおもラボ/CSEmulatorOSCMonitor")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<OscMonitorWindow>("OSCMonitor");
        }

        // Used to divide the update cycle
        const int _updateInterval = 20;
        int _countToUpdate;

        // Server list which have been already under observation
        List<uOscServer> _knownServers = new List<uOscServer>();

        // Log line array and log counter (used to detect updates)
        StringBuilder _stringBuilder = new StringBuilder();
        string[] _logLines = new string[32];
        int _logCount;
        int _lastLogCount;

        void MonitorCallback(Message[] messages)
        {
            foreach (var m in messages)
            {
                foreach (var v in m.values)
                {
                    _logLines[_logCount] = string.Format("{0}:{1}({2})", m.address, v.GetString(), CSEmulator.Commons.UnixEpochMs(m.timestamp.ToUtcTime()));
                    _logCount = (_logCount + 1) % _logLines.Length;
                }
            }

        }

        void Update()
        {
            // We put some intervals between updates to decrease the CPU load.
            if (--_countToUpdate > 0) return;
            _countToUpdate = _updateInterval;

            if (!_knownServers.Contains(uOscServer.instance))
            {
                if(uOscServer.instance != null)
                    uOscServer.instance.onDataReceived.AddListener(MonitorCallback);
                _knownServers.Clear();
                _knownServers.Add(uOscServer.instance);
            }

            // Invoke repaint if there are new log lines.
            if (_logCount != _lastLogCount) Repaint();
        }

        void OnGUI()
        {
            bool isEmpty = true;

            EditorGUILayout.BeginVertical();

            var maxLog = _logLines.Length;
            for (var i = 0; i < maxLog; i++)
            {
                var idx = (_logCount + maxLog - 1 - i) % maxLog;
                var line = _logLines[idx];
                if (line == null) break;
                if (line != "")
                {
                    EditorGUILayout.SelectableLabel(line, GUILayout.MaxHeight(16));
                    isEmpty = false;
                }
            }
            if (isEmpty)
            {
                EditorGUILayout.LabelField("受信していません。");
            }

            EditorGUILayout.EndVertical();

            _lastLogCount = _logCount;
        }
    }
}
