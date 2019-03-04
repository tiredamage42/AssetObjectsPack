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
    public class ElementSelectionSystem  {
        public bool singleSelection { get { return selectionSystem.singleElement; } }
        public bool hasSelection { get { return selectionSystem.hasElements; } }
        public SelectionElement selectedElement {
            get {
                int index;
                bool singleSelection = selectionSystem.HasSingleElement(out index);
                if (!hasSelection || !singleSelection) return SelectionElement.empty;
                return elements[1][index];
            }
        }
        SelectionSystem selectionSystem = new SelectionSystem();
        
        string searchFilter;

        Pagination pagination= new Pagination();
        DragAndDrop dragDrop = new DragAndDrop();
    
        public string curPath = "";
        Action onSelectionChange;
        Action<IEnumerable<int>, string, string> onDirDragDrop;
        Action<KeyboardListener> toolbarButtons;
        bool shouldRebuild, shouldResetPage;
        Func<string, IEnumerable<SelectionElement>> getPoolElements;
        Action<string, bool, HashSet<int>> getIDsInDirectory;
        public void OnEnable (Func<string, IEnumerable<SelectionElement>> getPoolElements, Action<string, bool, HashSet<int>> getIDsInDirectory, Action onSelectionChange, Action<IEnumerable<int>, string, string> onDirDragDrop, Action<KeyboardListener> toolbarButtons) {
            this.getIDsInDirectory = getIDsInDirectory;
            this.toolbarButtons = toolbarButtons;
            this.onDirDragDrop = onDirDragDrop;
            this.onSelectionChange = onSelectionChange;
            this.getPoolElements = getPoolElements;
        }

        SelectionElement[][] elements = new SelectionElement[][] {
            null, null
        };

        public void DrawElements (){
            KeyboardListener keyboardListener = new KeyboardListener();
            dragDrop.InputListen ();

            bool searchChanged;
            int pageOffset, clickedElementIndex, clickedElementCollection;
            ESS_GUI.DrawElementsView (elements, curPath, keyboardListener, toolbarButtons, pagination.pagesGUI, ref searchFilter, out searchChanged, out pageOffset, out clickedElementIndex, out clickedElementCollection);
            
            dragDrop.DrawDraggedGUIs();
            
            bool paginationSuccess = pageOffset != 0 && pagination.SwitchPage(pageOffset);
            
            //check drag drop
            int receiverIndex, receiverCollection, droppedCollection;
            IEnumerable<int> droppedIndicies;
            bool droppedOnReceiver = dragDrop.DrawAndUpdate(selectionSystem, out receiverIndex, out receiverCollection, out droppedCollection, out droppedIndicies);
            if (droppedOnReceiver) {
                onDirDragDrop(droppedIndicies, curPath, elements[receiverCollection][receiverIndex].filePath);
            }

            //check clicked element
            string forwardDir;
            bool folderBack, selectionChanged;
            CheckClickedElement(out selectionChanged, clickedElementIndex, clickedElementCollection, keyboardListener, out forwardDir, out folderBack);
            
            bool movedFolder = (folderBack && MoveFolder()) || (forwardDir != null && MoveFolder(forwardDir));
            
            selectionChanged = selectionSystem.HandlDirectionalSelection(keyboardListener[KeyCode.UpArrow], keyboardListener[KeyCode.DownArrow], keyboardListener.shift, elements[1].Length - 1) || selectionChanged;


            shouldRebuild = droppedOnReceiver || movedFolder || searchChanged|| paginationSuccess;            
            shouldResetPage = movedFolder;

            if (!shouldRebuild && selectionChanged) onSelectionChange();
        }


            
        public void CheckRebuild (bool forceRebuild, bool forceReset) {
            if (forceRebuild || shouldRebuild) DoReset(forceReset, shouldResetPage || forceReset);
            shouldResetPage = shouldRebuild = false;
        }

        void CheckClickedElement(out bool selectionChanged, int clickedIndex, int collection, KeyboardListener k, out string forwardDir, out bool folderBack) {
            forwardDir = null;
            folderBack = k[KeyCode.LeftArrow];
            selectionChanged = false;

            //right arrow movies fwd directory when one selected
            int lo = -1;
            if (clickedIndex == -1 && k[KeyCode.RightArrow] && (selectionSystem.HasSingleElement(out lo) && elements[1][lo].id == -1)) {
                clickedIndex = lo;
                collection = 1;
            }

            if (clickedIndex == -1) return;

            SelectionElement element = elements[collection][clickedIndex];
            if (element.isPrevDirectory) {
                folderBack = true;
            }
            else if (element.isDirectory) {
                string path = element.filePath;
                //if not already there
                if (path != curPath) forwardDir = path;
            }
            else {//element select
                selectionSystem.OnObjectSelection(clickedIndex, k.shift);
                selectionChanged = true;
            }
        }
        string CalcLastFolder(string dir) {
            if (dir.IsEmpty() || !dir.Contains("/")) return string.Empty;
            return dir.Substring(0, dir.LastIndexOf("/"));
        }
        public void ForceBackFolder () {
            MoveFolder();
            DoReset(false, true);
        }
        public string parentPath { get { return CalcLastFolder(curPath); } }
        
        public void ChangeCurrentDirectoryName (string newName) {
            if (curPath.Contains("/")) {
                curPath = parentPath + "/" + newName;
            }
            else {
                if (!curPath.IsEmpty()) curPath = newName;
            }
        }

        bool MoveFolder(string toPath = null){
            if (toPath == null) {
                if (curPath.IsEmpty()) return false;
                curPath = parentPath;
            }
            else curPath = toPath;

            return true;
        }
        
        public IEnumerable<int> GetPoolIndiciesInSelection (bool deep) {
            return selectionSystem.GetTrackedEnumerable().Generate( i => elements[1][i].id == -1 && !deep ? -1 : elements[1][i].poolIndex );
        }
        public IEnumerable<int> GetPoolIndiciesInShown (bool deep) {
            return elements[1].Generate( e => e.id == -1 && !deep ? -1 : e.poolIndex );
        }
        public IEnumerable<int> GetPoolIndiciesInSelectionOrAllShown () {
            return (hasSelection ? GetPoolIndiciesInSelection(false) : GetPoolIndiciesInShown(false)).Where( i => i != -1 );
        }

        IEnumerable<int> _GetIDsInIndicies(bool useRepeats, IEnumerable<int> indicies, bool includeDirs, string dirCheckTitle, string dirCheckMsg) {
            if (indicies == null) return null;
            HashSet<int> ids = new HashSet<int>();
            bool checkedDirs = false;
            foreach (var i in indicies) {
                int id = elements[1][i].id;
                if (id == -1) {
                    if (!includeDirs) continue;
                    if (!checkedDirs) {
                        if (!EditorUtility.DisplayDialog(dirCheckTitle, dirCheckMsg, "Ok", "Cancel")) return null;
                        checkedDirs = true;
                    }
                    getIDsInDirectory(elements[1][i].filePath, useRepeats, ids);
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
        
        public void Initialize () {
            DoReset(true, true);
        }


        void DoReset (bool resetPath, bool resetPage) {
            if (resetPath) curPath = "";
            if (resetPage) pagination.ResetPage();
            selectionSystem.ClearTracker();   
            dragDrop.ClearTracker();
            RebuildDisplayedElements();
            onSelectionChange();
        }

        void RebuildDisplayedElements() {    
            elements[1] = pagination.Paginate(BuildElementsList (curPath, true)).ToArray();
            List<SelectionElement> prevElement = new List<SelectionElement>();
            if (!curPath.IsEmpty()) {
                string prevPath = parentPath;
                prevElement = BuildElementsList (prevPath, false);    
                prevElement.Insert(0, new SelectionElement(prevPath, new GUIContent(" << " + (prevPath.IsEmpty() ? "Base" : prevPath))));
            }
            elements[0] = prevElement.ToArray();

            for (int e = 0; e < elements.Length; e++) {
                for (int i = 0; i < elements[e].Length; i++) {
                    elements[e][i].Initialize(i, selectionSystem, dragDrop, e == 0, curPath);
                }
            }
        }

        static string DirDisplayName (int folderOffset, string path, out bool isFile) {
            isFile = true;
            if (!path.Contains("/")) return path;
            string[] sp = path.Split('/');
            isFile = folderOffset == sp.Length - 1;
            return sp[folderOffset];
        }

        List<SelectionElement> BuildElementsList (string dir, bool showFiles) {
            int dirIndex = 0;
            HashSet<string> used = new HashSet<string>();    
            List<SelectionElement> newElements = new List<SelectionElement>();

            IEnumerable<SelectionElement> poolElements = getPoolElements(dir);
            if (showFiles && !searchFilter.IsEmpty()) {
                string lowerSearch = searchFilter.ToLower();
                poolElements = poolElements.Where(e => e.filePath.ToLower().Contains(lowerSearch));
            }

            int folderOffset = dir.IsEmpty() ? 0 : (!dir.IsEmpty() && !dir.Contains("/") ? 1 : dir.Split('/').Length);
            string dirPrefix = dir + (dir.IsEmpty() ? "" : "/");
                    
            foreach (var e in poolElements) {
                bool isFile;
                string name = DirDisplayName (folderOffset, e.filePath, out isFile);
                
                if (isFile) {
                    if (showFiles) {
                        e.SetGUI(new GUIContent( name ));
                        newElements.Add(e);
                    }
                }   
                //foldered directory
                else {
                    if (used.Contains(name)) continue;
                    used.Add(name);                
                    newElements.Insert(dirIndex++, new SelectionElement(e, dirPrefix + name, new GUIContent(name)));
                }
            }
            return newElements;
        }
    }
}