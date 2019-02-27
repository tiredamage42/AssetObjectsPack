using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
namespace AssetObjectsPacks {

    public class DragAndDrop 
    {
        public bool hasDrags { get { return hi != -1 && lo != -1; } }
        public int dragCount { get { return hasDrags ? (hi - lo) + 1 : 0; } }

        public bool dragging, checkedDown, hoveringOverReceiver, mouseDown, mouseUp;

        Vector2 origMouseOffset, mousePos;

        int lo = -1, hi = -1, dragStartIndex = -1, dropReceiverIndex = -1, dragStartCollection = -1, dropReceiverCollection = -1;
        
        public bool IsBeingDragged(int i) {
            return dragging && hasDrags && i>= lo && i <= hi;
        }

        public void CheckElementRectForDragsOrDrops (int index, int collection, Rect rt, bool isReceiver, bool isDraggable, out bool beingDragged, out bool isDragHovered, out bool dropReceived) {
            
            bool hasMousePos = rt.Contains(mousePos);

            beingDragged = isDraggable && IsBeingDragged(index);
            
            isDragHovered = hasMousePos && dragging && !beingDragged;
            
            if (isDragHovered && isReceiver) hoveringOverReceiver = true;
                
            //bool startedDrag = isDraggable && hasMousePos && mouseDown;
            if (isDraggable && hasMousePos && mouseDown) {
                dragStartIndex = index;
                dragStartCollection = collection;
                //Debug.Log("started drag: " + index);
            }

            dropReceived = isReceiver && isDragHovered && mouseUp;
            if (dropReceived) {
                dropReceiverIndex = index;
                dropReceiverCollection = collection;
            }

            //if (hoveringOverReceiver) {
            //    Debug.Log("hovering over receiver");
            //}
        }

        public bool CheckDrop (out int receiverIndex, out int receiverCollection, out int droppedCollection, out IEnumerable<int> droppedIndicies) {
            
            receiverIndex = dropReceiverIndex;
            receiverCollection = dropReceiverCollection;
            droppedCollection = dragStartCollection;

            droppedIndicies = null;
            if (mouseUp) {
                droppedIndicies = new int[dragCount].Generate( i => i + lo );// GetDraggedIndiciesEnumerator().ToArray();

                lo = hi = -1;
                dragging = false;
            }
            return mouseUp && dropReceiverIndex != -1;
        }

        public void CheckNewDragStart (Rect[] origRects, SelectionSystem selectionSystem) {
            if (dragStartIndex == -1) return;
            int i = dragStartIndex;
            dragStartIndex = -1;
            dragStartCollection = -1;
            dragging = false;      
            
            if (dragCount != 0) return;
            
            if (selectionSystem.IsSelected(i)) {
                lo = selectionSystem.lo;
                hi = selectionSystem.hi;
                
            }
            else lo = hi = i;

            //Debug.Log(lo + "/" + hi);
            origMouseOffset = new Vector2(origRects[lo].x - mousePos.x, origRects[lo].y - mousePos.y);  
        }

        public IEnumerable<int> GetDraggedIndiciesEnumerator () {
            for (int i = lo; i <= hi; i++) yield return i;
        }

        
        public void InputListen (){
            hoveringOverReceiver = false;
            dragStartIndex = -1;
            dropReceiverIndex = -1;


            UnityEngine.Event e = UnityEngine.Event.current;
            mousePos = e.mousePosition;

            mouseDown = e.type == EventType.MouseDown && e.button == 0 && !checkedDown;
            if (mouseDown) checkedDown = true;
                
            mouseUp = e.type == EventType.MouseUp;
            if (mouseUp) checkedDown = false;
                
            if (!dragging) dragging = e.type == EventType.MouseDrag && dragCount != 0;
        }

        public void DrawDragGUIs (Rect[] origRects, System.Action<int, Rect> drawDragHoverElement) {
            if (!dragging || dragCount == 0) return;

            EditorWindow.mouseOverWindow.Repaint();
            
            if (hoveringOverReceiver) EditorGUIUtility.AddCursorRect(new Rect(mousePos.x-5, mousePos.y-5, 10, 10), MouseCursor.ArrowPlus);
            
            float firstDragY = origRects[lo].y;
            float x = mousePos.x + origMouseOffset.x;
            float y = mousePos.y + origMouseOffset.y;
            
            foreach (var index in GetDraggedIndiciesEnumerator()) {
                //Debug.Log("draing index " + index);
                Rect r = origRects[index];
                drawDragHoverElement(index, new Rect(x, y + r.y - firstDragY, r.width, r.height));
            }                
        }
    }
}
