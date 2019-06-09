using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
namespace AssetObjectsPacks {

    /*
    
    call on path change
    keep new element as index in ev editor

    */
    public partial class ElementSelectionSystem  {

        public bool hasSelection { get { return selectionHandler.hasTracked; } }
        public bool singleSelected { get { return selectionHandler.singleTracked; } }
        

        SelectionHandler selectionHandler = new SelectionHandler();
        public DragAndDropHandler dragDropHandler = new DragAndDropHandler();



        // Pagination pagination= new Pagination();
        
        string searchFilter;

        public int currentShownDirectoryID;
                
        Action onSelectionChange, onChangeDisplayPath;
        Action<SelectionElement, HashSet<SelectionElement>> onDirDragDrop;
        
        Action toolbarButtons;
        bool shouldRebuild, shouldResetPage;
        Func<int, List<SelectionElement>> getPoolElements;
        
        public SelectionElement[] mainElements;


        Action<int, HashSet<int>> getElementsInDirectory;
        
        public void OnEnable (
            EditorProp serializedObject,
            
            Func<int, List<SelectionElement>> getPoolElements,     
            
            
            Func<Dictionary<int, SelectionElement>> buildDirectoryTreeElements,
            
            Action<int, HashSet<int>> getElementsInDirectory, 
            
            Action onSelectionChange, 
            Action<SelectionElement, HashSet<SelectionElement>> onDirDragDrop, 



            Action toolbarButtons, Action onChangeDisplayPath) {


            this.serializedObject = serializedObject;
            this.buildDirectoryTreeElements = buildDirectoryTreeElements;
            this.onChangeDisplayPath = onChangeDisplayPath;
            this.getElementsInDirectory = getElementsInDirectory;
            this.toolbarButtons = toolbarButtons;
            this.onDirDragDrop = onDirDragDrop;
            this.onSelectionChange = onSelectionChange;
            this.getPoolElements = getPoolElements;

            DoReset(true, true);

            dragDropHandler.OnEnable(OnDropReceive);
        }

        public EditorProp serializedObject;


        void UpdateDirectoryTreeWindow (bool updateDragDrop) {
            if (!showDirectoryTree) {
                if (directoryTreeWindow != null) {
                    directoryTreeWindow.Close();
                }
            }
            else {
                if (directoryTreeWindow == null) {
                    directoryTreeWindow = DirectoryTreeWindow.NewDirectoryTreeWindow(this);
                }
            }
        }


        public bool showDirectoryTree;
        DirectoryTreeWindow directoryTreeWindow;

        public SelectionElement clickedElement;
        

        public void DrawElements (Action<KeyboardListener> checkHotkeys){

            
            UpdateDirectoryTreeWindow(true);


            KeyboardListener keyboardListener = new KeyboardListener();
            
            bool mouseUp;
            dragDropHandler.InputListen (out mouseUp);

            

            bool searchChanged;
            // int pageOffset, 
            
            
            Vector2 mousePosition = UnityEngine.Event.current.mousePosition;

            
            DrawElementsView (
                mousePosition, 
                mouseUp,
                
                // pagination.pagesGUI, 
                out searchChanged//, 
                // out pageOffset,
            );


            dragDropHandler.UpdateLoop ( mousePosition, selectionHandler.trackedRange );
            
            
            
            
            
            // bool paginationSuccess = pageOffset != 0 && pagination.SwitchPage(pageOffset);
            
            
            //check clicked element
            int goToDirID;
            bool selectionChanged;
            
            CheckClickedElement(out selectionChanged, clickedElement, keyboardListener, out goToDirID);
            
            bool movedFolder = (goToDirID != -1 && MoveFolder(goToDirID));
            
            selectionChanged = selectionHandler.HandleDirectionalSelection(keyboardListener[KeyCode.UpArrow], keyboardListener[KeyCode.DownArrow], keyboardListener.shift, mainElements.Length - 1) || selectionChanged;

            shouldRebuild = droppedOnReceiver || movedFolder || searchChanged;//|| paginationSuccess;            
            shouldResetPage = movedFolder;

            if (!shouldRebuild && selectionChanged) onSelectionChange();

            checkHotkeys(keyboardListener);

            clickedElement = null;

            droppedOnReceiver = false;
        }
        bool droppedOnReceiver;


        bool ElementIsChildOf(SelectionElement a, SelectionElement parentCheck) {
            return CheckElementParentRecursive(parentCheck.elementID, directoryTreeElements[a.elementID]);
        }

        bool CheckElementParentRecursive (int parentCheckID, SelectionElement element) {
            if (parentCheckID == 0 || element.parentID == parentCheckID)
                return true;
            if (element.elementID == 0)
                return false;

            return CheckElementParentRecursive(parentCheckID, directoryTreeElements[element.parentID]);
        }

        void OnDropReceive(SelectionElement receiver, Vector2Int draggedRange){//, HashSet<SelectionElement> droppedElements) {

            HashSet<SelectionElement> draggedElements = draggedRange.Generate( i => mainElements[i] ).ToHashSet();
            foreach (var element in draggedElements) {
                // check if we're dragging a directory
                if (element.isDirectory) {
                    //check if the receiving directory is a child of the dragged directory
                    // if so, return out, so we dont have cyclical references...
                    bool receiverIsChildOfElement = ElementIsChildOf(receiver, element);
                    if ( receiverIsChildOfElement ) {
                        Debug.LogError("avoiding cyclical drag drop, " + receiver.gui.text + " is child of " + element.gui.text);
                        return;
                    }
                }
            }

            
            onDirDragDrop(receiver, draggedElements );
            droppedOnReceiver = true;
        }
            
        public void CheckRebuild (bool forceRebuild, bool forceReset) {
            if (forceRebuild || shouldRebuild) 
                DoReset(forceReset, shouldResetPage || forceReset);
                
            shouldResetPage = shouldRebuild = false;
        }

        public SelectionElement firstSelected {
            get {
                if (!selectionHandler.hasTracked) return null;
                return mainElements[selectionHandler.trackedRange.x];
            }
        }

        void CheckClickedElement(out bool selectionChanged, SelectionElement clickedElement, KeyboardListener k, out int goToDirID) {
            goToDirID = -1;
            selectionChanged = false;

            //right arrow movies fwd directory when one selected
            if (clickedElement == null){
                if ( k[KeyCode.RightArrow] ) {
                    if (selectionHandler.singleTracked) {
                        if (firstSelected.isDirectory) {
                            clickedElement = firstSelected;
                        }
                    }
                }
            }
            //left arrow goes back directory
            if (clickedElement == null) {
                if ( k[KeyCode.LeftArrow] ) {
                    if (backButton != null) {
                        clickedElement = backButton;
                    }
                }
            }


            if (clickedElement == null) return;


            if (clickedElement.isDirectory){
                //if not already there

                if (clickedElement.elementID != currentShownDirectoryID) {
                    goToDirID = clickedElement.elementID;
                }
            }
            else { //element select
                selectionHandler.OnObjectSelection(clickedElement, k.shift);
                selectionChanged = true;
            }
        }

        public void ForceBackFolder () {
            MoveFolder();
            DoReset(false, true);
        }
        

        bool MoveFolder(int goToDirID = -1){//  string toPath = null){

            if (goToDirID == -1) {
                if (currentShownDirectoryID == 0) return false;
                currentShownDirectoryID = directoryTreeElements[currentShownDirectoryID].parentID;
            }
            else {
                currentShownDirectoryID = goToDirID;
            } 
        
            onChangeDisplayPath ();
            return true;
        }
        

        public IEnumerable<int> GetReferenceIndiciesInSelection (bool includeDirectories) {
            return selectionHandler.trackedRange.Generate( i => (mainElements[i].isDirectory && !includeDirectories) ? -1 : mainElements[i].refIndex );
            
        }        
        IEnumerable<int> GetReferenceIndiciesInShown (bool includeDirectories) {
            return mainElements.Generate( e => (e.isDirectory && !includeDirectories) ? -1 : e.refIndex );
        }
        public IEnumerable<int> GetReferenceIndiciesInSelectionOrAllShown (bool includeDirectories) {
            return (selectionHandler.hasTracked ? GetReferenceIndiciesInSelection(includeDirectories) : GetReferenceIndiciesInShown(includeDirectories)).Where( i => i != -1 );
        }

        HashSet<int> _GetIDsInTrackedList(Vector2Int trackedRange, bool includeDirs, string dirCheckTitle, string dirCheckMsg) {
            if (trackedRange.x == -1) {
                return null;
            }
            
            HashSet<int> r = new HashSet<int>();
            
            bool checkedDirs = false;

            for (int i = trackedRange.x; i <= trackedRange.y; i++) {
                SelectionElement element = mainElements[i];
                if (element.isDirectory) {
                    if (!includeDirs) continue;
                    if (!checkedDirs) {
                        if (!EditorUtility.DisplayDialog(dirCheckTitle, dirCheckMsg, "Ok", "Cancel")) return null;
                        checkedDirs = true;
                    }
                    getElementsInDirectory(element.elementID, r);
                
                }
                else r.Add(element.elementID);
            }
            return r;
        }

        public HashSet<int> GetIDsInSelection_NoRepeats () {
            return _GetIDsInTrackedList(selectionHandler.trackedRange, false, null, null);
        }
        public HashSet<int> GetIDsInDirectoryAndSubDirs_NoRepeats (string dirCheckTitle, string dirCheckMessage) {
            return _GetIDsInTrackedList(selectionHandler.trackedRange, true, dirCheckTitle, dirCheckMessage);
        }
        
        void DoReset (bool resetPath, bool resetPage) {
            if (resetPath) {
                MoveFolder(0);
            }

            // if (resetPage) {
            //     pagination.ResetPage();
            // }

            selectionHandler.Clear();   
            dragDropHandler.Clear();
            
            RebuildDisplayedElements();
            onSelectionChange();
        }


        


        Func<Dictionary<int, SelectionElement>> buildDirectoryTreeElements;

        Dictionary<int, SelectionElement> BuildDirectoryTreeElements () {
            Dictionary<int, SelectionElement> dirTreeElements = buildDirectoryTreeElements();

            foreach (var k in dirTreeElements.Keys) {
                dirTreeElements[k].InitializeInternal(-1, this);
            }

            return dirTreeElements;
        }


        void UpdateDragDropElements () {
            dragDropHandler.shownElements.Clear();

            dragDropHandler.AddShownElements(mainElements);

            if (backButton != null) {
                dragDropHandler.AddShownElements(backButton);
            }

            dragDropHandler.AddShownElements(directoryTreeElements);
        }

        // on add remove any objects, directory changing, renaming, moving any objects
        void RebuildMainDisplay () {
            // elements[1] = pagination.Paginate(BuildElementsList (curPath, true)).ToArray();
            mainElements = BuildElementsList ().ToArray();//curPath, true).ToArray();
            for (int i = 0; i < mainElements.Length; i++) {
                mainElements[i].InitializeInternal(i, this);
            }
        }

        string GetCurrentDisplayPathText () {
            SelectionElement current = directoryTreeElements[currentShownDirectoryID];
            return GetFullPath(current.gui.text, current.elementID);
        }
        
        
        string GetFullPath (string ourName, int fromStateID) {
            if (fromStateID == 0) {
                return ourName;
            }

            int parentID = directoryTreeElements[fromStateID].parentID;
            SelectionElement parentDirectory = directoryTreeElements[parentID];
            string parentName = parentDirectory.gui.text;

            if (parentID != 0) {
                ourName = parentName + "/" + ourName;
            }
            return GetFullPath(ourName, parentID);
        }








        //on directory change only
        void RebuildBackButton () {

            if (currentShownDirectoryID != 0){
                int parentDirectoryID = directoryTreeElements[currentShownDirectoryID].parentID;
                
                backButton = new SelectionElement(false, true, parentDirectoryID, backButtonGUI, -1);
                backButton.InitializeInternal(0, this);
            }
            else {
                backButton = null;
            }
        }
        GUIContent backButtonGUI = new GUIContent(" << Back ");

        //when adding or removing directories, or moving directories, or renaming directories
        void RebuildDirectoryTreeElements () {
            Dictionary<int, SelectionElement> newElements = BuildDirectoryTreeElements();
            
            directoryTreeElements.Clear();
            foreach (var k in newElements.Keys) {
                directoryTreeElements[k] = newElements[k];
            }
        }
        
        Dictionary<int, SelectionElement> directoryTreeElements = new Dictionary<int, SelectionElement>();
        public SelectionElement GetDirectoryTreeElement(int byID) {
            return directoryTreeElements[byID];
        }


        



        void RebuildDisplayedElements() {    
            RebuildDirectoryTreeElements();

            RebuildMainDisplay();
            RebuildBackButton();
            UpdateDragDropElements();
        }
        SelectionElement backButton;

        List<SelectionElement> BuildElementsList () {
            List<SelectionElement> poolElements = getPoolElements(currentShownDirectoryID);
            if (!searchFilter.IsEmpty()) {
                string lowerSearch = searchFilter.ToLower();
                poolElements = poolElements.Where(e => e.gui.text.ToLower().Contains(lowerSearch)).ToList();
            }
            return poolElements;
        }




        void DrawTopToolbar (Vector2 mousePos, bool mouseUp, out bool searchChanged) {
            EditorGUILayout.BeginHorizontal();
            
            if (backButton != null) {
                backButton.DrawBackButton(mousePos, mouseUp, dragDropHandler);
            }

            //cur path
            GUI.enabled = false;
            GUI.backgroundColor = Color.white;

            string curPath = GetCurrentDisplayPathText();
            EditorGUILayout.TextField(curPath);
            GUI.enabled = true;

            //draw
            showDirectoryTree = GUIUtils.ToggleButton(new GUIContent("Show Directory Tree"), GUIStyles.toolbarButton, showDirectoryTree, out _);

            //search
            string lastSearch = searchFilter;
            string defSearch = searchFilter.IsEmpty() ? "Search" : searchFilter;
            string searchResult = GUIUtils.DrawTextField(defSearch, GUIUtils.TextFieldType.Delayed, true, "Search", out _, searchBarWidth);
            if (searchResult != defSearch) searchFilter = searchResult;
            searchChanged = searchFilter != lastSearch;
            
            toolbarButtons();
            
            EditorGUILayout.EndHorizontal();
        }

        static readonly GUILayoutOption searchBarWidth = GUILayout.Width(128);


        void DrawMainElementsList (Vector2 mousePos, bool mouseUp, out bool searchChanged) {
            EditorGUILayout.BeginVertical();

            DrawTopToolbar(mousePos, mouseUp, out searchChanged);        
            
            GUIUtils.Space();
    
            int l = mainElements.Length;
            if (l == 0) {
                GUIUtils.HelpBox("No Elements!", MessageType.Info);   
            }
            else {
                for (int i = 0; i < l; i++) {
                    mainElements[i].Draw(mousePos, mouseUp, dragDropHandler, selectionHandler.trackedRange);
                }
            }
            
                
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


        void DrawElementsView (Vector2 mousePos, bool mouseUp, 
            
            // GUIContent pagesGUI, 
            out bool searchChanged//, 
            // out int pageOffset, 
            ) {
            
            GUIUtils.StartBox(0);

            EditorGUILayout.BeginHorizontal();
            
            DrawMainElementsList(mousePos, mouseUp, out searchChanged);
            
            EditorGUILayout.EndHorizontal();
            
            GUIUtils.EndBox();

            //pagination gui
            // pageOffset = DrawPages(pagesGUI);   
        }
    }
}