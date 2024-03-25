using UnityEngine;
using UnityEngine.InputSystem;

namespace Ellyality.Runtime
{
    /// <summary>
    /// Build-In third-Person character controller <br />
    /// Have basic moving, looking jump, squat action
    /// <code>
    /// // You can create an custom character by inherit it
    /// public class CustomCharacter : TPSCharacter
    /// </code>
    /// </summary>
    [AddComponentMenu("Ellyality/Camera/Third-Person Character")]
    [BuiltInScript]
    public class TPSCharacter : CharacterBase
    {
        #region Field
        /// <summary>
        /// Camera look at self transform position with offset <br />
        /// Offset will apply current camera quaternion
        /// </summary>
        [Header("Setting")]
        [SerializeField] [Tooltip("Camera will look at character center with local offset")] protected Vector2 ViewOffset = new Vector2(0, 0);
        /// <summary>
        /// Character moving speed
        /// </summary>
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
        /// <summary>
        /// Distance between camera and character
        /// </summary>
        [SerializeField] [Tooltip("If there is nothing collide, how long does the distance between character and camera")] protected float CameraDistance = 8f;
        /// <summary>
        /// Camera hitting collision flag, Depend on the hit object, it will shorter the view distance
        /// </summary>
        [SerializeField] [Tooltip("Camera hitting flag, Depend on the hit object, it will shorter the view distance")] protected LayerMask HitLayer = new LayerMask();
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
        /// <summary>
        /// Virtual quaternion that repersent character to camera rotation
        /// </summary>
        protected Quaternion cam = Quaternion.identity;
        #endregion

        #region Override
        public override void Start()
        {
            base.Start();
            _CamMovingDest = Vector2.zero;
            _CamRiggingRotationDest = Vector2.zero;
        }
        /// <summary>
        /// Execute looking action
        /// </summary>
        public override void Update()
        {
            VibrationUpdate();
            ThridPersonCameraRiggingPositionSmoothDamp();
            CameraRiggingRotationSmoothDamp();
            CameraPositionSmoothDamp();
            CameraRotationSmoothDamp();
            CameraLookAtForView(ViewOffset);
            CameraPosition();
        }
        /// <summary>
        /// Execute moving action
        /// </summary>
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (movingDir != Vector3.zero && Controller.CanMove) MoveAction(movingDir);
        }
        /// <summary>
        /// When switch happen, ignore source camera center rotation <br />
        /// Change to zero, Because we only rotate camera <br />
        /// And camera center only care about its position
        /// </summary>
        public override void SwitchAction(CharacterBase source)
        {
            base.SwitchAction(source);
            _CamRiggingRotationDest = Vector3.zero;
        }
        #endregion

        #region Custom Camera Update
        protected void ThridPersonCameraRiggingPositionSmoothDamp()
        {
            if (Controller.CamRiggingMoveSmoothSpeed == 0f)
            {
                _CameraRigging.localPosition = _CamRiggingMovingDest;
            }
            else
            {
                _CameraRigging.localPosition = Vector3.SmoothDamp(_CameraRigging.localPosition, _CamRiggingMovingDest, ref _CamRiggingMovingVelocity, Controller.CamRiggingMoveSmoothSpeed);
            }
            Vector3 vector = _CameraRigging.localPosition.normalized;
            _CameraRigging.localPosition = CameraDistance * vector;
        }
        /// <summary>
        /// Move the camera center to the right position and apply with <seealso cref="lookDir"/>
        /// </summary>
        protected virtual void CameraPosition()
        {
            if (Controller.CanSee)
            {
                Vector3 og = cam.eulerAngles;
                og.x = MathE.RotationAngle360ToPositionNegative180(og.x);
                float anglex = og.x + lookDir.y;
                float UpperLimit = 90f - DegreeBounds;
                float LowerLimit = -90f + DegreeBounds;
                if (anglex > UpperLimit) og.x = UpperLimit;
                else if (anglex < LowerLimit) og.x = LowerLimit;
                else if (anglex > LowerLimit && anglex < UpperLimit) og.x = anglex;
                og.y += lookDir.x;
                cam.eulerAngles = og;
            }

            _CamRiggingMovingDest = GetCameraRayCastingPositionResult();
        }

        /// <summary>
        /// Let character facing camera's forward <br />
        /// Zero the camera rotation y <br />
        /// Then adding to character y axis
        /// </summary>
        protected void CameraYAxisApplyToCharacter()
        {
            float localRiggingYAxis = cam.eulerAngles.y;
            cam.eulerAngles = new Vector3(cam.eulerAngles.x, 0, cam.eulerAngles.z);
            Vector3 buffer = transform.localEulerAngles;
            buffer.y += localRiggingYAxis;
            transform.localEulerAngles = buffer;
            _CameraRigging.transform.localPosition = GetCameraRayCastingPositionResult();
        }

        protected Vector3 GetCameraRayCastingPositionResult()
        {
            Ray ray = new Ray(transform.position, transform.TransformDirection(cam * Vector3.back));
            RaycastHit[] hits = Physics.RaycastAll(ray, CameraDistance, HitLayer);
            float shortest = CameraDistance;
            foreach (var i in hits)
            {
                shortest = Mathf.Min(Vector3.Distance(transform.position, i.point), shortest);
            }
            return transform.InverseTransformPoint(ray.GetPoint(Mathf.Max(1f, shortest - 0.5f)));
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
            Vector3 result = MoveVector(new Vector3(dir.x, 0, dir.y), MoveSpeed, MoveSpeed);
            movingDir.x = result.x;
            movingDir.z = result.z;
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
        public virtual void MoveAction(Vector3 dir)
        {
            SimpleMove(dir, _Camera.transform);
            CameraYAxisApplyToCharacter();
        }
        #endregion
    }
}
