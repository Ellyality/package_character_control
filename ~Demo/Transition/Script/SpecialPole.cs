using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Ellyality.Runtime.Demo
{
    public class SpecialPole : MonoBehaviour
    {
        public Text InteractText = null;
        public Text InteractText2 = null;

        public static bool LastViewIsFPS = false;
        public static bool TopDownMode = false;

        public bool CursorOn
        {
            set
            {
                _CursorOn = value;
            }
            get
            {
                return _CursorOn;
            }
        }
        private bool _CursorOn = false;

        public Transform Player
        {
            set
            {
                _Player = value;
            }
            get
            {
                return _Player;
            }
        }
        private Transform _Player = null;

        private void Update()
        {
            InteractText.enabled = CursorOn && !TopDownMode;
            InteractText2.enabled = TopDownMode;
        }

        public void OnPoleInteract()
        {
            if (!CursorOn || !Player) return;
            CharacterBase CB = Player.GetComponent<CharacterBase>();
            if (CB)
            {
                if (!TopDownMode)
                {
                    LastViewIsFPS = CB.GetType().IsSubclassOf(typeof(FPSCharacter));
                    CB.SwitchType<TransitionTopdownCharacter>();
                    TopDownMode = true;
                }
                else
                {
                    if (LastViewIsFPS)
                    {
                        CB.SwitchType<TransitionFPSCharacter>();
                    }
                    else
                    {
                        CB.SwitchType<TransitionTPSCharacter>();
                    }
                    TopDownMode = false;
                }
            }
        }

        public static void Casting(Ray ray, Transform who, ref SpecialPole hitPole, float distance)
        {
            RaycastHit[] hits = Physics.RaycastAll(ray, distance);
            foreach(var i in hits)
            {
                SpecialPole SP = i.transform.GetComponent<SpecialPole>();
                if (SP)
                {
                    hitPole = SP;
                    if (hitPole)
                    {
                        hitPole.CursorOn = true;
                        hitPole.Player = who;
                    }
                    return;
                }
            }
            if (hitPole != null)
            {
                hitPole.CursorOn = false; hitPole.Player = null;
            }
            hitPole = null;
        }
    }
}