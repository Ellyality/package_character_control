using UnityEngine;

namespace Elly.Runtime.Demo
{
    public class TransitionTopdownCharacter : TopdownCharacter
    {
        public override void Start()
        {
            base.Start();
            HitLayer = ~0;
            MoveSpeed = 10f;
            ViewSpeed = 5f;
            _CamRiggingRotationDest = new Vector2(45, _CamRotationDest.y);
        }

        public override void SwitchAction(CharacterBase source)
        {
            base.SwitchAction(source);
            Controller.CanMove = false;
            StartDistance = 50;
        }
    }
}
