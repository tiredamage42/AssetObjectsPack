using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace AssetObjectsPacks {

    public class ElementSelectionSystem 
    {
        static void HoverSelectGUI (GUIContent gui, bool selected, bool hidden, bool directory, Rect rt) {
            //selected = false;

            GUI.enabled = false;
            GUIStyle s = GUIStyles.toolbarButton;
            TextAnchor a = s.alignment;
            Texture2D t = s.normal.background;
            s.fontStyle = hidden ? FontStyle.Italic : FontStyle.Normal;
            s.normal.background = (!selected && !directory) ? null : t;
            s.alignment = TextAnchor.MiddleLeft;
            Color32 txtColor = hidden ? Colors.yellow : (selected || directory ? Colors.black : Colors.white);
            //rt = GUILayoutUtility.GetRect(gui, s, new GUILayoutOption[] {GUILayout.ExpandWidth(true)});
            GUIUtils.Button(gui, s, Colors.Toggle(selected), txtColor, rt);
            s.normal.background = t;
            s.alignment = a;
            s.fontStyle = FontStyle.Normal;
            GUI.enabled = true;
        }
        /*
        public static bool SelectionSystemElementGUI (bool beingDragged, GUIContent gui, bool selected, bool hidden, bool directory, out Rect rt) {
            GUIStyle s = GUIStyles.toolbarButton;
            TextAnchor a = s.alignment;
            Texture2D t = s.normal.background;
            s.fontStyle = hidden ? FontStyle.Italic : FontStyle.Normal;
            s.normal.background = (!selected && !directory) ? null : t;
            s.alignment = TextAnchor.MiddleLeft;
            Color32 txtColor = hidden ? Colors.yellow : (selected || directory ? Colors.black : Colors.liteGray);


            rt = GUILayoutUtility.GetRect(gui, s, new GUILayoutOption[] {GUILayout.ExpandWidth(true)});
            bool pressed = false;
            if (!beingDragged) {

                pressed = GUIUtils.Button(gui, s, Colors.Toggle(selected), txtColor, rt);
            }
            s.normal.background = t;
            s.alignment = a;
            s.fontStyle = FontStyle.Normal;
            return pressed;
        }
        */
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
            //public bool folderBack, searchChanged, folderedToggled, changedTabView;
            public bool folderBack, searchChanged, changedTabView, folderBackFirst;
        
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
                if (!hasSelection || !singleSelection) {
                    return new Element(-1,"",-1);
                }
                return mainElements[index];
            }
        }

        SelectionSystem selectionSystem = new SelectionSystem();
        StateToggler hiddenIDsToggler = new StateToggler();
        SearchFilter searchFilter = new SearchFilter();
        Pagination pagination= new Pagination();


        
        public string currentFolderPath = "", prevFolderPath;

        void ResetCurrentPath () {
            currentFolderPath = prevFolderPath = "";
            //currentFolderOffset = 0;
            onMoveFolder(viewTab, null, currentFolderPath, true);//, false);
        }
        //int currentFolderOffset;
        
        Element[] mainElements, prevWindowElements;
        //bool folderedView;
        
        public delegate Element GetElementAtPoolIndex (int index, int variation, string dirPath);
        public delegate int GetPoolCount (int variation, string dirPath);
        public delegate HashSet<int> GetIgnorePoolIDs(int variation);
        GetElementAtPoolIndex getElementAtPoolIndex;
        GetPoolCount getPoolCount;
        GetIgnorePoolIDs getIgnorePoolIDs;
        System.Action onSelectionChange;
        System.Action<HashSet<int>, string, Element, int> onDirDragDrop;
        
        bool SingleDirectorySelected(out int index) {
            return selectionSystem.SingleSelection(out index) && mainElements[index].id == -1;
        }
        public void DrawPages (Inputs inputs, KeyboardListener kbListener){
            inputs.paginationAttempt = pagination.DrawPages(kbListener, out inputs.paginationSuccess);
        }

        //bool isHoldingDownSelection;


        List<int> draggedIndicies = new List<int>();
        bool wasDragged;
        Vector2 mouseDragOffset;

        string DirDisplayName ( string path, int mainFocusOffset, out bool isFile, bool debug = false) {
            int currentFolderOffset = 0;
                if (debug) {

                    Debug.Log("getting folder display name for: " + path);
                    Debug.Log("current folder display: " + currentFolderPath);
                }
            if (currentFolderPath.Contains("/")) {
                currentFolderOffset = currentFolderPath.Split('/').Length-1;
            }

                if (debug) {

                Debug.Log("current offset: " + currentFolderOffset);
                }
            //else {


            //}
            string[] sp = path.Split('/');
            isFile = currentFolderOffset + mainFocusOffset == sp.Length - 1;
            return sp[currentFolderOffset + mainFocusOffset];
                    
        }


        public void DrawElements (int viewVariation, Inputs inputs, KeyboardListener kbListener){
            UnityEngine.Event e = UnityEngine.Event.current;
            Vector2 mousePos = e.mousePosition;

            bool mouseClicked = e.type == EventType.MouseDown && e.button == 0 ;

            bool mouseUp = e.type == EventType.MouseUp;

            if (!wasDragged) {
                wasDragged = e.type == EventType.MouseDrag && draggedIndicies.Count != 0;
            }
                

            int clickedElementWindowIndex = -1, clickedElementIndex = -1;
            int dropReceiverWindowIndex = -1, dropReceiverElementIndex = -1;

            inputs.selectionChanged = false;

            //if (elements.Length == 0) {
            //    EditorGUILayout.HelpBox("No Elements!", MessageType.Info);
            //    return;
            //}

            /*

            GUI.enabled = draggedIndicies.Count != 0 && wasDragged;
            //Rect rt = GUILayoutUtility.GetRect(new GUIContent ("Drag Drop Here"), GUIStyles.box, GUILayout.ExpandWidth(true)
                
            //);

            //rt

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            GUIUtils.Space(1);

            GUIUtils.Label(new GUIContent("Move To"), true);

            EditorGUILayout.EndVertical();


            GUIUtils.StartBox(0, Colors.green, new GUILayoutOption[] { GUILayout.Height(16) });//, GUILayout.Height(128) });
            
            TextAnchor a = GUIStyles.label.alignment;
            int fontSize = GUIStyles.label.fontSize;
            GUIStyles.label.alignment = TextAnchor.UpperCenter;
            GUIStyles.label.fontSize = 8;

            GUIUtils.Label(new GUIContent("<b>Last Directory</b>"), Colors.black, new GUILayoutOption[] { GUILayout.MinWidth(32), GUILayout.Height(12) });
            
            GUIUtils.EndBox(0);
            Rect upRect = GUILayoutUtility.GetLastRect();

            if (upRect.Contains(mousePos)) {
                Debug.LogWarning("In Rect");
            }
            */

/*
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginVertical();
            GUIUtils.Space(1);

            GUIUtils.Label(new GUIContent("Move To"), true);

            EditorGUILayout.EndVertical();


GUILayout.FlexibleSpace();
 */





            /*

            GUIUtils.StartBox(0, Colors.yellow, new GUILayoutOption[] { GUILayout.Height(8) });//, GUILayout.Height(128) });
            GUIUtils.Label(new GUIContent("<b>Base Directory</b>"), Colors.black, new GUILayoutOption[] { GUILayout.MinWidth(32), GUILayout.Height(12) });
            GUIUtils.EndBox(0);
            
            Rect baseRect = GUILayoutUtility.GetLastRect();

            if (baseRect.Contains(mousePos)) {
                Debug.LogWarning("In base Rect");
            }

            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;

            GUIStyles.label.alignment = a;
            GUIStyles.label.fontSize = fontSize;

            */
            


            
            

        



            //EditorGUI.DrawRect(GUILayoutUtility.GetRect(

            //), Colors.green);



            //prev directory elements




            EditorGUILayout.BeginHorizontal();
            
            GUIUtils.StartBox(0, GUILayout.MaxWidth(64));
            //List<int> selectedIndicies = new List<int>();
            //List<Rect> selectedRects = new List<Rect>();


            int l = prevWindowElements.Length;
            int clickedRectIndex = -1;
            List<Rect> allElementRects = new List<Rect>();
            for (int i = 0; i < l; i++) {
                int id = prevWindowElements[i].id;

                string path = prevWindowElements[i].path;
                bool isDir = id == -1;
                bool isBackDir = id == -2;




                //string displayName = isDir ? path : (folderedView && path.Contains("/") ? path.Split('/').Last() : path);
                
                
                string displayName = path;// isDir || isBackDir ? path : (path.Contains("/") ? path.Split('/').Last() : path);
                        
                if (isBackDir){ 
                    if(displayName.IsEmpty()) {
                    displayName = "Base";
                    }
                }
                else {
                    //if (currentFolderOffset > 1) {

                        //Debug.Log(displayName);
                        //Debug.Log(sp.Length);
                    //}
                        //string[] sp = displayName.Split('/');
                        displayName = DirDisplayName(displayName, -1, out _, false);// sp[currentFolderOffset - 1];

                }

                

                

                //Debug.Log(path + " || " + currentFolderPath);
                bool isSelected = !isBackDir && currentFolderPath.StartsWith(path);

                
                
                

                //bool isHidden = !isDir && hiddenIDsToggler.IsState(prevWindowElements[i].id);
                GUIContent gui = new GUIContent( (isBackDir ? " << " : "") + displayName  );
                
                //bool beingDragged = wasDragged && draggedIndicies.Contains(i);
                
                
                    //Debug.Log("DOWN");
                    //e.Use();
                
                Rect rt;
                //if (SelectionSystemElementGUI(beingDragged, gui, isSelected, isHidden, isDir, out rt)) clickedElementIndex = i;  
                
                GUIStyle s = GUIStyles.toolbarButton;
            TextAnchor a = s.alignment;
            Texture2D t = s.normal.background;
            Texture2D th = s.active.background;
            
            s.fontStyle = FontStyle.Normal;
            //s.normal.background = (!isSelected && !isDir && !isBackDir) ? null : t;
            //s.active.background = (!isSelected && !isDir && !isBackDir) ? null : th;
            s.normal.background = (!isSelected && !isBackDir) ? null : t;
            s.active.background = (!isSelected && !isBackDir) ? null : th;
            
            s.alignment = TextAnchor.MiddleLeft;
            //Color32 txtColor = (isSelected || isDir ? Colors.black : Colors.liteGray);
            Color32 txtColor = (isSelected || isBackDir ? Colors.black : Colors.liteGray);
            
            Color32 buttonColor = isBackDir ? Colors.liteGray : Colors.Toggle(isSelected);

            rt = GUILayoutUtility.GetRect(gui, s, new GUILayoutOption[] {GUILayout.ExpandWidth(true)});
                allElementRects.Add(rt);

//if (isDir) {

                if (wasDragged){//} && !beingDragged) {
                    if (rt.Contains(mousePos)) {

                        s.normal.background = t;
                        s.active.background = th;
                        txtColor = Colors.black;

                        buttonColor = Colors.selected;
                        

                    }
                }
//}




            
                

                bool skipButton = false;
                if (mouseUp && rt.Contains(mousePos) && wasDragged) {

                    //if (isDir || isBackDir){

                    Debug.Log("mouse up");
                    dropReceiverElementIndex = i;
                    dropReceiverWindowIndex = 0;

                    //}





                    //e.Use();
                    skipButton = true;
                }
                
                
            
            
            
            
            
            
            
            bool pressed = false;

                        GUI.enabled = !skipButton;

            //if (!beingDragged && !skipButton) {
                pressed = GUIUtils.Button(gui, s, buttonColor, txtColor, rt);
                if (pressed) {

                    Debug.Log("pressed");
                }
            //}
                        GUI.enabled = true;


            s.normal.background = t;
            s.active.background = th;
            s.alignment = a;
            s.fontStyle = FontStyle.Normal;

            if (pressed) {
                clickedElementIndex = i;
                clickedElementWindowIndex = 0;
            }




                
                
                
            }
            GUIUtils.EndBox(0);



            //main elements







            GUIUtils.StartBox(0);
            //List<int> selectedIndicies = new List<int>();
            //List<Rect> selectedRects = new List<Rect>();


            l = mainElements.Length;
            //int clickedRectIndex = -1;
            allElementRects.Clear();
            //List<Rect> allElementRects = new List<Rect>();
            for (int i = 0; i < l; i++) {

                string path = mainElements[i].path;
                bool isDir = mainElements[i].id == -1;
                //string displayName = isDir ? path : (folderedView && path.Contains("/") ? path.Split('/').Last() : path);
                string displayName = isDir ? DirDisplayName(path, 0, out _) : (path.Contains("/") ? path.Split('/').Last() : path);
                
                bool isSelected = selectionSystem.IsSelected(i);
                bool isHidden = !isDir && hiddenIDsToggler.IsState(mainElements[i].id);
                GUIContent gui = new GUIContent(  displayName  );
                
                bool beingDragged = wasDragged && draggedIndicies.Contains(i);
                
                
                    //Debug.Log("DOWN");
                    //e.Use();
                
                Rect rt;
                //if (SelectionSystemElementGUI(beingDragged, gui, isSelected, isHidden, isDir, out rt)) clickedElementIndex = i;  
                
                GUIStyle s = GUIStyles.toolbarButton;
            TextAnchor a = s.alignment;
            Texture2D t = s.normal.background;
            Texture2D th = s.active.background;
            
            s.fontStyle = isHidden ? FontStyle.Italic : FontStyle.Normal;
            s.normal.background = (!isSelected && !isDir) ? null : t;
            s.active.background = (!isSelected && !isDir) ? null : th;
            
            s.alignment = TextAnchor.MiddleLeft;
            Color32 txtColor = isHidden ? Colors.yellow : (isSelected || isDir ? Colors.black : Colors.liteGray);
            Color32 buttonColor = Colors.Toggle(isSelected);

            rt = GUILayoutUtility.GetRect(gui, s, new GUILayoutOption[] {GUILayout.ExpandWidth(true)});
                allElementRects.Add(rt);

if (isDir) {

                if (wasDragged && !beingDragged) {
                    if (rt.Contains(mousePos)) {

                        s.normal.background = t;
                        s.active.background = th;
                        txtColor = Colors.black;

                        buttonColor = Colors.selected;
                        

                    }
                }
}




            
                

                bool skipButton = false;
                if (mouseUp && rt.Contains(mousePos) && wasDragged && !beingDragged) {

                    if (isDir){

                    Debug.Log("mouse up");
                    dropReceiverElementIndex = i;
                    dropReceiverWindowIndex = 1;
                    
                    //mouseUpIndex = i;
                    }





                    //e.Use();
                    skipButton = true;
                }
                
                if (mouseClicked && rt.Contains(mousePos))
                {
                    //isHoldingDownSelection = true;
                    //Debug.Log("DOWN: " + displayName);

                    

                    clickedRectIndex = i;


                
                }
            
            
            
            
            
            
            
            
            
            
            bool pressed = false;

                        GUI.enabled = !beingDragged && !skipButton;

            //if (!beingDragged && !skipButton) {
                pressed = GUIUtils.Button(gui, s, buttonColor, txtColor, rt);
                //if (pressed) {

                    //Debug.Log("pressed");
                //}
            //}
                        GUI.enabled = true;


            s.normal.background = t;
            s.active.background = th;
            s.alignment = a;
            s.fontStyle = FontStyle.Normal;

            if (pressed) {
                clickedElementIndex = i;
                clickedElementWindowIndex = 1;
            }




                
                
                
            }

            if (mouseUp) {

                if (dropReceiverElementIndex != -1) {

                    Element el = dropReceiverWindowIndex == 0 ? prevWindowElements[dropReceiverElementIndex] : mainElements[dropReceiverElementIndex];
                    if (dropReceiverWindowIndex == 0) {

                        Debug.Log("On Drop Previous: "+ el.path);

                        OnDropDir(new HashSet<int>().Generate( draggedIndicies, di => di ), el);


                    }
                    else {

                        bool onDir = el.id == -1;
                        if (onDir) {



                            OnDropDir(new HashSet<int>().Generate( draggedIndicies, di => di ), el);


                        }
                    }


                }
                draggedIndicies.Clear();
                //isHoldingDownSelection = false;
                wasDragged = false;
            }
            

            if (clickedRectIndex != -1) {
                    if (draggedIndicies.Count == 0) {
                bool selected = selectionSystem.IsSelected(clickedRectIndex);

                if (selected) {
                    draggedIndicies = selectionSystem.GetSelectionEnumerator().ToList();
                }
                else {
                    draggedIndicies.Clear();
                    draggedIndicies.Add(clickedRectIndex);
                }
                wasDragged = false;

                
                mouseDragOffset = new Vector2(allElementRects[draggedIndicies[0]].x - mousePos.x,allElementRects[draggedIndicies[0]].y - mousePos.y);




                    }

            }
 




            if (draggedIndicies.Count != 0 && wasDragged) {
                EditorWindow.mouseOverWindow.Repaint();
                //Repaint();

                EditorGUIUtility.AddCursorRect(new Rect(mousePos.x-5, mousePos.y-5, 10, 10), MouseCursor.ArrowPlus);

                for (int i = 0; i < draggedIndicies.Count; i++) {
                    Element el = mainElements[draggedIndicies[i]];
                    Rect rt = allElementRects[draggedIndicies[i]];

                    float offset = rt.y - allElementRects[draggedIndicies[0]].y;


                    Rect newRt = new Rect(mousePos.x + mouseDragOffset.x, mousePos.y + mouseDragOffset.y + offset, rt.width, rt.height);

                    string path = el.path;
                    bool isDir = el.id == -1;
                    //string displayName = isDir ? path : (folderedView && path.Contains("/") ? path.Split('/').Last() : path);
                    string displayName = isDir ? DirDisplayName(path, 0, out _) : (path.Contains("/") ? path.Split('/').Last() : path);
                    
                    HoverSelectGUI (new GUIContent (displayName), selectionSystem.IsSelected(draggedIndicies[i]), !isDir && hiddenIDsToggler.IsState(el.id),isDir, newRt);






                }

                

            }

                
            

            inputs.selectionChanged = selectionSystem.HandlDirectionalSelection(kbListener[KeyCode.UpArrow], kbListener[KeyCode.DownArrow], kbListener.shift, mainElements.Length - 1);

            int lo = -1;
            if (clickedElementIndex == -1 && kbListener[KeyCode.Return] && SingleDirectorySelected(out lo)) {
                clickedElementIndex = lo;
                clickedElementWindowIndex = 1;
            }

                
            if (clickedElementIndex != -1) {
                if (clickedElementWindowIndex == 1) {

                    if (mainElements[clickedElementIndex].id != -1){
                        selectionSystem.OnObjectSelection(clickedElementIndex, kbListener.shift);
                        inputs.selectionChanged = true;
                    }
                    else {
                        inputs.folderFwdDir = mainElements[clickedElementIndex].path;
                    }
                }
                else if (clickedElementWindowIndex == 0) {
                    if (prevWindowElements[clickedElementIndex].id == -2){
                        Debug.Log("Go To: prev Directory");
                        
                        inputs.folderBack = true;
                    }
                    else if (prevWindowElements[clickedElementIndex].id != -1){
                        
                        Debug.Log("Go To: Already there");
                        //selectionSystem.OnObjectSelection(clickedElementIndex, kbListener.shift);
                        //inputs.selectionChanged = true;
                    }
                    else {
                        bool isCurrent = prevWindowElements[clickedElementIndex].path == currentFolderPath;//.Contains(prevWindowElements[clickedElementIndex].path);

                        if (isCurrent) {
                            Debug.Log("Go To: Already There");
                        }
                        else {

                            //string[] sp = prevWindowElements[clickedElementIndex].path.Split('/');
                            //string folder = sp[currentFolderOffset - 1];

                            inputs.folderFwdDir = prevWindowElements[clickedElementIndex].path;
                            //inputs.folderBackFirst = true;
                            Debug.Log("Go To: " + prevWindowElements[clickedElementIndex].path);// prevWindowElements[clickedElementIndex].path);

                        }

                        //inputs.folderFwdDir = elements[clickedElementIndex].path;
                    }

                }
                
            }

            GUIUtils.EndBox(1);


            EditorGUILayout.EndHorizontal();
            
        }

        void OnDropDir(HashSet<int> dragged, Element dirElement) {

                    //void OnDirDragDrop(HashSet<int> dragIndicies, string curPath, ElementSelectionSystem.Element dirElement, int viewTab) {


            onDirDragDrop(dragged, currentFolderPath, dirElement, viewTab);
            

        }

        public void HandleInputs (Inputs inputs, bool forceRebuild) {


            //bool clearAndRebuild = forceRebuild || inputs.changedTabView || inputs.folderedToggled || inputs.searchChanged|| inputs.paginationSuccess;
            //bool resetPage = inputs.folderedToggled || inputs.changedTabView;
            bool clearAndRebuild = forceRebuild || inputs.changedTabView || inputs.searchChanged|| inputs.paginationSuccess;
            bool resetPage = inputs.changedTabView;
                
            clearAndRebuild = clearAndRebuild || ((inputs.hiddenToggleSuccess));// && selectionSystem.hasSelection));// || inputs.resetHidden);
                        
            //if (folderedView) {

                if (inputs.paginationAttempt == PaginationAttempt.Back && !inputs.paginationSuccess) inputs.folderBack = true;
                
                bool movedFolder = (inputs.folderBack && MoveFolder()) || (inputs.folderFwdDir != null && MoveFolder(inputs.folderFwdDir));
                
                clearAndRebuild = clearAndRebuild || movedFolder;
                resetPage = resetPage || movedFolder;
            //}

            if (inputs.changedTabView) {// || inputs.folderedToggled) {
                ResetCurrentPath();
            }
            if (clearAndRebuild) {                
                ClearSelectionsAndRebuild(resetPage);
                inputs.selectionChanged = true;
            }
            if (inputs.selectionChanged) onSelectionChange();
        }

        public void ForceBackFolder () {

            MoveFolder();
            ClearSelectionsAndRebuild(true);
            onSelectionChange();
        }

        System.Action<int, string, string, bool> onMoveFolder;


        string CalcLastFolder(string curFolder) {
            if (curFolder.IsEmpty()) return curFolder;
            return curFolder.Substring(0, curFolder.Substring(0, curFolder.Length-1).LastIndexOf("/") + 1);


        }

        //bool MoveFolder(string addPath = null, bool folderBackFirst = false) {
        bool MoveFolder(string toPath = null){//, bool folderBackFirst = false) {
            
            bool back = toPath == null;// addPath == null;
            if (back) {
                if (currentFolderPath.IsEmpty()) return false;
                //if (currentFolderOffset <= 0) return false;

                currentFolderPath = CalcLastFolder(currentFolderPath);
                prevFolderPath = CalcLastFolder(currentFolderPath);



            }
            else {
                //if (folderBackFirst) {

                    //currentFolderPath = CalcLastFolder(currentFolderPath);
                    //prevFolderPath = CalcLastFolder(currentFolderPath);
                    //currentFolderOffset--;


                //}

                currentFolderPath = toPath + "/";
                prevFolderPath = CalcLastFolder(currentFolderPath);

                Debug.Log("prevFolderPath: " + prevFolderPath);
                Debug.Log("currentFolderPath: " + currentFolderPath);

                

                //prevFolderPath = currentFolderPath;
                //currentFolderPath += addPath + "/";
            
            }
            //currentFolderOffset+= back ? -1 : 1;

            onMoveFolder (viewTab, toPath, WithoutLastSlash(currentFolderPath), false );//, folderBackFirst );

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
                    .Generate(poolCount, i => getElementAtPoolIndex(i, viewTab, WithoutLastSlash(currentFolderPath)))
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
            return FilterOutDirectories(GetPoolIndiciesInSelectionBase());
        }
        public HashSet<int> GetPoolIndiciesInSelectionOrAllShown () {
            return FilterOutDirectories( selectionSystem.hasSelection ? GetPoolIndiciesInSelectionBase() : GetPoolIndiciesInElementsBase() );
        }

        public HashSet<int> GetIDsInSelection(bool includeDirs, out bool hasDirs) {
            hasDirs = false;
            if (!selectionSystem.hasSelection) return null;
            int poolCount = getPoolCount(viewTab, WithoutLastSlash(currentFolderPath));
            HashSet<int> ignorePoolIDs = getIgnorePoolIDs(viewTab);
            HashSet<int> ids = new HashSet<int>();

            foreach (var i in selectionSystem.GetSelectionEnumerator()) {
                int id = mainElements[i].id;
                string path = mainElements[i].path;
                if (id == -1) {
                    if (includeDirs) {
                        string dir = currentFolderPath + path;
                        
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
        public HashSet<int> GetIDsInElements(bool includeDirs, out bool hasDirs) {
            hasDirs = false;
            int poolCount = getPoolCount(viewTab, WithoutLastSlash(currentFolderPath));
            HashSet<int> ignorePoolIDs = getIgnorePoolIDs(viewTab);
            HashSet<int> ids = new HashSet<int>();

            foreach (var e in mainElements) {// selectionSystem.GetSelectionEnumerator()) {
                int id = e.id;// elements[i].id;
                string path = e.path;//elements[i].path;
                if (id == -1) {
                    if (includeDirs) {
                        string dir = currentFolderPath + path;//elements[i].path;
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


        ViewTabOption[] viewTabOptions;
        
        public void Initialize (
            ViewTabOption[] viewTabOptions, 
            EditorProp hiddenIDsProp, 
            GetElementAtPoolIndex getElementAtPoolIndex, GetPoolCount getPoolCount, GetIgnorePoolIDs getIgnorePoolIDs, 

        System.Action onSelectionChange,
        System.Action<int, string, string, bool> onMoveFolder,
        System.Action<HashSet<int>, string, Element, int> onDirDragDrop
        ) {
            this.onMoveFolder = onMoveFolder;
            this.onDirDragDrop = onDirDragDrop;
            List<ViewTabOption> viewOpts = new List<ViewTabOption>();
            if (viewTabOptions.Length != 0) {
                for (int i = 0; i < viewTabOptions.Length; i++) {
                    viewOpts.Add(viewTabOptions[i]);
                }
            }
            else {
                viewOpts.Add(new ViewTabOption( new GUIContent("Select"), true ));
            }
            viewOpts.Add(new ViewTabOption( new GUIContent("Hidden"), true ));

            this.viewTabOptions = viewOpts.ToArray();

            hiddenIDsToggler.Initialize(hiddenIDsProp);
            
            this.getElementAtPoolIndex = getElementAtPoolIndex;
            this.getPoolCount = getPoolCount;
            this.getIgnorePoolIDs = getIgnorePoolIDs;
            this.onSelectionChange = onSelectionChange;

            ResetCurrentPath();
            ClearSelectionsAndRebuild(true);
            onSelectionChange();
        }

        public bool showingHidden { get { return viewTab == viewTabOptions.Length - 1; } }


        string WithoutLastSlash(string path) {
            if (path.IsEmpty() || !path.EndsWith("/")) {
                return path;
            }
            return path.Substring(0, path.Length - 1);
        }

        void RebuildDisplayedElements() {
            Debug.Log("rebuilding");


            int poolCount = getPoolCount(viewTab, WithoutLastSlash(currentFolderPath));
            HashSet<int> ignorePoolIDs = getIgnorePoolIDs(viewTab);

            HashSet<string> used = new HashSet<string>();         

            int lastDir = 0;
            
            IEnumerable<Element> filtered = new List<Element>()
                .Generate(poolCount, i => getElementAtPoolIndex(i, viewTab, WithoutLastSlash(currentFolderPath)))
                .Where(e => !ignorePoolIDs.Contains(e.id) && showingHidden == hiddenIDsToggler.IsState(e.id) && searchFilter.PassesSearchFilter(e.path))
                //.ToList()
            ;

            List<Element> unpaginated;
            //if (!folderedView) {
            //    unpaginated = filtered.ToList();
            //}
            //else {


                unpaginated = new List<Element>();

                bool debug = true;;
                foreach (var e in filtered) {
                    if (!currentFolderPath.IsEmpty() && !e.path.StartsWith(currentFolderPath)) continue;

                    string name = e.path;
                    bool isFile = true;

                    if (name.Contains("/")) {

                        name = DirDisplayName ( name, 0, out isFile, debug );

                        debug = false;

                        //string[] sp = name.Split('/');
                        //name = sp[currentFolderOffset];
                        //isFile = currentFolderOffset == sp.Length - 1;
                    }
                    
                    //foldered directory
                    if (!isFile) {
                        if (used.Contains(name)) continue;
                        used.Add(name);
                        unpaginated.Insert(lastDir, new Element(-1, currentFolderPath + name, -1));// name, -1));
                        lastDir++;       
                        continue;
                    }   
                    unpaginated.Add(e);
                }
            //}

            mainElements = pagination.Paginate(unpaginated).ToArray();


            //int poolCount = getPoolCount(viewTab);
            //HashSet<int> ignorePoolIDs = getIgnorePoolIDs(viewTab);

            //HashSet<string> used = new HashSet<string>();      

            used.Clear();   
               
            lastDir = 0;
            
            List<Element> prevElement;
                prevElement = new List<Element>();



                //if (currentFolderOffset > 0) {
                if (!currentFolderPath.IsEmpty()) {

                    Debug.Log("rebuilding previous");

            poolCount = getPoolCount(viewTab, WithoutLastSlash(prevFolderPath));


            IEnumerable<Element> prevfiltered = new List<Element>()
                .Generate(poolCount, i => getElementAtPoolIndex(i, viewTab, WithoutLastSlash(prevFolderPath)))
                .Where(e => !ignorePoolIDs.Contains(e.id) && showingHidden == hiddenIDsToggler.IsState(e.id) && searchFilter.PassesSearchFilter(e.path))
                //.ToList()
            ;

            //if (!folderedView) {
            //    unpaginated = filtered.ToList();
            //}
            //else {

                debug = true;


                foreach (var e in prevfiltered) {
                    if (!prevFolderPath.IsEmpty() && !e.path.StartsWith(prevFolderPath)) continue;

                    string name = e.path;
                    bool isFile = true;

                    if (name.Contains("/")) {
                        int offset = -1;
                        name = DirDisplayName(name, offset, out isFile, debug);

                        debug = false;
                        //string[] sp = name.Split('/');
                        //name = sp[currentFolderOffset - 1];
                        //isFile = currentFolderOffset - 1 == name.Split('/').Length - 1;
                    }
                    
                    //foldered directory
                    if (!isFile) {
                        if (used.Contains(name)) continue;
                        used.Add(name);
                        //Debug.Log("path: " + e.path);
            
                        prevElement.Add(new Element(-1, prevFolderPath + name, -1));
                        lastDir++;       
                        continue;
                    }   
                    //prevElement.Add(e);
                }
            //}
            //Debug.Log("path: " + WithoutLastSlash(prevFolderPath));
                prevElement.Insert(0, new Element(-2, WithoutLastSlash(prevFolderPath), -1));
                }
            prevWindowElements = prevElement.ToArray();

            //elements = pagination.Paginate(unpaginated).ToArray();
        }

        void ClearSelectionsAndRebuild(bool resetPage) {
            if (resetPage) pagination.ResetPage();
            selectionSystem.ClearSelection();
            RebuildDisplayedElements();
        }

        public void DrawToolbar (KeyboardListener kbListener, Inputs inputs) {
            
            GUIUtils.StartBox(0);
            inputs.changedTabView = GUIUtils.Tabs(new GUIContent[viewTabOptions.Length].Generate(viewTabOptions, e => e.name), ref viewTab);
            GUIUtils.Space();
            
            EditorGUILayout.BeginHorizontal();

            if (viewTabOptions[viewTab].canHide) {
                ToggleHiddenButtonGUI(kbListener, inputs, GUIStyles.miniButtonLeft);    
            }
            //FolderedViewButtonGUI(inputs, viewTabOptions[viewTab].canHide ? GUIStyles.miniButtonRight : GUIStyles.miniButton);
            

            
            inputs.searchChanged = searchFilter.SearchBarGUI();
            
            EditorGUILayout.EndHorizontal();
            
            GUIUtils.Space();
            
            DirectoryBackButtonGUI(inputs, GUIStyles.miniButtonLeft);
            
            GUIUtils.EndBox(1);
        }

        void DirectoryBackButtonGUI (Inputs input, GUIStyle s) {   
            EditorGUILayout.BeginHorizontal();
            GUIContent c = new GUIContent("   <<   ", "Back");
            GUI.enabled = !currentFolderPath.IsEmpty();// currentFolderOffset > 0;
            //GUI.enabled = folderedView && curFolderOffset > 0;
            
            input.folderBack = GUIUtils.Button(c, GUIStyles.miniButtonLeft, Colors.liteGray, Colors.black, new GUILayoutOption[] { GUILayout.Height(16), c.CalcWidth(GUIStyles.miniButtonRight)  });
            GUI.enabled = false;
            EditorGUILayout.TextField(currentFolderPath);            
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }
        //void FolderedViewButtonGUI (Inputs input, GUIStyle s) {
        //    folderedView = GUIUtils.ToggleButton(new GUIContent("Folders", "Enable/Disable Foldered View"), true, folderedView, s, out input.folderedToggled);
        //}   

        
        void ToggleHiddenButtonGUI (KeyboardListener kbListener, Inputs input, GUIStyle s) {
            
            //GUI.enabled = selectionSystem.hasSelection;
            hiddenIDsToggler.ToggleStateButton(
                kbListener[KeyCode.H], 
                new GUIContent("Hide/Unhide", "Toggle the hidden status of the selection (if any, else all shown elements)"), true, s, 
                out input.hiddenToggleSuccess,
                GetHiddenToggleSelection
            );


                //if (setProp != -1) AssetObjectEditor.CopyParameters(GetAOPropsSelectOrAll(), multiEditAO, setProp);
        

      






            //GUI.enabled = true;
        }
        HashSet<int> GetHiddenToggleSelection () {

            HashSet<int> selection = new HashSet<int>();
            //if (!selectionSystem.hasSelection) {

            //    return selection;
            //}
            
            
            
            
            bool containsDirs;
            selection = !selectionSystem.hasSelection ? GetIDsInElements(true, out containsDirs) : GetIDsInSelection(true, out containsDirs);
            if (containsDirs && !EditorUtility.DisplayDialog("Hide/Unhide Directory", "Selection contains directories, hidden status of all sub elements will be toggled", "Ok", "Cancel"))  
                selection.Clear();
            return selection;
        }
    }
}