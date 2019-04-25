using System.Collections.Generic;
using UnityEngine;
using AssetObjectsPacks;


namespace Combat {

    /*

        takes in laser bolt events
    */
    public class LaserPlayer : EventPlayerListener
    {

        protected override string ListenForPackName() {
            return "LaserBolts";
        }
        public Transform muzzleTransform;
        
        protected override void Awake () {
            base.Awake();
            if (muzzleTransform == null) {
                muzzleTransform = transform;
            }
        }

        

        /*
            called when the attached event player plays a 
            "LaserBolts" event and chooses an appropriate asset object

            played at laser hit point
        */
        protected override void UseAssetObject(AssetObject assetObject, bool asInterrupter, MiniTransform transforms, HashSet<System.Action> endUseCallbacks) {
            //speed, start width, end width, color, length, light color, light intensity, light range

            float speed = assetObject["Speed"].GetValue<float>();
            float startWidth = assetObject["Start Width"].GetValue<float>();
            float endWidth = assetObject["End Width"].GetValue<float>();
            float length = assetObject["Length"].GetValue<float>();


            int capVerts = assetObject["Cap Verts"].GetValue<int>();
            Color32 color = assetObject["Color"].GetValue<Color32>();
            float alphaSteepness = assetObject["Alpha Steepness"].GetValue<float>();
            float colorSteepness = assetObject["Color Steepness"].GetValue<float>();
            
            
            Color32 lightColor = assetObject["Light Color"].GetValue<Color32>();
            float lightIntensity = assetObject["Light Intensity"].GetValue<float>();
            float lightRange = assetObject["Light Range"].GetValue<float>();

            LaserBolt.GetAvailableBolt().FireBolt(
                muzzleTransform.position,
                transforms.pos, speed, startWidth, endWidth, length, capVerts, 
                color, alphaSteepness, colorSteepness, 
                lightColor, lightIntensity, lightRange
            );
            

            //end laser bolts immediately
            if (endUseCallbacks != null) {            
                foreach (var endUse in endUseCallbacks) {
                    endUse();    
                }
            }
        }
}
}
