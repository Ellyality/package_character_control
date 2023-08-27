using UnityEngine;
using UnityEngine.InputSystem;

namespace Elly.Runtime
{
    /// <summary>
    /// Build-In platform character controller <br />
    /// Have basic moving, looking jump, squat action
    /// <code>
    /// // You can create an custom character by inherit it
    /// public class CustomCharacter : PlatformCharacter
    /// </code>
    /// </summary>
    [AddComponentMenu("Elly/Camera/Platform Character")]
    [BuiltInScript]
    public class PlatformCharacter : CharacterBase
    {
        /// <summary>
        /// Platform facing direction
        /// </summary>
        public enum Direction
        {
            /// <summary>
            /// camera is at character's +x direction
            /// </summary>
            PositionX,
            /// <summary>
            /// camera is at character's +z direction
            /// </summary>
            PositionZ,
            /// <summary>
            /// camera is at character's -x direction
            /// </summary>
            NegativeX,
            /// <summary>
            /// camera is at character's -z direction
            /// </summary>
            NegativeZ
        }

        /// <summary>
        /// The distance between camera and character
        /// </summary>
        [SerializeField] [Tooltip("Offset camera distance")] protected float Distance = 5f;
        [SerializeField] [Tooltip("Facing direction")] protected Direction OffsetDirection = Direction.NegativeZ;
        [SerializeField] [Tooltip("Moving speed will multiply by delta time")] protected float MoveSpeed = 5f;
        [SerializeField] [Tooltip("Mouse rotation speed, aka mouse sensitivity")] protected float ViewDegree = 5f;

        #region Local Variable
        protected Vector2 movingDir = Vector2.zero;
        protected Vector2 lookDir = Vector2.zero;
        #endregion

        #region Override
        /// <summary>
        /// Execute looking, camera position update action
        /// </summary>
        public override void Update()
        {
            base.Update();
            CameraPositionUpdate();
            LookAction(lookDir);
        }
        /// <summary>
        /// Execute moving action
        /// </summary>
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (movingDir != Vector2.zero && Controller.CanMove) MoveAction(movingDir);
        }

        public override Transform CameraRiggingRoot()
        {
            return transform.parent;
        }
        #endregion

        #region Custom Camera Update
        protected void CameraPositionUpdate()
        {
            Vector3 pos = transform.localPosition;
            _CamRiggingMovingDest = pos;
            switch (OffsetDirection)
            {
                case Direction.PositionX:
                    _CamRiggingMovingDest.x = pos.x + Distance;
                    break;
                case Direction.PositionZ:
                    _CamRiggingMovingDest.z = pos.z + Distance;
                    break;
                case Direction.NegativeX:
                    _CamRiggingMovingDest.x = pos.x + (-Distance);
                    break;
                case Direction.NegativeZ:
                    _CamRiggingMovingDest.z = pos.z + (-Distance);
                    break;
            }
        }
        #endregion

        #region Input Event
        /// <summary>
        /// Apply vector 2D data to <seealso cref="lookDir"/>
        /// </summary>
        public virtual void OnLook(InputValue context)
        {
            Vector2 dir = context.Get<Vector2>();
            lookDir = dir.normalized * ViewDegree;
            lookDir.y *= Controller.InverseY ? -1f : 1f;
        }
        /// <summary>
        /// Apply vector 2D data to <seealso cref="movingDir"/>
        /// </summary>
        public virtual void OnMove(InputValue context)
        {
            Vector2 dir = context.Get<Vector2>();
            Vector3 result = MoveVector(new Vector3(dir.x, 0, dir.y), MoveSpeed, MoveSpeed);
            movingDir.x = result.x;
            movingDir.y = result.z;
        }
        #endregion

        #region Execute
        private void LookAction(Vector2 dir)
        {
            Vector2 degree = Controller.CanSee ? new Vector2(Mathf.Clamp(dir.y, -40f, 40f), Mathf.Clamp(dir.x, -40f, 40f)) : Vector2.zero;
            switch (OffsetDirection)
            {
                case Direction.PositionX:
                    _CamRotationDest = new Vector2(0, -90) + degree;
                    break;
                case Direction.PositionZ:
                    _CamRotationDest = new Vector2(0, 180) + degree;
                    break;
                case Direction.NegativeX:
                    _CamRotationDest = new Vector2(0, 90) + degree;
                    break;
                case Direction.NegativeZ:
                    _CamRotationDest = new Vector2(0, 0) + degree;
                    break;
            }
        }
        private void MoveAction(Vector3 dir)
        {
            switch (OffsetDirection)
            {
                case Direction.PositionX:
                    SimpleMove(new Vector3(0, dir.y, dir.x));
                    break;
                case Direction.PositionZ:
                    SimpleMove(new Vector3(-dir.x, dir.y, 0));
                    break;
                case Direction.NegativeX:
                    SimpleMove(new Vector3(0, dir.y, -dir.x));
                    break;
                case Direction.NegativeZ:
                    SimpleMove(new Vector3(dir.x, dir.y, 0));
                    break;
            }
        }
        #endregion
    }
}
