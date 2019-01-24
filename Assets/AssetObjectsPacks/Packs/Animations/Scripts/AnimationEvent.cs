

using System.Collections.Generic;
using UnityEngine;

namespace AssetObjectsPacks.Animations {
    public class AnimationEvent : AssetObjectEvent
    //<AnimationAssetObject, AnimationEvent, AnimationEventBehavior, AnimationPlayer>
    //<AnimationEvent, AnimationEventBehavior, AnimationPlayer>

    {
        
            
    
        public AnimationScene animationScene;
        //if the cue should wait for the actor to snap to the interest point (or transform)
        //before being considered ready
        public enum SnapActorStyle { None, Snap, Smooth };
        public SnapActorStyle snapActorStyle;
        public float smoothPositionTime = 1;
        public float smoothRotationTime = 1;    

/*
        protected override void OnTriggered (CorpusAnimator user, List<AnimInstance> filtered_asset_objects) {
            user.Play(filtered_asset_objects.RandomChoice(), looped, false);
        }
 */

        void OnDrawGizmos () {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, .25f);
        }   
    }
}



