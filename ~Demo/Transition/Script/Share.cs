using UnityEngine;

namespace Ellyality.Runtime.Demo
{
    public static class Share
    {
        public static bool Init = false;
        public static float MoveSpeed;
        public static float ViewSpeed;
        public static float JumpForce;
        public static float Running;
        public static CapsuleCollider PlayerCollider = null;
        public static SpecialPole hitPole = null;
    }
}
