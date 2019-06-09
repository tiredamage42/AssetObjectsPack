using UnityEngine;
using AssetObjectsPacks;

namespace Combat {
    /*
        use if an entity has various sub objects (like ragdoll bones)
    */
    public class ActorElement : MonoBehaviour 
    {
        

        public delegate void OnDamageReceiveCallback (Vector3 origin, Transform damagedTransform, float damage, int severity);
        public event OnDamageReceiveCallback onDamageReceive;


        public void OnDamageReceive (Vector3 origin, Transform damagedTransform, float damage, int severity) {
            if (onDamageReceive != null) {
                onDamageReceive(origin, damagedTransform, damage, severity);
            }
            else {
                Debug.LogError(name + " ActorElement doesnt have a callback to call");
            }
        }

        // public Entity baseEntity;
        // public void SetBaseEntity (Entity baseEntity) {
        //     this.baseEntity = baseEntity;
        // }
        // public void OnShot (Vector3 shotOrigin, Transform hitTransform, float damage, int severity) {
        //     if (baseEntity == null) {
        //         Debug.LogError(name + " entity element doesnt have a base entity");
        //         return;
        //     }
        //     baseEntity.OnShot(shotOrigin, hitTransform, damage);
        // }
    }
}
