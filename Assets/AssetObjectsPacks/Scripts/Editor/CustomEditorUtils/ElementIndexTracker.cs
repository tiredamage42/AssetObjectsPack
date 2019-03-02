//using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetObjectsPacks {
    public abstract class ElementIndexTracker {
        public int lo = -1, hi = -1;
        public bool hasElements { get { return hi != -1 && lo != -1; } }
        public bool singleElement { get { return lo == hi && lo != -1; }}
        public int elementCount { get { return hasElements ? (hi - lo) + 1 : 0; } }

        public bool HasSingleElement (out int index) {
            index = lo;
            return singleElement;
        }
        
        public void ClearTracker () {
            lo = hi = -1;
        }
        public bool IsTracked (int i) {
            return hasElements && i >= lo && i <= hi;
        }
        protected void Copy(ElementIndexTracker other) {
            lo = other.lo;
            hi = other.hi;
        }
        public IEnumerable<int> GetTrackedEnumerable () {
            if (!hasElements) return null;
            return new Vector2Int(lo, hi).Generate();
            //for (int i = lo; i <= hi; i++) yield return i;
        }        
    }
}
