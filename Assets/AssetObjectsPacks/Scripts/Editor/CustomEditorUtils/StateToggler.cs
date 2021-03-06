﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AssetObjectsPacks {
    public class StateToggler
    {
        //in case state is serializable
        EditorProp stateListProp;
        public HashSet<int> stateList = new HashSet<int>();
        
        public void OnEnable (EditorProp stateListProp) {
            this.stateListProp = stateListProp;

            stateList.Clear();
            if (stateListProp != null) {
                // update to serialized property
                stateList = stateListProp.arraySize.Generate(i => stateListProp[i].intValue ).ToHashSet();
            }
        }
        
        public bool IsState(int id) {
            return stateList.Contains(id);
        }

        //returns true if any states have been toggled
        public bool ToggleState (HashSet<int> idsToToggle) {
        
            if (idsToToggle.Count == 0)  {
                return false;
            }
            foreach (var i in idsToToggle) 
                stateList.ToggleElement(i);
            
            //save to serialized object if saving
            if (stateListProp != null) {
                stateListProp.Clear();
                foreach (var i in stateList) 
                    stateListProp.AddNew().SetValue(i);
            }
            return true;
        }
    }
}
