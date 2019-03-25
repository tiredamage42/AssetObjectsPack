using UnityEngine;
using AssetObjectsPacks;
using System;

[RequireComponent(typeof(CharacterMovement))]
public class Platformer : MovementControllerComponent
{

    //checked values in editor
    public const float tallPlatformStartDistance = 1; //platform ups are 1 unit away
    public const float shortPlatformStartDistance = .35f; //platform ups are .35 units away
    public const float tallPlatformSize = 2.25f;
    public const float smallPlatformSize = 1.0f;

    const float spaceBuffer = .1f;
    public float distanceAheadCheckUp = tallPlatformStartDistance + spaceBuffer; 
    public float distanceAheadCheckDown = shortPlatformStartDistance + spaceBuffer; 
    const float faceAngleThreshold = 45; //max face angle with platform up candidate
    const float steepnessRange = .1f; //dot steepness threshold (from 0) for up platforms
    const float dropHeightRange = .1f; //range buffer for checking if a drop is short or tall
    const float sphereCheckRadius = .05f;

    public bool doAutoPlatform;
    public LayerMask layerMask;
    public Cue platformUpCueShort, platformDownCueShort;
    public Cue platformUpCueTall, platformDownCueTall;


    bool overrideMovement { get { return !characterMovement.grounded || controller.overrideMovement; } }
    
    Action onPlatformEnd;

    CharacterMovement characterMovement;

    protected override void Awake () {
        base.Awake();
        characterMovement = GetComponent<CharacterMovement>();
    }
    public void SetCallback (Action onPlatformEnd) {
        this.onPlatformEnd = onPlatformEnd;
    }
    public override void UpdateLoop (float deltaTime) {
        if (doAutoPlatform) {
            AutoPlatformUpdate();
        }
    }

    public bool PlatformDownUpdate (bool triggerPlatform) {
        return PlatformUpdate (triggerPlatform, CheckForPlatformDown, platformDownCueShort, platformDownCueTall, true);
    }
    public bool PlatformUpUpdate (bool triggerPlatform) {
        return PlatformUpdate (triggerPlatform, CheckForPlatformUp, platformUpCueShort, platformUpCueTall, false);
    }
    public static bool SamePlatformLevels (Vector3 origin, Vector3 platformPos) {
        return Mathf.Abs(platformPos.y - origin.y) < (smallPlatformSize - .1f);
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

    delegate bool CheckForPlatform (out bool isShort, out Vector3 atPos, out Quaternion atRot);
    bool PlatformUpdate (bool triggered, CheckForPlatform checkFN, Cue shortCue, Cue tallCue, bool isDown) {
        if (overrideMovement) return false;
        
        bool isShort;
        Vector3 atPos;
        Quaternion atRot;
        bool foundPlatform = checkFN(out isShort, out atPos, out atRot);
        if (foundPlatform && triggered) {
            //Debug.Log("platform!!!");
            Playlist.InitializePerformance(
                "platform", 
                isShort ? shortCue : tallCue, 
                eventPlayer, 
                false, 
                eventLayer, 
                atPos, 
                atRot, 
                true, 
                onPlatformEnd
            );
        }    
        return foundPlatform;    
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
        
        Debug.DrawRay(checkAheadRay.origin, checkAheadRay.direction * distanceAheadCheckUp, Color.red);
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
        Vector3 rayDirection = controller.moveDireciton;


        //spawn at controller position and rotation
        spawnCuePosition = myPos;
        spawnCueRotation = Quaternion.LookRotation(rayDirection);

        /*
            check if theres an obstacle in front

              0
             /[]\
              /\    ----------->
        */

        

        float buffer = spaceBuffer + sphereCheckRadius;

        Ray checkAheadRay = new Ray(myPos + Vector3.up * buffer, rayDirection);

        Debug.DrawRay(checkAheadRay.origin, checkAheadRay.direction * distanceAheadCheckDown, Color.red);
        
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
