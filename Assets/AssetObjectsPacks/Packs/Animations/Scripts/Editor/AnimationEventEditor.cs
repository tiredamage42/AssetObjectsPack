using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks.Animations {
    [CustomEditor(typeof(AnimationEvent))]
    public class AnimationEventEditor : AssetObjectEventEditor {

       


        protected override AssetObjectParamDef[] DefaultParameters() {
            return new AssetObjectParamDef[] {
                new AssetObjectParamDef(new AssetObjectParam("Speed", 1.0f), ""),
                new AssetObjectParamDef(new AssetObjectParam("Transition Speed", .1f), ""),
                new AssetObjectParamDef(new AssetObjectParam("MirrorMode", 0), "0: None, 1: Mirror, 2: Random"),
            };
        }



   


















        //public List<AnimationAssetObject> multi_edit_instance = new List<AnimationAssetObject>();      

        protected override string AssetObjectUnityAssetType() {
            return "AnimationClip";
        }
        protected override string PackFileExtension() {
            return ".fbx";
        }
        protected override string PackName() {
            return "Animations";
        }
        /*

        protected override void MakeAssetObjectInstanceDefault(SerializedProperty obj_instance) {
            obj_instance.FindPropertyRelative(sSpeed).floatValue = 1.0f;
            obj_instance.FindPropertyRelative(sTransitionSpeed).floatValue = .1f;
            obj_instance.FindPropertyRelative(sMirrorMode).enumValueIndex = 0;
        }
         */
/*
        protected override GUIContent[] InstanceFieldLabels() {
            return new GUIContent[] { new GUIContent("Speed"), new GUIContent("Mirror     "), new GUIContent("Transition Time") };            
        }
        protected override string[] InstanceFieldNames() {
            return new string[] { sSpeed, sMirrorMode, sTransitionSpeed };
        }
 */

        const string sSpeed = "speed";
        const string sTransitionSpeed = "transition_speed";
        const string sMirrorMode = "mirror_mode";
        const string sSnapActorStyle = "snapActorStyle";
        const string sSnapSmoothPosTime = "smoothPositionTime";
        const string sSnapSmoothRotTime = "smoothRotationTime";
        const string sAnimationScene = "animationScene";
        GUIContent snap_style_gui = new GUIContent("Snap Style", "If the cue should wait for the actor to snap to the cue transform before being considered ready");
        GUIContent pos_smooth_gui = new GUIContent("Position Time (s)");
        GUIContent rot_smooth_gui = new GUIContent("Rotation Time (s)");

        static readonly string[] prop_names = new string[] {
            sSnapActorStyle,
            sSnapSmoothPosTime,
            sSnapSmoothRotTime,
            sAnimationScene,
            //sLooped,
            //sDuration, 
        };
        Dictionary<string, SerializedProperty> script_properties = new Dictionary<string, SerializedProperty>(prop_names.Length);
        
        void MakeInstanceDefault (SerializedProperty obj_instance_prop) {
            obj_instance_prop.FindPropertyRelative(sSpeed).floatValue = 1.0f;
            obj_instance_prop.FindPropertyRelative(sTransitionSpeed).floatValue = .1f;
            obj_instance_prop.FindPropertyRelative(sMirrorMode).enumValueIndex = 0;
        }
        
        void InitializeSerializedProperties () {
            int l = prop_names.Length;
            for (int i = 0; i < l; i++) {
                string n = prop_names[i];
                script_properties.Add(n, serializedObject.FindProperty(n));
            }
        }
        

        new AnimationEvent target;
        void OnEnable () {
            this.target = base.target as AnimationEvent;
            InitializeSerializedProperties();
        }
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(script_properties[sSnapActorStyle], snap_style_gui);
            if (target.snapActorStyle == AnimationEvent.SnapActorStyle.Smooth) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(script_properties[sSnapSmoothPosTime], pos_smooth_gui);
                EditorGUILayout.PropertyField(script_properties[sSnapSmoothRotTime], rot_smooth_gui);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(script_properties[sAnimationScene]);
            if (target.animationScene == null) {
                DrawAssetObjectEvent();
            }
            if (EditorGUI.EndChangeCheck()) {                
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}










