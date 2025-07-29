using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Preview
{
    public class PlainItemBridge
    {
        readonly Components.CSEmulatorPlayerController playerController;

        public PlainItemBridge(
            Components.CSEmulatorPlayerController playerController
        )
        {
            this.playerController = playerController;
        }

        public void Bind(Components.CSEmulatorItemHandler itemHandler)
        {
            if (itemHandler.grabbableItem == null) return;
            itemHandler.grabbableItem.OnGrabbed += GrabbableItem_OnGrabbed;
            itemHandler.grabbableItem.OnReleased += GrabbableItem_OnReleased;
        }

        void GrabbableItem_OnGrabbed(bool isLeftHand)
        {
            Grabbed(isLeftHand, true);

        }
        void GrabbableItem_OnReleased(bool isLeftHand)
        {
            Grabbed(isLeftHand, false);
        }
        private void Grabbed(bool isLeftHand, bool isGrab)
        {
            //一旦右手＆オーナーの検出機能実装まで固定
            playerController.ChangeGrabbing(isGrab);
        }


    }
}
