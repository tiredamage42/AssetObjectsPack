using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
namespace AssetObjectsPacks {

    public class SelectionElement {

        Action<int, string> onRename;
        bool isBeingRenamed;
        string renameName;

        public bool hasMousePos, receiverHovered;
        public bool isShown, isDirectory, isDraggable;
        
        public int elementID, refIndex;
        public int showIndex;
        public GUIContent gui;
        bool isCopy;

        ElementSelectionSystem baseSystem;
        Action<int> drawPrefix;
        bool showChildren;

        public HashSet<int> directoryChildIDs = new HashSet<int>(), objectChildIDsWithoutDuplicates = new HashSet<int>();
        public int parentID = -1;
        bool enterPressed;

        void DrawButtonBase (Rect rect, bool drawSelected, bool drawBackground, bool guiEnabled, Color32 textColor, FontStyle fontStyle = FontStyle.Normal) {
            isShown = true;
                
            GUIStyle style = GUIStyles.toolbarButton;
            
            TextAnchor a = style.alignment;
            Texture2D t = style.normal.background;
            Texture2D th = style.active.background;
                    
            style.fontStyle = fontStyle;

            style.normal.background = (!drawSelected && !drawBackground) ? null : t;
            style.active.background = (!drawSelected && !drawBackground) ? null : th;

            style.alignment = TextAnchor.MiddleLeft;
                        
            Color32 txtColor = drawSelected ? Colors.black : textColor;
                
            GUI.enabled = guiEnabled;
                
            bool pressed = GUIUtils.Button(gui, style, Colors.Toggle(drawSelected), txtColor, rect);
                
            GUI.enabled = true;

            style.normal.background = t;
            style.active.background = th;
                
            style.alignment = a;
            style.fontStyle = FontStyle.Normal;

            if (pressed) {
                baseSystem.clickedElement = this;
            }                    
        }

        public void DrawDirectoryTreeElement (bool mouseUp, Vector2 mousePos, bool isShown = true, bool isBase = true) {

            this.isShown = isShown;

            if (isShown) {
                
                if (!isBase) {
                    GUIUtils.BeginIndent();
                }
                
                EditorGUILayout.BeginHorizontal();

                bool hasChildren = directoryChildIDs.Count > 0;

                GUI.enabled = hasChildren;
                showChildren = GUIUtils.SmallToggleButton(new GUIContent("", "Show Children"), showChildren, out _);
                GUI.enabled = true;

                Rect rect = BuildGUIRectAndCalculateMousePos(mousePos);
                
                bool dropReceived, beingDragged;
                baseSystem.dragDropHandler.ElementDragDropValues(this, mousePos, mouseUp, out beingDragged, out receiverHovered, out dropReceived);

                bool drawSelected = receiverHovered || elementID == baseSystem.currentShownDirectoryID;
                
                bool guiEnabled = !dropReceived;
                bool drawBackground = false;

                DrawButtonBase (rect, drawSelected, drawBackground, guiEnabled, Colors.liteGray, FontStyle.Normal);
                            
                EditorGUILayout.EndHorizontal();

                if (hasChildren) {
                    foreach (var child in directoryChildIDs) baseSystem.GetDirectoryTreeElement(child).DrawDirectoryTreeElement(mouseUp, mousePos, showChildren && isShown, false);
                }

                if (!isBase) {
                    GUIUtils.EndIndent();
                }
            }
        }

        public SelectionElement (bool isDraggable, bool isDirectory, int id, GUIContent gui, int refIndex, bool isBeingRenamed, bool isCopy, Action<int> drawPrefix, Action<int, string> onRename, string renameName) 
        => (this.isDraggable, this.isDirectory, this.elementID, this.gui, this.refIndex, this.isBeingRenamed, this.isCopy, this.drawPrefix, this.onRename, this.renameName)
        = (isDraggable, isDirectory, id, gui, refIndex, isBeingRenamed, isCopy, drawPrefix, onRename, renameName);
        
        public SelectionElement (bool isDraggable, bool isDirectory, int id, GUIContent gui, int refIndex) 
        : this (isDraggable, isDirectory, id, gui, refIndex, false, false, null, null, null ) { }
        
        
        public void InitializeInternal (int showIndex, ElementSelectionSystem baseSystem){
            this.showIndex = showIndex;
            this.baseSystem = baseSystem;
        }

        public bool isSelected;

        Rect BuildGUIRectAndCalculateMousePos (Vector2 mousePos) {
            Rect rect = GUILayoutUtility.GetRect(gui, GUIStyles.toolbarButton);
            hasMousePos = rect.Contains(mousePos);
            return rect;
        }

        public void DrawBackButton (Vector2 currentMousePos, bool mouseUp, DragAndDropHandler dragDropHandler){
            
            Rect lastDrawnRect = BuildGUIRectAndCalculateMousePos(currentMousePos);
            
            bool dropReceived, beingDragged;
            dragDropHandler.ElementDragDropValues(this, currentMousePos, mouseUp, out beingDragged, out receiverHovered, out dropReceived);
            
            bool drawSelected = receiverHovered;
            bool guiEnabled = !dropReceived;
            bool drawBackground = true;

            DrawButtonBase (lastDrawnRect, drawSelected, drawBackground, guiEnabled, isCopy ? Colors.green : Colors.liteGray, isCopy ? FontStyle.Italic : FontStyle.Normal);            
        } 
            

        public void Draw (Vector2 currentMousePos, bool mouseUp, DragAndDropHandler dragDropHandler, Vector2Int selectionRange){
            
            isShown = true;
            if (isBeingRenamed) {
                isSelected = false;
                DrawRenameElement(BuildGUIRectAndCalculateMousePos(currentMousePos));
            }  
            else {

                EditorGUILayout.BeginHorizontal();
                if (drawPrefix != null) drawPrefix(refIndex);

                Rect lastDrawnRect = BuildGUIRectAndCalculateMousePos(currentMousePos);
        
                bool dropReceived, beingDragged;
                dragDropHandler.ElementDragDropValues(this, currentMousePos, mouseUp, out beingDragged, out receiverHovered, out dropReceived);
                
                isSelected = selectionRange.Contains(showIndex);

                bool drawSelected = receiverHovered || isSelected;
                bool guiEnabled = !beingDragged && !dropReceived;
                bool drawBackground = isDirectory;

                DrawButtonBase (lastDrawnRect, drawSelected, drawBackground, guiEnabled, isCopy ? Colors.green : Colors.liteGray, isCopy ? FontStyle.Italic : FontStyle.Normal);
                
                EditorGUILayout.EndHorizontal();
            }    
        } 
        
        void DrawRenameElement (Rect rect) {

            string controlName = GUIUtils.overrideKeyboardControlName;// "RenameElement";
        
            renameName = GUIUtils.DrawTextField(rect, renameName, true, controlName, out _);
            
            if (!GUIUtils.IsFocused(controlName)) 
                GUIUtils.FocusOnTextArea(controlName);
            
            bool isFocused = true;

            UnityEngine.Event e = UnityEngine.Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && !hasMousePos) isFocused = false;
            
            if (!enterPressed) enterPressed = isFocused && e.keyCode == KeyCode.Return;
    
            bool setName = !isFocused || (enterPressed && e.type ==EventType.Repaint);
            if (setName) {
                GUIUtils.FocusOnTextArea(string.Empty);
                if (onRename != null) 
                    onRename(refIndex, renameName);
                enterPressed = false;
            }
        }
    }    
}