using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidActor : MonoBehaviour
{
    public static int Bone2Index (HumanBodyBones bone) {
			switch (bone) {
				case HumanBodyBones.Hips: return 0;
				case HumanBodyBones.Chest:  return 1;
				case HumanBodyBones.Head: return 2;
				case HumanBodyBones.RightLowerLeg: return 3;
				case HumanBodyBones.LeftLowerLeg: return 4;
				case HumanBodyBones.RightUpperLeg:  return 5;
				case HumanBodyBones.LeftUpperLeg: return 6;
				case HumanBodyBones.RightLowerArm:  return 7;
				case HumanBodyBones.LeftLowerArm: return 8;
				case HumanBodyBones.RightUpperArm:  return 9;
				case HumanBodyBones.LeftUpperArm: return 10;
			}
			return -1;
		}

    public static HumanBodyBones[] humanBones = new HumanBodyBones[] {
        HumanBodyBones.Hips, //HIPS NEEDS TO BE FIRST
        
        HumanBodyBones.Chest, 
        HumanBodyBones.Head, 
        
        HumanBodyBones.RightLowerLeg, 
        HumanBodyBones.LeftLowerLeg, 
        HumanBodyBones.RightUpperLeg, 
        HumanBodyBones.LeftUpperLeg, 

        HumanBodyBones.RightLowerArm, 
        HumanBodyBones.LeftLowerArm, 
        HumanBodyBones.RightUpperArm, 
        HumanBodyBones.LeftUpperArm, 
    };


    // Transform[] myBones = new Transform[bonesCount];
    Dictionary<int, HumanBodyBones> transform2Bone = new Dictionary<int, HumanBodyBones>();

    void InitializeBoneReferences (Animator animator) {
        for (int i = 0; i < bonesCount; i++) {
            
            Transform boneTransform = animator.GetBoneTransform(humanBones[i]);
            
            transform2Bone.Add(boneTransform.GetInstanceID(), humanBones[i]);
        }
    }

    void InitializeBoneActorElements (Animator animator) {
        for (int i = 0; i < bonesCount; i++) {
            Transform boneTransform = animator.GetBoneTransform(humanBones[i]);
            // Combat.EntityElement e_el = boneTransform.gameObject.AddComponent<Combat.EntityElement>();
            boneTransform.gameObject.AddComponent<Combat.ActorElement>();
            
            // e_el.SetBaseEntity(entity);
        }
    }







    public readonly static int bonesCount = humanBones.Length;


    public HumanoidActorBehavior behavior;

    DynamicRagdoll.RagdollController ragdollController;

    // Dictionary<HumanBodyBones, HumanoidActorBehavior.BoneProfile> bone2profile = new Dictionary<HumanBodyBones, HumanoidActorBehavior.BoneProfile>();

    // void InitializeBehavior () {
    //     bone2profile.Clear();

    //     for (int i = 0; i < behavior.bones.Length; i++) {
    //         bone2profile.Add(behavior.bones[i].bone, behavior.bones[i]);
    //     }
    // }


    // HashSet<int> containedTransforms = new HashSet<int>();


    // public bool ContainsTransform (Transform transform) {
    //     return containedTransforms.Contains(transform.GetInstanceID());
    // }


    
    Combat.Actor entity;
    void Awake () {
        entity = GetComponent<Combat.Actor>();
        entity.SetDamageAdjuster(AdjustDamage);

        ragdollController = GetComponent<DynamicRagdoll.RagdollController>();
        
        Animator animator = ragdollController != null ? ragdollController.ragdoll.GetComponent<Animator>() : GetComponent<Animator>();
        InitializeBoneReferences(animator);
        InitializeBoneActorElements(animator);

        // InitializeBehavior();

    }

    float AdjustDamage(Vector3 shotOrigin, Transform hitTransform, float damage, int severity) {
        float damageMultiplier = 1;

        HumanBodyBones hitBone;
        if (transform2Bone.TryGetValue(hitTransform.GetInstanceID(), out hitBone)) {

            damageMultiplier = behavior.bones[Bone2Index(hitBone)].damageMultiplier;
        }
        else {
            Debug.LogError(name +  " does not contain transform: " + hitTransform.name);
        }
        
        // if (this.ContainsTransform(hitTransform, out hitBone)) {
        // }

        return damage * damageMultiplier;
    }

}
