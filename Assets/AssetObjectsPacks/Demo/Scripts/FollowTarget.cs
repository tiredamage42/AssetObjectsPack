using UnityEngine;
    
public class FollowTarget : VariableUpdateScript
{
    public Transform m_Target;  
    [SerializeField] private float m_MoveSpeed = 1f;  

    public override void UpdateLoop (float deltaTime) {
        if (m_Target == null) return;
        // Move the rig towards target position.

        /*
            maybe if enough distance in target transform
        */
        transform.position = Vector3.Lerp(transform.position, m_Target.position, deltaTime*m_MoveSpeed);
    }        
}