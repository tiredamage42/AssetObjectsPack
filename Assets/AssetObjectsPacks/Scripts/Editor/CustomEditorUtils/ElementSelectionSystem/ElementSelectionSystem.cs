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
        //public bool singleSelection { get { return selectionSystem.singleElement; } }
        public bool hasSelection { get { return selectionSystem.hasElements; } }
        /*

        public bool selectionIsOnlyFiles { 
            get { 
                if (!hasSelection) return false;
                foreach (var i in GetSelectionEnumerable()) {
                    SelectionElement e = elements[1][i];
                    if (elements[1][i].id == -1) {
                        return false;
                    }
                }
                return true;
            } 
        }
        public bool selectionIsSingleFile { get { 
            return singleSelection && selectionIsOnlyFiles;
        } }
            
        public bool selectionIsSingleCollection { get { 
            if (!hasSelection) return false;

            int collection = elements[1][GetSelectionEnumerable().First()].collectionID;
            foreach (var i in GetSelectionEnumerable()) {
                SelectionElement e = elements[1][i];
                if (elements[1][i].collectionID != collection) {
                    return false;
                }
            }
            return true;
        
        } }
        public int selectionCollection { get { return elements[1][GetSelectionEnumerable().First()].collectionID; } }
        
        public SelectionElement selectedElement {
            get {
                int index;
                bool singleSelection = selectionSystem.HasSingleElement(out index);
                if (!hasSelection || !singleSelection) return null;
                return elements[1][index];
            }
        }
         */
        SelectionSystem selectionSystem = new SelectionSystem();
        Pagination pagination= new Pagination();
        DragAndDrop dragDrop = new DragAndDrop();
        
        string searchFilter;
        public string curPath = "";
        Action onSelectionChange, onChangeDisplayPath;
        Action<IEnumerable<int>, string, string> onDirDragDrop;
        Action toolbarButtons;
        bool shouldRebuild, shouldResetPage;
        Func<string, IEnumerable<SelectionElement>> getPoolElements;
        //Action<string, bool, HashSet<int>> getIDsInDirectory;
        public SelectionElement[][] elements = new SelectionElement[][] {
            null, null
        };
        public string parentPath { get { return CalcLastFolder(curPath); } }
        Action<string, HashSet<Vector2Int>> getElementsInDirectory;
        
        public void OnEnable (
            Func<string, IEnumerable<SelectionElement>> getPoolElements, 
            Action<string, HashSet<Vector2Int>> getElementsInDirectory, 
            Action onSelectionChange, Action<IEnumerable<int>, string, string> onDirDragDrop, 
            Action toolbarButtons, Action onChangeDisplayPath) {
        
            this.onChangeDisplayPath = onChangeDisplayPath;
            this.getElementsInDirectory = getElementsInDirectory;
            this.toolbarButtons = toolbarButtons;
            this.onDirDragDrop = onDirDragDrop;
            this.onSelectionChange = onSelectionChange;
            this.getPoolElements = getPoolElements;

            DoReset(true, true);
        }

        public void DrawElements (Action<KeyboardListener> checkHotkeys){
            KeyboardListener keyboardListener = new KeyboardListener();
            dragDrop.InputListen ();

            bool searchChanged;
            int pageOffset, clickedElementIndex, clickedElementCollection;
            ESS_GUI.DrawElementsView (GUIStyles.toolbarButton, UnityEngine.Event.current.mousePosition, elements, curPath, toolbarButtons, pagination.pagesGUI, ref searchFilter, out searchChanged, out pageOffset, out clickedElementIndex, out clickedElementCollection);
            
            dragDrop.DrawDraggedGUIs();
            
            bool paginationSuccess = pageOffset != 0 && pagination.SwitchPage(pageOffset);
            
            //check drag drop
            int receiverIndex, receiverCollection, droppedCollection;
            IEnumerable<int> droppedIndicies;
            bool droppedOnReceiver = dragDrop.DrawAndUpdate(selectionSystem, out receiverIndex, out receiverCollection, out droppedCollection, out droppedIndicies);
            if (droppedOnReceiver) {
                onDirDragDrop(droppedIndicies, curPath, elements[receiverCollection][receiverIndex].displayPath);
            }

            //check clicked element
            string forwardDir;
            bool folderBack, selectionChanged;
            CheckClickedElement(out selectionChanged, clickedElementIndex, clickedElementCollection, keyboardListener, out forwardDir, out folderBack);
            
            bool movedFolder = (folderBack && MoveFolder()) || (forwardDir != null && MoveFolder(forwardDir));
            
            selectionChanged = selectionSystem.HandleDirectionalSelection(keyboardListener[KeyCode.UpArrow], keyboardListener[KeyCode.DownArrow], keyboardListener.shift, elements[1].Length - 1) || selectionChanged;

            shouldRebuild = droppedOnReceiver || movedFolder || searchChanged|| paginationSuccess;            
            shouldResetPage = movedFolder;

            if (!shouldRebuild && selectionChanged) onSelectionChange();

            checkHotkeys(keyboardListener);
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
                //if not already there
                if (element.displayPath != curPath) forwardDir = element.displayPath;
            }
            else {//element select
                selectionSystem.OnObjectSelection(clickedIndex, k.shift);
                selectionChanged = true;
            }
        }

        public void ForceBackFolder () {
            MoveFolder();
            DoReset(false, true);
        }
        string CalcLastFolder(string dir) {
            if (dir.IsEmpty() || !dir.Contains("/")) return string.Empty;
            return dir.Substring(0, dir.LastIndexOf("/"));
        }
        
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

            onChangeDisplayPath ();
            return true;
        }
        
        public IEnumerable<int> GetPoolIndiciesInSelection (bool deep, int collectionFilter) {
            return selectionSystem.GetTrackedEnumerable().Generate( i => (elements[1][i].id == -1 && !deep) || (elements[1][i].collectionID != collectionFilter && collectionFilter != -1) ? -1 : elements[1][i].poolIndex );
        }
        public IEnumerable<int> GetPoolIndiciesInShown (bool deep, int collectionFilter) {
            return elements[1].Generate( e => (e.id == -1 && !deep) || (e.collectionID != collectionFilter && collectionFilter != -1) ? -1 : e.poolIndex );
        }
        public IEnumerable<int> GetPoolIndiciesInSelectionOrAllShown (int collectionFilter) {
            return (hasSelection ? GetPoolIndiciesInSelection(false, collectionFilter) : GetPoolIndiciesInShown(false, collectionFilter)).Where( i => i != -1 );
        }

        IEnumerable<Vector2Int> _GetElementsInIndicies(IEnumerable<int> indicies, bool includeDirs, string dirCheckTitle, string dirCheckMsg) {
            if (indicies == null) return null;
            HashSet<Vector2Int> r = new HashSet<Vector2Int>();
            bool checkedDirs = false;
            foreach (var i in indicies) {
                SelectionElement e = elements[1][i];
                int id = e.id;
                if (id == -1) {
                    if (!includeDirs) continue;
                    if (!checkedDirs) {
                        if (!EditorUtility.DisplayDialog(dirCheckTitle, dirCheckMsg, "Ok", "Cancel")) return null;
                        checkedDirs = true;
                    }
                    getElementsInDirectory(elements[1][i].displayPath, r);
                }
                else r.Add(new Vector2Int(e.id, e.collectionID));
            }
            return r;
        }
        
        public IEnumerable<Vector2Int> GetElementsInSelection () {
            return _GetElementsInIndicies(GetSelectionEnumerable(), false, null, null);
        }
        public IEnumerable<Vector2Int> GetElementsInSelectionDeep (string dirCheckTitle, string dirCheckMessage) {
            return _GetElementsInIndicies(GetSelectionEnumerable(), true, dirCheckTitle, dirCheckMessage);
        }


        public IEnumerable<int> GetSelectionEnumerable () {
            return selectionSystem.GetTrackedEnumerable();
        }
        
        

        void DoReset (bool resetPath, bool resetPage) {
            if (resetPath) {
                curPath = "";
                onChangeDisplayPath();
            }

            if (resetPage) {
                pagination.ResetPage();
            }

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

        List<SelectionElement> BuildElementsList (string dir, bool showFiles) {
            int dirIndex = 0;
            HashSet<string> used = new HashSet<string>();    
            List<SelectionElement> newElements = new List<SelectionElement>();

            IEnumerable<SelectionElement> poolElements = getPoolElements(dir);

            if (showFiles && !searchFilter.IsEmpty()) {
                string lowerSearch = searchFilter.ToLower();
                poolElements = poolElements.Where(e => e.displayPath.ToLower().Contains(lowerSearch));
            }
            
            int folderOffset = dir.IsEmpty() ? 0 : (!dir.IsEmpty() && !dir.Contains("/") ? 1 : dir.Split('/').Length);
            string dirPrefix = dir + (dir.IsEmpty() ? "" : "/");
                    
            foreach (var e in poolElements) {
                
                bool isFile = true;
                string displayPath = e.displayPath;
                if (displayPath.Contains("/")) {
                    string[] sp = displayPath.Split('/');
                    isFile = folderOffset == sp.Length - 1;
                    displayPath = sp[folderOffset];
                }
                if (isFile) {
                    if (showFiles) {
                        e.SetGUI(new GUIContent( displayPath ));
                        newElements.Add(e);
                    }
                }   
                //foldered directory
                else {
                    if (used.Contains(displayPath)) continue;
                    used.Add(displayPath);                
                    newElements.Insert(dirIndex++, new SelectionElement(e, dirPrefix + displayPath, new GUIContent(displayPath)));
                }
            }
            return newElements;
        }
    }
}