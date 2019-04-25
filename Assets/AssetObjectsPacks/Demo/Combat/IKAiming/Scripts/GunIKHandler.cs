using UnityEngine;

namespace Combat {

    [RequireComponent(typeof(Gun))]
    public class GunIKHandler : MonoBehaviour
    {
        public GunIKHandlerBehavior behavior;
        public Transform leftElbowHint, rightElbowHint, leftHandHint, rightHandHint;
        [HideInInspector] public Transform ikRig;
        Recoiler recoiler;
        Vector3 originalIKRigOffsetPos;
        Quaternion originalIKRigOffsetRot;
        
        void Awake () {
            recoiler = GetComponent<Recoiler>();
        }
        void Start () {
            BuildIKRig();
        }
        
        //seperate ik rig handling from the base gun transform (for smoother transition)
        void BuildIKRig () {
            ikRig = new GameObject(name + "_ikRig").transform;    
            ikRig.position = transform.position;
            ikRig.rotation = transform.rotation;

            leftElbowHint.SetParent(ikRig);
            rightElbowHint.SetParent(ikRig);
            leftHandHint.SetParent(ikRig);
            rightHandHint.SetParent(ikRig);       

            Transform targetParent = recoiler != null ? recoiler.recoilTransform : transform;
            //set the ik rig parent as the recoiler, so hands move when the gun model moves while aiming
            ikRig.SetParent(targetParent);

            originalIKRigOffsetPos = ikRig.localPosition;
            originalIKRigOffsetRot = ikRig.localRotation;
        }    
        public void ResetIKRig() {
            Transform targetParent = recoiler != null ? recoiler.recoilTransform : transform;
            if (ikRig.parent != targetParent) {
                ikRig.SetParent(targetParent);
                ikRig.localPosition = originalIKRigOffsetPos;
                ikRig.localRotation = originalIKRigOffsetRot;
            }
        }
    }
}
