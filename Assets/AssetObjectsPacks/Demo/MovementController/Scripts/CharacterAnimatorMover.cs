using UnityEngine;

namespace Movement {
    /*
    
        attach to move the character movemnt component with animator's root motion
    */
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterMovement))]
    public class CharacterAnimatorMover : MonoBehaviour
    {
        Animator anim;
        CharacterMovement characterMovement;
        void Awake () {     
            characterMovement = GetComponent<CharacterMovement>();
            anim = GetComponent<Animator>();
            anim.applyRootMotion = false;   
        }

        // void OnEnable () {
            // Debug.Log("setting anim pos on enable");
            // characterMovement.SetMoveAndRotationDelta(Vector3.zero, Vector3.zero);
        // }

        public bool setPosition = true, setRotation = true;
        void OnAnimatorMove () {

            if (setPosition) {
                characterMovement.SetMoveDelta(anim.deltaPosition);
            }
            if (setRotation) {
                characterMovement.SetRotationDelta(anim.deltaRotation.eulerAngles);
            }

            // Debug.Log("setting anim pos");
            // characterMovement.SetMoveAndRotationDelta(anim.deltaPosition, anim.deltaRotation.eulerAngles);
        }
    }
}
