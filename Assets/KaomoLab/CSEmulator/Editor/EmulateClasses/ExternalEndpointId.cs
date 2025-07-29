using ClusterVR.CreatorKit.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class ExternalEndpointId
    {
        public readonly string id;

        public ExternalEndpointId(
            string id
        )
        {
            this.id = id;
        }

        public object toJSON(string key)
        {
            dynamic o = new System.Dynamic.ExpandoObject();
            return o;
        }
        public override string ToString()
        {
            return String.Format("[ExternalEndpointId][{0}]", id);
        }
    }
}
