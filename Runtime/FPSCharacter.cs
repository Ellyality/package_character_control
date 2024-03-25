﻿using UnityEngine;
using UnityEngine.InputSystem;

namespace Ellyality.Runtime
{
    /// <summary>
    /// Build-In first-Person character controller <br />
    /// Have basic moving, looking jump, squat action
    /// <code>
    /// // You can create an custom character by inherit it
    /// public class CustomCharacter : FPSCharacter
    /// </code>
    /// </summary>
    [AddComponentMenu("Ellyality/Camera/First-Person Character")]
    [BuiltInScript]
    public class FPSCharacter : CharacterBase
    {
        #region Field
        /// <summary>
        /// Camera offset control <seealso cref="_Camera"/> local position
        /// </summary>
        [Header("Setting")]
        [SerializeField] [Tooltip("Camera offset position")] protected Vector3 CamOffset = new Vector3(0, 0, 1);
        /// <summary>
        /// Character moving speed
        /// </summary>
        [Space]
        [SerializeField] [Tooltip("Moving speed will multiply by delta time")] protected float MoveSpeed = 5f;
        /// <summary>
        /// Character looking speed
        /// </summary>
        [SerializeField] [Tooltip("Mouse rotation speed, aka mouse sensitivity")] protected float ViewSpeed = 5f;
        /// <summary>
        /// Degree of free is control character y rotation tolerance degree <br />
        /// Lower limit: 90 - value, Upper limit: -90 + value
        /// </summary>
        [SerializeField] [Range(1f, 30f)] [Tooltip("Mouse rotation up down limit degree" +
            "\nLower limit: 90 - value, Upper limit: -90 + value" +
            "\nThis value will prevent player rotate over 90 degree causing bug happen")] protected float DegreeBounds = 1f;
        #endregion

        #region Local Variable
        /// <summary>
        /// Input moving data <br />
        /// In <seealso cref="Update"/>, it will apply this to change character position
        /// </summary>
        protected Vector3 movingDir = Vector3.zero;
        /// <summary>
        /// Input looking data <br />
        /// In <seealso cref="Update"/>, it will apply this to change character camera rotation
        /// </summary>
        protected Vector2 lookDir = Vector2.zero;
        /// <summary>
        /// Input jump data <br />
        /// </summary>
        protected bool jump = false;
        /// <summary>
        /// Input squat data <br />
        /// </summary>
        protected bool squat = false;
        #endregion

        #region Override
        public override void Start()
        {
            base.Start();
            _CamMovingDest = Vector3.zero;
            _CamRotationDest = Vector3.zero;
            _CamRiggingRotationDest = new Vector2(_CamRiggingRotationDest.x, 0);
        }
        /// <summary>
        /// Execute moving, looking action
        /// </summary>
        public override void Update()
        {
            base.Update();
            _CamRiggingMovingDest = CamOffset;
            if (lookDir != Vector2.zero && Controller.CanSee) LookAction(lookDir);
        }
        public override void FixedUpdate()
        {
            if (movingDir != Vector3.zero && Controller.CanMove) MoveAction(movingDir);
        }
        /// <summary>
        /// When switch happen, ignore source camera rotation <br />
        /// Change to zero, Because we only rotate camera center
        /// </summary>
        public override void SwitchAction(CharacterBase source)
        {
            base.SwitchAction(source);
            _CamRotationDest = Vector3.zero;
        }
        #endregion

        #region Input Event
        /// <summary>
        /// Apply vector 2D data to <seealso cref="lookDir"/>
        /// </summary>
        public virtual void OnLook(InputValue context)
        {
            Vector2 dir = context.Get<Vector2>();
            lookDir = LookVector(dir, ViewSpeed);
        }
        /// <summary>
        /// Apply vector 2D data to <seealso cref="movingDir"/>
        /// </summary>
        public virtual void OnMove(InputValue context)
        {
            Vector2 dir = context.Get<Vector2>();
            movingDir = MoveVector(new Vector3(dir.x, 0, dir.y), MoveSpeed, MoveSpeed);
        }
        /// <summary>
        /// Apply squat bool to <seealso cref="squat"/>
        /// </summary>
        public virtual void OnSquat(InputValue context)
        {
            squat = context.isPressed;
        }
        /// <summary>
        /// Apply squat bool to <seealso cref="jump"/>
        /// </summary>
        public virtual void OnJump(InputValue context)
        {
            jump = context.isPressed;
        }
        #endregion

        #region Execute
        protected virtual void LookAction(Vector2 dir)
        {
            CameraCenterRotateForView(DegreeBounds, dir);
        }
        protected virtual void MoveAction(Vector3 dir)
        {
            SimpleMove(dir);
        }
        #endregion
    }
}
