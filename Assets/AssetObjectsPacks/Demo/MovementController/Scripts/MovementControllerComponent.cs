using System.Collections;
using UnityEngine;
using System;
using AssetObjectsPacks;
public abstract class MovementControllerComponent
{
    protected MovementController movementController;
    protected EventPlayer eventPlayer { get { return movementController.eventPlayer; } }
    protected MovementBehavior behavior { get { return movementController.behavior; } }
    protected Transform transform { get { return movementController.transform; } }
    public void Initialize(MovementController movementController) {
        this.movementController = movementController;
    }

    /*
        utilities for setting variables via cue messages

    */
    /*
        parameters:
            layer (internally set), enabled, delaytime (optional), duration (optional)
    */
    protected void SetByMessage (object[] parameters, System.Action<bool, float, float> enableFN) { 
        bool enabledValue = (bool)parameters[1];

        float delayTime = 0;
        float duration = -1;
        if (parameters.Length > 2) {
            delayTime = (float)parameters[2];
        }
        if (parameters.Length > 3) {
            duration = (float)parameters[3];
        }
        enableFN(enabledValue, delayTime, duration); 
    }



    protected IEnumerator EnableAfterDelay (Action<bool, float, float> self, bool enabled, float delay, float duration) {
        yield return new WaitForSeconds(delay);
        self(enabled, 0, duration);
    }

    /*
        self: the method calling this
    */
    protected void EnableAfterDelay(Action<bool, float, float> self, bool enabled, float delay, float duration, Action<bool> enableFN) {
        if (delay > 0) {
            movementController.StartCoroutine(EnableAfterDelay(self, enabled, delay, duration));
            return;
        }
        enableFN(enabled);
        if (duration >= 0) {
            movementController.StartCoroutine(EnableAfterDelay(self, !enabled, duration, -1));
        }
    }



    
}