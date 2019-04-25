using UnityEngine;
using AssetObjectsPacks;

namespace Movement {

[RequireComponent(typeof(EventPlayer))]
[RequireComponent(typeof(MovementController))]
public abstract class MovementControllerComponent : VariableUpdateScript
{
    public int eventLayer;
    protected MovementController controller;
    protected EventPlayer eventPlayer;
    protected MovementBehavior behavior { get { return controller.behavior; } }
    protected virtual void Awake () {
        controller = GetComponent<MovementController>();
        eventPlayer = GetComponent<EventPlayer>();
    }
}
}