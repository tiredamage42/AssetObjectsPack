﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Game.FX;
namespace Game.Combat {

    [CreateAssetMenu(fileName = "New Gun Behavior", menuName = "Combat/Gun Behavior", order = 2)]
    public class GunBehavior : ScriptableObject
    {
        public int severity = 1;
        public float force = 10;
        public float damage = 10;
        
        public float fireRate = 1f;
        public LayerMask hitMask;
        public float maxDistance = 50;
        public AssetObjectsPacks.Event shootEvent, secondaryEvent;

        public GunFX gunFX;

        public float muzzleFlashSpeed = 1;
        public float muzzleFlashSize = 1;

        
        public float impactSpeed = 1;
        public float impactSize = 1;

        public float tracerSpeed=1;
        public float tracerSize=1;
        public float shellEjectSpeed=1;
        public float shellEjectSize=1;


        
    }
}

