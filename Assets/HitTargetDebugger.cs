using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Combat {

    public class HitTargetDebugger : MonoBehaviour
    {

        public Transform target;

        public Turret turretPrefab;
        public float aimHeightOffset = 1.0f;
        public float distanceFromTarget = 3.0f;

        public float timeBetweenShots = 3.0f;
        public int shotsPerTurret = 5;

        Turret[] allTurrets = new Turret[turretCount];

        const int angleSubdivision = 45;
        const int turretCount = 360 / angleSubdivision;

        int firingTurretIndex, turretIndexShotsCount;
        float lastShotTime;
        
        void UpdateTurrets () {
            if (target == null) {
                return;
            }

            // int i = 0;
            Vector3 pos = target.position;
            Vector3 fwd = target.forward * distanceFromTarget;
            Vector3 aim = pos + Vector3.up * aimHeightOffset;

            for (int i = 0; i < turretCount; i++) {
                float x = angleSubdivision * i;

            // for (float x = 0.0f; x <= 360.0f; x+=angleSubdivision) {
                Turret turret = allTurrets[i];
                Vector3 turretPos = pos + (Quaternion.Euler(0, x, 0) * fwd);// * distanceFromTarget;
                turret.transform.position = turretPos;
                Quaternion lookRotation = Quaternion.LookRotation(pos - turretPos);
                turret.transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
                
                turret.SetTargetPosition(aim);

                if (i == firingTurretIndex) {
                    if (Time.time - lastShotTime >= timeBetweenShots) {

                        turret.gun.FireWeapon();
                        lastShotTime = Time.time;
                        turretIndexShotsCount++;

                        if (turretIndexShotsCount >= shotsPerTurret) {
                            turretIndexShotsCount = 0;

                            firingTurretIndex++;
                            if (firingTurretIndex >= turretCount) {
                                firingTurretIndex = 0;
                            }
                        }
                    }
                }


                // i++;
            }
        }

        void BuildTurrets () {
            


            // int i = 0;

            for (int i = 0; i < turretCount; i++) {
            // for (float x = 0.0f; x <= 360.0f; x+=angleSubdivision) {
                allTurrets[i] = Instantiate(turretPrefab);
                // i++;
            }
        }



        void Awake () {
            BuildTurrets();
        }


        // Update is called once per frame
        void Update()
        {
            UpdateTurrets();
        }
    }
}
