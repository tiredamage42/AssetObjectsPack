using UnityEngine;
using System;
using AssetObjectsPacks;

namespace Movement {
    /* 
        Incorporate a simple one shot jump animation
        
        character movement specific



        (TODO: turn this into general obstacle avoidance for ai...)
    */
    [RequireComponent(typeof(CharacterMovement))]
    public class Jumper : MovementControllerComponent
    {
        // public Cue jumpCue;
        public CueBehavior jumpCueBehavior;
        CharacterMovement characterMove;
        protected override void Awake() {
            base.Awake();
            characterMove = GetComponent<CharacterMovement>();
        }

        public override void UpdateLoop(float deltaTime) {

        }

        public void Jump (Action onJumpDone = null) {
            if (!characterMove.grounded || controller.scriptedMove) return;

            Debug.Log("jumping");
            Playlist.InitializePerformance("jumper", jumpCueBehavior, eventPlayer, false, eventLayer,new MiniTransform( Vector3.zero, Quaternion.identity), true, onJumpDone);
        }
    }
}
