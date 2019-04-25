using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetObjectsPacks;
using Movement;

namespace Combat {

    public class CharacterCombat : MovementControllerComponent
    {

        public Gun currentGun;

        public delegate void OnGunChange(Gun newGun);
        public event OnGunChange onGunChange;
        
        public bool isAiming;
        public float aimSpeed = 3;
        // [HideInInspector] public Vector3 aimTarget; 
        [HideInInspector] public float aimLerp;
        ValueTracker<bool> aimChangeTracker = new ValueTracker<bool>(false);
        ValueTracker<Gun> gunChangeTracker = new ValueTracker<Gun>(null);
        
        void BroadcastGunChange () {
            if (onGunChange != null) {
                onGunChange(currentGun);
            }
        }
        protected override void Awake() {
            base.Awake();
            eventPlayer.AddParameter( new CustomParameter ( "Aiming", () => isAiming ) );
        }
        public override void UpdateLoop (float deltaTime) {
            
            if (aimChangeTracker.CheckValueChange(isAiming)) {
                Debug.Log("changin loop state because aim is " + isAiming);
                controller.UpdateLoopState();
            }
            if (gunChangeTracker.CheckValueChange(currentGun)) {
                BroadcastGunChange();
            }

            UpdateAimLerp(deltaTime);        
        }

        const float aimEndThreshold = .01f;
        void UpdateAimLerp (float deltaTime) {
            float target = isAiming ? 1.0f : 0.0f;
            if (aimLerp != target) {
                
                aimLerp = Mathf.Lerp(aimLerp, target, aimSpeed * deltaTime);
                
                if (isAiming && aimLerp > (1.0f - aimEndThreshold)) {
                    aimLerp = 1.0f;
                }
                else if (!isAiming && aimLerp < aimEndThreshold) {
                    aimLerp = 0.0f;
                }
            }
        }
            
        public void SetAimTarget (Vector3 target) {
            _aimTarget = target;
        }

        Vector3 _aimTarget;
        System.Func<Vector3> getAimTarget;

        public Vector3 aimTarget {
            get {
                if (getAimTarget != null) {
                    return getAimTarget();
                }
                return _aimTarget;
            }
        }

        public void SetAimTargetCallback (System.Func<Vector3> getAimTarget) {
            this.getAimTarget = getAimTarget;
        }
    }
}
