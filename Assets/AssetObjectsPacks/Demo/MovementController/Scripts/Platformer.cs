using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetObjectsPacks;

[System.Serializable] public class Platformer : MovementControllerComponent
{

    //checked in editor
    const float smallPlatformSize = 1.0f;
    const float tallPlatformSize = 2.25f;



    public bool doAutoPlatform;

    float angleThreshold { get { return behavior.platformAngleThreshold; } }
    int layerMask { get { return behavior.platformLayerMask; } }
    float platformUpDistanceAheadCheck { get { return behavior.platformUpDistanceAheadCheck; } }
    float sphereCheckRadius  { get { return behavior.platformRadiusCheck; } }

    float platformDownDistanceCheck { get { return behavior.platformDownDistanceAheadCheck; } }



    bool skipMove {
        get {
            return eventPlayer.overrideMovement || isPlatforming || !movementController.grounded || movementController.speed == 0;
        }
    }

    public bool isPlatforming;


    void OnPlatformEnd () {
        isPlatforming = false;
    }

    public void Update () {
        if (doAutoPlatform){//} && movementController.speed > 0) {
            AutoPlatformUpdate();
            AutoPlatformDownUpdate();
        }
    }

    public void AutoPlatformDownUpdate () {
        if (skipMove) return;
        

        bool isShort;
        Vector3 atPos;
        Quaternion atRot;
        if (CheckForPlatformDown(out isShort, out atPos, out atRot)) {
            isPlatforming = true;
            Debug.DrawLine(atPos, atPos + Vector3.up, Color.gray);
            
            
            Debug.Break();

            movementController.turner.InterruptTurn();
            eventPlayer.InterruptLayer(0);
        
            
            Playlist.InitializePerformance(new Cue[] { isShort ? behavior.platformDownCueShort : behavior.platformDownCueTall }, new EventPlayer[] {eventPlayer}, atPos, atRot, false, 0, OnPlatformEnd);
            
        }


        
    }




    public void AutoPlatformUpdate () {
        if (skipMove) return;

        bool isShort;
        Vector3 atPos;
        Quaternion atRot;
        if (CheckForPlatform(out isShort, out atPos, out atRot)) {
            isPlatforming = true;
            movementController.turner.InterruptTurn();
            eventPlayer.InterruptLayer(0);
        
            
            Playlist.InitializePerformance(new Cue[] { isShort ? behavior.platformUpCueShort : behavior.platformUpCueTall }, new EventPlayer[] {eventPlayer}, atPos, atRot, false, 0, OnPlatformEnd);
            
        }
    }

    const float spaceBuffer = .1f;

    float fullBuffer { 
        get {
            return spaceBuffer + sphereCheckRadius;
        }
    }
    const float steepnessRange = .1f;

    
    bool CheckForPlatform (out bool isShort, out Vector3 spawnCuePosition, out Quaternion spawnCueRotation) {
        Vector3 myPos = transform.position;
        Vector3 myFwd = transform.forward;
        

        bool foundPlatform = false;
        isShort = false;
        
        spawnCuePosition = Vector3.zero;
        spawnCueRotation = Quaternion.identity;

        Ray footRay = new Ray(myPos + Vector3.up * fullBuffer, myFwd);

        RaycastHit wallHit;
        if (Physics.SphereCast(footRay, sphereCheckRadius, out wallHit, platformUpDistanceAheadCheck, layerMask)) {

            Debug.DrawRay(footRay.origin, footRay.direction * platformUpDistanceAheadCheck, Color.red);

            //check steepness
            float steepness = Vector3.Dot(Vector3.up, wallHit.normal);
            if (IsWithinRange(steepness, 0, steepnessRange)) {
                
                //Debug.Log("WithinRange");            
                Vector3 norm = wallHit.normal;
                norm.y = 0;
                //check face angle
                float angleWfwd = Vector3.Angle(-norm, myFwd);
                if (angleWfwd < angleThreshold) {

                    //check if its a short
                    Ray checkHeightRay = new Ray(myPos + Vector3.up * (smallPlatformSize + fullBuffer), myFwd);
                    
                    if (Physics.SphereCast(checkHeightRay, sphereCheckRadius, platformUpDistanceAheadCheck, layerMask)) {

                        Debug.DrawRay(checkHeightRay.origin, footRay.direction * platformUpDistanceAheadCheck, Color.red);

                        //hit obstacle, not small
                        //check if tall
                        checkHeightRay = new Ray(myPos + Vector3.up * (tallPlatformSize + fullBuffer), myFwd);
                        
                        if (!Physics.SphereCast(checkHeightRay, sphereCheckRadius, platformUpDistanceAheadCheck, layerMask)) {
                            Debug.DrawRay(checkHeightRay.origin, footRay.direction * platformUpDistanceAheadCheck, Color.green);

                            //hit obstacle cant jump
                            foundPlatform = true;
                        }
                        else {
                            Debug.DrawRay(checkHeightRay.origin, footRay.direction * platformUpDistanceAheadCheck, Color.red);

                        }
    
                    }
                    else {
                        Debug.DrawRay(checkHeightRay.origin, footRay.direction * platformUpDistanceAheadCheck, Color.red);

                        //possible short
                        isShort = true;
                        foundPlatform = true;

                    }
                    

                }
                else {
                        Debug.Log("angle too wide: " + angleWfwd);
            
                }
            }
            else {
                Debug.Log("Steepness too shallow: " + steepness);
            }
            //Debug.Break();


        }
        if (foundPlatform) {
            spawnCuePosition = wallHit.point + wallHit.normal;
            spawnCuePosition.y = myPos.y;

            Vector3 norm = wallHit.normal;
            norm.y = 0;
            spawnCueRotation = Quaternion.LookRotation(-norm);
            
        }
        return foundPlatform;




    }

    const float dropHeightRange = .1f;


    bool IsWithinRange(float value, float originalValue, float buffer) {

        return value >= originalValue - buffer && value <= originalValue + buffer;

    }



    bool CheckForPlatformDown (out bool isShort, out Vector3 spawnCuePosition, out Quaternion spawnCueRotation) {
        Vector3 myPos = transform.position;
        Vector3 myFwd = transform.forward;

        isShort = false;
        spawnCuePosition = myPos;
        spawnCueRotation = transform.rotation;// Quaternion.identity;

        Ray footRay = new Ray(myPos + Vector3.up * fullBuffer, myFwd);

        RaycastHit hit;
        //nothing blocking us forward
        if (!Physics.SphereCast(footRay, sphereCheckRadius, out hit, platformDownDistanceCheck, layerMask)) {
            //check that forward postion's 'down'
            Vector3 checkPos2 = (myPos + Vector3.up * fullBuffer) + myFwd * platformDownDistanceCheck;
            Ray ray2 = new Ray(checkPos2, Vector3.down);

            float distCheck = 10;
            
            if (Physics.Raycast(ray2, out hit, distCheck)) {

                float yPos = hit.point.y;

                float diff = myPos.y - yPos;

                if (IsWithinRange(diff, smallPlatformSize, dropHeightRange)) {
                    isShort = true;
                    return true;

                }
                else if (IsWithinRange(diff, tallPlatformSize, dropHeightRange)) {
                    return true;
                    
                }

            }
        }
        return false;
    }


}
