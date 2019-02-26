using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetObjectsPacks;


public class ChooseTurnEvent : MonoBehaviour
{
    public AssetObjectsPacks.Event d45r, d45l;
    public AssetObjectsPacks.Event d90r, d90l;
    public AssetObjectsPacks.Event d135r, d135l;
    public AssetObjectsPacks.Event d180;
    public string animationPackName = "Animations";


    EventPlayer _player;
    EventPlayer player {
        get {
            if (_player == null) _player = GetComponent<EventPlayer>();
            return _player;
        }
    }   

/*
    bool doUpdate {
        get {
            return player.current_playlists.Count == 0;
        }
    }
*/
    public bool isTurning;
    public Vector3 turnTarget;
    public void SetTurnTarget(Vector3 newTarget) {
        turnTarget = newTarget;
    }



    void InitializeTurn_Event (Transform interestTransform) {
        SetTurnTarget(interestTransform.position);


        float angleBuffer = 45*.5f;
        
        Vector3 dir = turnTarget - transform.position;
        dir.y = 0;

        //Vector3 fwd = transform.forward;

        float angleFwd = Vector3.Angle(transform.forward, dir);
        
        /*
        float angleRight = Vector3.Angle(transform.right, dir);
        bool toRight = angleRight <= 90;
            
        if (angleFwd < 45 + angleBuffer) {
            turnToUse = toRight ? d45r : d45l;
        }
        else if (angleFwd < 90 + angleBuffer) {
            turnToUse = toRight ? d90r : d90l;
        }
        else if (angleFwd < 135 + angleBuffer) {
            turnToUse = toRight ? d135r : d135l;
        }
        else {
            turnToUse = d180;
        }
        */

        AssetObjectsPacks.Event turnToUse = null;


        if (angleFwd >= 180 - angleBuffer) {
            turnToUse = d180;
        }
        else {
            float angleRight = Vector3.Angle(transform.right, dir);
            bool toRight = angleRight <= 90;
            if (angleFwd >= 135 - angleBuffer) {
                turnToUse = toRight ? d135r : d135l;
            }
            else if (angleFwd >= 90 - angleBuffer) {
                turnToUse = toRight ? d90r : d90l;
            }
            else { //45
                turnToUse = toRight ? d45r : d45l;
            }
        }

        player.OverrideEventToPlay(animationPackName, turnToUse);

    }
}
