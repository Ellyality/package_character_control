using System;
using System.Collections.Generic;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Elly.Runtime
{
    /// <summary>
    /// Basic character component
    /// <code>
    /// // You can create an custom character by inherit it
    /// public class CustomCharacter : CharacterBase
    /// </code>
    /// </summary>
    public abstract class CharacterBase : MonoBehaviour
    {
        #region Field
        [Header("Base Setting")]
        /// <summary>
        /// Detail can check <seealso cref="ControllerStat"/>
        /// </summary>
        [SerializeField] [Tooltip("Controller data")] protected ControllerStat Controller = ControllerStat.Default;
        /// <summary>
        /// Detail can check <seealso cref="VibrationStat"/>
        /// </summary>
        [SerializeField] [Tooltip("Vibration data")] protected VibrationStat Vibration = VibrationStat.Default;
        #endregion

        #region Properties
        /// <summary>
        /// Return character normalized front ray
        /// </summary>
        public Ray CharacterFrontRay
        {
            get
            {
                return new Ray(transform.position, transform.forward);
            }
        }
        /// <summary>
        /// Return <seealso cref="_Camera"/> normalized front ray by go through center point of screen <br />
        /// Return null if camera is null
        /// </summary>
        public Ray? CameraFrontRay
        {
            get
            {
                return _Camera?.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
            }
        }

        /// <summary>
        /// Character use camera <br />
        /// In the <seealso cref="Update"/> method, it will trying to update the position and rotation <br />
        /// Default getter path: <seealso cref="CameraRiggingRoot"/>.find("Camera Rigging/Camera") <br />
        /// Link variable : <seealso cref="__Camera"/>
        /// <code>
        /// // You can override the camera search path 
        /// public override Camera _Camera { 
        ///     get { 
        ///         // Insert getter path 
        ///     } 
        /// }
        /// </code>
        /// </summary>
        public virtual Camera _Camera
        {
            private set
            {
                __Camera = value;
            }
            get
            {
                if (!__Camera && this && CameraRiggingRoot())
                {
                    __Camera = CameraRiggingRoot().Find("Camera Rigging/Camera")?.transform.GetComponent<Camera>();
                }
                return __Camera;
            }
        }
        /// <summary>
        /// <seealso cref="_Camera"/> property temp
        /// </summary>
        protected Camera __Camera = null;

        /// <summary>
        /// Character use camera parent <br />
        /// In the <seealso cref="Update"/> method, it will trying to update the position and rotation <br />
        /// Default getter path: <seealso cref="CameraRiggingRoot"/>.find("Camera Rigging") <br />
        /// Link variable : <seealso cref="__CameraRigging"/>
        /// <code>
        /// // You can override the camera rigging search path 
        /// public override Transform _CameraCenter {
        ///     get {
        ///         // Insert getter path 
        ///     }
        /// }
        /// </code>
        /// </summary>
        public virtual Transform _CameraRigging
        {
            private set
            {
                __CameraRigging = value;
            }
            get
            {
                if (!__CameraRigging && this && CameraRiggingRoot())
                {
                    __CameraRigging = CameraRiggingRoot().Find("Camera Rigging");
                }
                return __CameraRigging;
            }
        }
        /// <summary>
        /// <seealso cref="_CameraRigging"/> property temp
        /// </summary>
        protected Transform __CameraRigging = null;
        
        #endregion

        #region Destination
        /// <summary>
        /// Camera rigging transform smooth moving destination <br />
        /// Notice: in local space
        /// </summary>
        protected Vector3 _CamRiggingMovingDest = Vector3.zero;
        /// <summary>
        /// Camera rigging transform smooth rotating destination <br />
        /// Notice: in local space
        /// </summary>
        protected Vector2 _CamRiggingRotationDest = Vector2.zero;
        /// <summary>
        /// Camera transform smooth moving destination <br />
        /// Notice: in local space
        /// </summary>
        protected Vector3 _CamMovingDest = Vector3.zero;
        /// <summary>
        /// Camera transform smooth rotating destination <br />
        /// Notice: in local space
        /// </summary>
        protected Vector2 _CamRotationDest = Vector2.zero;
        #endregion

        #region Velocity
        /// <summary>
        /// Camera rigging transform smooth moving velocity
        /// </summary>
        protected Vector3 _CamRiggingMovingVelocity = Vector3.zero;
        /// <summary>
        /// Camera rigging transform smooth rotating velocity
        /// </summary>
        protected Vector2 _CamRiggingRotatingVelocity = Vector2.zero;
        /// <summary>
        /// Camera transform smooth moving velocity
        /// </summary>
        protected Vector3 _CamMovingVelocity = Vector3.zero;
        /// <summary>
        /// Camera transform smooth rotating velocity
        /// </summary>
        protected Vector2 _CamRotatingVelocity = Vector2.zero;
        #endregion

        #region Virtual
        /// <summary>
        /// Awake is called when the script instance is being loaded
        /// </summary>
        public virtual void Awake() { }
        /// <summary>
        /// Start is called before <seealso cref="Update"/> method is called
        /// </summary>
        public virtual void Start() { }
        /// <summary>
        /// Editor reset to default value
        /// </summary>
        public virtual void Reset() { }
        /// <summary>
        /// Update is called every frame, if script is enable <br />
        /// Default update will trying to smoothly moving and rotating <seealso cref="_Camera"/> and <seealso cref="_CameraRigging"/>
        /// </summary>
        public virtual void Update()
        {
            VibrationUpdate();
            CameraRiggingPositionSmoothDamp();
            CameraRiggingRotationSmoothDamp();
            CameraPositionSmoothDamp();
            CameraRotationSmoothDamp();
        }
        /// <summary>
        /// Update is called every fixed frame, if script is enable <br />
        /// </summary>
        public virtual void FixedUpdate() { }
        /// <summary>
        /// When <seealso cref="SwitchType"/> happen, Switch target type will call this method with source <seealso cref="CharacterBase"/> as arugment <br />
        /// User can writing custom transition code in here
        /// </summary>
        /// <param name="source"></param>
        public virtual void SwitchAction(CharacterBase source) { }
        /// <summary>
        /// This helps editor script detect if camera structure is exist or not <br />
        /// Default is self transform
        /// <code>
        /// // You can change the default structure 
        /// public override Transform CameraCenterRoot() { return transform.parent; }
        /// </code>
        /// </summary>
        /// <returns>Who's the camera rigging parent</returns>
        public virtual Transform CameraRiggingRoot() { return transform; }
        #endregion

        #region Build-In Camera Rigging Rotation
        /// <summary>
        /// Detect mouse x y delta and apply to <seealso cref="_CamRiggingRotationDest"/>
        /// </summary>
        /// <param name="bounds">Up down degree limit</param>
        /// <param name="viewSpeed">View speed multiplier</param>
        /// <param name="rootYRotate">Transfer the y rotate delta to transform.localEulerAngles.y</param>
        protected void CameraCenterRotateForView(float bounds, float viewSpeed, bool rootYRotate = true)
        {
            if (!_CameraRigging) return;
            Vector2 pos = Mouse.current.delta.ReadValue();
            Vector2 dir = LookVector(pos, viewSpeed);
            CameraCenterRotateForView(bounds, dir, rootYRotate);
        }
        /// <summary>
        /// Detect mouse x y delta and apply to <seealso cref="_CamRiggingRotationDest"/>
        /// </summary>
        /// <param name="bounds">Up down degree limit</param>
        /// <param name="delta">Rotate delta</param>
        /// <param name="rootYRotate">Transfer the y rotate delta to transform.localEulerAngles.y</param>
        protected void CameraCenterRotateForView(float bounds, Vector2 delta, bool rootYRotate = true)
        {
            if (!_CameraRigging) return;
            CameraCenterRotateForView(90 - bounds, -90 + bounds, delta, rootYRotate);
        }
        /// <summary>
        /// Detect mouse x y delta and apply to <seealso cref="_CamRiggingRotationDest"/>
        /// </summary>
        /// <param name="upperAngle">Self define upper angle bound</param>
        /// <param name="lowerAngle">Self define lower angle bound</param>
        /// <param name="viewSpeed">View speed multiplier</param>
        /// <param name="rootYRotate">Transfer the y rotate delta to transform.localEulerAngles.y</param>
        protected void CameraCenterRotateForView(float upperAngle, float lowerAngle, float viewSpeed, bool rootYRotate = true)
        {
            if (!_CameraRigging) return;
            Vector2 pos = Mouse.current.delta.ReadValue();
            Vector2 dir = LookVector(pos, viewSpeed);
            CameraCenterRotateForView(upperAngle, lowerAngle, dir, rootYRotate);
        }
        /// <summary>
        /// Detect mouse x y delta and apply to <seealso cref="_CamRiggingRotationDest"/>
        /// </summary>
        /// <param name="upperBound">Upper position degree</param>
        /// <param name="lowerBound">Lower negative degree</param>
        /// <param name="delta">Rotate delta</param>
        /// <param name="rootYRotate">Transfer the y rotate delta to transform.localEulerAngles.y</param>
        protected void CameraCenterRotateForView(float upperBound, float lowerBound, Vector2 delta, bool rootYRotate = true)
        {
            if (!_CameraRigging) return;
            if (rootYRotate)
            {
                float xangles = _CamRiggingRotationDest.x;
                float resulty = xangles + delta.y;
                if (resulty < lowerBound) xangles = lowerBound;
                else if (resulty > upperBound) xangles = upperBound;
                else if (resulty < upperBound && resulty > lowerBound) xangles = xangles + delta.y;
                _CamRiggingRotationDest = new Vector2(xangles, _CamRiggingRotationDest.y);
                Vector3 l = transform.localEulerAngles;
                l.y += delta.x;
                transform.localEulerAngles = l;
            }
            else
            {
                float xangles = _CamRiggingRotationDest.x;
                float yangles = _CamRiggingRotationDest.y;
                float resulty = xangles + delta.y;
                if (resulty < lowerBound) xangles = lowerBound;
                else if (resulty > upperBound) xangles = upperBound;
                else if (resulty < upperBound && resulty > lowerBound) xangles = xangles + delta.y;
                yangles = yangles + delta.x;
                _CamRiggingRotationDest = new Vector2(xangles, yangles);
            }
        }
        #endregion

        #region Build-In Self Camera Rotation
        /// <summary>
        /// Detect mouse x y delta and apply to <seealso cref="_CamRotationDest"/>
        /// </summary>
        /// <param name="bounds">Up down degree limit</param>
        /// <param name="viewSpeed">View speed multiplier</param>
        /// <param name="rootYRotate">Transfer the y rotate delta to transform.localEulerAngles.y</param>
        protected void CameraRotateForView(float bounds, float viewSpeed, bool rootYRotate = true)
        {
            if (!_Camera) return;
            Vector2 pos = Mouse.current.delta.ReadValue();
            float mx_delta = pos.x * viewSpeed * Time.deltaTime * 50f;
            float my_delta = pos.y * viewSpeed * (Controller.InverseY ? -1f : 1f) * Time.deltaTime * 50f;
            CameraRotateForView(bounds, bounds, new Vector2(mx_delta, my_delta), rootYRotate);
        }
        /// <summary>
        /// Detect mouse x y delta and apply to <seealso cref="_CamRotationDest"/>
        /// </summary>
        /// <param name="bounds">Up down degree limit</param>
        /// <param name="delta">Rotate delta</param>
        /// <param name="rootYRotate">Transfer the y rotate delta to transform.localEulerAngles.y</param>
        protected void CameraRotateForView(float bounds, Vector2 delta, bool rootYRotate = true)
        {
            if (!_Camera) return;
            CameraRotateForView(-90 + bounds, 90 - bounds, delta, rootYRotate);
        }
        /// <summary>
        /// Detect mouse x y delta and apply to <seealso cref="_CamRotationDest"/>
        /// </summary>
        /// <param name="upperAngle">Self define upper angle bound</param>
        /// <param name="lowerAngle">Self define lower angle bound</param>
        /// <param name="viewSpeed">View speed multiplier</param>
        /// <param name="rootYRotate">Transfer the y rotate delta to transform.localEulerAngles.y</param>
        protected void CameraRotateForView(float upperAngle, float lowerAngle, float viewSpeed, bool rootYRotate = true)
        {
            if (!_Camera) return;
            Vector2 pos = Mouse.current.delta.ReadValue();
            float mx_delta = pos.x * viewSpeed * Time.deltaTime * 50f;
            float my_delta = pos.y * viewSpeed * (Controller.InverseY ? -1f : 1f) * Time.deltaTime * 50f;
            CameraRotateForView(upperAngle, lowerAngle, new Vector2(mx_delta, my_delta), rootYRotate);
        }
        /// <summary>
        /// Detect mouse x y delta and apply to <seealso cref="_CamRotationDest"/>
        /// </summary>
        /// <param name="upperBound">Upper negative degree</param>
        /// <param name="lowerBound">Lower position degree</param>
        /// <param name="delta">Rotate delta</param>
        /// <param name="rootYRotate">Transfer the y rotate delta to transform.localEulerAngles.y</param>
        protected void CameraRotateForView(float upperBound, float lowerBound, Vector2 delta, bool rootYRotate = true)
        {
            if (!_Camera) return;
            if (rootYRotate)
            {
                float xangles = _CamRotationDest.x;
                float resulty = xangles + delta.y;
                if (resulty > lowerBound) xangles = lowerBound;
                else if (resulty < upperBound) xangles = upperBound;
                else if (resulty > upperBound && resulty < lowerBound) xangles = xangles + delta.y;
                _CamRiggingRotationDest = new Vector2(xangles, _CamRiggingRotationDest.y);
                Vector3 l = transform.localEulerAngles;
                l.y += delta.x;
                transform.localEulerAngles = l;
            }
            else
            {
                float xangles = _CamRotationDest.x;
                float yangles = _CamRotationDest.y;
                float resulty = xangles + delta.y;
                if (resulty > lowerBound) xangles = lowerBound;
                else if (resulty < upperBound) xangles = upperBound;
                else if (resulty > upperBound && resulty < lowerBound) xangles = xangles + delta.y;
                yangles = yangles + delta.x;
                _CamRotationDest = new Vector2(xangles, yangles);
            }
        }
        #endregion

        #region Build-In Special Rotation
        /// <summary>
        /// <seealso cref="_CamRotationDest"/> will look at self transform with offset <br />
        /// The offset is local depend on <seealso cref="_Camera"/> rotation <br />
        /// Useful when design third-person base character
        /// </summary>
        /// <param name="offset">View center offset</param>
        protected void CameraLookAtForView(Vector2 offset)
        {
            if (!_Camera || !_CameraRigging) return;

            Vector3 camPos = transform.TransformPoint(_CamRiggingMovingDest); // World cam pos
            Vector3 camVector = transform.position - camPos; // World cam vector
            Quaternion camLookCenterQ = Quaternion.LookRotation(camVector.normalized); // World cam rotation
            Vector3 pos = camPos + camVector + (camLookCenterQ * offset); // World pos
            Vector3 globalVector = (pos - _Camera.transform.position).normalized; // cam pos -> result pos

            Debug.DrawLine(_Camera.transform.position, pos, Color.cyan);

            Quaternion q = Quaternion.LookRotation(globalVector); // Then get the look at rotation
            q = MathE.Quaternion.WorldToLocal(transform.rotation, q);
            Vector3 v = q.eulerAngles;
            // Apply
            _CamRotationDest = new Vector2(v.x, v.y);
        }
        #endregion

        #region Build-In Movement
        /// <summary>
        /// Apply character movement <br />
        /// There will be no smoothdamp, it just apply the vector to self transform position <br />
        /// Moving vector y won't effect by rotation, only x and z effect by direction reference rotation y
        /// </summary>
        /// <param name="vec">Moving vec</param>
        /// <param name="dependYAxis">False if you want move it globally</param>
        protected void SimpleMove(Vector3 vec, bool dependYAxis = true)
        {
            SimpleMove(vec, transform, dependYAxis);
        }
        /// <summary>
        /// Apply character movement <br />
        /// There will be no smoothdamp, it just apply the vector to self transform position <br />
        /// Moving vector y won't effect by rotation, only x and z effect by direction reference rotation y
        /// </summary>
        /// <param name="vec">Moving vec</param>
        /// <param name="directionRef">Rotation reference</param>
        /// <param name="dependYAxis">False if you want move it globally</param>
        protected void SimpleMove(Vector3 vec, Transform directionRef, bool dependYAxis = true)
        {
            Vector3 dir = vec;
            if (dependYAxis)
            {
                Vector3 eular = directionRef.eulerAngles; // Depend on current rotation y to move
                transform.position += (Quaternion.Euler(0f, eular.y, 0f) * new Vector3(dir.x, 0f, dir.z)) + new Vector3(0f, dir.y, 0f);
            }
            else
            {
                transform.position += dir; // Global move
            }
        }
        #endregion

        #region Setter
        /// <summary>
        /// Change inverse y setting
        /// </summary>
        public void ChangeInverseY(bool value) => Controller.InverseY = value;
        /// <summary>
        /// Change moving controller enable
        /// </summary>
        public void ChangeCanMove(bool value) => Controller.CanMove = value;
        /// <summary>
        /// Change seeing controller enable
        /// </summary>
        public void ChangeCanSee(bool value) => Controller.CanSee = value;
        /// <summary>
        /// Vibration the camera
        /// </summary>
        /// <param name="strength"></param>
        public void SetVibration(float strength) => Vibration.Strength = strength;
        #endregion

        #region Switch Feature
        /// <summary>
        /// Switch character type <br />
        /// the new component will apply old component's <seealso cref="CharacterBase"/> variables <br />
        /// Then the subclass will be default value, but it will call <seealso cref="SwitchAction"/> method To notice it<br />
        /// You can trying to cast it by override the method
        /// </summary>
        public void SwitchType<T>() where T : CharacterBase
        {
            SwitchType(typeof(T));
        }
        /// <summary>
        /// Switch character type <br />
        /// the new component will apply old component's <seealso cref="CharacterBase"/> variables <br />
        /// Then the subclass will be default value, but it will call <seealso cref="SwitchAction"/> method To notice it<br />
        /// You can trying to cast it by override the method
        /// </summary>
        public void SwitchType(Type type)
        {
            if (!type.IsSubclassOf(typeof(CharacterBase)))
            {
                Debug.LogError($"{type.Name} must be subclass of CharacterBase");
                return;
            }
            Transform t = transform;
            CharacterBase target = t.gameObject.AddComponent(type) as CharacterBase;

            target.Controller = Controller;
            target.Vibration = Vibration;

            target._CamRiggingMovingDest = _CamRiggingMovingDest;
            target._CamRiggingRotationDest = _CamRiggingRotationDest;
            target._CamMovingDest = _CamMovingDest;
            target._CamRotationDest = _CamRotationDest;

            target._CamRiggingMovingVelocity = _CamRiggingMovingVelocity;
            target._CamRiggingRotatingVelocity = _CamRiggingRotatingVelocity;
            target._CamMovingVelocity = _CamMovingVelocity;
            target._CamRotatingVelocity = _CamRotatingVelocity;

            target.SwitchAction(this);

            CloneInputAction(target);

            if (Application.isEditor)
            {
                DestroyImmediate(this);
            }
            else
            {
                Destroy(this);
            }
        }
        /// <summary>
        /// Detect both <seealso cref="CharacterBase"/> unity actions <br />
        /// If same method name, argument <br />
        /// Then it will be clone to it <br />
        /// Player won't need to register persisten event again
        /// </summary>
        /// <param name="target">Clone to which instance</param>
        private void CloneInputAction(CharacterBase target)
        {
            PlayerInput IS = GetComponent<PlayerInput>();
            if (IS != null && IS.notificationBehavior == PlayerNotifications.InvokeCSharpEvents)
            {
                foreach (var current in IS.actionEvents)
                {
                    int eventCount = current.GetPersistentEventCount();
                    List<UnityAction<InputAction.CallbackContext>> eventsAddingElement = new List<UnityAction<InputAction.CallbackContext>>();
                    for (int j = 0; j < eventCount; j++)
                    {
                        UnityAction<InputAction.CallbackContext> action = null;
                        try
                        {
                            action = (UnityAction<InputAction.CallbackContext>)Delegate.CreateDelegate(typeof(UnityAction<InputAction.CallbackContext>), target, current.GetPersistentMethodName(j));
                        }catch(Exception e)
                        {
                            Debug.LogWarning(e.Message);
                            continue;
                        }
                        if (action == null)
                        {
                            Debug.LogWarning($"Action is null");
                            continue;
                        }
                        eventsAddingElement.Add(action);
                    }

                    for (int j = 0; j < eventCount; j++)
                    {
                        UnityEventTools.RemovePersistentListener(current, 0);
                    }

                    if (eventsAddingElement.Count > 0)
                    {
                        foreach (var j in eventsAddingElement)
                        {
                            UnityEventTools.AddPersistentListener(current);
                            UnityEventTools.RegisterPersistentListener(current, current.GetPersistentEventCount() - 1, j);
                        }
                    }
                }
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Apply vector to delta time and <seealso cref="InverseY"/>
        /// </summary>
        /// <param name="origin">Target vector</param>
        /// <param name="viewSpeed">Speed</param>
        protected Vector2 LookVector(Vector2 origin, float viewSpeed = 1f)
        {
            float mx_delta = origin.x * viewSpeed * Time.deltaTime * 10f;
            float my_delta = origin.y * viewSpeed * (Controller.InverseY ? -1f : 1f) * Time.deltaTime * 10f;
            return new Vector2(mx_delta, my_delta);
        }
        /// <summary>
        /// Apply vector to delta time
        /// </summary>
        /// <param name="origin">Target vector</param>
        /// <param name="horizontalSpeed">x, z Speed</param>
        /// <param name="VerticalSpeed">y Speed</param>
        protected Vector3 MoveVector(Vector3 origin, float horizontalSpeed = 1f, float VerticalSpeed = 1f)
        {
            return new Vector3(origin.x * horizontalSpeed * Time.fixedDeltaTime, origin.y * VerticalSpeed * Time.fixedDeltaTime, origin.z * horizontalSpeed * Time.fixedDeltaTime);
        }
        /// <summary>
        /// Vibration stat update
        /// </summary>
        protected void VibrationUpdate()
        {
            Vibration.Update();
            if (Vibration.Vibration != Vector3.zero) _Camera.transform.position += Vibration.Vibration;
        }
        /// <summary>
        /// Smoothly moving camera position
        /// </summary>
        protected void CameraPositionSmoothDamp()
        {
            if (Controller.CamMoveSmoothSpeed == 0f)
            {
                _Camera.transform.localPosition = _CamMovingDest;
            }
            else
            {
                _Camera.transform.localPosition = Vector3.SmoothDamp(_Camera.transform.localPosition, _CamMovingDest, ref _CamMovingVelocity, Controller.CamMoveSmoothSpeed);
            }
        }
        /// <summary>
        /// Smoothly moving camera rotation
        /// </summary>
        protected void CameraRotationSmoothDamp()
        {
            if (Controller.CamRotateSmoothSpeed == 0f)
            {
                Vector3 tl = _Camera.transform.localEulerAngles;
                tl.x = _CamRotationDest.x;
                tl.y = _CamRotationDest.y;
                _Camera.transform.localEulerAngles = tl;
            }
            else
            {
                Vector3 tl = _Camera.transform.localEulerAngles;
                tl = new Vector3(
                    Mathf.SmoothDampAngle(tl.x, _CamRotationDest.x, ref _CamRotatingVelocity.x, Controller.CamRotateSmoothSpeed),
                    Mathf.SmoothDampAngle(tl.y, _CamRotationDest.y, ref _CamRotatingVelocity.y, Controller.CamRotateSmoothSpeed), tl.z);
                _Camera.transform.localEulerAngles = tl;
            }
        }
        /// <summary>
        /// Smoothly moving camera rigging position
        /// </summary>
        protected void CameraRiggingPositionSmoothDamp()
        {
            if (Controller.CamRiggingMoveSmoothSpeed == 0f)
            {
                _CameraRigging.localPosition = _CamRiggingMovingDest;
            }
            else
            {
                _CameraRigging.localPosition = Vector3.SmoothDamp(_CameraRigging.localPosition, _CamRiggingMovingDest, ref _CamRiggingMovingVelocity, Controller.CamRiggingMoveSmoothSpeed);
            }
        }
        /// <summary>
        /// Smoothly moving camera rigging rotation
        /// </summary>
        protected void CameraRiggingRotationSmoothDamp()
        {
            if (Controller.CamRiggingRotateSmoothSpeed == 0f)
            {
                Vector3 tl = _CameraRigging.localEulerAngles;
                tl.x = _CamRiggingRotationDest.x;
                tl.y = _CamRiggingRotationDest.y;
                _CameraRigging.localEulerAngles = tl;
            }
            else
            {
                Vector3 tl = _CameraRigging.localEulerAngles;
                tl = new Vector3(
                    Mathf.SmoothDampAngle(tl.x, _CamRiggingRotationDest.x, ref _CamRiggingRotatingVelocity.x, Controller.CamRiggingRotateSmoothSpeed),
                    Mathf.SmoothDampAngle(tl.y, _CamRiggingRotationDest.y, ref _CamRiggingRotatingVelocity.y, Controller.CamRiggingRotateSmoothSpeed),
                    tl.z);
                _CameraRigging.localEulerAngles = tl;
            }
        }
        #endregion
    }
}