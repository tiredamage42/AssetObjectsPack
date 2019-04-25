using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Combat {


public class LaserBolt : MonoBehaviour
{
    static List<LaserBolt> pool = new List<LaserBolt>();
    static Queue<int> available = new Queue<int>();
    public static LaserBolt GetAvailableBolt () {
        if (available.Count == 0) {
            int key = pool.Count;
            pool.Add(NewLaserBolt(key));
            return pool[key];
        }
        return pool[available.Dequeue()];
    }
    static LaserBolt NewLaserBolt (int key) {
        LaserBolt b = new GameObject("LaserBolt").AddComponent<LaserBolt>();
        b.poolKey = key;
        return b;
    }
    static void ReturnToPool (int key) {
        if (!available.Contains(key)) {
            available.Enqueue(key);
        }
    }

    int poolKey = -1;

    const float maxLife = 5.0f;
    const float arriveThreshold = .025f;
    const float arriveThreshold2 = arriveThreshold * arriveThreshold;
    
    float lifeTimer;
    Vector3 target;

    bool inUse, moveEnd;

    float speed, length;

    LineRenderer lineRenderer;
    Light myLight;


    static Material _boltSharedMaterial = null;
    static Material boltSharedMaterial {
        get {
            if (_boltSharedMaterial == null) {
                _boltSharedMaterial = new Material(Shader.Find("Hidden/LaserBolt"));
            }
            return _boltSharedMaterial;
        }
    }


    void Awake () {
        myLight = GetComponent<Light>();
        if (myLight == null) {
            myLight = gameObject.AddComponent<Light>();
        }
        myLight.bounceIntensity = 0.0f;
        myLight.shadows = LightShadows.None;
        myLight.type = LightType.Point;
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null) {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        lineRenderer.sharedMaterial = boltSharedMaterial;
        
        DisableBolt(false);
    }

    void DisableBolt (bool returnToPool = true) {
        inUse = false;
        moveEnd = false;
        myLight.enabled = false;
        lineRenderer.enabled = false;

        if (returnToPool) {
            ReturnToPool(poolKey);
        }
    }


    public void FireBolt (
        Vector3 origin, Vector3 target, 
        float speed, float startWidth, float endWidth, float length, int capVerts, 
        Color32 color, float alphaSteepness, float colorSteepness, 
        Color32 lightColor, float lightIntensity, float lightRange
    ){
        
        this.target = target;
        transform.position = origin;
        transform.rotation = Quaternion.LookRotation(target - origin);

        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
        lineRenderer.numCapVertices = capVerts;

        
        lineRenderer.SetPosition(0, origin);

        lineRenderer.SetPosition(1, origin + transform.forward * length);
            

        lineRenderer.enabled = true;
        

        this.speed = speed;
        this.length = length;

        Material boltMaterial = lineRenderer.material;

        boltMaterial.SetColor("_TintColor", color);
        boltMaterial.SetFloat("_AlphaSteepness", alphaSteepness);
        boltMaterial.SetFloat("_ColorSteepness", colorSteepness);

        myLight.color = lightColor;
        myLight.intensity = lightIntensity;
        myLight.range = lightRange;
        myLight.enabled = true;
        
        lifeTimer = 0; 
        inUse = true;
        moveEnd = true;
        skippedFrame = false;
    }
    bool skippedFrame;
    void Update () {
        if (inUse) {
            if (!skippedFrame) {
                skippedFrame = true;
                return;
            }

            lifeTimer += Time.deltaTime;
            if (lifeTimer >= maxLife) {
                DisableBolt();
                return;
            }


            transform.position += transform.forward * Time.deltaTime * speed;

            Vector3 newStartPoint = transform.position;

            lineRenderer.SetPosition(0, newStartPoint);

            Vector3 end = target;
            if (moveEnd) {
                end = newStartPoint + transform.forward * length;
            }
            lineRenderer.SetPosition(1, end);
            
            if (Vector3.SqrMagnitude(target - newStartPoint) <= arriveThreshold2) {
                DisableBolt();
                return;
            }   

            if (moveEnd) {
                if (Vector3.SqrMagnitude(target - end) <= arriveThreshold2) {
                    moveEnd = false;
                }
            }
        }
    }
}
}

