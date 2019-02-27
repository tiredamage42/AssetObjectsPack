using System.Collections.Generic;
using UnityEngine;

namespace AssetObjectsPacks {
    public class StateToggler
    {
        //in case state is serializable
        EditorProp stateListProp;
        HashSet<int> stateList = new HashSet<int>();

        public void Initialize (EditorProp stateListProp){
            stateList.Clear();
            this.stateListProp = stateListProp;
            if (stateListProp != null) stateList = stateList.Generate(stateListProp.arraySize, i => stateListProp[i].intValue );
        }
        public bool IsState(int id) {
            return stateList.Contains(id);
        }
        void ToggleState (HashSet<int> idsToToggle) {
            foreach (var i in idsToToggle) {
                if (stateList.Contains(i)) stateList.Remove(i);
                else stateList.Add(i);
            }
            if (stateListProp == null) return;
            //save to serialized object
            stateListProp.Clear();
            foreach (var i in stateList) stateListProp.AddNew().SetValue(i);
        }

        //GUI
        public void ToggleStateButton (bool hotKey, GUIContent c, GUIStyle s, GUILayoutOption options, out bool toggleSuccess, System.Func<HashSet<int>> getIDsOnToggle) {
            bool attempt = GUIUtils.Button(c, s, Colors.liteGray, Colors.black, options) || hotKey;
            toggleSuccess = false;
            if (attempt) {
                Debug.Log("Attemtping state toggle");
                HashSet<int> idsToToggle = getIDsOnToggle();
                toggleSuccess = idsToToggle.Count != 0;
                if (toggleSuccess) {

                    ToggleState(idsToToggle);
                    Debug.Log("toggle success");
                }
            }
        }
    }
}
