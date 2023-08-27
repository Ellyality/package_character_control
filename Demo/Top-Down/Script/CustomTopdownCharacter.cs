using Elly.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class CustomTopdownCharacter : TopdownCharacter
{
    [SerializeField] public MoveBall ball;

    public void OnMovingBall()
    {
        foreach(var i in MouseHit)
        {
            if (i.transform.name.Equals("Floor"))
            {
                ball.MoveToHere(i.point);
                return;
            }
        }
    }
}
