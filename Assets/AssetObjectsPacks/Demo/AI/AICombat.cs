using UnityEngine;
using Game.Combat;


namespace Game.AI {


    public class AICombat : MonoBehaviour{
        public float maxAimAngle = 70.0f;

        [Header("Debug")]
        public Transform aimDebug;
        public float debugPhaseTime = 5.0f;
        bool debugPhase;
        float debugTimer;
        bool attemptAttack;
        AIAgent agent;
        CharacterCombat characterCombat;

        void Update () {
            if (false){
            // if (aimDebug) {
                agent.SetInterestPoint(aimDebug.position);
                characterCombat.SetAimTarget(aimDebug.position);
                
                debugTimer += Time.deltaTime;
                if (debugTimer > debugPhaseTime) {
                    debugPhase = !debugPhase;
                    debugTimer = 0;
                }   

                attemptAttack = false;
                if (debugPhase) {
                    Vector3 dirToTarget = aimDebug.position - transform.position;
                    dirToTarget.y = 0;
                    bool withinAngle = Vector3.Angle(dirToTarget, transform.forward) <= maxAimAngle;
                    attemptAttack = withinAngle;
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
