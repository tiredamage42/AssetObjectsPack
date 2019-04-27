//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;


using AssetObjectsPacks;
public class Recoiler : EventPlayerListener
{
    protected override string ListenForPackName() {
        return "Recoil";
    }

    class RecoilLerpHandler {
        
        public float curLerp;
        Smoother smoother = new Smoother();
        Vector2 speeds;
        public bool towardsRecoil, inRecoil;
        
        public void StartRecoil (Vector2 speeds, int smoothType) {
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
    int positionUpdateMode, rotationUpdateMode;


    void Update () {
        UpdateLoop(0, Time.deltaTime);
    }
    void FixedUpdate () {
        UpdateLoop(1, Time.fixedDeltaTime);
    }
    void LateUpdate () {
        UpdateLoop(2, Time.deltaTime);
    }

    
    void UpdateLoop (int checkUpdate, float deltaTime) {
        UpdateRecoilPosition(checkUpdate, deltaTime);
        UpdateRecoilRotation(checkUpdate, deltaTime);
    }

    void UpdateRecoilPosition (int checkUpdate, float deltaTime) {
        if (checkUpdate != positionUpdateMode) {
            return;
        }

        posRecoilHandler.UpdateLerp(() => recoilTransform.localPosition = originalLocalPos, deltaTime);
        
        if (posRecoilHandler.inRecoil) {
            recoilTransform.localPosition = Vector3.Lerp(originalLocalPos, originalLocalPos + recoilPosOffset, posRecoilHandler.curLerp);
        }

    }
    void UpdateRecoilRotation (int checkUpdate, float deltaTime) {
        if (checkUpdate != rotationUpdateMode) {
            return;
        }

        rotRecoilHandler.UpdateLerp(() => recoilTransform.localRotation = originalLocalRot, deltaTime);
        if (rotRecoilHandler.inRecoil) {
            recoilTransform.localRotation = Quaternion.Slerp(originalLocalRot, Quaternion.Euler(originalLocalRot.eulerAngles + recoilRotOffset), rotRecoilHandler.curLerp);
        }
    }



    float GetMaskedRandomValue (float value, float mask) {

        return value == 0 || mask != 0.0f ? value : (Random.value < .5f ? -value : value);
    }

    protected override void UseAssetObject(AssetObject assetObject, bool asInterrupter, MiniTransform transforms, System.Collections.Generic.HashSet<System.Action> endUseCallbacks) {

        positionUpdateMode = assetObject["Pos Update"].GetValue<int>();
        rotationUpdateMode = assetObject["Rot Update"].GetValue<int>();

        posRecoilHandler.StartRecoil(assetObject["Pos Speeds"].GetValue<Vector2>(), assetObject["Pos Smooth"].GetValue<int>());
        rotRecoilHandler.StartRecoil(assetObject["Rot Speeds"].GetValue<Vector2>(), assetObject["Rot Smooth"].GetValue<int>());

        Vector3 positionOffset = assetObject["Pos Offset"].GetValue<Vector3>();
        Vector3 positionOffsetMask = assetObject["Pos Offset Mask"].GetValue<Vector3>();

        recoilPosOffset = new Vector3(
            GetMaskedRandomValue(positionOffset.x, positionOffsetMask.x),
            GetMaskedRandomValue(positionOffset.y, positionOffsetMask.y),
            GetMaskedRandomValue(positionOffset.z, positionOffsetMask.z)    
        );

        Vector3 rotationOffset = assetObject["Rot Offset"].GetValue<Vector3>();
        Vector3 rotationOffsetMask = assetObject["Rot Offset Mask"].GetValue<Vector3>();

        recoilRotOffset = new Vector3(
            GetMaskedRandomValue(rotationOffset.x, rotationOffsetMask.x),
            GetMaskedRandomValue(rotationOffset.y, rotationOffsetMask.y),
            GetMaskedRandomValue(rotationOffset.z, rotationOffsetMask.z)    
        );

        //end use immediately
        if (endUseCallbacks != null) {            
            foreach (var endUse in endUseCallbacks) {
                endUse();    
            }
        }
    }

        


    public Transform recoilTransform;
    Vector3 originalLocalPos;
    Quaternion originalLocalRot;

    protected override void Awake() {
        base.Awake();
        originalLocalPos = recoilTransform.localPosition;
        originalLocalRot = recoilTransform.localRotation;
    }
}
