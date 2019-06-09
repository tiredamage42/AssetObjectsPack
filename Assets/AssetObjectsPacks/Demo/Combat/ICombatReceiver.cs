using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Combat {

    public interface IDamageable
    {

        void OnDamageReceive(Vector3 shotOrigin, Transform hitTransform, float damage);

    }
}
