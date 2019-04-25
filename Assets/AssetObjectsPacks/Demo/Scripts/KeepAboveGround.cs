// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

public class KeepAboveGround : MonoBehaviour
{

    public float minimumY = -5;
    public LayerMask layerMask;
    public float maxY = 500;
    public float buffer = .1f;

    void Update()
    {
        Vector3 myPos = transform.position;
        if (myPos.y < minimumY) {
            Ray ray = new Ray (new Vector3(myPos.x, maxY, myPos.z), Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxY * 2, layerMask)) {
                myPos.y = hit.point.y + buffer;
                transform.position = myPos;
            }
            else {
                Debug.LogWarning(name + " is below ground and cant find ground above, disabling");
                gameObject.SetActive(false);
            }
        }
    }
}
