using UnityEngine;
using UnityEngine.InputSystem;

namespace Ellyality.Runtime.Demo
{
    [RequireComponent(typeof(Rigidbody))]
    public class TransitionFPSCharacter : FPSCharacter
    {
        [Header("Custom Setting")]
        public CapsuleCollider PlayerCollider;
        public float JumpForce = 2f;
        public float RunningMultiply = 1.5f;

        private Rigidbody rigi;
        private bool isSquat = false;
        private bool isRunning = false;

        public override void Awake()
        {
            base.Awake();
            rigi = GetComponent<Rigidbody>();
            if (!Share.Init)
            {
                Share.PlayerCollider = PlayerCollider;
                Share.MoveSpeed = MoveSpeed;
                Share.ViewSpeed = ViewSpeed;
                Share.JumpForce = JumpForce;
                Share.Running = RunningMultiply;
                Share.Init = true;
            }
        }

        public override void Start()
        {
            base.Start();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            MoveSpeed = Share.MoveSpeed;
            ViewSpeed = Share.ViewSpeed;
            JumpForce = Share.JumpForce;
            RunningMultiply = Share.Running;
            CamOffset = Vector3.zero;
        }

        public override void Update()
        {
            base.Update();
            Share.PlayerCollider.height = isSquat ? 1f : 2.5f;

            Ray? ray = CameraFrontRay;
            if (ray.HasValue)
                SpecialPole.Casting(ray.Value, transform, ref Share.hitPole, 3f);
        }

        public override void FixedUpdate()
        {
            if (movingDir != Vector3.zero && Controller.CanMove) SimpleMove(movingDir * (isRunning && !isSquat ? RunningMultiply : 1f));
        }

        public virtual void OnRunning(InputValue context)
        {
            isRunning = context.isPressed;
        }

        public override void SwitchAction(CharacterBase source)
        {
            base.SwitchAction(source);
            Controller.CanMove = true;
        }

        public virtual void OnViewSwitch(InputValue context)
        {
            if (!SpecialPole.TopDownMode)
            {
                SwitchType<TransitionTPSCharacter>();
            }
        }

        public override void OnSquat(InputValue context)
        {
            isSquat = context.isPressed;
        }

        public override void OnJump(InputValue context)
        {
            if (context.isPressed)
                rigi.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
        }
    }
}
