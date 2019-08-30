using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.FX {

    public class ParticleFX : MonoBehaviour
    {
        static Dictionary<int, HashSet<ParticleFX>> prefabPools = new Dictionary<int, HashSet<ParticleFX>>();

        public static ParticleFX PlayParticlesPrefab(ParticleFX prefab, Vector3 position, Quaternion rotation, float speed, float size) {
            ParticleFX instance = GetParticles(prefab);
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.Play(speed, size);
            return instance;
        }
        public static ParticleFX GetParticles(ParticleFX prefab) {
            
            int id = prefab.GetInstanceID();

            HashSet<ParticleFX> systems;
            if (!prefabPools.TryGetValue(id, out systems)) {
                systems = new HashSet<ParticleFX>();
                prefabPools.Add(id, systems);
            }

            ParticleFX r = null;
            foreach (var ps in systems) {
                if (!ps.isPlaying) {
                    r = ps;
                    break;
                }
            }
            if (r == null) {
                // Debug.LogWarning("instantiating particle system prefab " + prefab.name);
                r = GameObject.Instantiate(prefab);
                systems.Add(r);
            }
            return r;
        }

        ParticleSystem ps;
        void Awake () {
            ps = GetComponent<ParticleSystem>();
        }
        public virtual bool isPlaying { get { return ps.isPlaying; } }
        
        public virtual void Play (float speed, float scale) {
                    
            ps.transform.localScale = Vector3.one * scale;
            ParticleSystem.MainModule m = ps.main;
            m.simulationSpeed = speed;
            //ps.main = m;
            ps.Play();
        }

    }
}
