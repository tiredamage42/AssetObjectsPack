using UnityEngine;
namespace AssetObjectsPacks {
    public abstract class ElementIndexTracker {

        protected const int NONE = -1;
        public int lo = NONE, hi = NONE;
        public Vector2Int trackedRange { get { return new Vector2Int(lo, hi); } } 
        public bool hasTracked { get { return lo != NONE; } }
        public bool singleTracked { get { return hasTracked && lo == hi; } }
        
        public void Clear () {
            SetTracked(NONE);
        }
        public void SetTracked(Vector2Int newRange) {
            lo = newRange.x;
            hi = newRange.y;
        }
        public void SetTracked(int singleTrackedIndex) {
            lo = hi = singleTrackedIndex;
        }
        public bool IsTracked(SelectionElement element) {
            return IsTracked(element.showIndex);
        }     
        public bool IsTracked(int showIndex) {
            return hasTracked && showIndex >= lo && showIndex <= hi;
        }     
    }
}
