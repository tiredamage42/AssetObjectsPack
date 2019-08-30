using UnityEngine;

namespace Game.Combat {

    [RequireComponent(typeof(Gun))]
    public class GunIKHandler : MonoBehaviour
    {
        public GunIKHandlerBehavior behavior;
        public Transform leftElbowHint, rightElbowHint, leftHandHint, rightHandHint;
        [System.NonSerialized] public Transform runtimeIKRig;
        Recoiler recoiler;
        Vector3 originalIKRigOffsetPos;
        Quaternion originalIKRigOffsetRot;
        
        void Awake () {
            recoiler = GetComponent<Recoiler>();
        }
        void Start () {
            BuildIKRig();
        }

        Transform gunParent { get { return recoiler != null ? recoiler.recoilTransform : transform; } }
        
        //seperate ik rig handling from the base gun transform (for smoother transition)
        void BuildIKRig () {
            runtimeIKRig = new GameObject(name + "_ikRig").transform;    
            runtimeIKRig.position = transform.position;
            runtimeIKRig.rotation = transform.rotation;

            leftElbowHint.SetParent(runtimeIKRig);
            rightElbowHint.SetParent(runtimeIKRig);
            leftHandHint.SetParent(runtimeIKRig);
            rightHandHint.SetParent(runtimeIKRig);       

            //set the ik rig parent as the recoiler, so hands move when the gun model moves while aiming
            runtimeIKRig.SetParent(gunParent);
            originalIKRigOffsetPos = runtimeIKRig.localPosition;
            originalIKRigOffsetRot = runtimeIKRig.localRotation;
        }    
        public void ResetIKRig() {
            if (runtimeIKRig.parent != gunParent) {

                //Debug.LogError("woooo");
                //Debug.Break();
                    
                runtimeIKRig.SetParent(gunParent);
                runtimeIKRig.localPosition = originalIKRigOffsetPos;
                runtimeIKRig.localRotation = originalIKRigOffsetRot;
            }
        }
    }
}
