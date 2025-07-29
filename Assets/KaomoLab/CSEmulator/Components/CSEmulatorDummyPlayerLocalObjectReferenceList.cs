using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Components
{
    [DisallowMultipleComponent, RequireComponent(typeof(ClusterVR.CreatorKit.Item.Implements.PlayerLocalObjectReferenceList))]
    public class CSEmulatorDummyPlayerLocalObjectReferenceList
         : MonoBehaviour
    {
        [SerializeField] ClusterVR.CreatorKit.Item.Implements.PlayerLocalObjectReferenceListEntry[] overwritePlayerLocalObjectReferences = { };

        public IReadOnlyCollection<ClusterVR.CreatorKit.Item.IPlayerLocalObjectReferenceListEntry> references => overwritePlayerLocalObjectReferences;

    }
}
