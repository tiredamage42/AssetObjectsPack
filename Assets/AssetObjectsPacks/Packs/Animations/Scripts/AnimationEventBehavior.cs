





using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetObjectsPacks.Animations {
    public abstract class AnimationEventBehavior : AssetObjectEventBehavior
    //<AnimationAssetObject, AnimationEvent, AnimationEventBehavior, AnimationPlayer>
    //<AnimationEvent, AnimationEventBehavior, AnimationPlayer>

    {
        public abstract void UpdateBehavior (AnimationScene.Performance.PerformanceCue performance_cue, AnimationPlayer actor);
    }
}


