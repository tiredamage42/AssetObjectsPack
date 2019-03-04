using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
namespace AssetObjectsPacks {
    public class DragAndDrop : ElementIndexTracker
    {
        public bool dragging, checkedDown, receiverHovered, mouseDown, mouseUp;
        Vector2Int dropInfo, dragInfo;
        GUIContent dragGUI, multipleGUI = new GUIContent("< Multiple >");

        public void DrawDraggedGUIs(){
            if (!dragging || elementCount == 0) return;
            
            EditorWindow.mouseOverWindow.Repaint();

            Vector2 mp = UnityEngine.Event.current.mousePosition;
            if (receiverHovered) EditorGUIUtility.AddCursorRect(new Rect(mp.x-5, mp.y-5, 10, 10), MouseCursor.ArrowPlus);
            
            GUIUtils.Label(new Rect(mp.x, mp.y - 8, GUIStyles.label.CalcSize(dragGUI).x, 16), dragGUI, receiverHovered ? Colors.black : Colors.liteGray);
        }
        
        
        public bool DrawAndUpdate(ElementIndexTracker selected, out int receiverIndex, out int receiverCollection, out int droppedCollection, out IEnumerable<int> droppedIndicies) {
            CheckNewDragStart (selected);
            return CheckDrop (out receiverIndex, out receiverCollection, out droppedCollection, out droppedIndicies);    
        }


        public void CheckElementRectForDragsOrDrops (GUIContent gui, bool hasMousePos, int index, int collection, bool isReceiver, bool isDraggable, out bool beingDragged, out bool receiverHovered, out bool dropReceived) {
            bool dragStarted = isDraggable && hasMousePos && mouseDown;
            beingDragged = isDraggable && dragging && IsTracked(index);
            receiverHovered = isReceiver && hasMousePos && dragging && !beingDragged;
            dropReceived = receiverHovered && mouseUp;
                        
            this.receiverHovered = this.receiverHovered || receiverHovered;

            Vector2Int info = new Vector2Int(index, collection);
            if (dragStarted) {
                dragInfo = info;
                dragGUI = gui;
            }
            if (dropReceived) dropInfo = info;
        }
        
        public bool CheckDrop (out int receiverIndex, out int receiverCollection, out int droppedCollection, out IEnumerable<int> droppedIndicies) {
            
            receiverIndex = dropInfo.x;
            receiverCollection = dropInfo.y;
            droppedCollection = dragInfo.y;

            droppedIndicies = null;
            if (mouseUp) {
                if (receiverIndex != -1) {
                    droppedIndicies = GetTrackedEnumerable().ToArray();
                    dragging = false;
                }
                ClearTracker();
                checkedDown = false;
                dragInfo.y = -1;
            }
            return receiverIndex != -1;
        
        }

        public void CheckNewDragStart (ElementIndexTracker selected) {
            if (dragInfo.x == -1) return;
            dragging = false;      
            if (elementCount != 0) return;
            lo = hi = dragInfo.x;
            if (selected.IsTracked(dragInfo.x)) {
                Copy(selected);
                dragGUI = multipleGUI;
            }
            
        }
        
        public void InputListen (){
            receiverHovered = false;
            dragInfo.x = dropInfo.x = -1;
            
            UnityEngine.Event e = UnityEngine.Event.current;
            mouseDown = e.type == EventType.MouseDown && e.button == 0 && !checkedDown;
            if (mouseDown) checkedDown = true;
            mouseUp = e.type == EventType.MouseUp;
            if (!dragging) dragging = e.type == EventType.MouseDrag && elementCount != 0;
        }
    }
}
