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

        public void SetAiming (bool aiming) {
            isAiming = aiming && currentGun && !controller.scriptedMove;
        }

        public void SetFiring(bool firing) {
            if (currentGun) {
                currentGun.isFiring = aimPercent >= .9f && firing;   
            }
        }

        public float aimPercent;
        ValueTracker gunChangeTracker;
        
        
        void BroadcastGunChange () {
            if (onGunChange != null) {
                onGunChange(currentGun);
            }
        }

        protected override void Awake() {
            base.Awake();
            eventPlayer.AddParameter( new CustomParameter ( "Aiming", () => isAiming ) );

            // change animation states when aiming changes
            controller.AddChangeLoopStateValueCheck( () => isAiming );

            gunChangeTracker = new ValueTracker( () => currentGun, null );
        }
        
        public override void UpdateLoop (float deltaTime) {
            
            if (gunChangeTracker.CheckValueChange()) {
                BroadcastGunChange();
            }

            UpdateAimLerp(deltaTime);        
        }

        const float aimEndThreshold = .001f;
        void UpdateAimLerp (float deltaTime) {
            float target = isAiming ? 1.0f : 0.0f;
            if (aimPercent != target) {

                aimPercent = aimSmoother.Smooth(aimPercent, target, deltaTime);
                
                if (isAiming && aimPercent > (1.0f - aimEndThreshold)) {
                    aimPercent = 1.0f;
                }
                else if (!isAiming && aimPercent < aimEndThreshold) {
                    aimPercent = 0.0f;
                }
            }
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

        public void SetAimTarget (Vector3 target) {
            _aimTarget = target;
        }
        public void SetAimTargetCallback (System.Func<Vector3> getAimTarget) {
            this.getAimTarget = getAimTarget;
        }
    }
}
