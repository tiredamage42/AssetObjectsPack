using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace AssetObjectsPacks {

    public class ElementSelectionSystem 
    {

        
        static bool WindowElementGUI (GUIContent gui, bool selected, bool hidden, bool directory, bool enabled, bool hover, Rect rt) {
            GUIStyle s = GUIStyles.toolbarButton;

            TextAnchor a = s.alignment;
            Texture2D t = s.normal.background;
            Texture2D th = s.active.background;
                
            s.fontStyle = hidden ? FontStyle.Italic : FontStyle.Normal;

            //if (!hover) {
                s.normal.background = (!selected && !directory) ? null : t;
                s.active.background = (!selected && !directory) ? null : th;
            //}
            s.alignment = TextAnchor.MiddleLeft;
                    
            Color32 txtColor = hidden ? Colors.yellow : (selected || directory || hover ? Colors.black : Colors.liteGray);

            GUI.enabled = enabled;
            bool pressed = GUIUtils.Button(gui, s, Colors.Toggle(selected), txtColor, rt);
            GUI.enabled = true;

            //if (!hover) {
                s.normal.background = t;
                s.active.background = th;
            //}
            s.alignment = a;
            s.fontStyle = FontStyle.Normal;

            return pressed;

        }
        
        public struct Element {
            public int id, poolIndex;
            public string path;
            public Element (int id, string path, int poolIndex) => (this.id, this.path, this.poolIndex) = (id, path, poolIndex);
        }
        public class Inputs {
            public string folderFwdDir = null;
            public PaginationAttempt paginationAttempt;
            public bool paginationSuccess, hiddenToggleSuccess, selectionChanged;
            public bool folderBack, searchChanged, changedTabView, createdDirectory, droppedOnReceiver;
            public IEnumerable<int> droppedIndicies;
            public Element dropReceiver;

        }

        public int viewTab;

        public struct ViewTabOption {
            public bool canHide;
            public GUIContent name;
            public ViewTabOption(GUIContent name, bool canHide) {
                this.name = name;
                this.canHide = canHide;
            }
        }

        public bool justFilesSelected { get { return selectionSystem.hasSelection && !selectionHasDirs; } }
        public bool hasSelection { get { return selectionSystem.hasSelection; } }
        public bool singleSelection { get { return selectionSystem.singleSelection; } }
        public Element selectedElement {
            get {
                int index;
                bool singleSelection = selectionSystem.SingleSelection(out index);
                if (!hasSelection || !singleSelection) return new Element(-1,"",-1);
                return mainElements[index];
            }
        }

        SelectionSystem selectionSystem = new SelectionSystem();
        StateToggler hiddenIDsToggler = new StateToggler();
        SearchFilter searchFilter = new SearchFilter();
        Pagination pagination= new Pagination();


        
        public string curPath = "", parentPath = "";

        void ResetCurrentPath () {
            curPath = parentPath = "";
        }
        
        Element[] mainElements, prevWindowElements;
        
        public delegate Element GetElementAtPoolIndex (int index, int variation, string dirPath);
        public delegate int GetPoolCount (int variation, string dirPath);
        public delegate HashSet<int> GetIgnorePoolIDs(int variation);
        GetElementAtPoolIndex getElementAtPoolIndex;
        GetPoolCount getPoolCount;
        GetIgnorePoolIDs getIgnorePoolIDs;
        System.Action onSelectionChange;
        System.Action<IEnumerable<int>, string, string, int> onDirDragDrop;
        
        bool SingleDirectorySelected(out int index) {
            return selectionSystem.SingleSelection(out index) && mainElements[index].id == -1;
        }
        public void DrawPages (Inputs inputs, KeyboardListener kbListener){
            inputs.paginationAttempt = pagination.DrawPages(kbListener, out inputs.paginationSuccess);
        }

        string DirDisplayName ( string path, int mainFocusOffset, out bool isFile) {

            int mainFolderOffset = 0;    
            if (curPath.Contains("/")) {
                mainFolderOffset = curPath.Split('/').Length;
            }
            else if (!curPath.IsEmpty()) {
                mainFolderOffset = 1;
            }

            int i = mainFolderOffset + mainFocusOffset;
            string[] sp = path.Split('/');
            isFile = i == sp.Length - 1;
            string res = sp[i];
            return res;
        }

        GUIContent GetValues(int atIndex, out bool isDir, out bool isSelected, out bool isHidden) {
            int id = mainElements[atIndex].id;
            string path = mainElements[atIndex].path;
            isDir = id == -1;
            isSelected = selectionSystem.IsSelected(atIndex);
            isHidden = showingHidden && !isDir;
            return new GUIContent(  isDir ? DirDisplayName(path, 0, out _) : (path.Contains("/") ? path.Split('/').Last() : path)  );                
        }

        void DrawTopToolbar (int viewTab, Inputs inputs, KeyboardListener kbListener) {
            EditorGUILayout.BeginHorizontal();
            
            //cur path
            GUI.enabled = false;
            EditorGUILayout.TextField(curPath);            
            GUI.enabled = true;
            //search
            inputs.searchChanged = searchFilter.SearchBarGUI();

            //buttons
            GUIStyle s = GUIStyles.toolbarButton;

            //add directory
            if (!showingHidden) {            
                if (GUIUtils.Button(new GUIContent(EditorGUIUtility.IconContent("Folder Icon").image, "Add Folder"), s, iconWidth)){
                    onDirectoryCreate(viewTab, curPath);
                    inputs.createdDirectory = true;
                }
            }

            //toggle hidden
            if (viewTabOptions[viewTab].canHide) {
                ToggleHiddenButtonGUI(kbListener, inputs, s);    
            }
            //extra
            extraToolbarButtons(viewTab, s);
            
            EditorGUILayout.EndHorizontal();
        }

        void DrawAlternateWindowElement (int i, ref int clickedI, ref int clickedCol) {
            string path = prevWindowElements[i].path;
            DrawBaseWindowElement(
                0, 
                new GUIContent(i == 0 ? (" << " + (path.IsEmpty() ? "Base" : path)) : DirDisplayName(path, -1, out _)), 
                i != 0 && curPath == path, i == 0, false, true, false, i, 
                ref clickedI, ref clickedCol
            );            
        }

        Rect DrawBaseWindowElement (int windowOffset, GUIContent gui, bool isSelected, bool drawDir, bool isHidden, bool isReceiver, bool isDraggable, int i, ref int clickedElementIndex, ref int clickedElementWindowIndex) {
            Rect rt = GUILayoutUtility.GetRect(gui, GUIStyles.toolbarButton);
            bool beingDragged, isDragHovered, dropReceived;
            dragDrop.CheckElementRectForDragsOrDrops (i, windowOffset, rt, isReceiver, isDraggable, out beingDragged, out isDragHovered, out dropReceived);            
            if (WindowElementGUI (gui, isSelected || (isReceiver && isDragHovered), isHidden, drawDir, !beingDragged && !dropReceived, false, rt)) {
                clickedElementIndex = i;
                clickedElementWindowIndex = windowOffset;
            }
            return rt;
        }
        Rect DrawMainWindowElement (int i, ref int clickedElementIndex, ref int clickedElementWindowIndex) {
            bool isDir, isHidden, isSelected;
            return DrawBaseWindowElement(1, GetValues(i, out isDir, out isSelected, out isHidden), isSelected, isDir, isHidden, isDir, true, i, ref clickedElementIndex, ref clickedElementWindowIndex);
        }
        void DrawDragHoverElement (int i, Rect r) {
            bool isDir, isHidden, isSelected;
            WindowElementGUI (GetValues(i, out isDir, out isSelected, out isHidden), isSelected, isHidden, isDir, !isDir && !isSelected, true, r);  
        }
            

        public void DrawElements (int viewTab, Inputs inputs, KeyboardListener keyboardListener){
            
            dragDrop.InputListen ();
            
            int clickedElementWindowIndex = -1, clickedElementIndex = -1;
            
            inputs.selectionChanged = false;

            EditorGUILayout.BeginHorizontal();

            //prev directory elements
            int l = prevWindowElements.Length;
            if (l != 0) {
            
                EditorGUILayout.BeginVertical(GUILayout.MaxWidth(64));

                GUIUtils.StartBox(0);


                //EditorGUILayout.BeginVertical(GUILayout.MinHeight(3));
                //EditorGUILayout.BeginHorizontal();//GUILayout.MinHeight(3));
                //EditorGUILayout.EndHorizontal();
                //EditorGUILayout.EndVertical();

                for (int i = 0; i < l; i++) {
                    bool isBackDir = i == 0;
                    DrawAlternateWindowElement (i, ref clickedElementIndex, ref clickedElementWindowIndex);
                    if (i == 0) GUIUtils.Space();
                }

                GUIUtils.EndBox(0);

                EditorGUILayout.EndVertical();
            }


            //main elements
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(1000));

            GUIUtils.StartBox(0);

            DrawTopToolbar (viewTab, inputs, keyboardListener);

            GUIUtils.Space();
            
            l = mainElements.Length;
            Rect[] mainRects = new Rect[l];
            
            if (l == 0) {
                EditorGUILayout.HelpBox("No Elements!", MessageType.Info);   
            }
            else {
                for (int i = 0; i < l; i++) mainRects[i] = DrawMainWindowElement (i, ref clickedElementIndex, ref clickedElementWindowIndex);
            }

            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            GUIUtils.EndBox(0);
            
            //end loop

            dragDrop.CheckNewDragStart (mainRects, selectionSystem);

            int receiverIndex, receiverCollection, droppedCollection;
            inputs.droppedOnReceiver = dragDrop.CheckDrop (out receiverIndex, out receiverCollection, out droppedCollection, out inputs.droppedIndicies);
            if (inputs.droppedOnReceiver) {
                //Debug.Log("drop on receiver");
                inputs.dropReceiver = receiverCollection == 0 ? prevWindowElements[receiverIndex] : mainElements[receiverIndex];
            }
    
            dragDrop.DrawDragGUIs (mainRects, DrawDragHoverElement);

            inputs.selectionChanged = selectionSystem.HandlDirectionalSelection(keyboardListener[KeyCode.UpArrow], keyboardListener[KeyCode.DownArrow], keyboardListener.shift, mainElements.Length - 1);

            //enter movies fwd directory when one selected
            CheckForEnterPressOnSingleDirectory (keyboardListener, ref clickedElementIndex, ref clickedElementWindowIndex);
            CheckClickedElement(clickedElementIndex, clickedElementWindowIndex, inputs, keyboardListener);

            
        }

        void CheckClickedElement(int clickedIndex, int collection, Inputs inputs, KeyboardListener keyboardListener) {
            if (clickedIndex == -1) return;

            //main window
            if (collection == 1) {
                //element select
                if (mainElements[clickedIndex].id != -1){
                    selectionSystem.OnObjectSelection(clickedIndex, keyboardListener.shift);
                    inputs.selectionChanged = true;
                }
                //folder select
                else {
                    inputs.folderFwdDir = mainElements[clickedIndex].path;
                }
            }
            //prev directory window
            else if (collection == 0) {
                //back directory
                if (prevWindowElements[clickedIndex].id == -2){
                    Debug.Log("Go To: prev Directory");    
                    inputs.folderBack = true;
                }
                //directory select
                else {
                    //not already there
                    string path = prevWindowElements[clickedIndex].path;
                    bool isCurrent = path == curPath;
                    if (!isCurrent) {
                        inputs.folderFwdDir = path;
                        Debug.Log("Go To: " + path);
                    }
                }
            }
            
        }

        void CheckForEnterPressOnSingleDirectory (KeyboardListener keyboardListener, ref int clickedElementIndex, ref int clickedElementWindowIndex) {
            //enter movies fwd directory when one selected
            int lo = -1;
            if (clickedElementIndex == -1 && keyboardListener[KeyCode.Return] && SingleDirectorySelected(out lo)) {
                clickedElementIndex = lo;
                clickedElementWindowIndex = 1;
            }
        }


        DragAndDrop dragDrop = new DragAndDrop();

        void OnDropDir(IEnumerable<int> dragged, Element dirElement) {
            onDirDragDrop(dragged, curPath, dirElement.path, viewTab);
        }

        public void HandleInputs (Inputs inputs, bool forceRebuild) {

            

            bool clearAndRebuild = forceRebuild || inputs.createdDirectory || inputs.changedTabView || inputs.searchChanged|| inputs.paginationSuccess;// || inputs.movedFiles;
            bool resetPage = inputs.changedTabView;
                
            clearAndRebuild = clearAndRebuild || ((inputs.hiddenToggleSuccess));
                        
            if (inputs.paginationAttempt == PaginationAttempt.Back && !inputs.paginationSuccess) {
                inputs.folderBack = true;
            }
            
            bool movedFolder = (inputs.folderBack && MoveFolder()) || (inputs.folderFwdDir != null && MoveFolder(inputs.folderFwdDir));
            clearAndRebuild = clearAndRebuild || movedFolder;
            resetPage = resetPage || movedFolder;
            




            if (inputs.droppedOnReceiver) {
                OnDropDir(inputs.droppedIndicies, inputs.dropReceiver);
                //inputs.movedFiles = true;
                clearAndRebuild = true;
            }
            
            if (inputs.changedTabView) {
                ResetCurrentPath();
            }
            if (clearAndRebuild) {                
                ClearSelectionsAndRebuild(resetPage);
                inputs.selectionChanged = true;
            }
            if (inputs.selectionChanged) onSelectionChange();
        }

        //public bool isDragging {
        //    get {
        //        return dragDrop.dragging;
        //    }
        //}

        public void ForceBackFolder () {
            MoveFolder();
            ClearSelectionsAndRebuild(true);
            onSelectionChange();
        }

        string CalcLastFolder(string dir) {
            if (dir.IsEmpty() || !dir.Contains("/")) return string.Empty;
            return dir.Substring(0, dir.LastIndexOf("/"));
        }
        bool MoveFolder(string toPath = null){
            if (toPath == null) {
                if (curPath.IsEmpty()) return false;
                curPath = CalcLastFolder(curPath);
            }
            else curPath = toPath;
            parentPath = CalcLastFolder(curPath);
            return true;
        }
        
        bool selectionHasDirs {
            get {
                if (!selectionSystem.hasSelection) return false;
                foreach (var i in selectionSystem.GetSelectionEnumerator()) {
                    if (mainElements[i].id == -1) return true;
                }
                return false;
            }
        }

        HashSet<int> IDsInDirectory(string dir, int poolCount, HashSet<int> ignorePoolIDs) { 
            return new HashSet<int>().Generate(
                new List<Element>()
                    .Generate(poolCount, i => getElementAtPoolIndex(i, viewTab, curPath))
                    .Where(e => !ignorePoolIDs.Contains(e.id) && showingHidden == hiddenIDsToggler.IsState(e.id) && e.path.StartsWith(dir)),
                e => e.id
            );
        }
        IEnumerable<int> GetPoolIndiciesInSelectionBase() {                
            return new HashSet<int>().Generate( selectionSystem.GetSelectionEnumerator(), i => (mainElements[i].id == -1 ? -1 : mainElements[i].poolIndex) );
        }
        IEnumerable<int> GetPoolIndiciesInElementsBase () {
            return new HashSet<int>().Generate( mainElements, e => e.id == -1 ? -1 : e.poolIndex );
        }
        HashSet<int> FilterOutDirectories (IEnumerable<int> e) {
            return e.Where( i => i != -1 ).ToHashSet();
        }
        public HashSet<int> GetPoolIndiciesInSelection() {
            return new HashSet<int>().Generate( selectionSystem.GetSelectionEnumerator(), i => mainElements[i].poolIndex );
        }
        public HashSet<int> GetPoolIndiciesInSelectionOrAllShown () {
            return FilterOutDirectories( selectionSystem.hasSelection ? GetPoolIndiciesInSelectionBase() : GetPoolIndiciesInElementsBase() );
        }

        public HashSet<int> GetIDsInElements(IEnumerable<int> selectionIndicies, bool includeDirs, out bool hasDirs) {
            hasDirs = false;
            int poolCount = getPoolCount(viewTab, curPath);
            HashSet<int> ignorePoolIDs = getIgnorePoolIDs(viewTab);
            HashSet<int> ids = new HashSet<int>();
            foreach (var i in selectionIndicies) {
                int id = mainElements[i].id;
                if (id == -1) {
                    if (includeDirs) {
                        ids.AddRange( IDsInDirectory(curPath + "/" + DirDisplayName( mainElements[i].path, 0, out _ ), poolCount, ignorePoolIDs) );
                        hasDirs = true;
                    }
                }
                else ids.Add(id);
            }
            return ids;
        }
        public HashSet<int> GetIDsInSelection(bool includeDirs, out bool hasDirs) {
            hasDirs = false;
            if (!selectionSystem.hasSelection) return null;
            return GetIDsInElements(selectionSystem.GetSelectionEnumerator(), includeDirs, out hasDirs);
        }
        public HashSet<int> GetIDsInElements(bool includeDirs, out bool hasDirs) {
            hasDirs = false;
            return GetIDsInElements(new HashSet<int>().Generate( mainElements.Length, i => i ), includeDirs, out hasDirs);
        }

        ViewTabOption[] viewTabOptions;
        System.Action<int, string> onDirectoryCreate;
        System.Action<int, GUIStyle> extraToolbarButtons;
        
        public void Initialize (
            ViewTabOption[] viewTabOptions, 
            EditorProp hiddenIDsProp, 
            GetElementAtPoolIndex getElementAtPoolIndex, 
            GetPoolCount getPoolCount, GetIgnorePoolIDs getIgnorePoolIDs, 

        System.Action onSelectionChange,
        System.Action<IEnumerable<int>, string, string, int> onDirDragDrop,
        System.Action<int, string> onDirectoryCreate,
        System.Action<int, GUIStyle> extraToolbarButtons
                
                
        ) {
            this.extraToolbarButtons = extraToolbarButtons;
            this.onDirectoryCreate = onDirectoryCreate;
            this.onDirDragDrop = onDirDragDrop;
            this.getElementAtPoolIndex = getElementAtPoolIndex;
            this.getPoolCount = getPoolCount;
            this.getIgnorePoolIDs = getIgnorePoolIDs;
            this.onSelectionChange = onSelectionChange;

            List<ViewTabOption> viewOpts = new List<ViewTabOption>();
            int l = viewTabOptions.Length;
            if (l != 0) {
                for (int i = 0; i < l; i++) viewOpts.Add(viewTabOptions[i]);
            }
            else {
                viewOpts.Add(new ViewTabOption( new GUIContent("Select"), true ));
            }
            viewOpts.Add(new ViewTabOption( new GUIContent("Hidden"), true ));
            this.viewTabOptions = viewOpts.ToArray();

            hiddenIDsToggler.Initialize(hiddenIDsProp);

            ResetCurrentPath();
            ClearSelectionsAndRebuild(true);
            onSelectionChange();
        }

        public bool showingHidden { get { return viewTab == viewTabOptions.Length - 1; } }

/*
        string WithoutLastSlash(string path) {
            if (path.IsEmpty() || !path.EndsWith("/")) {
                return path;
            }
            return path.Substring(0, path.Length - 1);
        }
*/

        void RebuildDisplayedElements() {
            
            int poolCount = getPoolCount(viewTab, curPath);
            HashSet<int> ignorePoolIDs = getIgnorePoolIDs(viewTab);

            HashSet<string> used = new HashSet<string>();         

            int lastDir = 0;
            
            IEnumerable<Element> filtered = new List<Element>()
                .Generate(poolCount, i => getElementAtPoolIndex(i, viewTab, curPath))
                .Where(e => (ignorePoolIDs == null || !ignorePoolIDs.Contains(e.id)) && showingHidden == hiddenIDsToggler.IsState(e.id) && searchFilter.PassesSearchFilter(e.path))
            ;

            List<Element> unpaginated = new List<Element>();

            foreach (var e in filtered) {
                if (!curPath.IsEmpty() && !e.path.StartsWith(curPath)) continue;

                string name = e.path;
                bool isFile = true;

                if (name.Contains("/")) name = DirDisplayName ( name, 0, out isFile );
                
                //foldered directory
                if (!isFile) {
                    if (used.Contains(name)) continue;
                    used.Add(name);
                    unpaginated.Insert(lastDir, new Element(-1, curPath + (curPath.IsEmpty() ? "" : "/") + name, e.poolIndex));
                    lastDir++;       
                    continue;
                }   
                unpaginated.Add(e);
            }
            mainElements = pagination.Paginate(unpaginated).ToArray();
            
            used.Clear();   
            
            List<Element> prevElement = new List<Element>();

            if (!curPath.IsEmpty()) {

                poolCount = getPoolCount(viewTab, parentPath);

                IEnumerable<Element> prevfiltered = new List<Element>()
                    .Generate(poolCount, i => getElementAtPoolIndex(i, viewTab, parentPath))
                    .Where(e => (ignorePoolIDs == null || !ignorePoolIDs.Contains(e.id)) && showingHidden == hiddenIDsToggler.IsState(e.id) && searchFilter.PassesSearchFilter(e.path))
                ;

                foreach (var e in prevfiltered) {
                    if (!parentPath.IsEmpty() && !e.path.StartsWith(parentPath)) continue;

                    string name = e.path;
                    bool isFile = true;
                    if (name.Contains("/")) name = DirDisplayName(name, -1, out isFile);
                    
                    if (!isFile) {
                        if (used.Contains(name)) continue;
                        used.Add(name);            
                        prevElement.Add(new Element(-1, parentPath + (parentPath.IsEmpty() ? "" : "/")+ name, -1));
                    }   
                }
                prevElement.Insert(0, new Element(-2, parentPath, -1));
            }
            prevWindowElements = prevElement.ToArray();
        }

        void ClearSelectionsAndRebuild(bool resetPage) {
            if (resetPage) pagination.ResetPage();
            selectionSystem.ClearSelection();
            RebuildDisplayedElements();
        }

        public void DrawToolbar (KeyboardListener kbListener, Inputs inputs) {
            GUIUtils.StartBox(0);
            inputs.changedTabView = GUIUtils.Tabs(new GUIContent[viewTabOptions.Length].Generate(viewTabOptions, e => e.name), ref viewTab);
            GUIUtils.EndBox(0);
        }

        GUILayoutOption iconWidth = GUILayout.Width(20);

        void ToggleHiddenButtonGUI (KeyboardListener kbListener, Inputs input, GUIStyle s) {
            hiddenIDsToggler.ToggleStateButton(
                kbListener[KeyCode.H], 
                EditorGUIUtility.IconContent("animationvisibilitytoggleon", "Toggle the hidden status of the selection (if any, else all shown elements)"),
                s, iconWidth, out input.hiddenToggleSuccess, GetHiddenToggleSelection
            );
        }

        HashSet<int> GetHiddenToggleSelection () {
            bool containsDirs;
            HashSet<int> selection = !selectionSystem.hasSelection ? GetIDsInElements(true, out containsDirs) : GetIDsInSelection(true, out containsDirs);
            if (containsDirs && !EditorUtility.DisplayDialog("Hide/Unhide Directory", "Selection contains directories, hidden status of all sub elements will be toggled", "Ok", "Cancel")) selection.Clear();
            return selection;
        }
    }
}