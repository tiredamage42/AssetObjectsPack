using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
namespace AssetObjectsPacks {
    public class ElementSelectionSystem  {
        public struct Element {
            public int id, poolIndex;
            public string path;
            public bool isNewDir, isCopy;
            public Element (int id, string path, int poolIndex, bool isNewDir, bool isCopy) 
            => (this.id, this.path, this.poolIndex, this.isNewDir, this.isCopy, this.gui) 
            = (id, path, poolIndex, isNewDir, isCopy, null);

            public GUIContent gui;
            public void SetGUI(GUIContent gui) {
                this.gui = gui;
            }
        }
        
        public bool justFilesSelected { get { return hasSelection && !selectionHasDirs; } }
        public bool singleSelection { get { return selectionSystem.singleElement; } }
        public bool hasSelection { get { return selectionSystem.hasElements; } }
        public Element selectedElement {
            get {
                int index;
                bool singleSelection = selectionSystem.HasSingleElement(out index);
                if (!hasSelection || !singleSelection) return new Element(-1, "", -1, false, false);
                return mainElements[index];
            }
        }
        bool selectionHasDirs {
            get {
                if (!hasSelection) return false;
                foreach (var i in selectionSystem.GetTrackedEnumerable()) {
                    if (mainElements[i].id == -1) return true;
                }
                return false;
            }
        }
        bool SingleDirectorySelected(out int index) {
            return selectionSystem.HasSingleElement(out index) && mainElements[index].id == -1;
        }

        SelectionSystem selectionSystem = new SelectionSystem();
        SearchFilter searchFilter = new SearchFilter();
        Pagination pagination= new Pagination();
        
        public string curPath = "";
        Element[] mainElements, prevWindowElements;        
        Action onSelectionChange;
        Action<IEnumerable<int>, string, string> onDirDragDrop;
        Action<int, string> onNameNewDirectory;
        DragAndDrop dragDrop = new DragAndDrop();
        Action<string, KeyboardListener> extraToolbarButtons;
        
        string origDirName;

        bool DrawNewDirectory (Rect rt, int poolIndex) {
            string nm = "NewDir";
            GUIUtils.NameNextControl(nm);
            origDirName = EditorGUI.TextField(rt, origDirName);
            GUIUtils.FocusOnTextArea(nm);
            GUI.FocusControl(nm);

            UnityEngine.Event e = UnityEngine.Event.current;
            bool isFocused = true;
            if (e.type == EventType.MouseDown && e.button == 0 && !rt.Contains(e.mousePosition)) isFocused = false;
            bool enterPressed = isFocused && e.keyCode == KeyCode.Return;

            bool setName = !isFocused || enterPressed;
            if (setName) {
                GUIUtils.FocusOnTextArea("");
                onNameNewDirectory(poolIndex, origDirName);
                origDirName = "";
            }
            return setName;
        }

        
        
        static bool WindowElementGUI (GUIContent gui, bool isCopy, bool selected, bool directory, bool enabled, bool hover, Rect rt) {

            GUIStyle s = GUIStyles.toolbarButton;
            TextAnchor a = s.alignment;
            Texture2D t = s.normal.background;
            Texture2D th = s.active.background;
                
            s.fontStyle = isCopy ? FontStyle.Italic : FontStyle.Normal;

            s.normal.background = (!selected && !directory) ? null : t;
            s.active.background = (!selected && !directory) ? null : th;

            s.alignment = TextAnchor.MiddleLeft;
                    
            Color32 txtColor = selected || directory || hover ? Colors.black : (isCopy ? Colors.green : Colors.liteGray);

            GUI.enabled = enabled;
            bool pressed = GUIUtils.Button(gui, s, Colors.Toggle(selected), txtColor, rt);
            GUI.enabled = true;

            s.normal.background = t;
            s.active.background = th;
            
            s.alignment = a;
            s.fontStyle = FontStyle.Normal;

            return pressed;
        }

        string DirDisplayName ( string path, int mainFocusOffset, out bool isFile) {

            int mainFolderOffset = 0;    
            if (curPath.Contains("/")) mainFolderOffset = curPath.Split('/').Length;
            else if (!curPath.IsEmpty()) mainFolderOffset = 1;
        
            int i = mainFolderOffset + mainFocusOffset;
            string[] sp = path.Split('/');
            isFile = i == sp.Length - 1;

            string res = sp[i];
            return res;
        }

        string curPathPrefix { get { return curPath.IsEmpty() ? curPath : curPath + "/"; } }

        public void OnDirectoryCreate (string origName) {
            origDirName = origName;
        }

        void DrawTopToolbar (KeyboardListener k, out bool searchChanged) {
            EditorGUILayout.BeginHorizontal();
            
            //cur path
            GUI.enabled = false;
            EditorGUILayout.TextField(curPath, GUILayout.Height(17));            
            GUI.enabled = true;
            
            //search
            searchChanged = searchFilter.SearchBarGUI();

            extraToolbarButtons(curPath, k);
            
            EditorGUILayout.EndHorizontal();
        }
        
        void DrawAlternateWindowElement (int i, ref int clickedI, ref int clickedCol) {
            string path = prevWindowElements[i].path;
            GUIContent gui = prevWindowElements[i].gui;
            
            DrawBaseWindowElement(GUILayoutUtility.GetRect(gui, GUIStyles.toolbarButton), prevWindowElements[i], 0, gui, i != 0 && curPath == path, i == 0, true, false, i, ref clickedI, ref clickedCol);
        }

        void DrawBaseWindowElement (Rect rt, Element e, int windowOffset, GUIContent gui, bool isSelected, bool drawDir, bool isReceiver, bool isDraggable, int i, ref int clickedI, ref int clickedCol) {
            
            bool beingDragged, isDragHovered, dropReceived;
            dragDrop.CheckElementRectForDragsOrDrops (i, windowOffset, rt, isReceiver, isDraggable, out beingDragged, out isDragHovered, out dropReceived);            
            if (WindowElementGUI (gui, e.isCopy, isSelected || (isReceiver && isDragHovered), drawDir, !beingDragged && !dropReceived, false, rt)) {
                clickedI = i;
                clickedCol = windowOffset;
            }
        }
        Rect DrawMainWindowElement (int i, ref int clickedI, ref int clickedCol) {
            
            GUIContent gui = mainElements[i].gui;
            Rect rt = GUILayoutUtility.GetRect(gui, GUIStyles.toolbarButton);            
            if (mainElements[i].isNewDir) {
                DrawNewDirectory(rt, mainElements[i].poolIndex); 
            }
            else {
                bool isDir = mainElements[i].id == -1;   
                DrawBaseWindowElement(rt, mainElements[i], 1, gui, selectionSystem.IsTracked(i), isDir, isDir, true, i, ref clickedI, ref clickedCol);
            }
            return rt;
        }

        void DrawDragHoverElement (int i, Rect r) {
            bool isDir = mainElements[i].id == -1, isSelected = selectionSystem.IsTracked(i);
            
            WindowElementGUI (mainElements[i].gui, mainElements[i].isCopy, isSelected, isDir, !isDir && !isSelected, true, r);  
        }

        public void ChangeCurrentDirectoryName (string newName) {
            if (curPath.Contains("/")) {
                IList<string> withoutLast = curPath.Split('/').Slice(0, -2);
                withoutLast.Add( newName );
                curPath = string.Join("/", withoutLast);
            }
            else {
                if (!curPath.IsEmpty()) curPath = newName;
            }
        }
            
        public void DrawElements (){
            KeyboardListener keyboardListener = new KeyboardListener();
            
            dragDrop.InputListen ();

            GUIUtils.StartBox(0);
            
            int clickedElementWindowIndex = -1, clickedElementIndex = -1;
            
            EditorGUILayout.BeginHorizontal();

            //prev directory elements
            int l = prevWindowElements.Length;
            if (l != 0) {
                EditorGUILayout.BeginVertical(GUILayout.MaxWidth(64));
                for (int i = 0; i < l; i++) {
                    DrawAlternateWindowElement (i, ref clickedElementIndex, ref clickedElementWindowIndex);
                    if (i == 0) GUIUtils.Space();
                }
                EditorGUILayout.EndVertical();
            }

            //main elements
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(999));

            bool searchChanged;
            DrawTopToolbar(keyboardListener, out searchChanged);
            GUIUtils.Space();
            
            l = mainElements.Length;
            Rect[] mainRects = new Rect[l];
            if (l == 0) {
                EditorGUILayout.HelpBox("No Elements!", MessageType.Info);   
            }
            else {
                for (int i = 0; i < l; i++) {
                    mainRects[i] = DrawMainWindowElement (i, ref clickedElementIndex, ref clickedElementWindowIndex);
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUIUtils.EndBox();
            
            //pagination gui
            GUIUtils.StartBox();
            bool paginationSuccess;
            PaginationAttempt paginationAttempt = pagination.DrawPages(keyboardListener, out paginationSuccess);
            GUIUtils.EndBox();

            //check drag drop
            int receiverIndex, receiverCollection, droppedCollection;
            IEnumerable<int> droppedIndicies;
            
            bool droppedOnReceiver = dragDrop.DrawAndUpdate(mainRects, DrawDragHoverElement, selectionSystem, out receiverIndex, out receiverCollection, out droppedCollection, out droppedIndicies);
            
            if (droppedOnReceiver) {
                onDirDragDrop(droppedIndicies, curPath, (receiverCollection == 0 ? prevWindowElements[receiverIndex] : mainElements[receiverIndex]).path);//, viewTab);
            }

            //check clicked element
            bool folderBack;
            string forwardDir;

            bool selectionChanged;
            CheckClickedElement(out selectionChanged, clickedElementIndex, clickedElementWindowIndex, keyboardListener, out forwardDir, out folderBack);
            
            //fialed page back attempts back folder
            //if (paginationAttempt == PaginationAttempt.Back && !paginationSuccess) f
            if (keyboardListener[KeyCode.LeftArrow]) folderBack = true;
            
            bool movedFolder = (folderBack && MoveFolder()) || (forwardDir != null && MoveFolder(forwardDir));
            
            selectionChanged = selectionSystem.HandlDirectionalSelection(keyboardListener[KeyCode.UpArrow], keyboardListener[KeyCode.DownArrow], keyboardListener.shift, mainElements.Length - 1) || selectionChanged;


            shouldRebuild = droppedOnReceiver || movedFolder || searchChanged|| paginationSuccess;            
            shouldResetPage = movedFolder;

            if (!shouldRebuild && selectionChanged) onSelectionChange();
        }

        bool shouldRebuild, shouldResetPage;
            
            
        public void CheckRebuild (bool forceRebuild, bool forceReset) {
            if (forceRebuild || shouldRebuild) DoReset(forceReset, shouldResetPage || forceReset);
            shouldResetPage = shouldRebuild = false;
        }

        void CheckClickedElement(out bool selectionChanged, int clickedIndex, int collection, KeyboardListener k, out string forwardDir, out bool folderBack) {
            forwardDir = null;
            folderBack = false;
            selectionChanged = false;

            //right arrow movies fwd directory when one selected
            int lo = -1;
            if (clickedIndex == -1 && k[KeyCode.RightArrow] && SingleDirectorySelected(out lo)) {
                clickedIndex = lo;
                collection = 1;
            }
            
            if (clickedIndex == -1) return;

            //main window
            if (collection == 1) {
                //element select
                if (mainElements[clickedIndex].id != -1){
                    selectionSystem.OnObjectSelection(clickedIndex, k.shift);
                    selectionChanged = true;
                }
                //folder select
                else forwardDir = mainElements[clickedIndex].path;
            }
            //prev directory window
            else if (collection == 0) {
                //back directory
                if (prevWindowElements[clickedIndex].id == -2){
                    folderBack = true;
                }
                //directory select
                else {
                    string path = prevWindowElements[clickedIndex].path;
                    //if not already there
                    if (path != curPath) forwardDir = path;
                }
            }
        }
        public void ForceBackFolder () {
            MoveFolder();
            DoReset(false, true);
        }
        public string CalcLastFolder(string dir) {
            if (dir.IsEmpty() || !dir.Contains("/")) return string.Empty;
            return dir.Substring(0, dir.LastIndexOf("/"));
        }
        bool MoveFolder(string toPath = null){
            if (toPath == null) {
                if (curPath.IsEmpty()) return false;
                curPath = CalcLastFolder(curPath);
            }
            else curPath = toPath;
            return true;
        }
        
        public IEnumerable<int> GetPoolIndiciesInSelection (bool deep) {
            return selectionSystem.GetTrackedEnumerable().Generate( i => mainElements[i].id == -1 && !deep ? -1 : mainElements[i].poolIndex );
        }
        public IEnumerable<int> GetPoolIndiciesInShown (bool deep) {
            return mainElements.Generate( e => e.id == -1 && !deep ? -1 : e.poolIndex );
        }
        public IEnumerable<int> GetPoolIndiciesInSelectionOrAllShown () {
            return (hasSelection ? GetPoolIndiciesInSelection(false) : GetPoolIndiciesInShown(false)).Where( i => i != -1 );
        }


        IEnumerable<int> _GetIDsInIndicies(bool useRepeats, IEnumerable<int> indicies, bool includeDirs, string dirCheckTitle, string dirCheckMsg) {
            if (indicies == null) return null;
            HashSet<int> ids = new HashSet<int>();
            bool checkedDirs = false;
            foreach (var i in indicies) {
                int id = mainElements[i].id;
                if (id == -1) {
                    if (!includeDirs) continue;
                    if (!checkedDirs) {
                        if (!EditorUtility.DisplayDialog(dirCheckTitle, dirCheckMsg, "Ok", "Cancel")) {
                            ids.Clear();
                            return ids;
                        }
                        checkedDirs = true;
                    }
                    getIDsInDirectory(curPathPrefix + DirDisplayName( mainElements[i].path, 0, out _ ), useRepeats, ids);
                }
                else ids.Add(id);
            }
            return ids;
        }
        
        public IEnumerable<int> GetIDsInSelectionDeep (string dirCheckTitle, string dirCheckMessage) {
            return _GetIDsInIndicies(false, GetSelectionEnumerable(), true, dirCheckTitle, dirCheckMessage);
        }
        public IEnumerable<int> GetIDsInSelection () {
            return _GetIDsInIndicies(false, GetSelectionEnumerable(), false, null, null);
        }
        public IEnumerable<int> GetSelectionEnumerable () {
            return selectionSystem.GetTrackedEnumerable();
        }

        Func<string, IEnumerable<Element>> getPoolElements;
        Action<string, bool, HashSet<int>> getIDsInDirectory;

        public void OnEnable (Func<string, IEnumerable<Element>> getPoolElements, Action<string, bool, HashSet<int>> getIDsInDirectory, Action onSelectionChange, Action<IEnumerable<int>, string, string> onDirDragDrop, Action<string, KeyboardListener> extraToolbarButtons, Action<int, string> onNameNewDirectory) {
            this.getIDsInDirectory = getIDsInDirectory;
            this.onNameNewDirectory = onNameNewDirectory;
            this.extraToolbarButtons = extraToolbarButtons;
            this.onDirDragDrop = onDirDragDrop;
            this.onSelectionChange = onSelectionChange;
            this.getPoolElements = getPoolElements;
        }

        public void Initialize () {
            DoReset(true, true);
        }

        void DoReset (bool resetPath, bool resetPage) {
            if (resetPath) curPath = "";//parentPath = "";
            if (resetPage) pagination.ResetPage();
            selectionSystem.ClearTracker();   
            dragDrop.ClearTracker();
            RebuildDisplayedElements();
            onSelectionChange();
        }


        void RebuildDisplayedElements() {    
            mainElements = pagination.Paginate(GetElementsList (curPath, 0, true)).ToArray();

            List<Element> prevElement = new List<Element>();
            if (!curPath.IsEmpty()) {
                string parentPath = CalcLastFolder(curPath);
                prevElement = GetElementsList (parentPath, -1, false);    
                Element prevDirEl = new Element(-2, parentPath, -1, false, false);
                prevDirEl.SetGUI(new GUIContent(" << " + (parentPath.IsEmpty() ? "Base" : parentPath)));
                prevElement.Insert(0, prevDirEl);
            }
            prevWindowElements = prevElement.ToArray();
        }

        List<Element> GetElementsList (string dirPath, int offsetFromMain, bool showFiles) {
            int lastDir = 0;
            HashSet<string> used = new HashSet<string>();    
            List<Element> newElements = new List<Element>();
            foreach (var e in getPoolElements( dirPath ).Where(e => searchFilter.PassesSearchFilter(e.path))) {
                string name = e.path;
                bool isFile = true;
                if (name.Contains("/")) name = DirDisplayName ( name, offsetFromMain, out isFile );

                if (isFile) {
                    if (showFiles) {
                        e.SetGUI(new GUIContent( (e.path.Contains("/") ? e.path.Split('/').Last() : e.path) ) );
                        newElements.Add(e);
                    }
                }   
                //foldered directory
                else {
                    if (used.Contains(name)) continue;
                    used.Add(name);

                    Element newElement = new Element(-1, dirPath + (dirPath.IsEmpty() ? "" : "/") + name, e.poolIndex, e.isNewDir, false);            
                    newElement.SetGUI(new GUIContent(name));

                    newElements.Insert(lastDir, newElement);
                    lastDir++;       
                }
            }
            return newElements;
        }
    }
}