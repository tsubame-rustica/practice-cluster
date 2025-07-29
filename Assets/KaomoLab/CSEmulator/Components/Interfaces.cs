using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Components
{
    public interface IVelocityYHolder
    {
        public float value { get; set; }
    }
    public interface IBaseMoveSpeedHolder
    {
        public float value { get; set; }
    }
    public interface IPlayerRotateHandler
    {
        public Transform rootTransform { get; }
        public void RotateDelta(Vector2 delta);
    }
    public interface IRidingHolder
    {
        public bool isRiding { get; }
        public ClusterVR.CreatorKit.Item.Implements.RidableItem ridableItem { get; } //このレイヤーのIFでCCKに依存してよい？
    }
    public interface IMouseEventEmitter
    {
        public event Action<Vector2> OnMoved;
        public event Action<Vector2> OnClicked;
    }
    public interface IPointEventEmitter
    {
        public event Action<Vector2> OnDown;
        public event Action<Vector2> OnUp;
    }
    public interface IPerspectiveChangeNotifier
    {
        event Handler<bool> OnChanged;
        void RequestNotify();
    }
    public interface IPlayerMeasurementsHolder
    {
        public float height { get; }
        public float radius { get; }
    }
    public interface IGrabController
    {
        bool isGrab { get; }
        Vector3 grabPoint { get; }
        void ApplyUpdate();
    }
    public interface IPlayerFaceController
    {
        Transform vrmRotateRoot { get; }
        float GetNowRotate();
        void SetBaseRotate(float degree);
        void SetFaceForward(int direction);
        void SetFaceRight(int direction);
    }
    public interface IVrmIKNotifier
    {
        event Action<int> OnIK;
    }
    public interface IPlayerMeta
    {
        string userIdfc { get; }
        string userId { get; }
        string userDisplayName { get; }
        EventRole eventRole { get; }
        bool exists { get; }
        bool isAndroid { get; }
        bool isDesktop { get; }
        bool isIos { get; }
        bool isMacOs { get; }
        bool isMobile { get; }
        bool isVr { get; }
        bool isWindows { get; }
    }
    public interface IControlActivator
    {
        event Handler<bool> OnActivated;
        void RequestNotify();
    }
    //ClusterVRのEditor系からのコピペ。なんとかならんのか
    public enum PermissionType
    {
        Performer,
        Audience
    }
    public readonly struct SpawnPoint
    {
        public readonly Vector3 position;
        public readonly float yRotation;

        public SpawnPoint(Vector3 position, float yRotation)
        {
            this.position = position;
            this.yRotation = yRotation;
        }
    }
    public interface ISpawnPointHolder
    {
        SpawnPoint GetSpawnPoint(PermissionType permissionType);
    }
    public interface ISubAudioController
    {
        event Action<ISubAudioFactory> OnSubAudioStarted;
        event Action OnSubAudioEnded;
    }
    public interface ISubAudioFactory
    {
        AudioClip GetReuseClip(ClusterVR.CreatorKit.World.SpeakerType speakerType);
        AudioClip CreateClip(ClusterVR.CreatorKit.World.SpeakerType speakerType);
    }

    public interface IShutdownNotifier
    {
        event Handler OnShutdown;
    }

    public interface IVariablesStore
    {
        IEnumerable<IVariable> GetVariables();
        event Action OnVariablesUpdated;
    }
    public interface IVariable
    {
        string name { get; }
        string value { get; }
        string type { get; }
        bool hasChild { get; }
        IEnumerable<IVariable> children { get; }
    }
}
