using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace AssetObjectsPacks {

    public class DragAndDropHandler : ElementIndexTracker
    {

        public void OnEnable (Action<SelectionElement, Vector2Int> onDropCallback) {
            this.onDropCallback = onDropCallback;
        }
        
        Action<SelectionElement, Vector2Int> onDropCallback;    
        public HashSet<SelectionElement> shownElements = new HashSet<SelectionElement>();
        
        bool dragging, mouseWasDown, anyReceiverHovered, mouseDown, mouseUp;
        SelectionElement receiver, newlyDragged;
        
        GUIContent dragGUI = new GUIContent("< Dragging >");
        float _dragGUIWidth = -1;
        float dragGUIWidth {
            get {
                if (_dragGUIWidth == -1) {
                    _dragGUIWidth = GUIStyles.label.CalcSize(dragGUI).x;
                }
                return _dragGUIWidth;
            }
        }
        
        public void AddShownElements(Dictionary<int, SelectionElement> newElements) {
            foreach (var k in newElements.Keys) {
                shownElements.Add(newElements[k]);
            }
        }
        public void AddShownElements(IEnumerable<SelectionElement> newElements) {
            shownElements.AddRange(newElements, false);
        }
        public void AddShownElements(SelectionElement newElements) {
            shownElements.Add(newElements);
        }

        public void InputListen (out bool mouseUp){
            anyReceiverHovered = false;

            UnityEngine.Event e = UnityEngine.Event.current;

            mouseDown = e.rawType == EventType.MouseDown && e.button == 0 && !mouseWasDown;
            if (mouseDown) mouseWasDown = true;
            
            mouseUp = e.rawType == EventType.MouseUp;
            if (mouseUp) mouseWasDown = false;

            this.mouseUp = mouseUp;
            
            if (!dragging) {
                // start dragging with a mouse drag
                dragging = e.rawType == EventType.MouseDrag && hasTracked;
            }
        }

        // need to evaluate while drawing
        // and afterwards as well (for multi window drag drop capability)
        public void ElementDragDropValues (SelectionElement element, Vector2 mousePos, bool mouseUp, out bool beingDragged, out bool receiverHovered, out bool dropReceived) {

            beingDragged = dragging && IsTracked(element);
            receiverHovered = element.isDirectory && element.hasMousePos && dragging && !beingDragged;
            dropReceived = receiverHovered && mouseUp;
        }


        public void UpdateLoop (Vector2 mousePos, Vector2Int selectedRange) {
            foreach (var element in shownElements) {
                if (element.isShown) {
                    anyReceiverHovered = anyReceiverHovered || element.receiverHovered;
                    if (mouseDown) {
                        if (element.isDraggable && element.hasMousePos) {
                            newlyDragged = element;
                            break;
                        }
                    }
                    else if (mouseUp) {
                        if (element.receiverHovered){
                            receiver = element;
                            break;
                        }
                    }
                }
            }
            
            DrawDraggedGUIs(mousePos);

            if (newlyDragged != null)
            {
                if (newlyDragged.isSelected) {
                    SetTracked(selectedRange);
                }
                else {
                    lo = hi = newlyDragged.showIndex;
                }
                newlyDragged = null;
            }

            if (mouseUp) {
                if (receiver != null) {
                    onDropCallback(receiver, trackedRange);
                    receiver = null;                 
                }
                dragging = false;
                Clear();
            }
        }

        public void UpdateLoopReceiverOnly (Vector2 mousePos) {

            foreach (var element in shownElements) {
                if (element.isShown) {
                    anyReceiverHovered = anyReceiverHovered || element.receiverHovered;
                    if (mouseUp) {
                        if (element.receiverHovered){
                            receiver = element;
                            break;
                        }
                    }
                }
            }
            
            DrawDraggedGUIs(mousePos);

            if (mouseUp) {
                if (receiver != null) {
                    onDropCallback(receiver, trackedRange);
                    receiver = null;                 
                }
                dragging = false;
                Clear();
            }
        }

        void DrawDraggedGUIs(Vector2 mousePos){    
            if (!dragging) return;
            EditorWindow.mouseOverWindow.Repaint();
            if (anyReceiverHovered) EditorGUIUtility.AddCursorRect(new Rect(mousePos.x-5, mousePos.y-5, 10, 10), MouseCursor.ArrowPlus);
            GUIUtils.Label(new Rect(mousePos.x, mousePos.y - 8, dragGUIWidth, 16), dragGUI, anyReceiverHovered ? Colors.black : Colors.liteGray);
        }
    }
}
