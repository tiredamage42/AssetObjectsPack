using UnityEngine;
using UnityEditor;
using System;
namespace AssetObjectsPacks {
    public class SelectionElement {
        public int id, poolIndex;
        public string filePath, renameName;
        public bool isBeingRenamed, isCopy;
        public GUIContent gui;
        public System.Action<int, string> onRename;

        System.Action<int> drawPrefix;
        SelectionSystem selectionSystem;
        DragAndDrop dragDrop;
        int showIndex;
        bool isSecondary, isCurrentPath;
        public bool isDirectory { get { return id == -1; } }
        public bool isPrevDirectory { get { return id == -2; } }
                
        //prev dir element
        public SelectionElement(string filePath, GUIContent gui) => (this.id, this.filePath, this.poolIndex, this.gui) = (-2, filePath, -1, gui);
        
        //directory element
        public SelectionElement(SelectionElement other, string filePath, GUIContent gui)
        => (this.id, this.filePath, this.poolIndex, this.isBeingRenamed, this.onRename, this.renameName, this.gui)
        = (-1, filePath, other.poolIndex, other.isBeingRenamed, other.onRename, other.renameName, gui);
        
        public SelectionElement (int id, string filePath, int poolIndex, bool isBeingRenamed, bool isCopy, System.Action<int> drawPrefix, System.Action<int, string> onRename, string renameName) 
        => (this.id, this.filePath, this.poolIndex, this.isBeingRenamed, this.isCopy, this.drawPrefix, this.onRename, this.renameName)
        = (id, filePath, poolIndex, isBeingRenamed, isCopy, drawPrefix, onRename, renameName);
        
        public void SetGUI(GUIContent gui) {
            this.gui = gui;
        }
        public void Initialize (int showIndex, SelectionSystem selectionSystem, DragAndDrop dragDrop, bool isSecondary, string currentDisplayPath) {
            this.isSecondary = isSecondary;
            isCurrentPath = isSecondary && filePath == currentDisplayPath;
            this.showIndex = showIndex;
            this.selectionSystem = selectionSystem;
            this.dragDrop = dragDrop;
        }
        
        //public static SelectionElement empty {
        //    get { return new SelectionElement(-1, "", -1, false, false, null, null, ""); }
        //}


        public bool Draw () {
            bool pressed = false;
            
            if (isBeingRenamed) {
                Rect displayRect = GUILayoutUtility.GetRect(gui, GUIStyles.toolbarButton);
                bool hasMousePos = displayRect.Contains(UnityEngine.Event.current.mousePosition);
                DrawRenameElement(displayRect, hasMousePos);
            }  
            else {

                EditorGUILayout.BeginHorizontal();
                
                if (drawPrefix != null) drawPrefix(poolIndex);

                Rect displayRect = GUILayoutUtility.GetRect(gui, GUIStyles.toolbarButton);
                bool hasMousePos = displayRect.Contains(UnityEngine.Event.current.mousePosition);
                
                
                bool isReceiver = isSecondary || isDirectory;
                bool isDraggable = !isSecondary;

                bool receiverHovered, beingDragged, dropReceived;
                dragDrop.CheckElementRectForDragsOrDrops (gui, hasMousePos, showIndex, isSecondary ? 0 : 1, isReceiver, isDraggable, out beingDragged, out receiverHovered, out dropReceived);
            
                int collection = isSecondary ? 0 : 1;          
                bool drawSelected = receiverHovered || (isCurrentPath && !isPrevDirectory) ||  (!isSecondary && selectionSystem.IsTracked(showIndex));
                bool guiEnabled = !beingDragged && !dropReceived;
                bool drawDirectory = (isSecondary && isPrevDirectory) || (!isSecondary && isDirectory); 

                GUIStyle s = GUIStyles.toolbarButton;
                TextAnchor a = s.alignment;
                Texture2D t = s.normal.background;
                Texture2D th = s.active.background;
                    
                s.fontStyle = isCopy ? FontStyle.Italic : FontStyle.Normal;

                s.normal.background = (!drawSelected && !drawDirectory) ? null : t;
                s.active.background = (!drawSelected && !drawDirectory) ? null : th;

                s.alignment = TextAnchor.MiddleLeft;
                        
                //Color32 txtColor = drawSelected || drawDirectory ? Colors.black : (isCopy ? Colors.green : Colors.liteGray);
                Color32 txtColor = drawSelected ? Colors.black : (isCopy ? Colors.green : Colors.liteGray);

                UnityEngine.GUI.enabled = guiEnabled;

                pressed = GUIUtils.Button(gui, s, Colors.Toggle(drawSelected), txtColor, displayRect);

                UnityEngine.GUI.enabled = true;

                s.normal.background = t;
                s.active.background = th;
                
                s.alignment = a;
                s.fontStyle = FontStyle.Normal;
                
                EditorGUILayout.EndHorizontal();
            }    

            return pressed;
        } 
        void DrawRenameElement (Rect displayRect, bool hasMousePos) {
            string controlName = "RenameElement";
            GUIUtils.NameNextControl(controlName);
            renameName = EditorGUI.TextField(displayRect, renameName);
            if (!GUIUtils.IsFocused(controlName)) GUIUtils.FocusOnTextArea(controlName);
            
            bool isFocused = true;

            UnityEngine.Event e = UnityEngine.Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && !hasMousePos) isFocused = false;
            
            bool enterPressed = isFocused && e.keyCode == KeyCode.Return;

            bool setName = !isFocused || enterPressed;
            if (setName) {
                GUIUtils.FocusOnTextArea(string.Empty);
                if (onRename != null) onRename(poolIndex, renameName);
            }
        }
    }
    public static class ESS_GUI 
    {
        public static void DrawTopToolbar (string displayPath, ref string search, KeyboardListener k, System.Action<KeyboardListener> toolbarButtons, out bool searchChanged) {
            EditorGUILayout.BeginHorizontal();
            
            //cur path
            UnityEngine.GUI.enabled = false;
            EditorGUILayout.TextField(displayPath);
            UnityEngine.GUI.enabled = true;

            //search
            string lastSearch = search;
            string defSearch = search.IsEmpty() ? "Search" : search;
            string searchResult = GUIUtils.DrawTextField(defSearch, GUIUtils.TextFieldType.Delayed, true, out _, searchBarWidth);
            if (searchResult != defSearch) search = searchResult;
            searchChanged = search != lastSearch;
            
            toolbarButtons(k);
            
            EditorGUILayout.EndHorizontal();
        }

        static readonly GUILayoutOption[] elementWindowsWidths = new GUILayoutOption[] {
            GUILayout.MaxWidth(64),
            GUILayout.MaxWidth(999),
        };
        static readonly GUILayoutOption searchBarWidth = GUILayout.Width(128);

        public static void DrawPreviousDirectoryList (SelectionElement[] elements, ref int clickedElementIndex, ref int clickedElementCollection) {
            EditorGUILayout.BeginVertical(elementWindowsWidths[0]);
            bool wasntClicked = clickedElementIndex == -1;
            for (int i = 0; i < elements.Length; i++) {
                if(elements[i].Draw()) clickedElementIndex = i;                
                if (i == 0) GUIUtils.Space();
            }
            if (clickedElementIndex != -1 && wasntClicked) clickedElementCollection = 0;
            EditorGUILayout.EndVertical();
        }
        
        public static void DrawMainElementsList (SelectionElement[] elements, string dirViewPath, ref int clickedElementIndex, ref int clickedElementCollection, KeyboardListener k, System.Action<KeyboardListener> toolbarButtons, ref string searchFilter, out bool searchChanged) {
            EditorGUILayout.BeginVertical(elementWindowsWidths[1]);

            DrawTopToolbar(dirViewPath, ref searchFilter, k, toolbarButtons, out searchChanged);        
            GUIUtils.Space();
    
            bool wasntClicked = clickedElementIndex == -1;
            int l = elements.Length;
            for (int i = 0; i < l; i++) {
                if(elements[i].Draw()) clickedElementIndex = i;    
            }
            if (l == 0) EditorGUILayout.HelpBox("No Elements!", MessageType.Info);   
        
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


        public static void DrawElementsView (SelectionElement[][] elements, string dirViewPath, KeyboardListener k, Action<KeyboardListener> toolbarButtons, GUIContent pagesGUI, ref string searchFilter, out bool searchChanged, out int pageOffset, out int clickedElementIndex, out int clickedElementCollection) {
            GUIUtils.StartBox(0);
            EditorGUILayout.BeginHorizontal();

            clickedElementCollection = -1; 
            clickedElementIndex = -1;

            DrawPreviousDirectoryList (elements[0], ref clickedElementIndex, ref clickedElementCollection);
            DrawMainElementsList(elements[1], dirViewPath, ref clickedElementIndex, ref clickedElementCollection, k, toolbarButtons, ref searchFilter, out searchChanged);
            
            EditorGUILayout.EndHorizontal();
            GUIUtils.EndBox();

            //pagination gui
            pageOffset = DrawPages(pagesGUI);
            
        }
        

    }
}
