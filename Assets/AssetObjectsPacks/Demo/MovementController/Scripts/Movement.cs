//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;


/*

    calculations used in movement components, seperated so component scripts contain mostly

    package functionality

*/
public static class Movement 
{
    public enum Direction  { Forward=0, Backwards=3, Left=1, Right=2, Calculated=-1 };

    public static Vector3 GetRelativeTransformDirection(Direction direction, Transform transform) {
        switch (direction) {
            case Direction.Forward: return transform.forward;
            case Direction.Backwards: return -transform.forward;
            case Direction.Left: return -transform.right;
            case Direction.Right: return transform.right;
        }
        return transform.forward;
    }

    public static Vector3 CalculateTargetFaceDirection(Direction direction, Vector3 origin, Vector3 destination) {
        Vector3 targetDir = destination - origin;
        targetDir.y = 0;
        switch (direction) {
            case Direction.Forward: return targetDir;
            case Direction.Backwards: return -targetDir;
            case Direction.Left: return -Vector3.Cross(targetDir.normalized, Vector3.up);
            case Direction.Right: return Vector3.Cross(targetDir.normalized, Vector3.up);        
        }
        return targetDir;
    }
    


    public static class AI {
        public static Direction CalculateMoveDirection (Vector3 origin, Vector3 destination, Vector3 interestPoint, float minDistanceThreshold) {

            Vector3 a = origin;
            Vector3 b = destination;
            Vector3 c = interestPoint;

            Vector3 a2b = b - a;
            
            //maybe return current direction (for no sudden changes)
            float threshold = minDistanceThreshold * minDistanceThreshold;
            if (a2b.sqrMagnitude < threshold) {
                return Direction.Forward;
            }
            
            a2b.y = 0;
            Vector3 midPoint = (a + b) * .5f;
            Vector3 mid2C = c - midPoint;
            mid2C.y = 0;
            
            float angle = Vector3.Angle(mid2C, a2b);

            /*
                ideal angle for look position is 90
                A -------------- B
                        /
                       /
                      C
            */
            //angle is too acute or obtuse between face ("enemy" point) and destination for strafing 
            //(backwards or forwards)
            if (angle <= 45 || angle >= 135) {
                return angle >= 135 ? Direction.Forward : Direction.Backwards;
            }
            /*
                      C
                       \
                        \
                A -------------- B
                        |
                        | <- a2bPerp
            */
            Vector3 a2bPerp = Vector3.Cross(a2b.normalized, Vector3.up);

            return Vector3.Angle(a2bPerp, mid2C) <= 45 ? Direction.Right : Direction.Left;
        }

    }
    
    

}
