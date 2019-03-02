using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
namespace AssetObjectsPacks {

    public class DragAndDrop : ElementIndexTracker
    {

        public bool dragging, checkedDown, hoveringOverReceiver, mouseDown, mouseUp;
        Vector2 origMouseOffset, mousePos;
        int dragStartIndex = -1, dropReceiverIndex = -1, dragStartCollection = -1, dropReceiverCollection = -1;
        
        public bool IsBeingDragged(int i) {
            return dragging && IsTracked(i);
        }

        public bool DrawAndUpdate(Rect[] rects, System.Action<int, Rect> drawDragHoverElement, ElementIndexTracker selected, out int receiverIndex, out int receiverCollection, out int droppedCollection, out IEnumerable<int> droppedIndicies) {
            DrawDragGUIs (rects, drawDragHoverElement);
            CheckNewDragStart (rects, selected);
            return CheckDrop (out receiverIndex, out receiverCollection, out droppedCollection, out droppedIndicies);    
        }

        public void CheckElementRectForDragsOrDrops (int index, int collection, Rect rt, bool isReceiver, bool isDraggable, out bool beingDragged, out bool isDragHovered, out bool dropReceived) {
            
            bool hasMousePos = rt.Contains(mousePos);

            beingDragged = isDraggable && IsBeingDragged(index);
            
            isDragHovered = hasMousePos && dragging && !beingDragged;
            
            if (isDragHovered && isReceiver) hoveringOverReceiver = true;
                
            if (isDraggable && hasMousePos && mouseDown) {
                dragStartIndex = index;
                dragStartCollection = collection;
            }

            dropReceived = isReceiver && isDragHovered && mouseUp;
            if (dropReceived) {
                dropReceiverIndex = index;
                dropReceiverCollection = collection;
            }
        }

        public bool CheckDrop (out int receiverIndex, out int receiverCollection, out int droppedCollection, out IEnumerable<int> droppedIndicies) {
            
            receiverIndex = dropReceiverIndex;
            receiverCollection = dropReceiverCollection;
            droppedCollection = dragStartCollection;

            droppedIndicies = null;
            if (dragging) {
                //Debug.Log("dragging " + lo + " / " + hi);
            }
            if (mouseUp) {
                if (dragging){
                    droppedIndicies = GetTrackedEnumerable().ToArray();
                    dragging = false;
                }
                ClearTracker();
            }
            return mouseUp && dropReceiverIndex != -1;
        }

        public void CheckNewDragStart (Rect[] origRects, ElementIndexTracker selected) {
            if (dragStartIndex == -1) return;
            int i = dragStartIndex;
            
            dragStartIndex = -1;
            dragStartCollection = -1;
            dragging = false;      

            if (elementCount != 0) 
                return;

            //Debug.Log("grabbed index: "+ i);
            if (selected.IsTracked(i)) {
                Copy(selected);
                //Debug.Log("grabbinh selection: " + lo + "/" + hi);
            }
            else {
                lo = hi = i;
                //Debug.Log("grabbinh single: " + lo + "/" + hi);

            } 
            origMouseOffset = new Vector2(origRects[lo].x - mousePos.x, origRects[lo].y - mousePos.y);  
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
            if (!dragging) dragging = e.type == EventType.MouseDrag && elementCount != 0;
        }

        public void DrawDragGUIs (Rect[] rects, System.Action<int, Rect> drawDragHoverElement) {
            if (!dragging || elementCount == 0) return;
            EditorWindow.mouseOverWindow.Repaint();
            if (hoveringOverReceiver) EditorGUIUtility.AddCursorRect(new Rect(mousePos.x-5, mousePos.y-5, 10, 10), MouseCursor.ArrowPlus);
            float x = mousePos.x + origMouseOffset.x;
            float y = mousePos.y + origMouseOffset.y;
            foreach (var i in GetTrackedEnumerable().ToArray()) {
                drawDragHoverElement(i, new Rect(x, y + rects[i].y - rects[lo].y, rects[i].width, rects[i].height));
            }
        }
    }
}
