using System.Collections.Generic;
namespace AssetObjectsPacks {
    public class SelectionSystem 
    {
        public bool hasSelection { get { return hi != -1 && lo != -1; } }
        public bool singleSelection { get { return hi == lo && lo != -1; } }
        public int selectionCount { get { return hasSelection ? (hi - lo) + 1 : 0; } }

        int lo = -1, hi = -1;

        public bool SingleSelection(out int index) { 
            index = lo;
            return singleSelection;
        }
        public IEnumerable<int> GetSelectionEnumerator () {
            return new int[selectionCount].Generate(i => i + lo);
        }
        public void ClearSelection () {
            hi = lo = -1;
        }
        public bool IsSelected(int i) {
            return hasSelection && i >= lo && i <= hi;        
        }
        public void OnObjectSelection (int i, bool multiple) {
            if (multiple) {
                if (hasSelection) {
                    if (i < lo) lo = i;
                    else if (i > hi) hi = i;
                }
                else hi = lo = i;
            }
            else hi = lo = (singleSelection && i == lo) ? -1 : i;
        }
        public bool HandlDirectionalSelection (bool up, bool down, bool multiple, int lastIndex) {
            bool changed = false;
            if ((down || up) && selectionCount == 0) {
                hi = lo = down ? 0 : lastIndex;
                changed = true;
            }
            else if (selectionCount != 0) {
                bool unMulti = selectionCount > 1 && !multiple;
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