using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class ProductGrantResult
    {
        public static string STATUS_GRANTED = "Granted";
        public static string STATUS_ALREADYOWNED = "AlreadyOwned";
        public static string STATUS_FAILED = "Failed";

        public string errorReason { get; private set; }
        public string meta { get; private set; }
        public PlayerHandle player { get; private set; }
        public string productId { get; private set; }
        public string productName { get; private set; }
        public string status { get; private set; }

        public ProductGrantResult(
            string errorReason,
            string meta,
            PlayerHandle player,
            string productId,
            string productName,
            string status
        )
        {
            this.errorReason = errorReason;
            this.meta = meta;
            this.player = player;
            this.productId = productId;
            this.productName = productName;
            this.status = status;
        }

        public object toJSON(string key)
        {
            return this;
        }
        public override string ToString()
        {
            return String.Format("[ProductGrantResult][{0}:{1}:{2}:{3}]", productId, productName, status, player.userDisplayName);
        }

    }
}
