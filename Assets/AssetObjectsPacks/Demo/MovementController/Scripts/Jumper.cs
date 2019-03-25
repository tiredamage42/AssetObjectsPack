using UnityEngine;
using AssetObjectsPacks;
using System;

/* 
    Incorporate a simple one shot jump animation
    
    character movement specific
*/
[RequireComponent(typeof(CharacterMovement))]
public class Jumper : MovementControllerComponent
{
    public Cue jumpCue;
    CharacterMovement characterMove;
    protected override void Awake() {
        base.Awake();
        characterMove = GetComponent<CharacterMovement>();
    }

    public override void UpdateLoop(float deltaTime) {

    }

    bool overrideMovement { get { return !characterMove.grounded || controller.overrideMovement; } }
    

    public void Jump (Action onJumpDone = null) {
        if (overrideMovement) return;
        Playlist.InitializePerformance("jumper", jumpCue, eventPlayer, false, eventLayer, Vector3.zero, Quaternion.identity, true, onJumpDone);
    }
}