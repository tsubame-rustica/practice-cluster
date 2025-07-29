using Assets.KaomoLab.CSEmulator.Editor.EmulateClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class ProductGranter
        : EmulateClasses.IProductGranter
    {
        class GrantResult
        {
            public readonly ulong itemId;
            public readonly ProductGrantResult result;
            public readonly long tick;
            public GrantResult(ulong itemId, ProductGrantResult result, long tick)
            {
                this.itemId = itemId;
                this.result = result;
                this.tick = tick;
            }
        }
        readonly List<GrantResult> grantResultQueue = new();
        readonly Dictionary<ulong, IJintCallback<ProductGrantResult>> requestGrantProductResultCallbacks = new();

        readonly ITicker ticker = new Implements.DateTimeTicks();

        public ProductGranter(
        )
        {
        }

        public void SendGrantResult(ulong itemId, ProductGrantResult result)
        {
            grantResultQueue.Add(
                new GrantResult(
                    itemId,
                    result,
                    ticker.Ticks() + 1_000_000
                )
            );
        }

        public void SetRequestGrantProductResultCallback(ulong itemId, IJintCallback<ProductGrantResult> Callback)
        {
            requestGrantProductResultCallbacks[itemId] = Callback;
        }

        public void DeleteCallbacks(ulong itemId)
        {
            if (requestGrantProductResultCallbacks.ContainsKey(itemId)) requestGrantProductResultCallbacks.Remove(itemId);
        }

        public void Routing()
        {
            var tick = ticker.Ticks();

            foreach (var p in grantResultQueue.ToArray())
            {
                if (p.tick < tick) continue;
                grantResultQueue.Remove(p);

                foreach(var kv in requestGrantProductResultCallbacks)
                {
                    if (p.itemId != kv.Key) continue;
                    kv.Value.Execute(p.result);
                }
            }
        }
    }
}
