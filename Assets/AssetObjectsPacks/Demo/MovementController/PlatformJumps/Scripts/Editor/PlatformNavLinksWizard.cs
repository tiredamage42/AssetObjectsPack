using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using System.Linq;

using Syd.AI;

namespace Movement {

namespace Platforms {

    public class PlatformNavLinksWizard : ScriptableWizard
    {
        public float maxEdgeSize = .5f;
        public float endEdgeDistance = 1.5f;
        public LayerMask layerMask;
        public Collider[] groundCols;

        [MenuItem("Platforms/Build Platform Nav Links")]
        static void CreateWizard() {
            ScriptableWizard.DisplayWizard<PlatformNavLinksWizard>("Build Platform Nav Links", "Build");
        }

        void OnWizardCreate() {
            BuildPlatformUpLinks();
        }  

        /*
            build an offmesh link object to connect platforms
        */
        GameObject CreatePlatformLink (bool isShort, string name) {
            GameObject link = new GameObject(name);
            GameObject end = new GameObject(name + "-End");

            end.transform.SetParent(link.transform);
            end.transform.localPosition = new Vector3(0, isShort ? Platformer.smallPlatformSize : Platformer.tallPlatformSize, endEdgeDistance);
            
            OffMeshLink linkC = link.AddComponent<OffMeshLink>();
            linkC.activated = true;
            linkC.costOverride = 0.0f;
            linkC.startTransform = link.transform;
            linkC.endTransform = end.transform;      
            return link;  
        }

        void BuildPlatformUpLinks () {
            Transform linkHolder = new GameObject("PlatFormUpLinks").transform;

            // get all static colliders that arent floor colliders (so we dont build platforms inside of geometry)
            Collider[] cols = GameObject.FindObjectsOfType<Collider>().Where( c => c.gameObject.isStatic && !groundCols.Contains(c) ).ToArray();
            
            //make the templates
            GameObject shortPlatformUpLinkPrefab = CreatePlatformLink(true, "ShortPlatformUpLink");
            GameObject tallPlatformUpLinkPrefab = CreatePlatformLink(false, "TallPlatformUpLink");
            
            // get all the navmesh borders, and split them evenly by maxEdgeSize
            HashSet<LineSegment> bordersSegmented = LineSegment.SplitSegments(NavigationManager.FindNavMeshBorders(), maxEdgeSize);
            
            foreach (var b in bordersSegmented) {

                // check if we're inside static geometry
                bool isInCol = false;
                for (int i  = 0; i < cols.Length; i++) {
                    if (cols[i].bounds.Contains(b.midPoint)) {
                        isInCol = true;
                        break;
                    }
                }
                if (isInCol) continue;
                
                bool isShort;
                Vector3 position;
                Quaternion rotation;
                if (Platformer.CheckForPlatformUp(b.midPoint, b.normal, 1.0f, layerMask, out isShort, out position, out rotation) ) {
                    GameObject link = MonoBehaviour.Instantiate(isShort ? shortPlatformUpLinkPrefab : tallPlatformUpLinkPrefab, position, rotation);
                    OffMeshLink linC = link.GetComponent<OffMeshLink>();

                    // if the other end of our generated link isnt on the navmesh, delete it
                    if (!NavMesh.SamplePosition(linC.endTransform.position, out _, .01f, NavMesh.AllAreas)) {
                        MonoBehaviour.DestroyImmediate(link);
                    }
                    else {
                        link.transform.SetParent(linkHolder);
                    }   
                }
            }

            //delete the templates
            MonoBehaviour.DestroyImmediate(shortPlatformUpLinkPrefab);
            MonoBehaviour.DestroyImmediate(tallPlatformUpLinkPrefab);    
        }      
    }
}
}
