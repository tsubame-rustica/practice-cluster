using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public interface ICallbackExecutor
    {
        void Execute(Jint.Native.Function.Function callback, params object[] args);
    }
    public class ExceptionCatchExecutor : ICallbackExecutor
    {
        readonly ICallbackExecutor parent;
        readonly Action<Exception> Catch;
        public ExceptionCatchExecutor(
            ICallbackExecutor parent,
            Action<Exception> Catch
        )
        {
            this.parent = parent;
            this.Catch = Catch;
        }

        public void Execute(Jint.Native.Function.Function callback, params object[] args)
        {
            try
            {
                parent.Execute(callback, args);
            }
            catch (Exception e)
            {
                Catch(e);
            }
        }
    }
    public interface IJintCallback
    {
        void Execute();
    }
    public interface IJintCallback<T>
    {
        void Execute(T arg);
    }
    public interface IJintCallback<T1, T2>
    {
        void Execute(T1 arg1, T2 arg2);
    }
    public interface IJintCallback<T1, T2, T3>
    {
        void Execute(T1 arg1, T2 arg2, T3 arg3);
    }
    public interface IJintCallback<T1, T2, T3, T4>
    {
        void Execute(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    }
    public class JintCallbackBase
    {
        public ICallbackExecutor executor; //基本的にengine超えは想定していないが、超えた場合にここを書き換える。
        protected readonly Jint.Native.Function.Function callback;
        public JintCallbackBase(ICallbackExecutor executor, Jint.Native.Function.Function callback)
        {
            this.executor = executor;
            this.callback = callback;
        }
    }
    public class JintCallback : JintCallbackBase, IJintCallback
    {
        public JintCallback() : base(null, null) { }
        public JintCallback(ICallbackExecutor executor, Jint.Native.Function.Function callback) : base(executor, callback) { }
        public void Execute()
        {
            if (callback == null) return;
            executor.Execute(callback);
        }
    }
    public class JintCallback<T> : JintCallbackBase, IJintCallback<T>
    {
        public JintCallback() : base(null, null) { }
        public JintCallback(ICallbackExecutor executor, Jint.Native.Function.Function callback) : base(executor, callback) { }
        public void Execute(T arg)
        {
            if (callback == null) return;
            executor.Execute(callback, arg);
        }
    }
    public class JintCallback<T1, T2> : JintCallbackBase, IJintCallback<T1, T2>
    {
        public JintCallback() : base(null, null) { }
        public JintCallback(ICallbackExecutor executor, Jint.Native.Function.Function callback) : base(executor, callback) { }
        public void Execute(T1 arg1, T2 arg2)
        {
            if (callback == null) return;
            executor.Execute(callback, arg1, arg2);
        }
    }
    public class JintCallback<T1, T2, T3> : JintCallbackBase, IJintCallback<T1, T2, T3>
    {
        public JintCallback() : base(null, null) { }
        public JintCallback(ICallbackExecutor executor, Jint.Native.Function.Function callback) : base(executor, callback) { }
        public void Execute(T1 arg1, T2 arg2, T3 arg3)
        {
            if (callback == null) return;
            executor.Execute(callback, arg1, arg2, arg3);
        }
    }
    public class JintCallback<T1, T2, T3, T4> : JintCallbackBase, IJintCallback<T1, T2, T3, T4>
    {
        public JintCallback() : base(null, null) { }
        public JintCallback(ICallbackExecutor executor, Jint.Native.Function.Function callback) : base(executor, callback) { }
        public void Execute(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (callback == null) return;
            executor.Execute(callback, arg1, arg2, arg3, arg4);
        }
    }

    public interface IUpdateListenerBinder
    {
        void SetUpdateCallback(string key, UnityEngine.GameObject source, IJintCallback<double> Callback);
        void DeleteUpdateCallback(string key);
        void SetLateUpdateCallback(string key, UnityEngine.GameObject source, Action<double> Callback);
        void DeleteLateUpdateCallback(string key);
    }

    public interface IItemReceiveListenerBinder
    {
        void SetItemReceiveCallback(
            Components.CSEmulatorItemHandler owner,
            EmulateClasses.IRunningContext runningContext,
            EmulateClasses.ISendableSanitizer sanitizer,
            IJintCallback<string, object, object> Callback
        );
        void DeleteItemReceiveCallback(Components.CSEmulatorItemHandler owner);
    }

    public interface IPlayerReceiveListenerBinder
    {
        void SetPlayerReceiveCallback(
            string playerId,
            EmulateClasses.IPlayerSendableSanitizer sanitizer,
            IJintCallback<string, object, object> Callback
        );
        void DeletePlayerReceiveCallback(string playerId);
    }


    public interface IStartListenerBinder
    {
        void SetUpdateCallback(IJintCallback Callback);
        void DeleteStartCallback();
    }

    public interface IMessageSender
    {
        void SendToItem(
            string id, string requestName, object arg,
            PlayerHandle senderPlayer,
            Components.CSEmulatorItemHandler senderItem
        );
        void SendToPlayer(
            PlayerId id, string messageType, object arg,
            PlayerHandle senderPlayer,
            Components.CSEmulatorItemHandler senderItem
        );
    }

    public interface IPrefabItemHolder
    {
        UnityEngine.GameObject GetPrefab(string uuid);
    }

    //CSETODO itemをinteractされたとき、そのplayerをどう取得する？できる？
    //それが解決するまでの仮
    public interface IItemOwnerHandler
    {
        string GetOwnerIdfc();
    }

    public interface IPlayerHandleFactory
    {
        PlayerHandle CreateByIdfc(string id, Components.CSEmulatorItemHandler csOwnerItemHandler);
    }

    public interface IPlayerTransformHolder
    {
        bool exists { get; }

        UnityEngine.Vector3 GetPosition();
        UnityEngine.Quaternion GetRotation();
    }
    public interface IPlayerTransformController
        : IPlayerTransformHolder
    {
        void SetPosition(UnityEngine.Vector3 position);
        void SetRotation(UnityEngine.Quaternion rotation);
    }

    //必要に応じてIPlayerTransformControllerのようにバラしていく。
    public interface IPlayerController
    {
        string id { get; }

        float gravity { get;  set; }
        float jumpSpeedRate { set; }
        float moveSpeedRate { set; }

        int movementFlags { get; }

        bool isFirstPersonView { get; }
        void SetPersonViewChangeable(bool canChange);
        UnityEngine.Vector3 GetCameraPosition();
        UnityEngine.Quaternion GetCameraRotation();
        void SetCameraPosition(UnityEngine.Vector3? position);
        void SetCameraRotation(UnityEngine.Quaternion? rotation);
        void SetCameraFieldOfViewTemporary(float value);
        void SetCameraFieldOfView(float value);
        float GetCameraFieldOfViewNow();
        float GetCameraFieldOfView();
        void SetThirdPersonCameraDistanceTemporary(float value);
        float GetThirdPersonCameraDistanceNow();
        float GetThirdPersonCameraDistanceDefault();
        void SetThirdPersonCameraScreenPosition(UnityEngine.Vector2 pos);
        UnityEngine.Vector2 GetThirdPersonCameraScreenPositionNow();

        UnityEngine.Transform GetFirstCameraTransform();

        void Respawn();

        void AddVelocity(UnityEngine.Vector3 velocity);

        void SetHumanPosition(UnityEngine.Vector3? position);
        void SetHumanRotation(UnityEngine.Quaternion? rotation);
        void SetHumanMuscles(float[] muscles, bool[] hasMascles);
        void InvalidateHumanMuscles();
        void SetHumanTransition(double timeoutSeconds, double timeoutTransitionSeconds, double transitionSeconds);
        void InvalidateHumanTransition();
        UnityEngine.HumanPose GetHumanPose();
        void MergeHumanPoseOnFrame(UnityEngine.Vector3? position, UnityEngine.Quaternion? rotation, float[] muscles, bool[] hasMascles, float weight);
        void OverwriteHumanoidBoneRotation(UnityEngine.HumanBodyBones bone, UnityEngine.Quaternion rotation);
        UnityEngine.Transform GetBoneTransform(UnityEngine.HumanBodyBones bone);

        void ChangeGrabbing(bool isGrab);
        void OverwriteFaceConstraint(bool? forward);

        void RunCoroutine(Func<System.Collections.IEnumerator> Coroutine);
    }

    public interface IUserInputInterfaceHandler
    {
        bool isUserInputting { get; }
        void StartTextInput(string caption, Action<string> SendCallback, Action CancelCallback, Action BusyCallback);
        void StartPurchase(string productName, string productId, string meta, Action<PurchaseRequestStatus> Callback);
        void StartDialog(string caption, string[] buttons, Action<int> Callback);
    }

    public interface IButtonInterfaceHandler
    {
        void ShowButton(int index, IconAsset icon);
        void HideButton(int index);
        void SetButtonCallback(int index, IJintCallback<bool> Callback);
        void HideAllButtons();
        void DeleteAllButtonCallbacks();
    }

    //雑にIF化している。引数に過不足が発生して困ったことになった時にFactoryを挟むかどうか等を考える。
    public interface IPlayerLocalObjectGatherer
    {
        PlayerLocalObject GetPlayerLocalObject(string playerId, string id, UnityEngine.GameObject listObject, IItemExceptionFactory itemExceptionFactory, ILogger logger);
    }

    public interface IPostProcessApplier
    {
        void Apply(BloomSettings settings);
        void Apply(ChromaticAberrationSettings settings);
        void Apply(ColorGradingSettings settings);
        void Apply(DepthOfFieldSettings settings);
        void Apply(FogSettings settings);
        void Apply(GrainSettings settings);
        void Apply(LensDistortionSettings settings);
        void Apply(MotionBlurSettings settings);
        void Apply(VignetteSettings settings);
    }

    public interface ITextInputListenerBinder
    {
        void SetReceiveCallback(Components.CSEmulatorItemHandler owner, IJintCallback<string, string, TextInputStatus> Callback);
        void DeleteReceiveCallback(Components.CSEmulatorItemHandler owner);
    }

    public interface IOscReceiveListenerBinder
    {
        void SetOscReceiveCallback(string id, IJintCallback<EmulateClasses.OscMessage[]> Callback);
        void DeleteOscReceiveCallback(string id);
    }

    public interface IOscSender
    {
        void Send(OscMessage payload);
        void Send(OscBundle payload);
    }

    public interface ITextInputSender
    {
        void Send(ulong id, string text, string meta, TextInputStatus status);
    }

    public interface IItemLifecycler
    {
        ClusterVR.CreatorKit.Item.IItem CreateItem(
            ItemTemplateId itemTemplateId,
            EmulateVector3 position,
            EmulateQuaternion rotation
        );
        ClusterVR.CreatorKit.Item.IItem CreateItem(
            ClusterVR.CreatorKit.Item.IWorldItemTemplateListEntry worldItemTemplateListEntry,
            EmulateVector3 position,
            EmulateQuaternion rotation
        );
        void DestroyItem(ClusterVR.CreatorKit.Item.IItem item);
    }

    public interface ICckComponentFacade
    {
        /// <summary>
        /// bool isLeftHand
        /// bool isGrab true:Grab false:Release
        /// </summary>
        event Handler<bool, bool> onGrabbed;

        /// <summary>
        /// bool isOn
        /// </summary>
        event Handler<bool> onRide;

        /// <summary>
        /// bool isDown
        /// </summary>
        event Handler<bool> onUse;

        event Handler onInteract;
        void AddInteractItemTrigger();

        event Handler<UnityEngine.Vector2> onSteerMove;
        event Handler<float> onSteerAdditionalAxis;

        bool isGrab { get; }
        bool hasCollider { get; }
        bool hasGrabbableItem { get; }
        bool hasRidableItem { get; }
        bool hasSteerItemTrigger { get; }

        void SendSignal(string target, string key);
        void SetState(string target, string key, object value);
        object GetState(string target, string key, string parameterType);

        void InvalidUseItemTrigger();
        void ResumeUseItemTrigger();
    }
    public interface IExternalCaller
    {
        void CallExternal(ExternalEndpointId endpointId, string request, string meta);
        void SetCallEndCallback(IJintCallback<string, string, string> Callback);
    }
    public interface ICckComponentFacadeFactory
    {
        ICckComponentFacade Create(UnityEngine.GameObject gameObject);
    }

    public interface IMaterialSubstituter
    {
        UnityEngine.Material Prepare(UnityEngine.Material material);
        void Destroy();
    }

    public interface IPlayerScriptRunner
    {
        void Run(PlayerScript playerScript, string id);
    }

    public interface ISendableSanitizer
    {
        public object Sanitize(
            object value,
            Func<Components.CSEmulatorItemHandler, ItemHandle> SanitizeItemHandle = null,
            Func<PlayerHandle, PlayerHandle> SanitizeItemPlayerHandle = null
        );
    }

    public interface IJsValueConverter
    {
        Jint.Native.JsValue FromObject(object value);
    }

    public interface IGroupStateProxyMapper
    {
        void ApplyState(GroupStateProxy state, string id);
    }

    public interface IHasTypeNameAlias
    {
        string GetAliasTypeName();
    }

    public interface IHasUnofficialMembers
    {
        string[] GetPropertyNames();
    }

    public interface ISendableSize
    {
        int GetSize();
    }

    public interface IPlayerSendableSanitizer
    {
        public object Sanitize(
            object value
        );
    }

    public interface IRunningContext
    {
        bool isTopLevel { get; }
        bool CheckTopLevel(string method);
    }

    public interface ISpaceContext
    {
        //bool TrySendOperate(int size);
    }

    public interface IOscContext
    {
        bool isReceiveEnabled { get; }
        bool isSendEnabled { get; }
    }

    public interface IHapticsSettings
    {
        bool isAvailable { get; }
        float maxFrequencyHz { get; }
        float minFrequencyHz { get; }
        bool isFrequencySupported { get; }

        float volume { get; }
        float defaultAmplitude { get; }
        float defaultDuration { get; }
        float defaultFrequency { get; }
    }
    public interface IHapticsAudioController
    {
        void PlayLeft(float amp, float dur, float freq);
        void PlayRight(float amp, float dur, float freq);
        void PlayBoth(float amp, float dur, float freq);
        void StopLeft();
        void StopRight();
    }

    public interface IRayDrawer
    {
        void DrawRay(UnityEngine.Vector3 start, UnityEngine.Vector3 end, UnityEngine.Color color);
    }

    public interface IProductPurchaser
    {
        bool IsGetOwnProductsLimit();
        void GetOwnProducts(ulong itemId, string productId, PlayerHandle[] players, string meta);
        void SetGetOwnProductsCallback(ulong itemId, IJintCallback<OwnProduct[], string, string> Callback);
        void SetPurchaseUpdateCallback(ulong itemId, IJintCallback<PlayerHandle, string> Callback);
        void SetRequestPurchaseStatusCallback(ulong itemId, IJintCallback<string, PurchaseRequestStatus, string, PlayerHandle> Callback);
        void DeleteCallbacks(ulong itemId);
        void SubscribePurchase(ulong itemId, string productId);
        void UnsubscribePurchase(ulong itemId, string productId);
        string GetProductNameById(string productId); //nullの場合はproductが無い
        bool IsPublicProduct(string productId);
        void SendPurchaseResult(ulong itemId, string productId, string meta, PlayerHandle player, PurchaseRequestStatus status);
    }

    public interface IProductAmount
    {
        (int, int) GetProductAmount(string productId); //(plus,minus)
        void SetProductAmount(string productId, int plus, int minus);
    }

    public interface IProductGranter
    {
        void SetRequestGrantProductResultCallback(ulong itemId, IJintCallback<ProductGrantResult> Callback);
        void SendGrantResult(ulong itemId, ProductGrantResult result);
    }

    public interface ISerializedPlayerStorage
    {
        void SavePlayerStorage(string serialized);
        string LoadPlayerStorage();
    }
    public interface IPlayerStoragerFactory
    {
        IPlayerStorager Create(
            Components.CSEmulatorItemHandler csItemHandler,
            ISerializedPlayerStorage serializedPlayerStorage
        );
    }
    public interface IPlayerStorager
    {
        void Save(object value);
        object Load();
    }

    public interface IClusterEvent
    {
        bool isEvent { get; }
        event Handler<string, int> OnGiftSent;
    }

    public interface IPlayerLooks
    {
        bool exists { get; }
        IReadOnlyList<string> accessoryProductIds { get; }
        string avatarProductId { get; }
    }

    public interface IPlayerBelongings
    {
        IReadOnlyList<string> accessoryProductIds { get; }
        void AddAccessory(string productId);
        IReadOnlyList<string> avatarProductIds { get; }
        void AddAvater(string productId);
        IReadOnlyList<string> craftItemProductIds { get; }
        void AddCraftItem(string productId);
    }

    public enum StoredProductType : int
    {
        Accessory,
        Avatar,
        CraftItem,
        Disable, //Failed
        Forbidden //Failed
    }
    public interface IStoredProducts
    {
        string GetProductName(string productId);
        StoredProductType GetProductType(string productId);
    }

    public enum CommentVia : int
    {
        cluster,
        YouTube
    };
    public interface ICommentHandler
    {
        IReadOnlyList<Comment.Source> getLatestComments(int count);
        void SetCommentReceivedCallback(ulong itemId, IJintCallback<IReadOnlyList<Comment.Source>> Callback);
        void RemoveCommentReceivedCallback(ulong itemId);
    }

    public interface IVoiceHandler
    {
        void SetVoiceVolumeRate(float rate);
    }

    public interface ILoggingOptions
    {
        bool loggingOnReceiveFail { get; }
    }
}
