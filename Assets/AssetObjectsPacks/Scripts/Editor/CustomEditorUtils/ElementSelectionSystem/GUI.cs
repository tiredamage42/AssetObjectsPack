using UnityEngine;
using UnityEditor;
using System;
namespace AssetObjectsPacks {
    public class SelectionElement {
        public int id, poolIndex, collectionID;
        public string displayPath, renameName;
        public bool isBeingRenamed, isCopy;
        public GUIContent gui;
        public Action<int, string> onRename;
        Action<int> drawPrefix;
        SelectionSystem selectionSystem;
        DragAndDrop dragDrop;
        int showIndex;
        bool isSecondary, isCurrentPath;
        public bool isDirectory { get { return id == -1; } }
        public bool isPrevDirectory { get { return id == -2; } }
        Texture img;
                
        //prev dir element
        public SelectionElement(string displayPath, GUIContent gui) => (this.id, this.displayPath, this.poolIndex, this.gui) = (-2, displayPath, -1, gui);
        
        //directory element
        public SelectionElement(SelectionElement other, string displayPath, GUIContent gui)
        => (this.id, this.displayPath, this.poolIndex, this.isBeingRenamed, this.onRename, this.renameName, this.gui)
        = (-1, displayPath, other.poolIndex, other.isBeingRenamed, other.onRename, other.renameName, gui);
        
        public SelectionElement (int id, string displayPath, int poolIndex, bool isBeingRenamed, bool isCopy, Action<int> drawPrefix, Action<int, string> onRename, string renameName, int collectionID, Texture img) 
        => (this.id, this.displayPath, this.poolIndex, this.isBeingRenamed, this.isCopy, this.drawPrefix, this.onRename, this.renameName, this.collectionID, this.img)
        = (id, displayPath, poolIndex, isBeingRenamed, isCopy, drawPrefix, onRename, renameName, collectionID, img);
        
        public void SetGUI(GUIContent gui) {
            this.gui = gui;
            this.gui.image = img;
        }


        public void Initialize (int showIndex, SelectionSystem selectionSystem, DragAndDrop dragDrop, bool isSecondary, string currentDisplayPath) {
            this.isSecondary = isSecondary;
            isCurrentPath = isSecondary && displayPath == currentDisplayPath;
            this.showIndex = showIndex;
            this.selectionSystem = selectionSystem;
            this.dragDrop = dragDrop;
        }
        
        public bool Draw (Vector2 currentMousePos, GUIStyle s) {
            bool pressed = false;
            
            if (isBeingRenamed) {
                Rect displayRect = GUILayoutUtility.GetRect(gui, GUIStyles.toolbarButton);
                DrawRenameElement(displayRect, displayRect.Contains(currentMousePos));
            }  
            else {

                EditorGUILayout.BeginHorizontal();

                if (drawPrefix != null) drawPrefix(poolIndex);

                Rect displayRect = GUILayoutUtility.GetRect(gui, GUIStyles.toolbarButton);
                bool hasMousePos = displayRect.Contains(currentMousePos);
                
                bool isReceiver = isSecondary || isDirectory;
                bool isDraggable = !isSecondary;

                bool receiverHovered = false, beingDragged = false, dropReceived = false;
                dragDrop.CheckElementRectForDragsOrDrops (gui, hasMousePos, showIndex, isSecondary ? 0 : 1, isReceiver, isDraggable, out beingDragged, out receiverHovered, out dropReceived);
            
                int collection = isSecondary ? 0 : 1;          
                bool drawSelected = receiverHovered || (isCurrentPath && !isPrevDirectory) ||  (!isSecondary && selectionSystem.IsTracked(showIndex));
                bool guiEnabled = !beingDragged && !dropReceived;
                bool drawDirectory = (isSecondary && isPrevDirectory) || (!isSecondary && isDirectory); 

                //GUIStyle s = GUIStyles.toolbarButton;
                TextAnchor a = s.alignment;
                Texture2D t = s.normal.background;
                Texture2D th = s.active.background;
                    
                s.fontStyle = isCopy ? FontStyle.Italic : FontStyle.Normal;

                s.normal.background = (!drawSelected && !drawDirectory) ? null : t;
                s.active.background = (!drawSelected && !drawDirectory) ? null : th;

                s.alignment = TextAnchor.MiddleLeft;
                        
                Color32 txtColor = drawSelected ? Colors.black : (isCopy ? Colors.green : Colors.liteGray);
                
                GUI.enabled = guiEnabled;
                pressed = GUIUtils.Button(gui, s, Colors.Toggle(drawSelected), txtColor, displayRect);
                GUI.enabled = true;

                s.normal.background = t;
                s.active.background = th;
                
                s.alignment = a;
                s.fontStyle = FontStyle.Normal;
                
                EditorGUILayout.EndHorizontal();
                //}
                
            }    
            return pressed;
        } 
        void DrawRenameElement (Rect displayRect, bool hasMousePos) {

            string controlName = GUIUtils.overrideKeyboardControlName;// "RenameElement";
        
            renameName = GUIUtils.DrawTextField(displayRect, renameName, true, controlName, out _);
            
            //GUIUtils.NameNextControl(controlName);
            //renameName = EditorGUI.TextField(displayRect, renameName);
            if (!GUIUtils.IsFocused(controlName)) GUIUtils.FocusOnTextArea(controlName);
            
            bool isFocused = true;

            UnityEngine.Event e = UnityEngine.Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && !hasMousePos) isFocused = false;
            
            if (!enterPressed) enterPressed = isFocused && e.keyCode == KeyCode.Return;
    
            //bool 
            bool setName = !isFocused || (enterPressed && e.type ==EventType.Repaint);
            if (setName) {
                GUIUtils.FocusOnTextArea(string.Empty);
                if (onRename != null) 
                    onRename(poolIndex, renameName);
                enterPressed = false;
            }
        }
        bool enterPressed;
    }
    public static class ESS_GUI 
    {
        public static void DrawTopToolbar (string displayPath, ref string search, Action toolbarButtons, out bool searchChanged) {
            EditorGUILayout.BeginHorizontal();
            
            //cur path
            GUI.enabled = false;
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.TextField(displayPath);
            GUI.enabled = true;

            //search
            string lastSearch = search;
            string defSearch = search.IsEmpty() ? "Search" : search;
            string searchResult = GUIUtils.DrawTextField(defSearch, GUIUtils.TextFieldType.Delayed, true, "Search", out _, searchBarWidth);
            if (searchResult != defSearch) search = searchResult;
            searchChanged = search != lastSearch;
            
            toolbarButtons();
            
            EditorGUILayout.EndHorizontal();
        }

        static readonly GUILayoutOption[] elementWindowsWidths = new GUILayoutOption[] {
            GUILayout.MaxWidth(64),
            GUILayout.MaxWidth(999),
        };
        static readonly GUILayoutOption searchBarWidth = GUILayout.Width(128);

        static void DrawPreviousDirectoryList (GUIStyle s, Vector2 mousePos, SelectionElement[] elements, ref int clickedElementIndex, ref int clickedElementCollection) {
            EditorGUILayout.BeginVertical(elementWindowsWidths[0]);
            bool wasntClicked = clickedElementIndex == -1;
            int l = elements.Length;

            for (int i = 0; i < l; i++) {
                if(elements[i].Draw(mousePos, s)) clickedElementIndex = i;                
                if (i == 0) GUIUtils.Space();
            }
            if (clickedElementIndex != -1 && wasntClicked) clickedElementCollection = 0;
            EditorGUILayout.EndVertical();
        }
        static void DrawMainElementsList (GUIStyle s, Vector2 mousePos, SelectionElement[] elements, string dirViewPath, ref int clickedElementIndex, ref int clickedElementCollection, Action toolbarButtons, ref string searchFilter, out bool searchChanged) {
            EditorGUILayout.BeginVertical(elementWindowsWidths[1]);

            DrawTopToolbar(dirViewPath, ref searchFilter, toolbarButtons, out searchChanged);        
            
            GUIUtils.Space();
    
            bool wasntClicked = clickedElementIndex == -1;
            int l = elements.Length;

            for (int i = 0; i < l; i++) {
                if(elements[i].Draw(mousePos, s)) clickedElementIndex = i;    
            }
            
            if (l == 0) 
                GUIUtils.HelpBox("No Elements!", MessageType.Info);   
        
            if (clickedElementIndex != -1 && wasntClicked) clickedElementCollection = 1;
            EditorGUILayout.EndVertical();
        }

        static readonly GUILayoutOption pageButtonWidth = GUILayout.Width(32);
        
        static readonly GUIContent pageFwdGUI = new GUIContent(" >> "), pageBackGUI = new GUIContent(" << ");
        
        public static int DrawPages (GUIContent pagesGUI) {
            
            GUIUtils.StartBox();
            
            EditorGUILayout.BeginHorizontal();

            int r = 0;
            if (GUIUtils.Button(pageBackGUI, GUIStyles.toolbarButton, pageButtonWidth)) r = -1;
            
            TextAnchor ol = GUIStyles.label.alignment;
            GUIStyles.label.alignment = TextAnchor.LowerCenter;
            GUIUtils.Label(pagesGUI);
            GUIStyles.label.alignment = ol;

            if (GUIUtils.Button(pageFwdGUI, GUIStyles.toolbarButton, pageButtonWidth)) r = 1;
            
            EditorGUILayout.EndHorizontal();
            GUIUtils.EndBox();

            return r;
        }


        public static void DrawElementsView (GUIStyle s, Vector2 mousePos, SelectionElement[][] elements, string dirViewPath, Action toolbarButtons, GUIContent pagesGUI, ref string searchFilter, out bool searchChanged, out int pageOffset, out int clickedElementIndex, out int clickedElementCollection) {
            
            GUIUtils.StartBox(0);

            //if (UnityEngine.Event.current.type == EventType.Repaint) {

            EditorGUILayout.BeginHorizontal();
            //}

            clickedElementCollection = -1; 
            clickedElementIndex = -1;

            DrawPreviousDirectoryList (s, mousePos, elements[0], ref clickedElementIndex, ref clickedElementCollection);
            DrawMainElementsList(s, mousePos, elements[1], dirViewPath, ref clickedElementIndex, ref clickedElementCollection, toolbarButtons, ref searchFilter, out searchChanged);
            
            //if (UnityEngine.Event.current.type == EventType.Repaint) {

                EditorGUILayout.EndHorizontal();
            //}
            
            GUIUtils.EndBox();

            //pagination gui
            pageOffset = DrawPages(pagesGUI);   
        }
    }
}