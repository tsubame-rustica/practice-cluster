using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using ICckPlayerController = ClusterVR.CreatorKit.Preview.PlayerController.IPlayerController;
using ICckMoveInputController = ClusterVR.CreatorKit.Preview.PlayerController.IMoveInputController;
using CckCameraControlSettings = ClusterVR.CreatorKit.Preview.PlayerController.CameraControlSettings;
using ClusterVR.CreatorKit.Item.Implements;

namespace Assets.KaomoLab.CSEmulator.Components
{
    //ClusterVR.CreatorKit.Preview.PlayerController.DesktopPlayerControllerのコピー
    [DisallowMultipleComponent, AddComponentMenu(""),
        RequireComponent(typeof(CharacterController))
    ]
    public class CSEmulatorDummyPlayerController
        : MonoBehaviour,
        ICckPlayerController,
        //以下DesktopPlayerControllerReflector分
        Components.IVelocityYHolder,
        Components.IBaseMoveSpeedHolder,
        Components.IPlayerRotateHandler,
        Components.IRidingHolder,
        Components.IMouseEventEmitter
    {
        //マルチ操作対応する時にこのあたりを実装して切り替えるという目論見。
        public interface IDesktopPointerEventListener
        {
            event Action<Vector2> OnMoved;
            event Action<Vector2> OnClicked;
        }
        public class DummyDesktopPointerEventListener
            : IDesktopPointerEventListener
        {
            public event Action<Vector2> OnMoved;
            public event Action<Vector2> OnClicked;
        }
        public class DummyMoveInputController
            : ICckMoveInputController
        {
            public Vector2 MoveDirection => Vector2.zero;
            public float AdditionalAxis => 0;
            public bool RideOffButtonPressed => false;
            public bool IsJumpButtonDown => false;
            public event Action<Vector2> OnMoveDirectionChanged;
            public event Action<float> OnAdditionalAxisChanged;
        }

        //DesktopPlayerControllerReflector分
        float IVelocityYHolder.value { get => velocityY; set => velocityY = value; }
        bool IRidingHolder.isRiding => IsRiding;
        RidableItem IRidingHolder.ridableItem => (RidableItem)ridingItem;
        Transform IPlayerRotateHandler.rootTransform => rootTransform;
        float IBaseMoveSpeedHolder.value { get => baseMoveSpeed; set => baseMoveSpeed = value; }
        event Action<Vector2> IMouseEventEmitter.OnMoved
        {
            add => desktopPointerEventListener.OnMoved += value;
            remove => desktopPointerEventListener.OnMoved -= value;
        }
        event Action<Vector2> IMouseEventEmitter.OnClicked
        {
            add => desktopPointerEventListener.OnClicked += value;
            remove => desktopPointerEventListener.OnClicked -= value;
        }



        const float MaxHeadPitch = 80f;
        const float MinHeadPitch = -80f;
        const float MaxHeadYaw = 90f;

        Transform rootTransform;
        public GameObject avaterParent { get; private set; }
        public GameObject vrm { get; private set; }
        Transform cameraTransform;
        CharacterController characterController;
        IDesktopPointerEventListener desktopPointerEventListener;
        ICckMoveInputController desktopMoveInputController;
        float baseMoveSpeed = 2.5f; //PreviewOnly参照
        float baseJumpSpeed = 4f; //PreviewOnly参照
        float velocityY;
        float moveSpeedRate = 1f;
        float jumpSpeedRate = 1f;

        ClusterVR.CreatorKit.Item.IRidableItem ridingItem;
        bool prevIsRiding;

        public Transform PlayerTransform => characterController.transform;
        public Quaternion RootRotation => rootTransform.rotation;
        public Transform CameraTransform => cameraTransform;


        public void Construct(
            GameObject vrmPrefab,
            CharacterController characterController,
            IDesktopPointerEventListener desktopPointerEventListener,
            ICckMoveInputController desktopMoveInputController
        )
        {
            //PreviewOnly参照
            var root = new GameObject("Root");
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;
            root.transform.SetParent(transform, false);
            this.rootTransform = root.transform;

            var camera = new GameObject("MainCamera");
            camera.transform.position = new Vector3(0, 1.6f, 0);
            camera.transform.rotation = Quaternion.identity;
            camera.transform.SetParent(root.transform, false);
            this.cameraTransform = camera.transform;
            //ContactableItemRaycasterなどの問題で、直近では操作不可能と判断。
            //操作を検討する段階になったらこの辺なんとかする。
            var c = camera.AddComponent<Camera>();
            c.enabled = false;
            camera.AddComponent<UnityEngine.Rendering.PostProcessing.PostProcessLayer>();
            var vector = new GameObject("Vector");
            vector.transform.SetParent(camera.transform, false);

            //Rootと並列。名称はVrmPrepareと一致させている。
            this.avaterParent = new GameObject("CSEmulatorVrmYRotation");
            this.avaterParent.transform.position = Vector3.zero;
            this.avaterParent.transform.rotation = Quaternion.identity;
            this.avaterParent.transform.SetParent(transform, false);

            this.vrm = GameObject.Instantiate(vrmPrefab);
            this.vrm.transform.position = Vector3.zero;
            this.vrm.transform.rotation = Quaternion.identity;
            this.vrm.transform.SetParent(avaterParent.transform, false);

            this.characterController = characterController;
            this.desktopPointerEventListener = desktopPointerEventListener;
            this.desktopMoveInputController = desktopMoveInputController;
        }

        public void WarpTo(Vector3 position)
        {
            characterController.enabled = false;
            characterController.transform.position = position;
            characterController.enabled = true;
        }

        void ICckPlayerController.SetMoveSpeedRate(float moveSpeedRate)
        {
            this.moveSpeedRate = moveSpeedRate;
        }

        void ICckPlayerController.SetJumpSpeedRate(float jumpSpeedRate)
        {
            this.jumpSpeedRate = jumpSpeedRate;
        }

        void ICckPlayerController.SetRidingItem(ClusterVR.CreatorKit.Item.IRidableItem ridingItem)
        {
            this.ridingItem = ridingItem;
        }

        void ICckPlayerController.SetRotationKeepingHeadPitch(Quaternion rotation)
        {
            var rootRotation = rootTransform.rotation;
            var cameraRotation = cameraTransform.rotation;
            var cameraLocalPitch = (Quaternion.Inverse(rootRotation) * cameraRotation).eulerAngles.x;
            rootTransform.rotation = rotation;
            cameraTransform.rotation = rotation * ClampPitch(new Vector3(cameraLocalPitch, 0f, 0f));
        }

        void ICckPlayerController.ResetCameraRotation(Quaternion rotation)
        {
            SetCameraRotation(rotation);
        }

        void IPlayerRotateHandler.RotateDelta(Vector2 delta)
        {
            Rotate(delta);
        }

        void SetCameraRotation(Quaternion rotation)
        {
            var rootRotation = rootTransform.rotation;
            var cameraLocalRotation = Quaternion.Inverse(rootRotation) * rotation;
            cameraLocalRotation = ClampAsHeadAngle(cameraLocalRotation.eulerAngles);
            var res = rootRotation * cameraLocalRotation;
            cameraTransform.rotation = res;
        }

        void Start()
        {
            desktopPointerEventListener.OnMoved += Rotate;
            //#if STEAMAUDIO_ENABLED
            //                SteamAudio.SteamAudioManager.NotifyAudioListenerChanged();
            //#endif
        }

        void Update()
        {
            var isRiding = IsRiding;
            if (isRiding)
            {
                SetEyeHeight(CckCameraControlSettings.SittingEyeHeight);
            }
            else
            {
                SetEyeHeight(CckCameraControlSettings.StandingEyeHeight);
                if (prevIsRiding)
                {
                    RestoreRotation();
                }
                Move();
            }

            prevIsRiding = isRiding;
        }

        void LateUpdate()
        {
            if (TryGetRidingSeat(out var seat))
            {
                PlayerTransform.position = seat.position;
                rootTransform.rotation = seat.rotation;
            }
        }

        void SetEyeHeight(float eyeHeight)
        {
            cameraTransform.localPosition = new Vector3(0f, eyeHeight, 0f);
        }

        void Move()
        {
            var moveDirection = desktopMoveInputController.MoveDirection;
            var direction = new Vector3(moveDirection.x, 0, moveDirection.y);
            direction.Normalize();
            direction = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0) * direction;

            var moveSpeed = baseMoveSpeed * moveSpeedRate;
            var velocity = new Vector3(direction.x * moveSpeed, velocityY, direction.z * moveSpeed);
            characterController.Move(velocity * Time.deltaTime);

            if (characterController.isGrounded)
            {
                if (desktopMoveInputController.IsJumpButtonDown)
                {
                    velocityY = baseJumpSpeed * jumpSpeedRate;
                }
                else
                {
                    velocityY = 0f;
                }
            }

            velocityY -= Time.deltaTime * 9.81f;
        }

        void Rotate(Vector2 delta)
        {
            var rootRotation = rootTransform.rotation;
            var cameraLocalRotation = Quaternion.Inverse(rootRotation) * cameraTransform.rotation;
            var euler = cameraLocalRotation.eulerAngles;
            delta *= 120;
            cameraLocalRotation = ClampPitch(new Vector3(euler.x - delta.y, euler.y + delta.x, 0f));
            SetRotation(rootRotation * cameraLocalRotation);
        }

        Quaternion ClampPitch(Vector3 eulerAngles)
        {
            return Quaternion.Euler(ClampAngle(eulerAngles.x, MinHeadPitch, MaxHeadPitch), eulerAngles.y, 0f);
        }

        Quaternion ClampAsHeadAngle(Vector3 eulerAngles)
        {
            return Quaternion.Euler(ClampAngle(eulerAngles.x, MinHeadPitch, MaxHeadPitch), ClampAngle(eulerAngles.y, -MaxHeadYaw, MaxHeadYaw), 0f);
        }

        void SetRotation(Quaternion rotation)
        {
            if (IsRiding)
            {
                SetCameraRotation(rotation);
            }
            else
            {
                var onlyYawRotation = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
                rootTransform.rotation = onlyYawRotation;
                cameraTransform.rotation = rotation;
            }
        }

        void RestoreRotation()
        {
            var rootRotation = rootTransform.rotation;
            var cameraRotation = cameraTransform.rotation;
            var cameraLocalPitch = (Quaternion.Inverse(rootRotation) * cameraRotation).eulerAngles.x;
            var cameraGlobalYaw = cameraRotation.eulerAngles.y;
            rootRotation = Quaternion.Euler(0f, cameraGlobalYaw, 0f);
            rootTransform.rotation = rootRotation;
            cameraTransform.rotation = rootRotation * ClampPitch(new Vector3(cameraLocalPitch, 0f, 0f));
        }

        bool IsRiding => TryGetRidingSeat(out _);

        bool TryGetRidingSeat(out Transform seat)
        {
            if (ridingItem == null)
            {
                seat = default;
                return false;
            }
            else
            {
                seat = ridingItem.Seat;
                return seat != null;
            }
        }

        static float ClampAngle(float angle, float min, float max)
        {
            angle += 180;
            angle = Mathf.Repeat(angle, 360);
            angle -= 180;
            angle = Mathf.Clamp(angle, min, max);

            return angle;
        }
    }
}
