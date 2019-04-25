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
            bool hitTarget = Physics.Raycast(ray, out hit, behavior.maxDistance, behavior.hitMask);
            
            Quaternion lookRotation = hitTarget ? Quaternion.LookRotation(hit.normal) : Quaternion.identity;
            Vector3 endPoint = hitTarget ? hit.point : ray.origin + ray.direction * behavior.maxDistance;
            eventPlayer["HitTarget"].SetValue(hitTarget);

            AssetObjectsPacks.Playlist.InitializePerformance("shoot", behavior.shootEvent, eventPlayer, false, 0, new MiniTransform(endPoint, lookRotation), true, null);
            AssetObjectsPacks.Playlist.InitializePerformance("muzzleflash", behavior.secondaryEvent, eventPlayer, false, 1, new MiniTransform( muzzleTransform, true ), true, null);
        }
    }
}

