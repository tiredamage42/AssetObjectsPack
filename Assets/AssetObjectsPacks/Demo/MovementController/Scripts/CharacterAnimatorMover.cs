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
        System.Func<Vector3, Vector3> moveModifier;
        void Awake () {     
            characterMovement = GetComponent<CharacterMovement>();
            anim = GetComponent<Animator>();
            anim.applyRootMotion = false;   
        }

        public void SetMoveModifier (System.Func<Vector3, Vector3> moveModifier) {
            this.moveModifier = moveModifier;
        }
        void OnAnimatorMove () {
            if (setPosition) {
                Vector3 move = anim.deltaPosition;
                if (moveModifier != null) {
                    move = moveModifier(move);
                }
                characterMovement.SetMoveDelta(move);
            }
            if (setRotation) {
                characterMovement.SetRotationDelta(anim.deltaRotation.eulerAngles);
            }
        }
    }
}
