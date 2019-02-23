using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace AssetObjectsPacks {

    public class ElementSelectionSystem 
    {
        public static bool SelectionSystemElementGUI (GUIContent gui, bool selected, bool hidden, bool directory) {
            GUIStyle s = GUIStyles.toolbarButton;
            TextAnchor a = s.alignment;
            Texture2D t = s.normal.background;
            s.fontStyle = hidden ? FontStyle.Italic : FontStyle.Normal;
            s.normal.background = (!selected && !directory) ? null : t;
            s.alignment = TextAnchor.MiddleLeft;
            Color32 txtColor = hidden ? Colors.yellow : (selected || directory ? Colors.black : Colors.liteGray);
            bool pressed = GUIUtils.Button(gui, false, s, Colors.Toggle(selected), txtColor);
            s.normal.background = t;
            s.alignment = a;
            s.fontStyle = FontStyle.Normal;
            return pressed;
        }
        public struct Element {
            public int id, poolIndex;
            public string path;
            public Element (int id, string path, int poolIndex) 
            => (this.id, this.path, this.poolIndex) = (id, path, poolIndex);
        }
        public class Inputs {
            public string folderFwdDir = null;
            public PaginationAttempt paginationAttempt;
            public bool paginationSuccess, hiddenToggleSuccess, selectionChanged;
            public bool folderBack, searchChanged, folderedToggled, changedTabView;

        }
        public bool justFilesSelected { get { return selectionSystem.hasSelection && !selectionHasDirs; } }
        public int viewTab;

        public bool hasSelection { get { return selectionSystem.hasSelection; } }
        public bool singleSelection { get { return selectionSystem.singleSelection; } }
        public Element selectedElement {
            get {
                int index;
                bool singleSelection = selectionSystem.SingleSelection(out index);
                if (!hasSelection || !singleSelection) {
                    return new Element(-1,"",-1);
                }
                return elements[index];
            }
        }

        SelectionSystem selectionSystem = new SelectionSystem();
        StateToggler hiddenIDsToggler = new StateToggler();
        SearchFilter searchFilter = new SearchFilter();
        Pagination pagination= new Pagination();
        
        string curFolderPath = "";

        void ResetCurrentPath () {
            curFolderPath = "";
            curFolderOffset = 0;
        }
        int curFolderOffset;
        
        Element[] elements;
        bool folderedView;
        
        public delegate Element GetElementAtPoolIndex (int index, int variation);
        public delegate int GetPoolCount (int variation);
        //public delegate bool DrawElement(Element e, GUIContent gui, bool selected, bool hidden, int variation, out bool drawNormal);
        public delegate HashSet<int> GetIgnorePoolIDs(int variation);
        //DrawElement drawElement;
        GetElementAtPoolIndex getElementAtPoolIndex;
        GetPoolCount getPoolCount;
        GetIgnorePoolIDs getIgnorePoolIDs;
        System.Action onSelectionChange;
        
        bool SingleDirectorySelected(out int index) {
            return selectionSystem.SingleSelection(out index) && elements[index].id == -1;
        }
        public void DrawPages (Inputs inputs, KeyboardListener kbListener){
            inputs.paginationAttempt = pagination.DrawPages(kbListener, out inputs.paginationSuccess);
        }
        public void DrawElements (int viewVariation, Inputs inputs, KeyboardListener kbListener){
            int clickedElementIndex = -1;
            inputs.selectionChanged = false;

            if (elements.Length == 0) {
                EditorGUILayout.HelpBox("No Elements!", MessageType.Info);
                return;
            }
            GUIUtils.StartBox(0);
            int l = elements.Length;
            for (int i = 0; i < l; i++) {
                bool s = selectionSystem.IsSelected(i);
                bool clicked = false;
                string path = elements[i].path;
                int id = elements[i].id;
                if (id == -1) clicked = SelectionSystemElementGUI (new GUIContent(  path  ), s, false, true);
                else {
                    GUIContent gui = new GUIContent(folderedView && path.Contains("/") ? path.Split('/').Last() : path);
                    bool hidden = hiddenIDsToggler.IsState(id);
                    //bool drawNormal;
                    //clicked = drawElement(elements[i], gui, s, hidden, viewVariation, out drawNormal);
                    //if (drawNormal) 
                    clicked = SelectionSystemElementGUI(gui, s, hidden, false);
                   
                }    
                if (clicked) clickedElementIndex = i;
            }

            inputs.selectionChanged = selectionSystem.HandlDirectionalSelection(kbListener[KeyCode.UpArrow], kbListener[KeyCode.DownArrow], kbListener.shift, elements.Length - 1);

            int lo = -1;
            if (clickedElementIndex == -1 && kbListener[KeyCode.Return] && SingleDirectorySelected(out lo)) clickedElementIndex = lo;
            if (clickedElementIndex != -1) {
                if (elements[clickedElementIndex].id != -1){
                    selectionSystem.OnObjectSelection(clickedElementIndex, kbListener.shift);
                    inputs.selectionChanged = true;
                }
                else {
                    inputs.folderFwdDir = elements[clickedElementIndex].path;
                }
            }

            GUIUtils.EndBox(1);
        }

        public void HandleInputs (Inputs inputs, bool forceRebuild) {


            bool clearAndRebuild = forceRebuild || inputs.changedTabView || inputs.folderedToggled || inputs.searchChanged|| inputs.paginationSuccess;
            bool resetPage = inputs.folderedToggled || inputs.changedTabView;
                
            clearAndRebuild = clearAndRebuild || ((inputs.hiddenToggleSuccess && selectionSystem.hasSelection));// || inputs.resetHidden);
                        
            if (folderedView) {

                if (inputs.paginationAttempt == PaginationAttempt.Back && !inputs.paginationSuccess) inputs.folderBack = true;
                
                bool movedFolder = (inputs.folderBack && MoveFolder()) || (inputs.folderFwdDir != null && MoveFolder(inputs.folderFwdDir));
                
                clearAndRebuild = clearAndRebuild || movedFolder;
                resetPage = resetPage || movedFolder;
            }

            if (inputs.changedTabView || inputs.folderedToggled) {
                ResetCurrentPath();
            }
            if (clearAndRebuild) {                
                ClearSelectionsAndRebuild(resetPage);
                inputs.selectionChanged = true;
            }
            if (inputs.selectionChanged) onSelectionChange();
        }


        bool MoveFolder(string addPath = null) {
            bool back = addPath == null;
            if (back) {
                if (curFolderOffset <= 0) return false;
                curFolderPath = curFolderPath.Substring(0, curFolderPath.Substring(0, curFolderPath.Length-1).LastIndexOf("/") + 1);
            }
            else curFolderPath += addPath + "/";
            curFolderOffset+= back ? -1 : 1;
            return true;
        }
        
        bool selectionHasDirs {
            get {
                if (!selectionSystem.hasSelection) return false;
                foreach (var i in selectionSystem.GetSelectionEnumerator()) {
                    if (elements[i].id == -1) return true;
                }
                return false;
            }
        }

        
        
        HashSet<int> IDsInDirectory(string dir, int poolCount, HashSet<int> ignorePoolIDs) {
            /*
            HashSet<int> ids = new HashSet<int>();
            for (int i = 0; i < poolCount; i++) {    
                Element e = getElementAtPoolIndex(i, viewTab);
                if (!e.path.StartsWith(dir)) continue;
                if (ignorePoolIDs.Contains(e.id)) continue;


            






                ids.Add(e.id);
            }
            return ids;

            */
            
            
            
            
            return new HashSet<int>().Generate(
                new List<Element>().Generate(poolCount, i => getElementAtPoolIndex(i, viewTab)).Where(e => !ignorePoolIDs.Contains(e.id) && showingHidden == hiddenIDsToggler.IsState(e.id) && e.path.StartsWith(dir)),
                e => e.id
            );
        }

        IEnumerable<int> GetPoolIndiciesInSelectionBase() {                
            return new HashSet<int>().Generate( selectionSystem.GetSelectionEnumerator(), i => (elements[i].id == -1 ? -1 : elements[i].poolIndex) );
        }
        
        IEnumerable<int> GetPoolIndiciesInElementsBase () {
            return new HashSet<int>().Generate( elements, e => e.id == -1 ? -1 : e.poolIndex );
        }

        HashSet<int> FilterOutDirectories (IEnumerable<int> e) {
            return e.Where( i => i != -1 ).ToHashSet();
        }
        public HashSet<int> GetPoolIndiciesInSelection() {
            return FilterOutDirectories(GetPoolIndiciesInSelectionBase());
        }
        public HashSet<int> GetPoolIndiciesInSelectionOrAllShown () {
            return FilterOutDirectories( selectionSystem.hasSelection ? GetPoolIndiciesInSelectionBase() : GetPoolIndiciesInElementsBase() );
        }

        public HashSet<int> GetIDsInSelection(bool includeDirs, out bool hasDirs) {
            hasDirs = false;
            if (!selectionSystem.hasSelection) return null;
            int poolCount = getPoolCount(viewTab);
            HashSet<int> ignorePoolIDs = getIgnorePoolIDs(viewTab);
            HashSet<int> ids = new HashSet<int>();

            foreach (var i in selectionSystem.GetSelectionEnumerator()) {
                int id = elements[i].id;
                string path = elements[i].path;
                if (id == -1) {
                    if (includeDirs) {
                        string dir = curFolderPath + elements[i].path;
                        
                        ids.AddRange( IDsInDirectory(dir, poolCount, ignorePoolIDs) );

                        hasDirs = true;
                    }
                }
                else {
                    ids.Add(id);
                }
            }
            return ids;
        }


        GUIContent[] viewVariationGUIs;
        
        public void Initialize (GUIContent[] viewVariationGUIs, EditorProp hiddenIDsProp, GetElementAtPoolIndex getElementAtPoolIndex, GetPoolCount getPoolCount, GetIgnorePoolIDs getIgnorePoolIDs, 
        //DrawElement drawElement, 
        System.Action onSelectionChange) {
            List<GUIContent> viewVars = new List<GUIContent>();
            if (viewVariationGUIs.Length != 0) {
                for (int i = 0; i < viewVariationGUIs.Length; i++) {
                    viewVars.Add(viewVariationGUIs[i]);
                }
            }
            else {
                viewVars.Add(new GUIContent("Select"));
            }
            viewVars.Add(new GUIContent("Hidden"));

            this.viewVariationGUIs = viewVars.ToArray();

            hiddenIDsToggler.Initialize(hiddenIDsProp);
            
            this.getElementAtPoolIndex = getElementAtPoolIndex;
            this.getPoolCount = getPoolCount;
            this.getIgnorePoolIDs = getIgnorePoolIDs;
            //this.drawElement = drawElement;
            this.onSelectionChange = onSelectionChange;

            ResetCurrentPath();
            ClearSelectionsAndRebuild(true);
            onSelectionChange();
        }

        public bool showingHidden { get { return viewTab == viewVariationGUIs.Length - 1; } }

        void RebuildDisplayedElements() {
            int poolCount = getPoolCount(viewTab);
            HashSet<int> ignorePoolIDs = getIgnorePoolIDs(viewTab);

            HashSet<string> used = new HashSet<string>();            
            int lastDir = 0;
            
            IEnumerable<Element> filtered = new List<Element>()
                .Generate(poolCount, i => getElementAtPoolIndex(i, viewTab))
                .Where(e => !ignorePoolIDs.Contains(e.id) && showingHidden == hiddenIDsToggler.IsState(e.id) && searchFilter.PassesSearchFilter(e.path))
                //.ToList()
            ;

            List<Element> unpaginated;
            if (!folderedView) {
                unpaginated = filtered.ToList();
            }
            else {
                unpaginated = new List<Element>();
                foreach (var e in filtered) {
                    if (!curFolderPath.IsEmpty() && !e.path.StartsWith(curFolderPath)) continue;

                    string name = e.path;
                    bool isFile = true;

                    if (name.Contains("/")) {
                        string[] sp = name.Split('/');
                        name = sp[curFolderOffset];
                        isFile = curFolderOffset == sp.Length - 1;
                    }
                    
                    //foldered directory
                    if (!isFile) {
                        if (used.Contains(name)) continue;
                        used.Add(name);
                        unpaginated.Insert(lastDir, new Element(-1, name, -1));
                        lastDir++;       
                        continue;
                    }   
                    unpaginated.Add(e);
                }
            }

            elements = pagination.Paginate(unpaginated).ToArray();
        }

        void ClearSelectionsAndRebuild(bool resetPage) {
            if (resetPage) pagination.ResetPage();
            selectionSystem.ClearSelection();
            RebuildDisplayedElements();
        }

        public void DrawToolbar (KeyboardListener kbListener, Inputs inputs) {
            
            GUIUtils.StartBox(0);
            inputs.changedTabView = GUIUtils.Tabs(viewVariationGUIs, ref viewTab);
            GUIUtils.Space();
            
            EditorGUILayout.BeginHorizontal();

            ToggleHiddenButtonGUI(kbListener, inputs, GUIStyles.miniButtonLeft);
            FolderedViewButtonGUI(inputs, GUIStyles.miniButtonRight);
            
            inputs.searchChanged = searchFilter.SearchBarGUI();
            
            EditorGUILayout.EndHorizontal();
            
            GUIUtils.Space();
            
            DirectoryBackButtonGUI(inputs, GUIStyles.miniButtonLeft);
            
            GUIUtils.EndBox(1);
        }

        void DirectoryBackButtonGUI (Inputs input, GUIStyle s) {   
            EditorGUILayout.BeginHorizontal();
            GUIContent c = new GUIContent("   <<   ", "Back");
            GUI.enabled = folderedView && curFolderOffset > 0;
            input.folderBack = GUIUtils.Button(c, GUIStyles.miniButtonLeft, Colors.liteGray, Colors.black, new GUILayoutOption[] { GUILayout.Height(16), c.CalcWidth(GUIStyles.miniButtonRight)  });
            GUI.enabled = false;
            EditorGUILayout.TextField(curFolderPath);            
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }
        void FolderedViewButtonGUI (Inputs input, GUIStyle s) {
            folderedView = GUIUtils.ToggleButton(new GUIContent("Folders", "Enable/Disable Foldered View"), true, folderedView, s, out input.folderedToggled);
        }   
        void ToggleHiddenButtonGUI (KeyboardListener kbListener, Inputs input, GUIStyle s) {
            GUI.enabled = selectionSystem.hasSelection;
            hiddenIDsToggler.ToggleStateButton(
                kbListener[KeyCode.H], 
                new GUIContent("Hide/Unhide", "Toggle the hidden status of the selection (if any)"), true, s, 
                out input.hiddenToggleSuccess,
                GetHiddenToggleSelection
            );
            GUI.enabled = true;
        }
        HashSet<int> GetHiddenToggleSelection () {
            HashSet<int> selection = new HashSet<int>();
            if (!selectionSystem.hasSelection) return selection;
            bool containsDirs;
            selection = GetIDsInSelection(true, out containsDirs);
            if (containsDirs && !EditorUtility.DisplayDialog("Hide/Unhide Directory", "Selection contains directories, hidden status of all sub elements will be toggled", "Ok", "Cancel"))  
                selection.Clear();
            return selection;
        }
    }
}