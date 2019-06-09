using System.Collections;
using System.Collections.Generic;
using UnityEngine;






[CreateAssetMenu()]
public class HumanoidActorBehavior : ScriptableObject
{
    [System.Serializable] public class BoneProfile {
        public HumanBodyBones bone;
        public float damageMultiplier;
        public BoneProfile(HumanBodyBones bone, float damageMultiplier) {
            this.bone = bone;
            this.damageMultiplier = damageMultiplier;
        }
    }
    


    public BoneProfile[] bones = new BoneProfile[] {
        new BoneProfile(HumanBodyBones.Hips, 1),
        new BoneProfile(HumanBodyBones.Chest, 1), 
        new BoneProfile(HumanBodyBones.Head, 2), 
        new BoneProfile(HumanBodyBones.RightLowerLeg, .25f), 
        new BoneProfile(HumanBodyBones.LeftLowerLeg, .25f),
        new BoneProfile(HumanBodyBones.RightUpperLeg, .25f), 
        new BoneProfile(HumanBodyBones.LeftUpperLeg, .25f),
        new BoneProfile(HumanBodyBones.RightLowerArm, .5f), 
        new BoneProfile(HumanBodyBones.LeftLowerArm, .5f),
        new BoneProfile(HumanBodyBones.RightUpperArm, .5f),      
        new BoneProfile(HumanBodyBones.LeftUpperArm, .5f),
                
    };
}
