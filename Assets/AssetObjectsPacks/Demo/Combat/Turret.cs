// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using AssetObjectsPacks;
namespace Combat {
    /*
        tracking only works right side up or upside down, not sideways
    */
    public class Turret : VariableUpdateScript
    {
        public Transform debugTransform;

        public bool active;
        public Smoother swivelSmoother, gunSmoother;
        public float maxLookAngle = 45f;
        public Transform swivelTransform;
        [HideInInspector] public Gun gun;


        public void SetTargetPosition(Vector3 targetPosition) {
            this.targetPosition = targetPosition;
        }
        Vector3 targetPosition;

        public override void UpdateLoop (float deltaTime) {

            if (!active) {
                return;
            }
            if (debugTransform != null) {
                SetTargetPosition(debugTransform.position);
            }

            //rotate turret
            Vector3 targetPointTurret = (targetPosition - swivelTransform.position).normalized; //get normalized vector toward target
            Quaternion targetRotationTurret = Quaternion.LookRotation (targetPointTurret, swivelTransform.up); //get a rotation for the turret that looks toward the target
            swivelTransform.rotation = swivelSmoother.Smooth(swivelTransform.rotation, targetRotationTurret, deltaTime); //gradually turn towards the target at the specified turnSpeed
            swivelTransform.localRotation = Quaternion.Euler(0, swivelTransform.localRotation.eulerAngles.y, 0);
        
            //rotate barrels
            Vector3 targetPointBarrels = (targetPosition - gun.transform.position).normalized; //get a normalized vector towards the target
            Quaternion targetRotationBarrels = Quaternion.LookRotation (targetPointBarrels, gun.transform.up); //get a rotation that looks at the target
            gun.transform.rotation = gunSmoother.Smooth(gun.transform.rotation, targetRotationBarrels, deltaTime); //gradually turn towards the target as the specified turnSpeed
            
            float x = gun.transform.localRotation.eulerAngles.x;
            if (x >= 180) {
                x -= 360;
            }
            
            gun.transform.localRotation = Quaternion.Euler(Mathf.Clamp(x, -maxLookAngle, maxLookAngle), 0, 0);            
        }
                    
        void InitializeGun () {
            gun = GetComponentInChildren<Gun>();
        }

        void Awake () {
            InitializeGun();
        }
    }
}