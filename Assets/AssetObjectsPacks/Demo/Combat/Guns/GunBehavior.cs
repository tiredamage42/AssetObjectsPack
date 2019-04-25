using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Combat {

    [CreateAssetMenu(fileName = "New Gun Behavior", menuName = "Combat/Gun Behavior", order = 2)]
    public class GunBehavior : ScriptableObject
    {
        
        public float fireRate = 1f;
        public LayerMask hitMask;
        public float maxDistance = 50;
        public AssetObjectsPacks.Event shootEvent, secondaryEvent;
        
    }
}

