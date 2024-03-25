using UnityEngine;
using UnityEngine.InputSystem;

namespace Ellyality.Runtime.Demo
{
    [RequireComponent(typeof(Rigidbody))]
    public class CustomTPSScript : TPSCharacter
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
        }

        public override void Start()
        {
            base.Start();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public override void Update()
        {
            base.Update();
            PlayerCollider.height = isSquat ? 1f : 2.5f;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (movingDir != Vector3.zero && Controller.CanMove) SimpleMove(movingDir * (isRunning && !isSquat ? RunningMultiply : 1f), _Camera.transform);
        }

        public virtual void OnRunning(InputValue context)
        {
            isRunning = context.isPressed;
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