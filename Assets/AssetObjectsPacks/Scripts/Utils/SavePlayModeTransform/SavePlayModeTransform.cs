#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using AssetObjectsPacks;

/*
    allows transform changes to be saved in play mode 
    
    and loaded afterwards in the editor
*/
public class SavePlayModeTransform : MonoBehaviour {
    public static Dictionary<int, MiniTransform> savedTransforms = new Dictionary<int, MiniTransform>();
}
#endif