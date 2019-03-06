using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Movement
{

    //messed up and made backwards last so directions are 
    //0 - fwd / 1 - left / 2 - right / 3 - back
    
    public enum Direction  {
        Forward=0, Backwards=3, Left=1, Right=2, Calculated=-1
    };
    public static bool TargetAngleAboveTurnThreshold (Direction direction, Vector3 position, Vector3 fwd, Vector3 target, float threshold, out Vector3 dir, out float angleFwd) {
        dir = GetTargetLookDirection(direction, position, target);
        angleFwd = Vector3.Angle(fwd, dir);
        return angleFwd >= threshold;
    }
    

    public static Vector3 GetTargetLookDirection(Direction direction, Vector3 a, Vector3 b) {
        Vector3 startToDest = b - a;
        startToDest.y = 0;
        switch (direction) {
            case Direction.Forward: return startToDest;
            case Direction.Backwards: return -startToDest;
            case Direction.Left: return -Vector3.Cross(startToDest.normalized, Vector3.up);
            case Direction.Right: return Vector3.Cross(startToDest.normalized, Vector3.up);        
        }
        return startToDest;
    }
    
    public static Direction CalculateMovementDirection (Vector3 a, Vector3 b, Vector3 interestPoint, bool allowStrafe, float minStrafeDistance) {
        if (!allowStrafe) return 0;
        
        Vector3 startToDest = b - a;
        
        //maybe return current direction (for no sudden changes)
        if (startToDest.sqrMagnitude < (minStrafeDistance * minStrafeDistance)) {
            return 0;
        }
        
        startToDest.y = 0;
        Vector3 midPoint = (a + b) * .5f;
        Vector3 midToInterest = interestPoint - midPoint;
        midToInterest.y = 0;

        float angle = Vector3.Angle(midToInterest, startToDest);

        /*
            ideal  angle fr look position is 90 or -90
        
            a -------------- b
                    /
                   /
                  /
            interest point

        */

        //angle is too acute or obtuse between interest (enemy point) and destination for strafing 
        //(backwards or forwards)
        if (angle <= 45 || angle >= 135) return angle >= 135 ? Direction.Forward : Direction.Backwards;
        
        /*
              interest point
                  \
                   \
                    \
            a -------------- b
                    |
                    | <- startToDestPerp
                    |

        */
        Vector3 startToDestPerp = Vector3.Cross(startToDest.normalized, Vector3.up);

        angle = Vector3.Angle(startToDestPerp, midToInterest);

        return angle <= 45 ? Direction.Right : Direction.Left;
    }
}
