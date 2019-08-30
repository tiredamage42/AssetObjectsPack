using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetObjectsPacks;

public class ParticleSystemPlayer : EventPlayerListener
{
    static Dictionary<int, HashSet<ParticleSystem>> prefabPools = new Dictionary<int, HashSet<ParticleSystem>>();
    
    static ParticleSystem GetParticleSystem(Object objRef) {
        ParticleSystem psPrefab = (ParticleSystem)objRef;
        int id = psPrefab.GetInstanceID();

        HashSet<ParticleSystem> systems;
        if (!prefabPools.TryGetValue(id, out systems)) {
            systems = new HashSet<ParticleSystem>();
            prefabPools.Add(id, systems);
        }

        ParticleSystem r = null;
        foreach (var ps in systems) {
            if (!ps.isPlaying) {
                r = ps;
                break;
            }
        }
        if (r == null) {
            // Debug.LogWarning("instantiating particle system prefab " + psPrefab.name);
            r = GameObject.Instantiate(psPrefab);
            systems.Add(r);
        }
        return r;
    }
    
    protected override string ListenForPackName() {
        return "ParticleFX";
    }

        /*
            called when the attached event player plays a 
            "ParticleFX" event and chooses an appropriate asset object
        */
        protected override void UseAssetObject(AssetObject assetObject, bool asInterrupter, MiniTransform transforms, HashSet<System.Action> endUseCallbacks) {
            //speed, start width, end width, color, length, light color, light intensity, light range

            // float speed = assetObject["Speed"].GetValue<float>();
            // float scale = assetObject["Scale"].GetValue<float>();

            // ParticleSystem ps = GetParticleSystem(assetObject.objRef);
            // ps.transform.position = transforms.pos;
            // ps.transform.rotation = transforms.rot;
            // if (transforms.targetParent != null) {
            //     ps.transform.SetParent(transforms.targetParent);
            // }

            // ps.transform.localScale = Vector3.one * scale;
            // ParticleSystem.MainModule m = ps.main;
            // m.simulationSpeed = speed;
            // //ps.main = m;

            // ps.Play();

            
            // //end use immediately
            // if (endUseCallbacks != null) {            
            //     foreach (var endUse in endUseCallbacks) {
            //         endUse();    
            //     }
            // }
        }
}
