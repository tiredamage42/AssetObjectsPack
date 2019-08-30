// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

using Game.FX;
namespace Game.Combat {

    [System.Serializable] public class ImpactFX {
        [System.Serializable] public class TaggedImpact {
            public string tag;
            public ParticleFX particle;
        }
        public ParticleFX defaultImpact;
        public TaggedImpact[] taggedImpacts;

        public ParticleFX GetParticleFX (Transform hitTransform) {
            for (int i = 0; i < taggedImpacts.Length; i++) {
                if (hitTransform.CompareTag(taggedImpacts[i].tag)) {
                    return taggedImpacts[i].particle;
                }
            }
            return defaultImpact;
        }
    }

    [System.Serializable] public class GunFX {
        public ParticleFX muzzleFlashFX;
        public ParticleFX tracerFX;
        public ParticleFX shellFX;
        public ImpactFX impacts;

    }


        
    [CreateAssetMenu()]
    public class CombatSettings : GameSettings
    {
        public GunFX gunFX;
       
    }
}
