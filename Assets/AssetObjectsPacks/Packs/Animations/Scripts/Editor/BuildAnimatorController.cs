using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
using UnityEditor;
using System.Linq;

namespace AssetObjectsPacks.Animations {
    struct ClipIDPair {
        public AnimationClip clip;
        public int id;
        public ClipIDPair (AnimationClip clip, int id) {
            this.clip = clip;
            this.id = id;
        }
    }
    public class AnimatorControllerBuilder : ScriptableWizard
    {
        [MenuItem("Animations Pack/Generate Anim Controller")]
        static void CreateWizard() {
            ScriptableWizard.DisplayWizard<AnimatorControllerBuilder>("Generate Anim Controller", "Generate");
        }
        void OnWizardCreate() {
            AssetObjectsManager instance = AssetObjectsManager.instance;
            if (instance == null) return;
            AssetObjectPacks packs = instance.packs;
            if (packs == null) return;
            
            if (!saveDirectory.EndsWith("/")) saveDirectory += "/";
            for (int i = 0; i < packs.packs.Length; i++) {
                if( packs.packs[i].name == animationsPackName) {
                    BuildAnimatorController(packs.packs[i]);
                    break;
                }
            }
        }

        [Header("HIGHLY RECOMMENDED")]
        [Tooltip("Use only the animations specified in cue lists in the project, setting to false can cause performance issues")]
        public bool usedOnly = true;
        public float exitTransitionDuration = .25f;
        public string animationsPackName = "Animations";
        public string saveDirectory = "Assets/";

        public static List<AssetObjectEventPack> GetAllEventPacks() {
            string[] paths = AssetDatabase.GetAllAssetPaths().Where( f => f.Contains(".asset") ).ToArray();
            List<AssetObjectEventPack> all_event_packs = new List<AssetObjectEventPack>();
            int l = paths.Length;
            for (int i = 0; i < l; i++) {
                all_event_packs.AddRange(EditorUtils.GetAssetsAtPath<AssetObjectEventPack>(paths[i]));
            }
            return all_event_packs;
        }

        
    
        //only add used animations to controller, to avoid adding thousands of states
        List<int> GetAllUsedIDs () {
            List<int> used_ids = new List<int>();
            List<AssetObjectEventPack> all_event_packs = GetAllEventPacks();
            int l = all_event_packs.Count;
            for (int i = 0; i < l; i++) {
                AssetObjectEventPack ep = all_event_packs[i];
                if (AssetObjectsManager.instance.packs.FindPackByID( ep.assetObjectPackID ).name == animationsPackName) {
                    int y = ep.assetObjects.Count;
                    for (int z = 0; z < y; z++) {
                        int id = ep.assetObjects[z].id;
                        if (!used_ids.Contains(id)) {
                            used_ids.Add(id);
                        }
                    }
                }
            }
            
            if (used_ids.Count == 0) {
                Debug.LogError("no scenes saved or no animations specified in scene cues");
            }
            return used_ids;
        }

        void BuildAnimatorController (AssetObjectPack animation_type) {
            List<int> used_ids = new List<int>();
            if (usedOnly) {
                used_ids = GetAllUsedIDs();
                if (used_ids.Count == 0) {
                    return;
                }
            } 

            string[] file_paths = AssetObjectsEditor.GetAllAssetObjectPaths(animation_type.objectsDirectory, animation_type.fileExtensions, true);
            
            //filter out unused file paths
            if (usedOnly) file_paths = file_paths.Where(f => used_ids.Contains(AssetObjectsEditor.GetObjectIDFromPath(f))).ToArray();
            

            

            int c = file_paths.Length;
            ClipIDPair[] clip_id_pairs = BuildClipIDPairs(file_paths, c);
            if (clip_id_pairs == null) return;
            
            ControllerBuild(clip_id_pairs, c);   

            Debug.Log("Controller built at: " + saveDirectory + "AnimationsController.controller");         
        }

        // fbx files have extra "preview" clip that was getting in the awy (mixamo)
        Object FilterOutPreview (Object[] all_objects) {
            int l = all_objects.Length;
            for (int i = 0; i < l; i++) {
                Object o = all_objects[i];
                if (!o.name.Contains("__preview__")) {
                    return o;
                }
            }

            return null;

        }

        ClipIDPair[] BuildClipIDPairs (string[] file_paths, int c) {
            ClipIDPair[] results = new ClipIDPair[c];
            for (int i = 0; i < c; i++) {
                string f = file_paths[i];
                
                AnimationClip clip = (AnimationClip)FilterOutPreview(EditorUtils.GetAssetsAtPath<AnimationClip>(f));
                if (clip == null) {
                    Debug.LogError("Clip is null: " + f);
                    return null;
                }
                results[i] = new ClipIDPair(clip, AssetObjectsEditor.GetObjectIDFromPath(f));
            }
            return results;
        }
        void AddParameters (AnimatorController controller) {
            controller.AddParameter(AnimationsPack.p_Mirror, AnimatorControllerParameterType.Bool);
            controller.AddParameter(AnimationsPack.p_Speed, AnimatorControllerParameterType.Float);

            for (int i = 0; i < AnimationsPack.p_Loop_Indicies.Length; i++) {
                controller.AddParameter(AnimationsPack.p_Loop_Indicies[i], AnimatorControllerParameterType.Float);
                controller.AddParameter(AnimationsPack.p_Loop_Mirrors[i], AnimatorControllerParameterType.Bool);
                controller.AddParameter(AnimationsPack.p_Loop_Speeds[i], AnimatorControllerParameterType.Float);
            }
            controller.AddParameter(AnimationsPack.p_ActiveLoopSet, AnimatorControllerParameterType.Int);
        }
        void ControllerBuild (ClipIDPair[] clips, int c) {
            // Create the animator in the project
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(saveDirectory + "AnimationsController.controller");
            
            //add parameters
            AddParameters(controller);

            //make blend trees for looped animations (2 to smoothly transition between loops)
            int blend_tree_count = 2;
            AnimatorState[] loop_blend_trees = new AnimatorState[blend_tree_count];
            for (int i =0 ; i < blend_tree_count; i++) {
                loop_blend_trees[i] = AddBlendTree(controller, 0, clips, AnimationsPack.p_Loop_Names[i], AnimationsPack.p_Loop_Mirrors[i], AnimationsPack.p_Loop_Speeds[i], c, AnimationsPack.p_Loop_Indicies[i]);
            }      
            for (int i = 0; i < c; i++) {
                AddState(controller, 0, clips[i], loop_blend_trees);
            }    
        }


        void AddState (AnimatorController controller, int layer_index, ClipIDPair clip_id_pair, AnimatorState[] blend_trees) {
            int state_id = clip_id_pair.id;
            AnimationClip clip = clip_id_pair.clip;

            AnimatorStateMachine sm = controller.layers[layer_index].stateMachine;
            
            // Add the state to the state machine
            // (The Vector3 is for positioning in the editor window)
            AnimatorState state = sm.AddState(state_id.ToString());//, new Vector3(300, 0, 0));
            
            state.mirrorParameterActive = true;
            state.mirrorParameter = AnimationsPack.p_Mirror;

            state.speedParameterActive = true;
            state.speedParameter = AnimationsPack.p_Speed;
                
            state.tag = AnimationsPack.shots_name;
            state.motion = clip;

            float exit_time = 1.0f - (exitTransitionDuration / clip.length);

            int c = blend_trees.Length;
            for (int i = 0; i < c; i++) {
                AnimatorStateTransition exit_transition = state.AddTransition(blend_trees[i]);
                exit_transition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, i, AnimationsPack.p_ActiveLoopSet);
                exit_transition.canTransitionToSelf = false;
                exit_transition.hasExitTime = true;
                exit_transition.exitTime = exit_time;
                exit_transition.hasFixedDuration = true;
                exit_transition.duration = exitTransitionDuration;
        
            }
        }

        AnimatorState AddBlendTree (AnimatorController controller, int layer_index, ClipIDPair[] clip_id_pairs, string name, string mirrorParameter, string speedParameter, int c, string blend_param) {
            AnimatorState blendTreeState = controller.layers[layer_index].stateMachine.AddState(name);
            BlendTree blendTree = new BlendTree();
            AssetDatabase.AddObjectToAsset(blendTree, controller);
            blendTree.name = name;
            blendTree.hideFlags = HideFlags.HideInHierarchy;
            blendTree.blendType = BlendTreeType.Simple1D;
            blendTree.blendParameter = blend_param;
            blendTree.useAutomaticThresholds = false;
            
            for (int i = 0; i < c; i++) blendTree.AddChild(clip_id_pairs[i].clip, threshold: clip_id_pairs[i].id);

            blendTreeState.motion = blendTree;

            blendTreeState.mirrorParameterActive = true;
            blendTreeState.mirrorParameter = mirrorParameter;

            blendTreeState.speedParameterActive = true;
            blendTreeState.speedParameter = speedParameter;
            
            return blendTreeState;
        }
    }
}






