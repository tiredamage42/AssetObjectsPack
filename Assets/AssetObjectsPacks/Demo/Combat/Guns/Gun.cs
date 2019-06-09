using UnityEngine;
using AssetObjectsPacks;

namespace Combat {

    public class Gun : MonoBehaviour
    {
        public GunBehavior behavior;
        public Transform rayOrigin, muzzleTransform;
        public bool isFiring;
        EventPlayer eventPlayer;
        float fireTimer;
        bool wasFiring;

        void Awake () {
            eventPlayer = GetComponent<EventPlayer>();

            eventPlayer.AddParameter(new CustomParameter("HitTarget", false));
            eventPlayer.AddParameter(new CustomParameter("HitTargetTag", ""));
            
            if (rayOrigin == null) {
                rayOrigin = transform;
            }
            if (muzzleTransform == null) {
                muzzleTransform = transform;
            }
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

        public void FireWeapon () {

            RaycastHit hit;
            Ray ray = new Ray (rayOrigin.position, rayOrigin.forward);
            bool hitTarget = Physics.Raycast(ray, out hit, behavior.maxDistance, behavior.hitMask, QueryTriggerInteraction.Ignore);
            Quaternion lookRotation = hitTarget ? Quaternion.LookRotation(hit.normal) : Quaternion.identity;
            Vector3 endPoint = hitTarget ? hit.point : ray.origin + ray.direction * behavior.maxDistance;
            
            eventPlayer["HitTarget"].SetValue(hitTarget);
            
            if (hitTarget) {
                //set hit target type (blood, concrete, wood, etc...)
                eventPlayer["HitTargetTag"].SetValue(hit.transform.tag);
            }

            AssetObjectsPacks.Playlist.InitializePerformance("shoot", behavior.shootEvent, eventPlayer, false, 0, new MiniTransform(endPoint, lookRotation), true, null);
            AssetObjectsPacks.Playlist.InitializePerformance("muzzleflash", behavior.secondaryEvent, eventPlayer, false, 1, new MiniTransform( muzzleTransform, true ), true, null);

            if (hitTarget) {

                Debug.LogError("hit " + hit.transform.name);

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

