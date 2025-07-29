using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Components
{
    [DisallowMultipleComponent, RequireComponent(typeof(ClusterVR.CreatorKit.Item.IItem))]
    public class CSEmulatorPrefabItem
         : MonoBehaviour
    {
        [SerializeField] public string _id;

        public string itemTemplateId {
            get => _id.ToLowerInvariant();
            set => _id = value.ToLowerInvariant();
        }
    }
}
