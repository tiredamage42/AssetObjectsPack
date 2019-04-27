using UnityEngine;
using Combat;
// using System.Collections;
// using System.Collections.Generic;
// using System.Collections;
// using System.Collections.Generic;
// using AssetObjectsPacks;
// using System;
// using UnityEngine.AI;
// using System.Linq;
// using Movement.Platforms;
// using Movement;



namespace Syd.AI {


    public class AICombat : MonoBehaviour{
        public float maxAimAngle = 70.0f;

        [Header("Debug")]
        public Transform aimDebug;
        public float debugPhaseTime = 5.0f;
        bool debugPhase;
        float debugTimer;

        CharacterCombat characterCombat;

        bool attemptAttack;
        
        AIAgent agent;

        void Update () {
            if (aimDebug) {
                agent.SetInterestPoint(aimDebug.position);
                characterCombat.SetAimTarget(aimDebug.position);
                
                debugTimer += Time.deltaTime;
                if (debugTimer > debugPhaseTime) {
                    debugPhase = !debugPhase;
                    debugTimer = 0;
                }   



                if (debugPhase) {
                    Vector3 dirToTarget = aimDebug.position - transform.position;
                    dirToTarget.y = 0;
                    bool withinAngle = Vector3.Angle(dirToTarget, transform.forward) <= maxAimAngle;
                    attemptAttack = Vector3.Angle(dirToTarget, transform.forward) < maxAimAngle;
                }
                else {
                    attemptAttack = false;
                }
                    
            }
            characterCombat.SetAiming(attemptAttack);
            characterCombat.SetFiring(attemptAttack);
        }
    
        void Awake () {
            characterCombat = GetComponent<CharacterCombat>();
            agent = GetComponent<AIAgent>();
        }
    }
}
