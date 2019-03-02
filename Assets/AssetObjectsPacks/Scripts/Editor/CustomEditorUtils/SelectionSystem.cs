namespace AssetObjectsPacks {
    public class SelectionSystem : ElementIndexTracker
    {
        public void OnObjectSelection (int i, bool multiple) {
            if (multiple) {
                if (hasElements) {
                    if (i < lo) lo = i;
                    else if (i > hi) hi = i;
                }
                else hi = lo = i;
            }
            else hi = lo = (singleElement && i == lo) ? -1 : i;
        }
        public bool HandlDirectionalSelection (bool up, bool down, bool multiple, int lastIndex) {

            if (lastIndex < 0) return false;
            
            bool changed = false;
            if ((down || up) && !hasElements) {
                hi = lo = down ? 0 : lastIndex;
                changed = true;
            }
            else if (hasElements) {
                bool unMulti = elementCount > 1 && !multiple;
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