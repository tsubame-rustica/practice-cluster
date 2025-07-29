using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class PlayerLocalObject
    {
        public string name => gameObject.name;
        public string playerId { get; private set; }
        readonly GameObject gameObject;
        readonly IItemExceptionFactory itemExceptionFactory;
        readonly ILogger logger;

        public PlayerLocalObject(string playerId, GameObject gameObject, IItemExceptionFactory itemExceptionFactory, ILogger logger)
        {
            this.playerId = playerId;
            this.gameObject = gameObject;
            this.itemExceptionFactory = itemExceptionFactory;
            this.logger = logger;
        }

        public PlayerLocalObject findObject(string name)
        {
            if (gameObject == null) return null;

            var child = ClusterScript.FindChild(gameObject.transform, name);
            if (child == null) return null;

            if (child.GetComponent<ClusterVR.CreatorKit.Item.IItem>() != null)
            {
                //そもそもItemのPlayerLocalObjectは取得できないのでここには来ない。
                logger.Warning(String.Format("Itemが付いています。{0}", gameObject));
                return null;
            }
            var childParent = child.GetComponentInParent<ClusterVR.CreatorKit.Item.IItem>();
            var thisParent = gameObject.GetComponentInParent<ClusterVR.CreatorKit.Item.IItem>();
            //Itemの子であっても同じItemの子であるならセーフ？
            if (childParent != null && ((thisParent != null && childParent != thisParent) || thisParent == null))
            {
                logger.Warning(String.Format("Itemの子です。{0}", gameObject));
                return null;
            }

            return new PlayerLocalObject(playerId, child.gameObject, itemExceptionFactory, logger);
        }

        public bool getEnabled()
        {
            if (gameObject == null) return false;
            return gameObject.activeSelf;
        }

        public bool getTotalEnabled()
        {
            if (gameObject == null) return false;
            return gameObject.activeInHierarchy;
        }

        public UnityComponent getUnityComponent(string type)
        {
            if (gameObject == null) return null;
            CheckItemRelation();

            var ret = UnityComponent.GetPlayerLocalUnityComponent(
                playerId, gameObject, type, itemExceptionFactory, logger
            );
            return ret;
        }

        public void setEnabled(bool v)
        {
            if (gameObject == null) return;
            CheckItemRelation();
            gameObject.SetActive(v);
        }

        void CheckItemRelation()
        {
            if (gameObject.GetComponent<ClusterVR.CreatorKit.Item.IItem>() != null)
                throw itemExceptionFactory.CreateJsError(String.Format("Itemが付いています。{0}", gameObject));
            //Itemの子でも動いている？ドキュメントによると例外になりそうだけども
            //if (gameObject.GetComponentInParent<ClusterVR.CreatorKit.Item.IItem>() != null)
            //    throw itemExceptionFactory.CreateJsError(String.Format("Itemの子です。{0}", gameObject));
            if (gameObject.GetComponentInChildren<ClusterVR.CreatorKit.Item.IItem>() != null)
                throw itemExceptionFactory.CreateJsError(String.Format("子にItemがあります。{0}", gameObject));
        }

        public object toJSON(string key)
        {
            return this;
        }
        public override string ToString()
        {
            return String.Format("[PlayerLocalObject][{0}]", gameObject);
        }

    }
}
