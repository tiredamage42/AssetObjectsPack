using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
namespace AssetObjectsPacks {
    [CustomEditor(typeof(Cue))]
    public class CueEditor : Editor {
        const string messageBlocksField = "messageBlocks";
        //const string overrideDurationField = "overrideDuration", repeatsField = "repeats", playlistField = "playlist", eventsField = "events";
        const string overrideDurationField = "overrideDuration", repeatsField = "repeats", eventsField = "events";
        
        const string snapStyleField = "snapPlayerStyle", smoothPosTimeField = "smoothPositionTime", smoothRotTimeField = "smoothRotationTime", playImmediateField = "playImmediate";
        const string gizmoColorField = "gizmoColor", useRandomPlaylistField = "useRandomPlaylist";
               
        EditorProp so;
        Transform soT;
        int currentMsgBlock;

        GUIContent useRandomPlaylistGUI = new GUIContent("Use Random Playlist Choice", "Sub playlists/cues play one random choice, not all sequentially");
        GUIContent msgBlocksLabel = new GUIContent("<b>Send Messages</b>:", "");
        //GUIContent playlistGUI = new GUIContent("Playlist", "Playlist to trigger");
        GUIContent overrideDurationGUI = new GUIContent("Override Duration", "negative values give control to the events");
        GUIContent repeatsGUI = new GUIContent("Repeats", "How Many times this cue repeats");
        GUIContent positionTimeGUI = new GUIContent("Position Time (s)");
        GUIContent snapStyleGUI = new GUIContent("Snap Style", "If the event should wait for the player to snap to the event transform before being considered ready");
        GUIContent playImmediateGUI = new GUIContent("Play Immediate", "Play before the position snap");
        GUIContent rotationTimeGUI = new GUIContent("Rotation Time (s)");
        GUIContent gizmoColorgUI = new GUIContent("Gizmo Color");
        GUIContent[] tabLbls = 4.Generate( i => new GUIContent(((Cue.MessageEvent)i).ToString()) ).ToArray();

        Cue cue;

        void OnEnable () {
            so = new EditorProp( serializedObject );
            soT = (target as MonoBehaviour).transform;
            cue = target as Cue;
        }
        
        bool HasSubCues() {
            if (soT.childCount == 0) return false;
            for (int i = 0; i < soT.childCount; i++) {
                if (soT.GetChild(i).GetComponent<Cue>() != null) return true;
            }
            return false;
        }
        
        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();

            bool hasSubs = HasSubCues();
            bool isPlaylist = hasSubs;// || so[playlistField].objRefValue != null;
            bool isSmoothSnap = so[snapStyleField].enumValueIndex == (int)Cue.SnapPlayerStyle.Smooth;
            
            GUIUtils.StartCustomEditor();
            GUIUtils.StartBox();
            GUIUtils.DrawProp(so[gizmoColorField], gizmoColorgUI);
            GUIUtils.EndBox();
            GUIUtils.StartBox(1);

            GUIUtils.Label(msgBlocksLabel);
            GUIUtils.Tabs((isSmoothSnap ? 4 : 3).Generate( i=> tabLbls[i]).ToArray(), ref currentMsgBlock);
            GUIUtils.DrawMultiLineExpandableString(so[messageBlocksField][currentMsgBlock], false);
            GUIUtils.Space(2);
            EditorGUI.indentLevel++;
            //GUIUtils.BeginIndent(1);

            GUIUtils.DrawProp(so[repeatsField], repeatsGUI);
            if (so[repeatsField].intValue < 1) {
                so[repeatsField].SetValue(1);
            }
            
            GUIUtils.Space();

            //GUIUtils.DrawProp(so[snapStyleField], snapStyleGUI);

            GUIUtils.DrawEnumProp(
                so[snapStyleField], 
                snapStyleGUI, 
                (int i) => (Cue.SnapPlayerStyle)i, 
                (System.Enum s) => (int)((Cue.SnapPlayerStyle)s)
            );
                    
                
            if (isSmoothSnap){
                EditorGUI.indentLevel++;
                GUIUtils.DrawProp(so[smoothPosTimeField], positionTimeGUI);
                GUIUtils.DrawProp(so[smoothRotTimeField], rotationTimeGUI);
                GUIUtils.DrawToggleProp(so[playImmediateField], playImmediateGUI);


                EditorGUI.indentLevel--;
                GUIUtils.Space();
            }
                
            //if (!hasSubs) {

                //GUIUtils.DrawProp(so[playlistField], playlistGUI);
            //}

            if (isPlaylist) {

                GUIUtils.DrawToggleProp(so[useRandomPlaylistField], useRandomPlaylistGUI);
            }
            
                

            if (!isPlaylist) {
                GUIUtils.DrawProp(so[overrideDurationField], overrideDurationGUI);
            }
            EditorGUI.indentLevel--;
            //GUIUtils.EndIndent();
            
            if (!isPlaylist) {
                GUIUtils.Space();
                GUIUtils.DrawObjArrayProp( so[eventsField] );
            }
            GUIUtils.EndBox(1);
            if (Application.isPlaying) {

                GUIUtils.StartBox();

                //cue.transformTracker.tracking = GUIUtils.DrawToggle(cue.transformTracker.tracking, new GUIContent("Track Transform"));
                cue.transformTracker.tracking = GUIUtils.ToggleButton(new GUIContent("Track Transform"), GUIStyles.button, cue.transformTracker.tracking, out _);


/*
                EditorGUILayout.BeginHorizontal();
                if (GUIUtils.Button(new GUIContent("Save Transform Changes"), GUIStyles.button)) {
                    cue.SaveTransform();
                }
                if (cue.HasTransformChanges()) {

                if (GUIUtils.Button(new GUIContent("Clear Transform Changes"), GUIStyles.button)) {
                    cue.ClearChanges();
                }
                }
                
                
                EditorGUILayout.EndHorizontal();
 */
                
                GUIUtils.EndBox();
            }

/*
 */
            
            GUIUtils.EndCustomEditor(so);                
        }
        GameObject FindPrefab () {
            //cue







        if (PrefabUtility.IsPartOfPrefabInstance(soT.gameObject)) {
            Debug.Log("is part of prefab instance");


        }


      
        //o = PrefabUtility.GetOutermostPrefabInstanceRoot(soT.gameObject);
        //o = PrefabUtility.GetPrefabObject(nearestPrefabRoot);


        //var instanceRoot = PrefabUtility.FindRootGameObjectWithSameParentPrefab(soT.gameObject);
        //var instanceRoot = PrefabUtility.FindValidUploadPrefabInstanceRoot(soT.gameObject);
        //EditorGUIUtility.PingObject(instanceRoot);
        


        GameObject nearestPrefabRoot = PrefabUtility.GetNearestPrefabInstanceRoot(soT);






/*
        GameObject o = PrefabUtility.GetCorrespondingObjectFromOriginalSource(nearestPrefabRoot);

        GameObject prefabRoot = PrefabUtility.FindPrefabRoot(soT.gameObject);


 */
        GameObject o2 = PrefabUtility.GetCorrespondingObjectFromSource(nearestPrefabRoot);
        
        //Debug.Log(o);

        //string path = AssetDatabase.GetAssetPath(o);

        //PrefabUtility.ApplyObjectOverride(nearestPrefabRoot, path, InteractionMode.AutomatedAction);
        
        //Debug.Log("applied overrides at path " + path);
        //EditorUtility.FocusProjectWindow();
        ///EditorGUIUtility.PingObject(o);
        if (PrefabUtility.HasPrefabInstanceAnyOverrides(soT.gameObject, false)) {
            //Debug.Log("has overrides base");

            PropertyModification[] mods = PrefabUtility.GetPropertyModifications(soT);
            List<PropertyModification> nMods = new List<PropertyModification>();
            for (int i = 0; i < mods.Length; i++) {
                Object targ = mods[i].target;
                //Debug.Log(mods[i].target);


                bool isTransform = targ.GetType() == typeof(Transform);
                if (isTransform) {

                    if (((Transform)mods[i].target).IsChildOf(o2.transform)) {

                        //Debug.Log(mods[i].objectReference);
                        //Debug.Log(mods[i].propertyPath);
                        Debug.Log(mods[i].target);

                    
                        nMods.Add(mods[i]);

                        //EditorGUIUtility.PingObject(mods[i].target);

                    }
                }
                
                
            }
            //PrefabUtility.ApplyPrefabInstance(nearestPrefabRoot, InteractionMode.UserAction);


            //PrefabUtility.SetPropertyModifications(o2, nMods.ToArray());


        }
            //ApplyChangesToRoots(instanceRoot);
/*
 */

//        ApplyObjectOverride(Object instanceComponentOrGameObject, string assetPath, InteractionMode action






        //o = PrefabUtility.FindPrefabRoot(soT.gameObject);
    /*
     */        

//Debug.Break();
        
        return null;
    }
    void ApplyChangesToRoots (GameObject instanceRoot) {
        PrefabUtility.UnpackPrefabInstance(instanceRoot, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
    }




    }







    
}