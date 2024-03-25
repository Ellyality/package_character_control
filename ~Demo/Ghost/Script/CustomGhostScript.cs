using UnityEngine;
using UnityEngine.InputSystem;

namespace Ellyality.Runtime.Demo
{
    public class CustomGhostScript : GhostCharacter
    {
        [Header("Custom Setting")]
        public float RunningMultiply = 1.5f;
        private bool isRunning = false;

        public override void Start()
        {
            base.Start();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (movingDir != Vector3.zero && Controller.CanMove) SimpleMove(movingDir * (isRunning ? RunningMultiply : 1f));
        }

        public virtual void OnRunning(InputValue context)
        {
            isRunning = context.isPressed;
        }
    }
}