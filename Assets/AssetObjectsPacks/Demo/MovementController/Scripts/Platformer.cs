using UnityEngine;
using AssetObjectsPacks;
using System;
public class Platformer : MovementControllerComponent
{

    //checked in editor

    public const float tallPlatformStartDistance = 1;
    public const float tallPlatformSize = 2.25f;
    public const float smallPlatformSize = 1.0f;
    const float distanceAheadCheckUp = tallPlatformStartDistance + .1f; //platform ups are 1 unit away
    const float distanceAheadCheckDown = .35f + .1f; //platform ups are .35 units away

    const float faceAngleThreshold = 45; //max face angle with platform up candidate
    const float steepnessRange = .1f; //dot steepness threshold (from 0) for up platforms
    const float dropHeightRange = .1f; //range buffer for checking if a drop is short or tall
    const float sphereCheckRadius = .05f;
    const float spaceBuffer = .1f;

    public bool doAutoPlatform;
    public LayerMask layerMask;


    bool overrideMovement { get { return !controller.grounded || controller.overrideMovement; } }
    delegate bool CheckForPlatform (out bool isShort, out Vector3 atPos, out Quaternion atRot);



    Action<bool> onPlatformEnd;
    public void SetCallback (Action<bool> onPlatformEnd) {
        this.onPlatformEnd = onPlatformEnd;
    }
    

    void FixedUpdate () {
        if (doAutoPlatform){// && controller.speed > 0 && turner.FacingTarget()) {
            AutoPlatformUpdate();
        }
    }
    void AutoPlatformUpdate () {
        //check up
        if (!PlatformUpUpdate(true)) {
            //if no up chekc down
            PlatformDownUpdate(true);
        }
    }

    
    bool IsWithinRange(float value, float originalValue, float buffer) {
        return value >= originalValue - buffer && value <= originalValue + buffer;
    }

    
    bool PlatformUpdate (bool triggered, CheckForPlatform checkFN, Cue shortCue, Cue tallCue, bool isDown) {
        if (overrideMovement) return false;
        
        bool isShort;
        Vector3 atPos;
        Quaternion atRot;
        bool foundPlatform = checkFN(out isShort, out atPos, out atRot);
        if (foundPlatform && triggered) {
            Debug.Log("platform!!!");
            Playlist.InitializePerformance(
                "platform", 
                isShort ? shortCue : tallCue, 
                eventPlayer, 
                false, 
                eventLayer, 
                atPos, 
                atRot, 
                true, 
                () => {
                    if (onPlatformEnd != null) {
                        onPlatformEnd(isDown); 
                    }
                }
            );
        }    
        return foundPlatform;    
    }

    public bool PlatformDownUpdate (bool triggerPlatform) {
        return PlatformUpdate (triggerPlatform, CheckForPlatformDown, behavior.platformDownCueShort, behavior.platformDownCueTall, true);
    }

    public bool PlatformUpUpdate (bool triggerPlatform) {
        return PlatformUpdate (triggerPlatform, CheckForPlatformUp, behavior.platformUpCueShort, behavior.platformUpCueTall, false);
    }

    bool CheckForPlatformUp (out bool isShort, out Vector3 spawnCuePosition, out Quaternion spawnCueRotation) {
        
        bool foundPlatform = false;
        isShort = false;
        spawnCuePosition = Vector3.zero;
        spawnCueRotation = Quaternion.identity;

        float buffer = spaceBuffer + sphereCheckRadius;

        /*
            check if theres an obstacle in front

                                 ________
              0                  |
             /[]\                |
              /\    -----------> |
        */

        Vector3 rayDirection = controller.moveDireciton;
        Vector3 myPos = transform.position;

        Ray checkAheadRay = new Ray(myPos + Vector3.up * buffer, rayDirection);
        RaycastHit wallHit;
        if (Physics.SphereCast(checkAheadRay, sphereCheckRadius, out wallHit, distanceAheadCheckUp, layerMask)) {
            
            //check steepness of hit obstacle
            float steepness = Vector3.Dot(Vector3.up, wallHit.normal);

            if (IsWithinRange(steepness, 0, steepnessRange)) {
                
                Vector3 norm2D = wallHit.normal;
                norm2D.y = 0;
                //check face angle
                float angleWfwd = Vector3.Angle(-norm2D, rayDirection);
                
                if (angleWfwd < faceAngleThreshold) {

                    /*
                        face angle within range, check for tall or short

                                           
                         0     ----------->
                        /[]\                ________
                         /\                 |
                    */

                    Ray checkHeightRay = new Ray(myPos + Vector3.up * (smallPlatformSize + buffer), rayDirection);
                    if (Physics.SphereCast(checkHeightRay, sphereCheckRadius, distanceAheadCheckUp, layerMask)) {
                        //hit obstacle, not small, check if tall
                        checkHeightRay.origin = myPos + Vector3.up * (tallPlatformSize + buffer);
                        
                        foundPlatform = !Physics.SphereCast(checkHeightRay, sphereCheckRadius, distanceAheadCheckUp, layerMask);
                    }
                    else {
                        isShort = true;
                        foundPlatform = true;
                    }
                }
            }

        }
        if (foundPlatform) {
            Vector3 norm2D = wallHit.normal;
            norm2D.y = 0;
            norm2D.Normalize();

            spawnCuePosition = wallHit.point + norm2D;
            spawnCuePosition.y = myPos.y;
            
            spawnCueRotation = Quaternion.LookRotation(-norm2D);
        }
        return foundPlatform;
    }

    bool CheckForPlatformDown (out bool isShort, out Vector3 spawnCuePosition, out Quaternion spawnCueRotation) {
        isShort = false;

        Vector3 myPos = transform.position;

        //spawn at controller position and rotation
        spawnCuePosition = myPos;
        spawnCueRotation = transform.rotation;

        /*
            check if theres an obstacle in front

              0
             /[]\
              /\    ----------->
        */

        

        float buffer = spaceBuffer + sphereCheckRadius;

        Ray checkAheadRay = new Ray(myPos + Vector3.up * buffer, controller.moveDireciton);
        if (!Physics.SphereCast(checkAheadRay, sphereCheckRadius, distanceAheadCheckDown, layerMask)) {
            //no obstacle

            /*
                check that forward postion's 'down'
                
                 0
                /[]\
                 /\    -----------> |
                                    |
                                    |
                                    V
            */

            Ray checkDownFromAheadRay = new Ray(checkAheadRay.origin + checkAheadRay.direction * distanceAheadCheckDown, Vector3.down);
            
            float distCheck = 10;
            RaycastHit hit;

            if (Physics.Raycast(checkDownFromAheadRay, out hit, distCheck)) {
                //hit floor

                float distanceToFloor = myPos.y - hit.point.y;
                
                //check if short drop
                if (IsWithinRange(distanceToFloor, smallPlatformSize, dropHeightRange)) {
                    isShort = true;
                    return true;
                }
                else if (IsWithinRange(distanceToFloor, tallPlatformSize, dropHeightRange)) {
                    return true;                    
                }
            }
        }
        return false;
    }
}
