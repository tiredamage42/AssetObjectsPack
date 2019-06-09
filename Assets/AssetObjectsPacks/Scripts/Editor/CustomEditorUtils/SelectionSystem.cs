
namespace AssetObjectsPacks {
    public class SelectionHandler : ElementIndexTracker
    { 
        public void OnObjectSelection (SelectionElement selected, bool multiple) {
            int i = selected.showIndex;
            if (multiple) {
                if (hasTracked) {
                    if (i < lo) lo = i;
                    else if (i > hi) hi = i;
                }
                else SetTracked(i);
            }
            else SetTracked((singleTracked && i == lo) ? NONE : i);
        }


        public bool HandleDirectionalSelection (bool up, bool down, bool multiple, int lastIndex) {
            if (lastIndex < 0) return false;
            
            bool changed = false;
            if ((down || up) && !hasTracked) {
                SetTracked(down ? 0 : lastIndex);
                changed = true;
            }
            else if (hasTracked) {
                bool unMulti = !singleTracked && !multiple;
                if (down && (hi < lastIndex || unMulti)) {
                    if (hi < lastIndex) hi++;
                    if (unMulti || !multiple) lo = hi;
                    changed = true;
                }
                if (up && (lo > 0 || unMulti)) {
                    if (lo > 0) lo--;
                    if (unMulti || !multiple) hi = lo;
                    changed = true;
                }
            }
            return changed;
        }
    }
}