using UnityEngine;
using UnityEditor;
using System.Linq;
namespace AssetObjectsPacks {
    [CustomEditor(typeof(CueBehavior))]
    public class CueBehaviorEditor : Editor {
        const string messageBlocksField = "messageBlocks";
        const string overrideDurationField = "overrideDuration", eventsField = "events";
        const string snapStyleField = "snapPlayerStyle", smoothPosTimeField = "smoothPositionTime", smoothRotTimeField = "smoothRotationTime", playImmediateField = "playImmediate";
        const string rotationOffsetField = "rotationOffset", positionOffsetField = "positionOffset";

        GUIContent positionOffsetGUI = new GUIContent("Position Offset");
        GUIContent rotationOffsetGUI = new GUIContent("Rotation Offset");
        GUIContent msgBlocksLabel = new GUIContent("<b>Send Messages</b>:", "");
        GUIContent overrideDurationGUI = new GUIContent("Override Duration", "negative values give control to the events");
        GUIContent positionTimeGUI = new GUIContent("Position Time (s)");
        GUIContent snapStyleGUI = new GUIContent("Snap Style", "If the event should wait for the player to snap to the event transform before being considered ready");
        GUIContent playImmediateGUI = new GUIContent("Play Immediate", "Play before the position snap");
        GUIContent rotationTimeGUI = new GUIContent("Rotation Time (s)");
        GUIContent[] tabLbls = 4.Generate( i => new GUIContent(((Cue.MessageEvent)i).ToString()) ).ToArray();
        EditorProp so;
        int currentMsgBlock;

        public static string[] copiedMessages = new string[4];

        void OnEnable () {
            so = new EditorProp( serializedObject );
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            bool isSmoothSnap = so[snapStyleField].enumValueIndex == (int)Cue.SnapPlayerStyle.Smooth;
            GUIUtils.StartCustomEditor();
            
            GUIUtils.StartBox(0);
            EditorGUILayout.BeginHorizontal();
            GUIUtils.Label(msgBlocksLabel);
            if (GUIUtils.SmallButton(new GUIContent("", "Paste"), Colors.green, Colors.black)) {
                for (int i = 0; i < 4; i ++) so[messageBlocksField][i].SetValue( copiedMessages[i] );
            }
            if (GUIUtils.SmallButton(new GUIContent("", "Copy"), Colors.yellow, Colors.black)) {
                for (int i = 0; i < 4; i ++) copiedMessages[i] = so[messageBlocksField][i].stringValue;
            }
            if (GUIUtils.SmallDeleteButton()) {
                for (int i = 0; i < 4; i ++) so[messageBlocksField][i].SetValue( "" );
            }
            EditorGUILayout.EndHorizontal();
            GUIUtils.Tabs((isSmoothSnap ? 4 : 3).Generate( i=> tabLbls[i]).ToArray(), ref currentMsgBlock);
            GUIUtils.DrawMultiLineExpandableString(so[messageBlocksField][currentMsgBlock], false, "mesage block", 50);
            GUIUtils.EndBox(1);
            

            GUIUtils.StartBox(1);
            EditorGUI.indentLevel++;
            GUIUtils.DrawEnumProp(
                so[snapStyleField], snapStyleGUI, 
                (int i) => (Cue.SnapPlayerStyle)i, 
                (System.Enum s) => (int)((Cue.SnapPlayerStyle)s)
            );
                     
            if (isSmoothSnap){
                EditorGUI.indentLevel++;
                GUIUtils.DrawProp(so[smoothPosTimeField], positionTimeGUI);
                GUIUtils.DrawProp(so[smoothRotTimeField], rotationTimeGUI);
                GUIUtils.DrawToggleProp(so[playImmediateField], playImmediateGUI);
                EditorGUI.indentLevel--;
            }
                
            GUIUtils.DrawProp(so[positionOffsetField], positionOffsetGUI);
            GUIUtils.DrawProp(so[rotationOffsetField], rotationOffsetGUI);
            GUIUtils.EndBox(1);

            GUIUtils.Space();
            EditorGUI.indentLevel--;
            GUIUtils.Label(new GUIContent("<b>The following variables wont be used if the cue has sub-cues:</b>"));
            EditorGUI.indentLevel++;
                            
            GUIUtils.StartBox(1);
            GUIUtils.DrawProp(so[overrideDurationField], overrideDurationGUI);
            EditorGUI.indentLevel--;
            GUIUtils.Space();
            GUIUtils.DrawObjArrayProp<Event>( so[eventsField] );
            GUIUtils.EndBox(1);

            GUIUtils.EndCustomEditor(so);                
        }
    }
}