using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetObjectsPacks;
using Movement;

namespace Combat {



    public class CharacterCombat : MovementControllerComponent
    {
        public Gun currentGun;
        public Smoother aimSmoother;

        public delegate void OnGunChange(Gun newGun);
        public event OnGunChange onGunChange;
        public bool isAiming;
        // public float aimSpeed = 3;
        // [HideInInspector] public Vector3 aimTarget; 
        public float aimPercent;
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
                // Debug.Log("changin loop state because aim is " + isAiming);
                controller.UpdateLoopState();
            }
            if (gunChangeTracker.CheckValueChange(currentGun)) {
                BroadcastGunChange();
            }

            UpdateAimLerp(deltaTime);        
        }

        const float aimEndThreshold = .001f;
        void UpdateAimLerp (float deltaTime) {
            float target = isAiming ? 1.0f : 0.0f;
            if (aimPercent != target) {

                aimPercent = aimSmoother.Smooth(aimPercent, target, deltaTime);
                
                // aimLerp = Mathf.Lerp(aimLerp, target, aimSpeed * deltaTime);
                
                if (isAiming && aimPercent > (1.0f - aimEndThreshold)) {
                    aimPercent = 1.0f;
                }
                else if (!isAiming && aimPercent < aimEndThreshold) {
                    aimPercent = 0.0f;
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
