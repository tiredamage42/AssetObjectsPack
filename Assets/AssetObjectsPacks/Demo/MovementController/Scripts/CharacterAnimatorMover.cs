using UnityEngine;

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
    void OnAnimatorMove () {
        characterMovement.SetMoveAndRotationDelta(anim.deltaPosition, anim.deltaRotation.eulerAngles);
    }
}
