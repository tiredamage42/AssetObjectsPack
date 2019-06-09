using UnityEngine;
namespace Combat {

    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterCombat))]
    public class HumanoidAim : MonoBehaviour
    {
        [Range(0,1)] public float gunSwitchToHeadPercent = .75f;
        [Tooltip("0.0 : completely unrestrained in motion\n1.0 : completely clamped (look at becomes impossible)\n0.5 : half the possible range (180 degrees).")] 
        [Range(0,1)] public float lookAtClampWeight = 0.0f;
        GunIKHandler gunIKHandler;
        CharacterCombat characterCombat;


        public void SetRightHandTransform (Transform rightHand) {
            if (rightHand == null) {
                rightHand = anim.GetBoneTransform(HumanBodyBones.RightHand);
            } 
            //bool switchParent = gunIKHandler != null && gunIKHandler.transform.parent == rightHandTransform;

            rightHandTransform = rightHand;

            // if (switchParent) {
            //     Vector3 lPos = gunIKHandler.transform.localPosition;
            //     Quaternion lRot = gunIKHandler.transform.localRotation;

            //     gunIKHandler.transform.SetParent(rightHandTransform);
            //     gunIKHandler.transform.localPosition = lPos;
            //     gunIKHandler.transform.localRotation = lRot;
                
            // }
        
        }
            
        
        Transform headTransform, rightHandTransform;
        Animator anim;
        Vector3 startGunAimPos;
        Quaternion startGunAimRot;


        void Awake() {
            anim = GetComponent<Animator>();
            characterCombat = GetComponent<CharacterCombat>();

            characterCombat.onGunChange += OnGunChange;

            headTransform = anim.GetBoneTransform(HumanBodyBones.Head);
            rightHandTransform = anim.GetBoneTransform(HumanBodyBones.RightHand);
        }
        
        
        void OnGunChange(Gun newGun) {
            gunIKHandler = null;
            if (newGun != null) {
                gunIKHandler = newGun.GetComponent<GunIKHandler>();
            }
        }
        
        
        void HandlePositioning (Vector3 aimTarget, float aimLerp) {
            
            Vector3 gunAimPos = headTransform.position + (headTransform.rotation * gunIKHandler.behavior.localAimHeadPos);
            Quaternion gunAimRot = Quaternion.LookRotation(aimTarget - gunAimPos);
            
            if (aimLerp != 0.0f) {
                   
                // set ik rig to aim target as soon as aim starts (while in transition).
                // sticking to gun makes left hand go trhough body to reach gun while it's still going up or down from aim.
            
                if (aimLerp != 1.0f && gunIKHandler.ikRig.parent != headTransform) {
                    gunIKHandler.ikRig.SetParent(headTransform);
                    gunIKHandler.ikRig.localPosition = gunIKHandler.behavior.localAimHeadPos;
                }

                if (gunIKHandler.ikRig.parent == headTransform) {
                    gunIKHandler.ikRig.rotation = gunAimRot;
                }

                if (aimLerp == 1.0f) {
                    //Debug.LogError("woooo");
                    //Debug.Break();
                    //resets ik rig to its original offset (now that gun is going to be following the aim position)
                    gunIKHandler.ResetIKRig();
                }
            }
            else {
                // ik rig not needed, reset to gun parent
                gunIKHandler.ResetIKRig();
            }
                
            //lerp the gun position from the local hand position to the desired aim position
            if (aimLerp >= gunSwitchToHeadPercent) {
                SetTargetParentIfNot(headTransform);
                SmoothPositioningIf (
                    characterCombat.isAiming,
                    gunIKHandler.behavior.localAimHeadPos, 
                    Quaternion.Inverse(headTransform.rotation) * gunAimRot, 
                    (aimLerp - gunSwitchToHeadPercent) / (1.0f - gunSwitchToHeadPercent)
                );                
            }
            else {
                SetTargetParentIfNot(rightHandTransform);
                SmoothPositioningIf (
                    !characterCombat.isAiming,
                    gunIKHandler.behavior.localHipHandPos, 
                    Quaternion.Euler(gunIKHandler.behavior.localHipHandRot), 
                    (gunSwitchToHeadPercent - aimLerp) / gunSwitchToHeadPercent
                );
            }
        }

        void SetTargetParentIfNot (Transform targetParent) {
            if (gunIKHandler.transform.parent != targetParent) {
                gunIKHandler.transform.SetParent(targetParent);
                startGunAimPos = gunIKHandler.transform.localPosition;
                startGunAimRot = gunIKHandler.transform.localRotation;
            }
        }

        void SmoothPositioningIf (bool smooth, Vector3 targetLocalPos, Quaternion targetLocalRot, float t) {
            if (smooth) {
                gunIKHandler.transform.localPosition = Vector3.Lerp(startGunAimPos, targetLocalPos, t);
                gunIKHandler.transform.localRotation = Quaternion.Slerp(startGunAimRot, targetLocalRot, t);
            }
        }

        // stored to get the values after IK
        void LateUpdate() {
            if (characterCombat.aimPercent != 0.0f) {
                AlterLookAtHipBones(characterCombat.aimPercent);
            }
            if (gunIKHandler != null) {
                HandlePositioning(characterCombat.aimTarget, characterCombat.aimPercent);
            }
        }


        /*
            fixes too much sway when animation is moving too much and trying to look
        */
        static void ResetZRotation (Transform transform, float aimLerp, float targetZ = 0.0f) {
            Vector3 eulerAngles = transform.rotation.eulerAngles;
            eulerAngles.z = targetZ;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(eulerAngles), aimLerp);
        }

        void AlterLookAtHipBones (float aimLerp) {

            ResetZRotation(anim.GetBoneTransform(HumanBodyBones.Hips), aimLerp);
            ResetZRotation(anim.GetBoneTransform(HumanBodyBones.Spine), aimLerp);
            // ResetZRotation(anim.GetBoneTransform(HumanBodyBones.Chest));
            // ResetZRotation(anim.GetBoneTransform(HumanBodyBones.UpperChest));
            // ResetZRotation(anim.GetBoneTransform(HumanBodyBones.Neck));
            // ResetZRotation(anim.GetBoneTransform(HumanBodyBones.Head));   
        }
            
        void OnAnimatorIK (int layerIndex) {
            float ikWeight = characterCombat.aimPercent;

            //set weights
            anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, ikWeight);
            anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, ikWeight);
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, ikWeight);
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, ikWeight);
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);

            if (gunIKHandler != null) {
                //set ik values
                anim.SetIKHintPosition(AvatarIKHint.LeftElbow, gunIKHandler.leftElbowHint.position);
                anim.SetIKHintPosition(AvatarIKHint.RightElbow, gunIKHandler.rightElbowHint.position);
                anim.SetIKPosition(AvatarIKGoal.RightHand, gunIKHandler.rightHandHint.position);
                anim.SetIKRotation(AvatarIKGoal.RightHand, gunIKHandler.rightHandHint.rotation);
                anim.SetIKPosition(AvatarIKGoal.LeftHand, gunIKHandler.leftHandHint.position);
                anim.SetIKRotation(AvatarIKGoal.LeftHand, gunIKHandler.leftHandHint.rotation);
            }

            //set look at
            anim.SetLookAtWeight(ikWeight, 1.0f, 1.0f, 0.0f, lookAtClampWeight);
            anim.SetLookAtPosition(characterCombat.aimTarget);
        }
    }
}
