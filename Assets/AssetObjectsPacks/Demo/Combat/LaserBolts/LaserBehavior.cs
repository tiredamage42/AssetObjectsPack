using UnityEngine;

namespace Game.Combat {

    [CreateAssetMenu()]
    public class LaserBehavior : ScriptableObject
    {
        public LayerMask hitMask = -1;
        public float speed = 5;
        [Header("Line Renderer")]
        public float startWidth = .1f;
        public float endWidth = .1f;
        public float length = 1;
        public int capVerts = 2;

        [Header("Material")]
        public Color32 color = Color.red;
        public float alphaSteepness = 1;
        public float colorSteepness = 1;

        [Header("Light")]
        public Color32 lightColor = Color.red;
        public float lightIntensity = 1;
        public float lightRange = 5;
    }
}