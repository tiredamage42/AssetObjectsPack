//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;


using AssetObjectsPacks;



namespace Game.Combat {

   
public class Recoiler : MonoBehaviour// VariableUpdateScript// EventPlayerListener
{
    public RecoilBehavior behavior;
    // protected override string ListenForPackName() {
    //     return "Recoil";
    // }

    class RecoilLerpHandler {
        
        public float curLerp;
        Smoother smoother = new Smoother();
        Vector2 speeds;
        public bool towardsRecoil, inRecoil;
        
        public void StartRecoil (Vector2 speeds, Smoother.SmoothMethod smoothType){// int smoothType) {
            towardsRecoil = true;
            inRecoil = true;
            this.speeds = speeds;
            smoother.smoothMethod = (Smoother.SmoothMethod)smoothType;
        }

        public void UpdateLerp (System.Action onRecoilEnd, float deltaTime) {
            float targ = towardsRecoil ? 1.0f : 0.0f;
            if (curLerp != targ) {
                smoother.speed = towardsRecoil ? speeds.x : speeds.y;
                curLerp = smoother.Smooth(curLerp, targ, deltaTime);
            }

            if (towardsRecoil) {
                if (curLerp > .99f) {
                    curLerp = 1.0f;
                    towardsRecoil = false;
                }
            }
            else {
                if (curLerp < .01f) {
                    curLerp = 0.0f;
                    inRecoil = false;
                    onRecoilEnd();
                }
            }

        }
    }
    
    RecoilLerpHandler posRecoilHandler = new RecoilLerpHandler();
    RecoilLerpHandler rotRecoilHandler = new RecoilLerpHandler();
    Vector3 recoilPosOffset, recoilRotOffset;
    // int positionUpdateMode, rotationUpdateMode;


    void Update () {
        UpdateLoop(VariableUpdateScript.UpdateMode.Update, Time.deltaTime);
    }
    void FixedUpdate () {
        UpdateLoop(VariableUpdateScript.UpdateMode.FixedUpdate, Time.fixedDeltaTime);
    }
    void LateUpdate () {
        UpdateLoop(VariableUpdateScript.UpdateMode.LateUpdate, Time.deltaTime);
    }

    
    void UpdateLoop (VariableUpdateScript.UpdateMode updateMode, float deltaTime) {
        UpdateRecoilPosition(updateMode, deltaTime);
        UpdateRecoilRotation(updateMode, deltaTime);
    }

    void UpdateRecoilPosition (VariableUpdateScript.UpdateMode updateMode, float deltaTime) {
        // if (updateMode != positionUpdateMode) {
        if (updateMode != behavior.position.updateMode) {
        
            return;
        }

        posRecoilHandler.UpdateLerp(() => recoilTransform.localPosition = originalLocalPos, deltaTime);
        
        if (posRecoilHandler.inRecoil) {
            recoilTransform.localPosition = Vector3.Lerp(originalLocalPos, originalLocalPos + recoilPosOffset, posRecoilHandler.curLerp);
            
        }

    }
    void UpdateRecoilRotation (VariableUpdateScript.UpdateMode updateMode, float deltaTime) {
        // if (updateMode != rotationUpdateMode) {
        if (updateMode != behavior.rotation.updateMode) {
        
            return;
        }

        rotRecoilHandler.UpdateLerp(() => recoilTransform.localRotation = originalLocalRot, deltaTime);
        if (rotRecoilHandler.inRecoil) {
            recoilTransform.localRotation = Quaternion.Slerp(originalLocalRot, Quaternion.Euler(originalLocalRot.eulerAngles + recoilRotOffset), rotRecoilHandler.curLerp);
        }
    }

    float GetRandomSignedValue(float value, float randomSignOn) {
        return value == 0 || randomSignOn != 1.0f ? value : (Random.value < .5f ? -value : value);
    }

    // float GetMaskedRandomValue (float value, float mask) {
    //     return value == 0 || mask != 0.0f ? value : (Random.value < .5f ? -value : value);
    // }


    public void StartRecoil () {
        posRecoilHandler.StartRecoil(behavior.position.toFromSpeed, behavior.position.smoothMethod);
        rotRecoilHandler.StartRecoil(behavior.rotation.toFromSpeed, behavior.rotation.smoothMethod);

        recoilPosOffset = new Vector3(
            GetRandomSignedValue(behavior.position.targetOffset.x, behavior.position.offsetRandomSign.x),
            GetRandomSignedValue(behavior.position.targetOffset.y, behavior.position.offsetRandomSign.y),
            GetRandomSignedValue(behavior.position.targetOffset.z, behavior.position.offsetRandomSign.z)    
        );

        recoilRotOffset = new Vector3(
            GetRandomSignedValue(behavior.rotation.targetOffset.x, behavior.rotation.offsetRandomSign.x),
            GetRandomSignedValue(behavior.rotation.targetOffset.y, behavior.rotation.offsetRandomSign.y),
            GetRandomSignedValue(behavior.rotation.targetOffset.z, behavior.rotation.offsetRandomSign.z)    
        );

    }

    // protected override void UseAssetObject(AssetObject assetObject, bool asInterrupter, MiniTransform transforms, System.Collections.Generic.HashSet<System.Action> endUseCallbacks) {

    //     positionUpdateMode = assetObject["Pos Update"].GetValue<int>();
    //     rotationUpdateMode = assetObject["Rot Update"].GetValue<int>();

    //     posRecoilHandler.StartRecoil(assetObject["Pos Speeds"].GetValue<Vector2>(), assetObject["Pos Smooth"].GetValue<int>());
    //     rotRecoilHandler.StartRecoil(assetObject["Rot Speeds"].GetValue<Vector2>(), assetObject["Rot Smooth"].GetValue<int>());

    //     Vector3 positionOffset = assetObject["Pos Offset"].GetValue<Vector3>();
    //     Vector3 positionOffsetMask = assetObject["Pos Offset Mask"].GetValue<Vector3>();

    //     recoilPosOffset = new Vector3(
    //         GetMaskedRandomValue(positionOffset.x, positionOffsetMask.x),
    //         GetMaskedRandomValue(positionOffset.y, positionOffsetMask.y),
    //         GetMaskedRandomValue(positionOffset.z, positionOffsetMask.z)    
    //     );

    //     Vector3 rotationOffset = assetObject["Rot Offset"].GetValue<Vector3>();
    //     Vector3 rotationOffsetMask = assetObject["Rot Offset Mask"].GetValue<Vector3>();

    //     recoilRotOffset = new Vector3(
    //         GetMaskedRandomValue(rotationOffset.x, rotationOffsetMask.x),
    //         GetMaskedRandomValue(rotationOffset.y, rotationOffsetMask.y),
    //         GetMaskedRandomValue(rotationOffset.z, rotationOffsetMask.z)    
    //     );

    //     //end use immediately
    //     if (endUseCallbacks != null) {            
    //         foreach (var endUse in endUseCallbacks) {
    //             endUse();    
    //         }
    //     }
    // }

        


    public Transform recoilTransform;
    Vector3 originalLocalPos;
    Quaternion originalLocalRot;

    // protected override 
    void Awake() {
        // base.Awake();
        originalLocalPos = recoilTransform.localPosition;
        originalLocalRot = recoilTransform.localRotation;
    }
}
}
