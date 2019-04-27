using UnityEngine;

namespace Movement {
    /*
    
        attach to move the character movemnt component with animator's root motion
    */
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterMovement))]
    public class CharacterAnimatorMover : MonoBehaviour
    {
        public bool setPosition = true, setRotation = true;
        Animator anim;
        CharacterMovement characterMovement;
        void Awake () {     
            characterMovement = GetComponent<CharacterMovement>();
            anim = GetComponent<Animator>();
            anim.applyRootMotion = false;   
        }
        void OnAnimatorMove () {
            if (setPosition) {
                characterMovement.SetMoveDelta(anim.deltaPosition);
            }
            if (setRotation) {
                characterMovement.SetRotationDelta(anim.deltaRotation.eulerAngles);
            }
        }
    }
}
