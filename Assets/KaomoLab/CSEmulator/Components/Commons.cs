using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Components
{
    public static class Commons
    {
        public static bool IsVrmPrefab(GameObject gameObject)
        {
            if (gameObject == null) return false;
            //UniVRMを入れていないと型名をコンパイルできずにエラーになるため
            //UnityではGetTypeする時はアセンブリ名(DLL名)も併せて必要
            var vrmMetaType = Type.GetType("VRM.VRMMeta, VRM");
            if (vrmMetaType == null) return false;
            if (null == gameObject.GetComponent(vrmMetaType)) return false;

            return true;
        }

    }
}
