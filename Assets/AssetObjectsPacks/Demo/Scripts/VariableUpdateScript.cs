using UnityEngine;
public abstract class VariableUpdateScript : MonoBehaviour
{
    public enum UpdateMode { Update, FixedUpdate, LateUpdate, Custom };
    public UpdateMode updateMode = UpdateMode.LateUpdate;
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
