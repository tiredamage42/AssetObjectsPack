using UnityEngine;
using System.Linq;
using AssetObjectsPacks;

[RequireComponent(typeof(EventPlayer))]
[RequireComponent(typeof(MovementController))]
public abstract class MovementControllerComponent : MonoBehaviour
{
    public enum UpdateMode { Update, FixedUpdate, LateUpdate, Custom };
    public UpdateMode updateMode = UpdateMode.LateUpdate;
    public int eventLayer;
    protected MovementController controller;
    protected EventPlayer eventPlayer;
    protected MovementBehavior behavior { get { return controller.behavior; } }
    protected virtual void Awake () {
        controller = GetComponent<MovementController>();
        eventPlayer = GetComponent<EventPlayer>();
    }

    public abstract void UpdateLoop (float deltaTime);

    void UpdateLoop(UpdateMode updateMode, float deltaTime) {
        if (this.updateMode == updateMode) {
            UpdateLoop(deltaTime);
        }
    }
    protected virtual void FixedUpdate () {
        UpdateLoop(UpdateMode.FixedUpdate, Time.fixedDeltaTime);
    }
    protected virtual void LateUpdate () {
        UpdateLoop(UpdateMode.LateUpdate, Time.deltaTime);
    }
    protected virtual void Update () {
        UpdateLoop(UpdateMode.Update, Time.deltaTime);
    }
    

}