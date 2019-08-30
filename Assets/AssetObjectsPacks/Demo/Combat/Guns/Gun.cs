using UnityEngine;
// using AssetObjectsPacks;

using Game.FX;

namespace Game.Combat {

    public class Gun : MonoBehaviour
    {
        public Recoiler recoiler;
        public GunBehavior behavior;
        public Transform rayOrigin, muzzleTransform, shellEjectTransform;
        
        public bool isFiring;
        // EventPlayer eventPlayer;
        float fireTimer;
        bool wasFiring;

        void Awake () {
            // eventPlayer = GetComponent<EventPlayer>();

            // eventPlayer.AddParameter(new CustomParameter("HitTarget", false));
            // eventPlayer.AddParameter(new CustomParameter("HitTargetTag", ""));
            
            if (rayOrigin == null) {
                rayOrigin = transform;
            }
            if (muzzleTransform == null) {
                muzzleTransform = transform;
            }

            recoiler = GetComponent<Recoiler>();
        }

        void HandleAutoFire () {
            if (isFiring) {
            
                if (!wasFiring) {
                    fireTimer = behavior.fireRate;
                }
            
                fireTimer += Time.deltaTime;

                if (fireTimer >= behavior.fireRate) {
                    FireWeapon();
                    fireTimer = 0;
                }
            }
            wasFiring = isFiring;
        }
        void Update () {
            HandleAutoFire();
        }

        public void SetRayTransform (Transform newRayOrigin) {
            rayOrigin = newRayOrigin;
        }


        // public System.Action<Vector3, Vector3, Transform> onFireGun;

        CombatSettings _combatSettings;
        CombatSettings combatSettings {
            get {
                if (_combatSettings == null) _combatSettings = GameSettings.GetGameSettings<CombatSettings>();
                return _combatSettings;
            }
        }

        ParticleFX DecideTracerFX () {
            if (behavior.gunFX.tracerFX != null) {
                return behavior.gunFX.tracerFX;
            }
            return combatSettings.gunFX.tracerFX;
        }
        ParticleFX DecideShellEjectFX () {
            if (behavior.gunFX.shellFX != null) {
                return behavior.gunFX.shellFX;
            }
            return combatSettings.gunFX.shellFX;
        }

        // use override from behavior, else use default settings
        ParticleFX DecideMuzzleFlashFX () {
            if (behavior.gunFX.muzzleFlashFX != null) {
                return behavior.gunFX.muzzleFlashFX;
            }
            return combatSettings.gunFX.muzzleFlashFX;
        }

        ParticleFX DecideImpactFX (Transform hitTransform) {
            ParticleFX particlesToUse = behavior.gunFX.impacts.GetParticleFX(hitTransform);
            if (particlesToUse == null) {
                particlesToUse = combatSettings.gunFX.impacts.GetParticleFX(hitTransform);
            }
            return particlesToUse;
        }




        public void FireWeapon () {
            recoiler.StartRecoil();

            RaycastHit hit;
            Ray ray = new Ray (rayOrigin.position, rayOrigin.forward);
            bool hitTarget = Physics.Raycast(ray, out hit, behavior.maxDistance, behavior.hitMask, QueryTriggerInteraction.Ignore);
            
            Quaternion lookRotation = hitTarget ? Quaternion.LookRotation(hit.normal) : Quaternion.identity;
            Vector3 endPoint = hitTarget ? hit.point : ray.origin + ray.direction * behavior.maxDistance;
            
            // if (onFireGun != null) {
            //     onFireGun(ray.origin, endPoint, hitTarget ? hit.transform : null);
            // }

            // eventPlayer["HitTarget"].SetValue(hitTarget);
            
            // if (hitTarget) {
            //     //set hit target type (blood, concrete, wood, etc...)
            //     eventPlayer["HitTargetTag"].SetValue(hit.transform.tag);
            // }

            // AssetObjectsPacks.Playlist.InitializePerformance("shoot", behavior.shootEvent, eventPlayer, false, 0, new MiniTransform(endPoint, lookRotation), true, null);
            // AssetObjectsPacks.Playlist.InitializePerformance("muzzleflash", behavior.secondaryEvent, eventPlayer, false, 1, new MiniTransform( muzzleTransform, true ), true, null);

            ParticleFX muzzleFlashInstance = ParticleFX.PlayParticlesPrefab(DecideMuzzleFlashFX(), muzzleTransform.position, muzzleTransform.rotation, behavior.muzzleFlashSpeed, behavior.muzzleFlashSize);

            ParticleFX tracerInstance = ParticleFX.PlayParticlesPrefab(DecideTracerFX(), muzzleTransform.position, Quaternion.LookRotation(endPoint - muzzleTransform.position), behavior.tracerSpeed, behavior.tracerSize);
            
            if (shellEjectTransform != null) {
                ParticleFX shellsInstance = ParticleFX.PlayParticlesPrefab(DecideShellEjectFX(), shellEjectTransform.position, shellEjectTransform.rotation, behavior.shellEjectSpeed, behavior.shellEjectSize);
            }

            if (hitTarget) {

                ParticleFX impactInstance = ParticleFX.PlayParticlesPrefab(DecideImpactFX(hit.transform), endPoint, lookRotation, behavior.impactSpeed, behavior.impactSize);


                // Debug.LogError("hit " + hit.transform.name);

                ActorElement actorElement = hit.transform.GetComponent<ActorElement>();

                if (actorElement != null) {
                    actorElement.OnDamageReceive(ray.origin, hit.transform, behavior.damage, behavior.severity);
                }


                //add rb force
                Rigidbody rb = hit.transform.GetComponent<Rigidbody>();
                if (rb != null) {
                    rb.AddForceAtPosition(ray.direction.normalized * behavior.force, hit.point, ForceMode.VelocityChange);
                }
            }
        }
    }
}

