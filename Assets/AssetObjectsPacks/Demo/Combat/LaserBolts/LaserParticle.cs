using UnityEngine;

using Game.FX;
namespace Game.Combat {

    public class LaserParticle : ParticleFX
    {
        public LaserBehavior laserBehavior;
        bool laserActive;
        public override bool isPlaying { get { return laserActive; } }

        public override void Play(float speed, float scale) {
            laserActive = true;

            Transform boltTransform = myLight.transform;

            boltTransform.position = transform.position;
            boltTransform.rotation = transform.rotation;

            lineRenderer.startWidth = laserBehavior.startWidth;
            lineRenderer.endWidth = laserBehavior.endWidth;
            lineRenderer.numCapVertices = laserBehavior.capVerts;

            lineRenderer.SetPosition(0, boltTransform.position);
            lineRenderer.SetPosition(1, boltTransform.position + boltTransform.forward * laserBehavior.length);
            
            lineRenderer.enabled = true;
            
            Material boltMaterial = lineRenderer.material;
            boltMaterial.SetColor("_TintColor", laserBehavior.color);
            boltMaterial.SetFloat("_AlphaSteepness", laserBehavior.alphaSteepness);
            boltMaterial.SetFloat("_ColorSteepness", laserBehavior.colorSteepness);

            myLight.color = laserBehavior.lightColor;
            myLight.intensity = laserBehavior.lightIntensity;
            myLight.range = laserBehavior.lightRange;
            myLight.enabled = true;
            
            lifeTimer = 0; 
            
            moveEnd = true;

            RaycastHit hit;
            if (Physics.Raycast(new Ray (boltTransform.position, boltTransform.forward), out hit, 100, laserBehavior.hitMask, QueryTriggerInteraction.Ignore)) {
                endPoint = hit.point;

            }
            else {
                endPoint = boltTransform.position + boltTransform.forward * 500;
            }
        }

        void BuildBoltObject () {
            GameObject boltObject = new GameObject("bolt");
            
            myLight = boltObject.AddComponent<Light>();
            myLight.bounceIntensity = 0.0f;
            myLight.shadows = LightShadows.None;
            myLight.type = LightType.Point;

            lineRenderer = boltObject.AddComponent<LineRenderer>();
            lineRenderer.sharedMaterial = boltSharedMaterial;
        }

        const float maxLife = 5.0f;
        const float arriveThreshold = .25f;
        const float arriveThreshold2 = arriveThreshold * arriveThreshold;
        
        float lifeTimer;
        bool moveEnd;

        LineRenderer lineRenderer;
        Light myLight;
        Vector3 endPoint;


        static Material _boltSharedMaterial = null;
        static Material boltSharedMaterial {
            get {
                if (_boltSharedMaterial == null) _boltSharedMaterial = new Material(Shader.Find("Hidden/LaserBolt"));
                return _boltSharedMaterial;
            }
        }

        void Awake () {
            BuildBoltObject();
            DisableBolt();
        }

        void DisableBolt () {
            
            myLight.enabled = false;
            lineRenderer.enabled = false;
            laserActive = false;
            moveEnd = false;
        }

        void Update () {
            if (laserActive) {
                
                lifeTimer += Time.deltaTime;
                if (lifeTimer >= maxLife) {
                    DisableBolt();
                    return;
                }

                Transform boltTransform = myLight.transform;
                Vector3 fwd = boltTransform.forward;

                float delta = Mathf.Min(Vector3.Distance(endPoint, boltTransform.position), Time.deltaTime * laserBehavior.speed);
                
                boltTransform.position += fwd * delta;
                
                Vector3 newStartPoint = boltTransform.position;

                if (Vector3.SqrMagnitude(endPoint - newStartPoint) <= arriveThreshold2) {
                    DisableBolt();
                    return;
                }   

                if (moveEnd) {
                    if (Vector3.SqrMagnitude(endPoint - endPoint) <= arriveThreshold2) {
                        moveEnd = false;
                    }
                }
                lineRenderer.SetPosition(0, newStartPoint);
                lineRenderer.SetPosition(1, moveEnd ? newStartPoint + fwd * laserBehavior.length : endPoint);
            }
        }
    }
}

