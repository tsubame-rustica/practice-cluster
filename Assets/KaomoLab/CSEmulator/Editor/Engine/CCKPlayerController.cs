using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using ClusterVR.CreatorKit.Preview.PlayerController;
using ClusterVR.CreatorKit.Editor.Preview.World;
using ICckPlayerController = ClusterVR.CreatorKit.Preview.PlayerController.IPlayerController;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class CCKPlayerController
        : EmulateClasses.IPlayerController, EmulateClasses.IPlayerTransformController
    {
        readonly Components.CSEmulatorPlayerHandler csPlayerHandler;
        readonly Components.CSEmulatorPlayerController csPlayerController;
        readonly ICckPlayerController playerController;
        readonly IPlayerViewOptions playerOptions;
        readonly HumanPoseManager humanPoseManager;
        readonly PointOfViewManager pointOfViewManager;
        readonly FaceConstraintManager faceConstraintManager;
        readonly SpawnPointManager spawnPointManager;

        public string id => csPlayerHandler.id;

        public Transform transform => playerController.PlayerTransform;

        //playerのswapn機能を追加して消去機能まで追加したらfalseにするようにする。
        public bool exists => true;

        public float jumpSpeedRate
        {
            set => playerController.SetJumpSpeedRate(value);
        }
        public float moveSpeedRate
        {
            set => playerController.SetMoveSpeedRate(value);
        }

        public float gravity
        {
            get => csPlayerController.gravity;
            set => csPlayerController.gravity = value;
        }

        public int movementFlags => csPlayerController.GetMovementFlags();

        public bool isFirstPersonView => playerOptions.isFirstPersonView;
        bool? firstPersonViewResetTo = null;

        public PermissionType permissionType = PermissionType.Audience;

        public CCKPlayerController(
            Components.CSEmulatorPlayerHandler csPlayerHandler,
            Components.CSEmulatorPlayerController csPlayerController,
            ICckPlayerController playerController,
            IPlayerViewOptions playerOptions,
            PointOfViewManager pointOfViewManager,
            HumanPoseManager humanPoseManager,
            FaceConstraintManager faceConstraintManager,
            SpawnPointManager spawnPointManager
        )
        {
            this.csPlayerHandler = csPlayerHandler;
            this.csPlayerController = csPlayerController;
            this.playerController = playerController;
            this.playerOptions = playerOptions;
            this.humanPoseManager = humanPoseManager;
            this.pointOfViewManager = pointOfViewManager;
            this.faceConstraintManager = faceConstraintManager;
            this.spawnPointManager = spawnPointManager;
        }

        public void Respawn()
        {
            //AudienceかPerformerかを変えるのが必要。
            var spawnPoint = spawnPointManager.GetRespawnPoint(permissionType);
            playerController.WarpTo(spawnPoint.Position);

            //PlayerPresenterがPlayerが一人のみ設計のようなのでコピペして引き取り。
            var yawOnlyRotation = Quaternion.Euler(0f, spawnPoint.YRotation, 0f);
            playerController.SetRotationKeepingHeadPitch(yawOnlyRotation);
            playerController.ResetCameraRotation(yawOnlyRotation);

            SetRotation(yawOnlyRotation);
            //ここの処理はCSEmulatorDummyPlayerにコピペしている。編集した場合はそちらも対応。
        }

        public void AddVelocity(Vector3 velocity)
        {
            csPlayerController.AddVelocity(velocity);
        }

        public void SetPosition(Vector3 position)
        {
            playerController.WarpTo(position);
        }

        public void SetRotation(Quaternion rotation)
        {
            csPlayerController.ForceForward();
            playerController.SetRotationKeepingHeadPitch(rotation);
            csPlayerController.ForceRotate(rotation.eulerAngles.y);
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }
        public Quaternion GetRotation()
        {
            var ret = csPlayerController.GetPlayerRotation();
            return ret;
        }

        public void SetPersonViewChangeable(bool canChange)
        {
            playerOptions.SetPersonViewChangeable(canChange);
        }
        public Vector3 GetCameraPosition()
        {
            return pointOfViewManager.GetCameraPosition();
        }
        public Quaternion GetCameraRotation()
        {
            return pointOfViewManager.GetCameraRotation();
        }
        public void SetCameraPosition(Vector3? position)
        {
            if(position == null)
            {
                playerOptions.SetPersonViewChangeable(true);
                if(firstPersonViewResetTo != null)
                {
                    playerOptions.isFirstPersonView = firstPersonViewResetTo.Value;
                    firstPersonViewResetTo = null;
                }
                pointOfViewManager.ClearCameraPosition();
            }
            else
            {
                if (firstPersonViewResetTo == null)
                {
                    //人称が切り替える前に保存する
                    firstPersonViewResetTo = playerOptions.isFirstPersonView;
                }

                playerOptions.SetPersonViewChangeable(false);
                if (playerOptions.isFirstPersonView)
                {
                    //カメラの回転はそのまま
                    playerOptions.isFirstPersonView = false;
                    pointOfViewManager.SetCameraPosition(position.Value);
                }
                else
                {
                    //カメラをアバター正面側に強制的に向ける。Yのみ。
                    pointOfViewManager.SetCameraPosition(position.Value);
                    var roy = OnlyY(csPlayerController.GetPlayerRotation());
                    playerController.ResetCameraRotation(roy);
                }
            }
        }
        public void SetCameraRotation(Quaternion? rotation)
        {
            if (rotation == null)
            {
                pointOfViewManager.ClearCameraRotation();
            }
            else
            {
                var rnz = playerOptions.isFirstPersonView ? RemoveZ(rotation.Value) : rotation.Value;
                pointOfViewManager.SetCameraRotation(rnz);
                var roy = OnlyY(rotation.Value);
                playerController.SetRotationKeepingHeadPitch(roy);
            }
        }
        Quaternion RemoveZ(Quaternion? rotation)
        {
            var r = rotation.Value.eulerAngles;
            return Quaternion.Euler(r.x, r.y, 0);
        }
        Quaternion OnlyY(Quaternion? rotation)
        {
            var r = rotation.Value.eulerAngles;
            return Quaternion.Euler(0, r.y, 0);
        }

        public void SetCameraFieldOfViewTemporary(float value)
        {
            pointOfViewManager.SetCameraFieldOfViewTemporary(value);
        }
        public void SetCameraFieldOfView(float value)
        {
            pointOfViewManager.SetCameraFieldOfView(value);
        }
        public float GetCameraFieldOfViewNow()
        {
            return pointOfViewManager.GetCameraFieldOfViewNow();
        }
        public float GetCameraFieldOfView()
        {
            return pointOfViewManager.GetCameraFieldOfView();
        }

        public void SetThirdPersonCameraDistanceTemporary(float value)
        {
            pointOfViewManager.SetThirdPersonCameraDistanceTemporary(value);
        }
        public float GetThirdPersonCameraDistanceNow()
        {
            return pointOfViewManager.GetThirdPersonCameraDistanceNow();
        }
        public float GetThirdPersonCameraDistanceDefault()
        {
            return pointOfViewManager.GetThirdPersonCameraDistanceDefault();
        }

        public void SetThirdPersonCameraScreenPosition(Vector2 pos)
        {
            pointOfViewManager.SetThirdPersonCameraScreenPosition(pos);
        }
        public Vector2 GetThirdPersonCameraScreenPositionNow()
        {
            return pointOfViewManager.GetThirdPersonCameraScreenPositionNow();
        }

        public Transform GetFirstCameraTransform()
        {
            return pointOfViewManager.GetFirstPersonCameraTransform();
        }

        public void SetHumanPosition(Vector3? position)
        {
            humanPoseManager.SetPosition(position);
        }

        public void SetHumanRotation(Quaternion? rotation)
        {
            humanPoseManager.SetRotation(rotation);
        }

        public void SetHumanMuscles(float[] muscles, bool[] hasMascles)
        {
            humanPoseManager.SetMuscles(muscles, hasMascles);
        }

        public void InvalidateHumanMuscles()
        {
            humanPoseManager.InvalidateMuscles();
        }

        public void SetHumanTransition(double timeoutSeconds, double timeoutTransitionSeconds, double transitionSeconds)
        {
            humanPoseManager.SetHumanTransition(timeoutSeconds, timeoutTransitionSeconds, transitionSeconds);
        }

        public void InvalidateHumanTransition()
        {
            humanPoseManager.InvalidateHumanTransition();
        }

        public HumanPose GetHumanPose()
        {
            return humanPoseManager.GetHumanPose();
        }

        public Transform GetBoneTransform(UnityEngine.HumanBodyBones bone)
        {
            return csPlayerController.GetBoneTransform(bone);
        }

        public void MergeHumanPoseOnFrame(UnityEngine.Vector3? position, UnityEngine.Quaternion? rotation, float[] muscles, bool[] hasMascles, float weight)
        {
            humanPoseManager.MergeHumanPoseOnFrame(
                position, rotation, muscles, hasMascles, weight
            );
        }

        public void OverwriteHumanoidBoneRotation(HumanBodyBones bone, Quaternion rotation)
        {
            var transform = csPlayerController.GetBoneTransform(bone);
            if (transform == null) return;
            humanPoseManager.OverwriteHumanoidBoneRotation(transform, rotation);
        }

        public void ChangeGrabbing(bool isGrab)
        {
            csPlayerController.ChangeGrabbing(isGrab);
        }
        public void OverwriteFaceConstraint(bool? forward)
        {
            faceConstraintManager.OverwriteFaceConstraint(forward);
        }

        public void RunCoroutine(Func<System.Collections.IEnumerator> Coroutine)
        {
            csPlayerController.RunCoroutine(Coroutine);
        }

    }
}
