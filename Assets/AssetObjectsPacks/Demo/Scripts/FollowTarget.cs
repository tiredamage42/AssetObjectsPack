using UnityEngine;
using AssetObjectsPacks;    

public class FollowTarget : VariableUpdateScript
{

    public Transform m_Target;  
    public Smoother followSmooth;

    public override void UpdateLoop (float deltaTime) {
        if (m_Target == null) return;
        /*
            Move the rig towards target position.
            
            maybe if enough distance in target transform
        */
        transform.position = followSmooth.Smooth(transform.position, m_Target.position, deltaTime);
    }        
}