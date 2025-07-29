﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class PlayerHandle
        : ISendableSize, IHasUnofficialMembers
    {
        public readonly string type = "player";

        //こういうタイプの公開は悪手だと分かっているがもう面倒なので
        //公開したらIHasUnofficialMembersも対応
        public Components.IPlayerMeta playerMeta { get; private set; }
        readonly ISpaceContext spaceContext;
        public IPlayerController playerController { get; private set; }
        readonly IPlayerTransformController playerTransformController;
        readonly IUserInputInterfaceHandler userInterfaceHandler;
        readonly ITextInputSender textInputSender;
        public IButtonInterfaceHandler buttonInterfaceHandler { get; private set; }
        public IPlayerLocalObjectGatherer playerLocalObjectGatherer { get; private set; }
        readonly IProductPurchaser productPurchaser;
        readonly IProductGranter productGranter;
        public IProductAmount productAmount { get; private set; }
        readonly IClusterEvent clusterEvent;
        readonly IPlayerLooks playerLooks;
        readonly IPlayerBelongings playerBelongings;
        readonly IStoredProducts storedProducts;
        readonly IPostProcessApplier postEffectApplier;
        public IVoiceHandler voiceHandler { get; private set; }
        public IHapticsAudioController hapticsAudio { get; private set; }
        public ISerializedPlayerStorage serializedPlayerStorage { get; private set; }
        readonly IMessageSender messageSender;
        //ownerの切り替えにはnewを強制しておきたいためprivate
        readonly Components.CSEmulatorItemHandler csOwnerItemHandler;

        //csOwnerItemHandlerとはこのハンドルがいるスクリプト空間($)のこと。
        public PlayerHandle(
            Components.IPlayerMeta playerMeta,
            ISpaceContext spaceContext,
            IPlayerController playerController,
            IPlayerTransformController playerTransformController,
            IUserInputInterfaceHandler userInterfaceHandler,
            ITextInputSender textInputSender,
            IButtonInterfaceHandler buttonInterfaceHandler,
            IPlayerLocalObjectGatherer playerLocalObjectGatherer,
            IProductPurchaser productPurchaser,
            IProductAmount productAmount,
            IProductGranter productGranter,
            IClusterEvent clusterEvent,
            IPlayerLooks playerLooks,
            IPlayerBelongings playerBelongings,
            IStoredProducts storedProducts,
            IPostProcessApplier postEffectApplier,
            IVoiceHandler voiceHandler,
            IHapticsAudioController hapticsAudio,
            ISerializedPlayerStorage serializedPlayerStorage,
            IMessageSender messageSender,
            Components.CSEmulatorItemHandler csOwnerItemHandler
        )
        {
            this.playerMeta = playerMeta;
            this.spaceContext = spaceContext;
            this.playerController = playerController;
            this.playerTransformController = playerTransformController;
            this.userInterfaceHandler = userInterfaceHandler;
            this.textInputSender = textInputSender;
            this.buttonInterfaceHandler = buttonInterfaceHandler;
            this.playerLocalObjectGatherer = playerLocalObjectGatherer;
            this.productPurchaser = productPurchaser;
            this.productAmount = productAmount;
            this.productGranter = productGranter;
            this.clusterEvent = clusterEvent;
            this.playerLooks = playerLooks;
            this.playerBelongings = playerBelongings;
            this.storedProducts = storedProducts;
            this.postEffectApplier = postEffectApplier;
            this.voiceHandler = voiceHandler;
            this.hapticsAudio = hapticsAudio;
            this.serializedPlayerStorage = serializedPlayerStorage;
            this.messageSender = messageSender;
            //★ここの項目が増えたら下のコンストラクタでも対応をする。
            this.csOwnerItemHandler = csOwnerItemHandler;
        }
        public PlayerHandle(
            PlayerHandle source,
            Components.CSEmulatorItemHandler csOwnerItemHandler
        )
        {
            this.playerMeta = source.playerMeta;
            this.spaceContext = source.spaceContext;
            this.playerController = source.playerController;
            this.playerTransformController = source.playerTransformController;
            this.userInterfaceHandler = source.userInterfaceHandler;
            this.textInputSender = source.textInputSender;
            this.buttonInterfaceHandler = source.buttonInterfaceHandler;
            this.playerLocalObjectGatherer = source.playerLocalObjectGatherer;
            this.productPurchaser = source.productPurchaser;
            this.productAmount = source.productAmount;
            this.productGranter = source.productGranter;
            this.clusterEvent = source.clusterEvent;
            this.playerLooks = source.playerLooks;
            this.playerBelongings = source.playerBelongings;
            this.storedProducts = source.storedProducts;
            this.postEffectApplier = source.postEffectApplier;
            this.voiceHandler = source.voiceHandler;
            this.hapticsAudio = source.hapticsAudio;
            this.serializedPlayerStorage = source.serializedPlayerStorage;
            this.messageSender = source.messageSender;
            this.csOwnerItemHandler = csOwnerItemHandler;
        }

        public string id => playerController.id;

        public string idfc => playerMeta.exists ? playerMeta.userIdfc : null;
        public string userId => playerMeta.exists ? playerMeta.userId : null;
        public string userDisplayName => playerMeta.exists ? playerMeta.userDisplayName : null;

        public void addVelocity(EmulateVector3 velocity)
        {
            CheckOwnerOperationLimit();

            _addVelocity(velocity);
        }
        public void _addVelocity(EmulateVector3 velocity)
        {
            playerController.AddVelocity(velocity._ToUnityEngine());
        }

        public bool exists()
        {
            return playerMeta.exists ? playerTransformController.exists : false;
        }

        public string[] getAccessoryProductIds()
        {
            if(!exists()) return new string[0];
            var ret = playerLooks.accessoryProductIds.Where(id => id != "").ToArray();
            return ret;
        }

        public string getAvatarProductId()
        {
            if (!exists()) return null;
            return playerLooks.avatarProductId == "" ? null : playerLooks.avatarProductId;
        }

        public object getEventRole()
        {
            if (!clusterEvent.isEvent) return null;
            if (!playerMeta.exists) return null;
            return playerMeta.eventRole;
        }

        public EmulateVector3 getHumanoidBonePosition(HumanoidBone bone)
        {
            if (!playerTransformController.exists) return null;

            var v = playerController.GetBoneTransform((HumanBodyBones)bone);
            return new EmulateVector3(v.position);
        }

        public EmulateQuaternion getHumanoidBoneRotation(HumanoidBone bone)
        {
            if (!playerTransformController.exists) return null;

            var v = playerController.GetBoneTransform((HumanBodyBones)bone);
            return new EmulateQuaternion(v.rotation);
        }

        public EmulateVector3 getPosition()
        {
            if (!playerTransformController.exists) return null;

            var v = playerTransformController.GetPosition();
            return new EmulateVector3(v);
        }

        public EmulateQuaternion getRotation()
        {
            if (!playerTransformController.exists) return null;

            var q = playerTransformController.GetRotation();
            return new EmulateQuaternion(q);
        }

        public void requestGrantProduct(string productId, string meta)
        {
            var belongingsIds = playerBelongings.avatarProductIds
                .Concat(playerBelongings.accessoryProductIds)
                .Concat(playerBelongings.craftItemProductIds)
                .Where(id => id != "").ToList();
            var isAlreadyOwned = belongingsIds.Contains(productId);

            var productName = storedProducts.GetProductName(productId);
            var productType = storedProducts.GetProductType(productId);

            var status = ProductGrantResult.STATUS_GRANTED;
            var reason = "";
            if (isAlreadyOwned)
            {
                status = ProductGrantResult.STATUS_ALREADYOWNED;
            }
            else if (productName == "")
            {
                status = ProductGrantResult.STATUS_FAILED;
                reason = "商品がありません。";
            }
            else if (productType == StoredProductType.Disable)
            {
                status = ProductGrantResult.STATUS_FAILED;
                reason = "商品がありません。";
            }
            else if (productType == StoredProductType.Forbidden)
            {
                status = ProductGrantResult.STATUS_FAILED;
                reason = "他人の商品のため、商品付与の権限がありません。";
            }

            productGranter.SendGrantResult(
                csOwnerItemHandler.item.Id.Value, new ProductGrantResult(
                    //正常時はreasonがnullとドキュメントにあるが実際はnullではない。おそらく空文字列
                    //だが公式サンプルで!==null比較をしているので一旦これでいく。
                    reason == "" ? null : reason, meta, this, productId, productName, status
                )
            );

            if(status == ProductGrantResult.STATUS_GRANTED)
            {
                if(productType == StoredProductType.Accessory)
                {
                    playerBelongings.AddAccessory(productId);
                }
                else if (productType == StoredProductType.Avatar)
                {
                    playerBelongings.AddAvater(productId);
                }
                else if (productType == StoredProductType.CraftItem)
                {
                    playerBelongings.AddCraftItem(productId);
                }
            }
        }

        public void requestPurchase(string productId, string meta)
        {
            CheckOwnerOperationLimit();

            if (userInterfaceHandler.isUserInputting)
            {
                productPurchaser.SendPurchaseResult(csOwnerItemHandler.item.Id.Value, productId, meta, this, PurchaseRequestStatus.Busy);
                return;
            }
            var isPublic = productPurchaser.IsPublicProduct(productId);
            var productName = productPurchaser.GetProductNameById(productId);
            if (productName == null || !isPublic)
            {
                userInterfaceHandler.StartDialog(
                    "購入しようとした商品は現在販売しておりません。\n" + productId
                    + (productName != null && !isPublic ? "\n非公開商品はテストスペースでは販売できますが、通常スペースでは販売できません。" : "")
                    + (productName == null ? "\n設定画面に登録されていない商品です。" : ""),
                    new string[] { "OK" }, _ => { });
                return;
            }
            userInterfaceHandler.StartPurchase(
                productName, productId, meta,
                status =>
                {
                    productPurchaser.SendPurchaseResult(csOwnerItemHandler.item.Id.Value, productId, meta, this, status);
                }
            );
        }

        public void requestTextInput(string meta, string title)
        {
            CheckOwnerOperationLimit();

            if (userInterfaceHandler.isUserInputting)
            {
                textInputSender.Send(csOwnerItemHandler.item.Id.Value, "", meta, TextInputStatus.Busy);
                return;
            }
            //呼び出し時のownerのidを保持しておく
            var id = csOwnerItemHandler.item.Id.Value;
            userInterfaceHandler.StartTextInput(
                title,
                text => textInputSender.Send(id, text, meta, TextInputStatus.Success),
                () => textInputSender.Send(id, "", meta, TextInputStatus.Refused),
                () => textInputSender.Send(id, "", meta, TextInputStatus.Busy)
            );
        }

        public void resetPlayerEffects()
        {
            CheckOwnerOperationLimit();

            _resetPlayerEffects();
        }
        public void _resetPlayerEffects()
        {
            playerController.moveSpeedRate = 1;
            playerController.jumpSpeedRate = 1;
            playerController.gravity = CSEmulator.Commons.STANDARD_GRAVITY;
        }

        public void respawn()
        {
            CheckOwnerOperationLimit();

            _respawn();
        }
        public void _respawn()
        {
            playerController.Respawn();
        }

        public void send(string messageType, Jint.Native.JsValue arg)
        {
            if (arg is Jint.Native.JsUndefined)
            {
                UnityEngine.Debug.LogWarning("undefinedはsendできません。");
                return;
            }

            var obj = arg.ToObject();
            send(messageType, obj);
        }
        public void send(string messageType, object arg)
        {
            //新制限相当でかいので不要そうな気がする。
            //CheckSendOperationLimit(messageType, StateProxy.CalcSendableSize(arg, 0));
            CheckRequestSizeLimit(arg);

            messageSender.SendToPlayer(
                new PlayerId(this), messageType, arg, null, csOwnerItemHandler
            );
        }

        public void setGravity(float gravity)
        {
            CheckOwnerOperationLimit();

            _setGravity(gravity);
        }
        public void _setGravity(float gravity)
        {
            playerController.gravity = gravity;
        }

        //開発用
        public HumanoidPose __getHumanoidPose()
        {
            var humanPose = playerController.GetHumanPose();

            var muscles = new Muscles();
            for(var i = 0; i < humanPose.muscles.Length; i++)
            {
                muscles.Set(i, humanPose.muscles[i]);
            }

            var ret = new HumanoidPose(
                new EmulateVector3(humanPose.bodyPosition),
                new EmulateQuaternion(humanPose.bodyRotation),
                muscles
            );

            return ret;
        }

        public void setHumanoidPose(
            HumanoidPose pose,
            SetHumanoidPoseOption setHumanoidPoseOption = null
        )
        {
            CheckOwnerOperationLimit();

            if(pose == null)
            {
                playerController.SetHumanPosition(null);
                playerController.SetHumanRotation(null);
                playerController.InvalidateHumanMuscles();
                playerController.InvalidateHumanTransition();
                return;
            }

            playerController.SetHumanPosition(
                pose.centerPosition == null ? null : pose.centerPosition._ToUnityEngine()
            );
            playerController.SetHumanRotation(
                pose.centerRotation == null ? null : pose.centerRotation._ToUnityEngine()
            );

            if (pose.muscles == null)
            {
                playerController.InvalidateHumanMuscles();
            }
            else
            {
                playerController.SetHumanMuscles(
                    pose.muscles.muscles,
                    pose.muscles.changed
                );
            }

            if(setHumanoidPoseOption == null)
            {
                playerController.InvalidateHumanTransition();
            }
            else
            {
                var op = setHumanoidPoseOption.Nomalized();
                playerController.SetHumanTransition(
                    op.timeoutSeconds,
                    op.timeoutTransitionSeconds,
                    op.transitionSeconds
                );
            }
        }

        public void setJumpSpeedRate(float jumpSpeedRate)
        {
            CheckOwnerOperationLimit();

            _setJumpSpeedRate(jumpSpeedRate);
        }
        public void _setJumpSpeedRate(float jumpSpeedRate)
        {
            playerController.jumpSpeedRate = jumpSpeedRate;
        }

        public void setMoveSpeedRate(float moveSpeedRate)
        {
            CheckOwnerOperationLimit();

            _setMoveSpeedRate(moveSpeedRate);
        }
        public void _setMoveSpeedRate(float moveSpeedRate)
        {
            playerController.moveSpeedRate = moveSpeedRate;
        }

        public void setPosition(
            EmulateVector3 position
        )
        {
            _setPosition(position, true);
        }
        public void _setPosition(
            EmulateVector3 position, bool checkLimit
        )
        {
            if (checkLimit)
            {
                CheckOwnerOperationLimit();
            }

            playerTransformController.SetPosition(position._ToUnityEngine());
        }

        public void setPostProcessEffects(
            PostProcessEffects effects
        )
        {
            CheckOwnerOperationLimit();

            _setPostProcessEffects(effects);
        }
        public void _setPostProcessEffects(
            PostProcessEffects effects
        )
        {
            if (effects != null)
            {
                postEffectApplier.Apply(effects.bloom);
                postEffectApplier.Apply(effects.chromaticAberration);
                postEffectApplier.Apply(effects.colorGrading);
                postEffectApplier.Apply(effects.depthOfField);
                postEffectApplier.Apply(effects.fog);
                postEffectApplier.Apply(effects.grain);
                postEffectApplier.Apply(effects.lensDistortion);
                postEffectApplier.Apply(effects.motionBlur);
                postEffectApplier.Apply(effects.vignette);
            }
            else
            {
                postEffectApplier.Apply(new BloomSettings());
                postEffectApplier.Apply(new ChromaticAberrationSettings());
                postEffectApplier.Apply(new ColorGradingSettings());
                postEffectApplier.Apply(new DepthOfFieldSettings());
                postEffectApplier.Apply(new FogSettings());
                postEffectApplier.Apply(new GrainSettings());
                postEffectApplier.Apply(new LensDistortionSettings());
                postEffectApplier.Apply(new MotionBlurSettings());
                postEffectApplier.Apply(new VignetteSettings());
            }
        }

        public void setRotation(
            EmulateQuaternion rotation
        )
        {
            _setRotation(rotation, true);
        }
        public void _setRotation(
            EmulateQuaternion rotation, bool checkLimit
        )
        {
            if (checkLimit)
            {
                CheckOwnerOperationLimit();
            }

            //Ｙ軸回転(鉛直軸)のみ
            var r = rotation._ToUnityEngine().eulerAngles;
            var y = Quaternion.Euler(0, r.y, 0);
            playerTransformController.SetRotation(y);
        }

        void CheckOwnerDistanceLimit()
        {
            var p1 = playerTransformController.GetPosition();
            var p2 = csOwnerItemHandler.gameObject.transform.position;
            var d = UnityEngine.Vector3.Distance(p1, p2);
            //30メートル以内はOK
            if (d <= 30f) return;

            throw csOwnerItemHandler.itemExceptionFactory.CreateDistanceLimitExceeded(
                String.Format("[{0}]>>>[Player]", csOwnerItemHandler)
            );
        }
        void CheckOwnerOperationLimit()
        {
            var result = csOwnerItemHandler.TryPlayerOperate();
            if (result) return;

            throw csOwnerItemHandler.itemExceptionFactory.CreateRateLimitExceeded(
                String.Format("[{0}]>>>[Player]", csOwnerItemHandler)
            );
        }
        //void CheckSendOperationLimit(string messageType, int size)
        //{
        //    var spaceLimit = spaceContext.TrySendOperate(size);

        //    if (spaceLimit && playerLimit) return;

        //    throw csOwnerItemHandler.itemExceptionFactory.CreateRateLimitExceeded(
        //        String.Format("Send制限:{3}:スペース({0}):プレイヤー({1}):[{2}]", spaceLimit ? "OK" : "NG", playerLimit ? "OK" : "NG", this, messageType)
        //    );
        //}
        void CheckRequestSizeLimit(object arg)
        {
            //軽く調べたところ$.computeSendableSizeと同じ模様
            var alen = StateProxy.CalcSendableSize(arg, 0);
            if (alen <= 1000) return;

            Debug.LogWarning(String.Format("[Player]argの制限長を超えています。({0}>1000)", alen));
        }

        public object toJSON(string key)
        {
            dynamic o = new System.Dynamic.ExpandoObject();
            o.id = id;
            o.userId = userId;
            o.userDisplayName = userDisplayName;
            return o;
        }
        public override string ToString()
        {
            return string.Format("[PlayerHandle][{0}]", id);
        }

        public int GetSize()
        {
            //2キャラで確認おそらく固定
            return 40;
        }

        string[] IHasUnofficialMembers.GetPropertyNames()
        {
            return new string[]
            {
                nameof(playerMeta),
                nameof(playerController),
            };
        }
    }
}
