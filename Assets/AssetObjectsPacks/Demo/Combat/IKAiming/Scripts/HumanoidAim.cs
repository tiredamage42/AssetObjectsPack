using UnityEngine;
namespace Combat {

    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterCombat))]
    public class HumanoidAim : VariableUpdateScript
    {
        [Range(.1f, 50f)] public float gunfollowSmoothSpeed = 50;

        void Awake() {
            anim = GetComponent<Animator>();
            characterCombat = GetComponent<CharacterCombat>();


            characterCombat.onGunChange += OnGunChange;

            headTransform = anim.GetBoneTransform(HumanBodyBones.Head);
            rightHandTransform = anim.GetBoneTransform(HumanBodyBones.RightHand);
        }
        
        GunIKHandler gunIKHandler;
        CharacterCombat characterCombat;

        void OnGunChange(Gun newGun) {
            gunIKHandler = null;
            Debug.LogError("changed gun");
            if (newGun != null) {
                gunIKHandler = newGun.GetComponent<GunIKHandler>();
            }
        }
        
        
        [Tooltip("0.0 : completely unrestrained in motion\n1.0 : completely clamped (look at becomes impossible)\n0.5 : half the possible range (180 degrees).")] 
        [Range(0,1)] public float lookAtClampWeight = 0.0f;
        
        Transform headTransform, rightHandTransform;
        Animator anim;
        
        Vector3 lastAimTarget;
        public override void UpdateLoop (float deltaTime) {

            lastAimTarget = characterCombat.aimTarget;

if (characterCombat.aimLerp != 0.0f) {
                        AlterLookAtHipBones(characterCombat.aimLerp);
}

            lastHeadPosition = headTransform.position;
            lastHeadRotation = headTransform.rotation;
            
            lastHandPosition = rightHandTransform.position;
            lastHandRotation = rightHandTransform.rotation;



            if (gunIKHandler != null) {
                UpdateAim(deltaTime, lastAimTarget);
            }
        }

        Vector3 startGunAimPos;
        
        void HandleTransitionPositioning (Vector3 gunAimPos, Quaternion gunAimRot, float deltaTime) {
            
            // set ik rig to aim target as soon as aim starts (while in transition).
            // sticking to gun makes left hand go trhough body to reach gun while it's still going up or down from aim.
            // UpdateTransformPositioning(gunIKHandler.ikRig, gunAimPos, gunAimRot, deltaTime, -1);
            if (gunIKHandler.ikRig.parent != headTransform) {

                gunIKHandler.ikRig.SetParent(headTransform);
                gunIKHandler.ikRig.localPosition = gunIKHandler.behavior.localAimHeadPos;

            }
            gunIKHandler.ikRig.rotation = gunAimRot;






            //lerp the gun position from the local hand position to the desired aim position
            // Vector3 noAimPos = rightHandT.position + (rightHandT.rotation * gunIKHandler.behavior.localHipHandPos);// rightHandT.TransformPoint(ikHandler.behavior.localHipHandPos);
            // Quaternion noAimRot = rightHandT.rotation * Quaternion.Euler(gunIKHandler.behavior.localHipHandRot);

            Vector3 noAimPos = lastHandPosition + (lastHandRotation * gunIKHandler.behavior.localHipHandPos);// rightHandT.TransformPoint(ikHandler.behavior.localHipHandPos);
            Quaternion noAimRot = lastHandRotation * Quaternion.Euler(gunIKHandler.behavior.localHipHandRot);

            if (gunIKHandler.transform.parent != headTransform) {

                gunIKHandler.transform.SetParent(headTransform);
                // gunIKHandler.transform.localPosition = gunIKHandler.behavior.localAimHeadPos;
                startGunAimPos = gunIKHandler.transform.localPosition;

            }

            gunIKHandler.transform.localPosition = Vector3.Lerp(startGunAimPos, gunIKHandler.behavior.localAimHeadPos, characterCombat.aimLerp);
            gunIKHandler.transform.rotation = Quaternion.Slerp(noAimRot, gunAimRot, characterCombat.aimLerp);
            

            // UpdateTransformPositioning(gunIKHandler.transform, Vector3.Lerp(noAimPos, gunAimPos, characterCombat.aimLerp), Quaternion.Slerp(noAimRot, gunAimRot, characterCombat.aimLerp), deltaTime, gunfollowSmoothSpeed);        
        }
        void HandleFullAimPositioning (Vector3 gunAimPos, Quaternion gunAimRot, float deltaTime) {
            //resets ik rig to its original offset (now that gun is going to be following the aim position)
            gunIKHandler.ResetIKRig();

            if (gunIKHandler.transform.parent != headTransform) {

                gunIKHandler.transform.SetParent(headTransform);
                gunIKHandler.transform.localPosition = gunIKHandler.behavior.localAimHeadPos;

            }
            gunIKHandler.transform.rotation = gunAimRot;


            //make the gun follow aim position
            // UpdateTransformPositioning(gunIKHandler.transform, gunAimPos, gunAimRot, deltaTime, gunfollowSmoothSpeed);            
        }

        static void UpdateTransformPositioning (Transform transform, Vector3 position, Quaternion rotation, float deltaTime, float smoothSpeed) {
            
            //(keeping transforms with ik targets parented to animated hand or head broke IK)
            if (transform.parent != null) {
                transform.SetParent(null);
            }
            
            // if (smoothSpeed >= 0) {
            //     transform.rotation = Quaternion.Slerp(transform.rotation, rotation, deltaTime * smoothSpeed);
            //     transform.position = Vector3.Lerp(transform.position, position, deltaTime * smoothSpeed);
            // }
            // else {

                transform.position = position;
                transform.rotation = rotation;
            // }
        }

        // stored to get the values after IK
        Vector3 lastHeadPosition, lastHandPosition;
        Quaternion lastHeadRotation, lastHandRotation;

        protected override void LateUpdate() {
            // lastHeadPosition = headTransform.position;
            // lastHeadRotation = headTransform.rotation;
            
            // lastHandPosition = rightHandTransform.position;
            // lastHandRotation = rightHandTransform.rotation;

            base.LateUpdate();
        }


        /*
            fixes too much sway when animation is moving too much and trying to look
        */
        static void ResetZRotation (Transform transform, float aimLerp, float targetZ = 0.0f) {
            Vector3 eulerAngles = transform.rotation.eulerAngles;
            eulerAngles.z = 0;// Mathf.Lerp(eulerAngles.z, targetZ, aimLerp);
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
            
        
        void UpdateAim (float deltaTime, Vector3 aimTarget) {
            if (characterCombat.aimLerp != 0.0f) {
                //Vector3 gunAimPos = headT.position + (headT.rotation * gunIKHandler.behavior.localAimHeadPos);
                Vector3 gunAimPos = lastHeadPosition + (lastHeadRotation * gunIKHandler.behavior.localAimHeadPos);
                
                Quaternion gunAimRot = Quaternion.LookRotation(aimTarget - gunAimPos);
                if (characterCombat.aimLerp == 1.0f) {
                    HandleFullAimPositioning(gunAimPos, gunAimRot, deltaTime);
                    // Debug.Log("woot ");
                }
                else {
                    HandleTransitionPositioning(gunAimPos, gunAimRot, deltaTime);
                }
            }
            else {
                if (gunIKHandler.transform.parent != rightHandTransform) {
                    ResetGunToRightHand();
                }
            }
        }
        void ResetGunToRightHand () {
            gunIKHandler.transform.SetParent(rightHandTransform);
            gunIKHandler.transform.localPosition = gunIKHandler.behavior.localHipHandPos;
            gunIKHandler.transform.localRotation = Quaternion.Euler(gunIKHandler.behavior.localHipHandRot);
        }

        // float lastTime;
        
        void OnAnimatorIK (int layerIndex) {
            Vector3 aimTarget = lastAimTarget;
            
            // if (gunIKHandler != null) {
            //     UpdateAim(Time.deltaTime, aimTarget);
            // }
            
            float ikWeight = characterCombat.aimLerp;

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
            anim.SetLookAtPosition(aimTarget);
        }
    }
}
