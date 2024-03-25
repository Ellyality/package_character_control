using UnityEngine;
using UnityEngine.InputSystem;

namespace Ellyality.Runtime
{
    /// <summary>
    /// Build-In top-down character controller <br />
    /// Have basic moving, looking jump, squat action
    /// <code>
    /// // You can create an custom character by inherit it
    /// public class CustomCharacter : TopdownCharacter
    /// </code>
    /// </summary>
    [AddComponentMenu("Ellyality/Camera/Top-Down Character")]
    [BuiltInScript]
    public class TopdownCharacter : CharacterBase
    {
        #region Field
        /// <summary>
        /// Should it trying to casting the ground and teleport it
        /// </summary>
        [Header("Setting")]
        [SerializeField] protected bool ForceOnGround = false;
        /// <summary>
        /// <seealso cref="Start"/> will apply this distance
        /// </summary>
        [SerializeField] [HideInInspector] protected float StartDistance = 5f;
        /// <summary>
        /// Minimum distance between camera to character
        /// </summary>
        [SerializeField] [HideInInspector] protected float MinDistance = 3f;
        /// <summary>
        /// Maximum distance between camera to character
        /// </summary>
        [SerializeField] [HideInInspector] protected float MaxDistance = 50f;
        /// <summary>
        /// Hitting
        /// </summary>
        [SerializeField] protected LayerMask HitLayer = new LayerMask();
        /// <summary>
        /// Character moving speed
        /// </summary>
        [Space]
        [SerializeField] [Tooltip("Moving speed will multiply by delta time")] protected float MoveSpeed = 5f;
        /// <summary>
        /// Character looking speed
        /// </summary>
        [SerializeField] [Tooltip("Mouse rotation speed, aka mouse sensitivity")] protected float ViewSpeed = 5f;
        [SerializeField] [Tooltip("Zomming sensitivity")] protected float ZoomSpeed = 3f;
        /// <summary>
        /// Degree of free is control character y rotation tolerance degree <br />  
        /// Lower limit: always -10, Upper limit: -90 + value
        /// </summary>
        [SerializeField] [Range(1f, 30f)] [Tooltip("Mouse rotation up down limit degree" +
            "\nLower limit: always 10, Upper limit: 90 - value" +
            "\nThis value will prevent player rotate over 90 degree causing bug happen")] protected float DegreeBounds = 1f;
        /// <summary>
        /// When all the ray casting failed, we need a default ground height to set the height
        /// </summary>
        [SerializeField] protected float DefaultGroundHeight = 0f;
        #endregion

        #region Properties
        /// <summary>
        /// The minimum distance editor slider value <br />
        /// Default: 3
        /// <code>
        /// // You can override this value to effect editor
        /// public override float MinimumDistanceBounds { get { return 0.01f; } }
        /// </code>
        /// </summary>
        public virtual float MinimumDistanceBounds { get { return 3f; } }
        /// <summary>
        /// The maximum distance editor slider value <br />
        /// Default: 100
        /// <code>
        /// // You can override this value to effect editor
        /// public override float MinimumDistanceBounds { get { return 100f; } }
        /// </code>
        /// </summary>
        public virtual float MaximumDistanceBounds { get { return 100f; } }
        /// <summary>
        /// Mouse go through camera ray 
        /// </summary>
        protected Ray MouseRay
        {
            get
            {
                Ray r = _Camera.ScreenPointToRay(Mouse.current.position.ReadValue());
                Debug.DrawRay(r.origin, r.direction * 1000f, Color.green, 1f);
                return r;
            }
        }
        /// <summary>
        /// Get all the hitting object filter by layer from mouse ray
        /// </summary>
        protected RaycastHit[] MouseHit
        {
            get
            {
                Ray ray = MouseRay;
                RaycastHit[] hits = Physics.RaycastAll(ray, 9999, HitLayer.value);
                return hits;
            }
        }
        /// <summary>
        /// Mouse hit position and filter by layer mask <br />
        /// If hit nothing return null
        /// </summary>
        protected Vector3? MouseHitPoint
        {
            get
            {
                Ray ray = MouseRay;
                RaycastHit[] hits = Physics.RaycastAll(ray, HitLayer);
                if (hits.Length > 0)
                {
                    Vector3 closestPoint = hits[0].point;
                    float closestDistance = 99999f;
                    foreach (var i in hits)
                    {
                        float dist = Vector3.Distance(_Camera.transform.position, i.point);
                        if (dist < closestDistance)
                        {
                            closestPoint = i.point;
                            closestDistance = dist;
                        }
                    }
                    return closestPoint;
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// Get the closest ground viewing point <br />
        /// Take hit layer as ray casting filter <br />
        /// And if it hit nothing, it will spawn an default plane and casting the hit point <br />
        /// If even the plane does not intersect with the ray, Return transform position
        /// </summary>
        protected Vector3 ViewPos
        {
            get
            {
                Ray worldDownRay = new Ray(transform.position + new Vector3(0, 99999, 0), Vector3.down);

                RaycastHit[] hits = Physics.RaycastAll(worldDownRay, HitLayer);
                if (hits.Length > 0)
                {
                    Vector3 closestPoint = hits[0].point;
                    float closestDistance = 99999f;
                    foreach(var i in hits)
                    {
                        float dist = Vector3.Distance(_Camera.transform.position, i.point);
                        if (dist < closestDistance)
                        {
                            closestPoint = i.point;
                            closestDistance = dist;
                        }
                    }
                    return closestPoint;
                }
                else
                {
                    Plane plane = new Plane(Vector3.up, new Vector3(0, DefaultGroundHeight, 0));
                    float dist;
                    if(plane.Raycast(worldDownRay, out dist))
                    {
                        return worldDownRay.GetPoint(dist);
                    }
                    else
                    {
                        return transform.position;
                    }
                }
            }
        }
        /// <summary>
        /// A vector that from ground point pointing to camera point
        /// </summary>
        protected Vector3 Offset
        {
            get
            {
                Vector3 gp = ViewPos;
                return _Camera.transform.position - gp;
            }
        }
        #endregion

        #region Local Variable
        /// <summary>
        /// Input moving data <br />
        /// In <seealso cref="Update"/>, it will apply this to change character position
        /// </summary>
        protected Vector2 movingDir = Vector3.zero;
        /// <summary>
        /// Input looking data <br />
        /// In <seealso cref="Update"/>, it will apply this to change character camera rotation
        /// </summary>
        protected Vector2 lookDir = Vector2.zero;
        /// <summary>
        /// Current distance between camera to character
        /// </summary>
        protected float currentDistance = 0f;
        /// <summary>
        /// Input zoom data <br />
        /// In <seealso cref="Update"/>, it change the distance between camera to character
        /// </summary>
        protected float zoomDir = 0f;
        /// <summary>
        /// Input look mode <br />
        /// If enable it will execute look function
        /// </summary>
        protected bool lookMode = false;
        #endregion

        #region Override
        /// <summary>
        /// Set distance and rotation
        /// </summary>
        public override void Start()
        {
            base.Start();
            currentDistance = StartDistance;
            _CamMovingDest = new Vector3(0, 0, _CamMovingDest.z);
            _CamRotationDest = Vector2.zero;
            _CamRiggingMovingDest = Vector3.zero;
            LookAction(Vector2.zero);
        }
        /// <summary>
        /// Execute looking, zooming action
        /// </summary>
        public override void Update()
        {
            base.Update();
            CameraUpdate();
            if (lookDir != Vector2.zero && lookMode && Controller.CanSee) LookAction(lookDir);
            if (zoomDir != 0f && Controller.CanSee) ZoomAction(zoomDir);
        }
        /// <summary>
        /// Execute moving action
        /// </summary>
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (movingDir != Vector2.zero && Controller.CanMove) MoveAction(movingDir);
        }
        #endregion

        #region Custom Update
        /// <summary>
        /// Set character position to <seealso cref="ViewPos"/> if force on ground is enable <br />
        /// And keep distance between camera to character right
        /// </summary>
        protected void CameraUpdate()
        {
            if (ForceOnGround) transform.position = ViewPos;
            _CamMovingDest = new Vector3(0, 0, -currentDistance); // Cam pos only effect by distance
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
        /// Apply look mode bool to <seealso cref="lookMode"/>
        /// </summary>
        public virtual void OnLookMode(InputValue context)
        {
            lookMode = context.isPressed;
        }
        /// <summary>
        /// Apply zoom number to <seealso cref="zoomDir"/>
        /// </summary>
        public virtual void OnZoom(InputValue context)
        {
            zoomDir = context.Get<float>() * Time.deltaTime * 10f * ZoomSpeed;
        }
        /// <summary>
        /// Apply vector 2D data to <seealso cref="movingDir"/>
        /// </summary>
        public virtual void OnMove(InputValue context)
        {
            Vector2 dir = context.Get<Vector2>();
            Vector3 r = MoveVector(new Vector3(dir.x, 0, dir.y), MoveSpeed);
            movingDir = new Vector2(r.x, r.z);
        }
        #endregion

        #region Execute
        public virtual void LookAction(Vector2 dir)
        {
            dir.x *= -1;
            CameraCenterRotateForView(90 - DegreeBounds, 10, dir, false);
        }
        public virtual void ZoomAction(float dir)
        {
            currentDistance += dir;
            currentDistance = Mathf.Clamp(currentDistance, MinDistance, MaxDistance);
        }
        public virtual void MoveAction(Vector2 dir)
        {
            SimpleMove(new Vector3(dir.x, 0, dir.y), _CameraRigging);
        }
        #endregion
    }
}
