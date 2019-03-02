using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace AssetObjectsPacks {
    public class StateToggler
    {
        //in case state is serializable
        EditorProp stateListProp;
        HashSet<int> stateList = new HashSet<int>();

        public void OnEnable (EditorProp stateListProp) {
            this.stateListProp = stateListProp;
        }

        public void Initialize (){
            stateList.Clear();
            if (stateListProp != null) {
                stateList = stateListProp.arraySize.Generate(i => stateListProp[i].intValue ).ToHashSet();
            }
        }
        public bool IsState(int id) {
            return stateList.Contains(id);
        }
        bool ToggleState (IEnumerable<int> idsToToggle) {
            if (idsToToggle.Count() == 0)  return false;
            foreach (var i in idsToToggle) {
                if (stateList.Contains(i)) stateList.Remove(i);
                else stateList.Add(i);
            }
            if (stateListProp == null) return true;
            //save to serialized object
            stateListProp.Clear();
            foreach (var i in stateList) stateListProp.AddNew().SetValue(i);
            return true;
        }

        //GUI
        public void ToggleStateButton (GUIContent c, GUIStyle s, GUILayoutOption options, bool hotKey, Func<IEnumerable<int>> getIDsOnToggle, out bool toggleSuccess) {
            toggleSuccess = (GUIUtils.Button(c, s, options) || hotKey) && ToggleState(getIDsOnToggle());
        }
    }
}
