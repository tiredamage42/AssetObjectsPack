using UnityEngine;
using System.Linq;
using AssetObjectsPacks;

[RequireComponent(typeof(EventPlayer))]
[RequireComponent(typeof(MovementController))]
public abstract class MovementControllerComponent : MonoBehaviour
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