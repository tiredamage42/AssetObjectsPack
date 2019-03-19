using UnityEngine;

using UnityEngine.AI;

public class PlatformNavLink : MonoBehaviour
{
    OffMeshLink link;

    void Awake () {
        link = GetComponent<OffMeshLink>();
        link.startTransform = transform;
        Transform endLink = new GameObject("endLink").transform;
        endLink.SetParent(transform);
        endLink.localRotation = Quaternion.identity;
        endLink.localPosition = new Vector3(0, Platformer.tallPlatformSize, Platformer.tallPlatformStartDistance);
        link.endTransform = endLink;
    }

    void OnDrawGizmos () {
        Gizmos.color = new Color32 (255, 125, 0, 255);
        Gizmos.DrawWireSphere(transform.position, .25f);
        Vector3 endPos = CalculateEndPosition();
        Gizmos.DrawLine(transform.position, endPos);
        Gizmos.DrawSphere(endPos, .25f);
    }


    Vector3 CalculateEndPosition () {
        Vector3 upVector = Vector3.up * Platformer.tallPlatformSize;
        Vector3 fwdVector = transform.forward * Platformer.tallPlatformStartDistance;
        return transform.position + fwdVector + upVector;
    }
        

}
