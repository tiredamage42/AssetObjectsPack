

//using System.Collections;
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
            //If you don't want to use the secondary button simply leave it out:
            ScriptableWizard.DisplayWizard<AnimatorControllerBuilder>("Generate Anim Controller", "Generate");
        }
        void OnWizardCreate() {
            BuildAnimatorController();
        }

        
        static readonly string save_path = AssetObjectsEditor.GetPackRootDirectory("Animations") + "AnimationsController.controller";
        
        
        [Header("HIGHLY RECOMMENDED")]
        [Tooltip("Use only the animations specified in cue lists in the project, setting to false can cause performance issues")]
        public bool usedOnly = true;
        public float exitTransitionDuration = .1f;



        public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object {

            string[] paths = AssetDatabase.GetAllAssetPaths().Where( f => f.Contains(".prefab") ).ToArray();
            int l = paths.Length;
            List<T> assets = new List<T>();
            for (int i = 0; i < l; i++) {
                assets.AddRange(EditorUtils.GetAssetsAtPath<T>(paths[i]));
            }
            return assets;
        }

        List<AnimationEvent> GetAllCuesInProject() {
            return FindAssetsByType<AnimationEvent>();
        }

    
        //only add used animations to controller, to avoid adding thousands of states
        List<int> GetAllUsedIDs () {
            List<int> used_ids = new List<int>();
            List<AnimationEvent> all_cues = GetAllCuesInProject();
            int l = all_cues.Count;
            for (int i = 0; i < l; i++) {
                AnimationEvent c = all_cues[i];
                int y = c.assetObjects.Count;
                for (int x = 0; x < y; x++) {
                    int id = c.assetObjects[x].id;
                    if (!used_ids.Contains(id)) {
                        used_ids.Add(id);
                    }
                }
            }
            if (used_ids.Count == 0) {
                Debug.LogError("no scenes saved or no animations specified in scene cues");
            }
            return used_ids;
        }




        void BuildAnimatorController () {
            List<int> used_ids = new List<int>();
            if (usedOnly) {
                used_ids = GetAllUsedIDs();
                if (used_ids.Count == 0) {
                    Debug.LogError("no scenes saved or no animations specified in scene cues");
                    return;
                }
            } 



            string[] file_paths = AssetObjectsEditor.GetAllAssetObjectPaths("Animations", ".fbx", true);
            if (usedOnly) {
                file_paths = file_paths.Where(f => used_ids.Contains(AssetObjectsEditor.GetObjectIDFromPath(f))).ToArray();
            }
            //string[] file_paths = AssetUtils.GetAllAssetObjectPaths(EditorUtils.animations_dir, ".fbx", true);// CheckPath);

            
            int c = file_paths.Length;

            ClipIDPair[] clip_ids = BuildClipIDPairs(file_paths, c);
            if (clip_ids == null) {
                return;
            }
            ControllerBuild(clip_ids, c);   
            Debug.Log("Controller built at: " + save_path);         
        }
        ClipIDPair[] BuildClipIDPairs (string[] file_paths, int c) {
            ClipIDPair[] results = new ClipIDPair[c];
            for (int i = 0; i < c; i++) {
                string f = file_paths[i];// f_start + file_paths[i];
                AnimationClip clip = EditorUtils.GetAssetAtPath<AnimationClip>(f);
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
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(save_path);
            AddParameters(controller);

            AnimatorState[] blend_trees = new AnimatorState[2];
            for (int i =0 ; i < 2; i++) {
                blend_trees[i] = AddBlendTree(AnimationsPack.p_Loop_Mirrors[i], AnimationsPack.p_Loop_Speeds[i], controller, 0, AnimationsPack.p_Loop_Names[i], clips, c, AnimationsPack.p_Loop_Indicies[i]);
            }   
            for (int i = 0; i < c; i++) {
                AddState(controller, 0, clips[i], blend_trees);
            }            
        }
        


        void AddState (AnimatorController controller, int layer_index, ClipIDPair clip_id_pair, AnimatorState[] blend_trees) {
            
            AnimatorStateMachine sm = controller.layers[layer_index].stateMachine;
            
            // Add the state to the state machine
            // (The Vector3 is for positioning in the editor window)
            
                
            AnimatorState state = sm.AddState(clip_id_pair.id.ToString());//, new Vector3(300, 0, 0));
            state.mirrorParameterActive = true;
            state.mirrorParameter = AnimationsPack.p_Mirror;

            state.speedParameterActive = true;
            state.speedParameter = AnimationsPack.p_Speed;
                
            state.tag = AnimationsPack.shots_name;
            state.motion = clip_id_pair.clip;

            float clip_length = clip_id_pair.clip.length;
            float exit_time = 1.0f - (exitTransitionDuration / clip_length);

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

        static AnimatorState AddBlendTree (string mirrorParameter, string speedParameter, AnimatorController controller, int layer_index, string name, ClipIDPair[] clip_id_pairs, int c, string blend_param) {
            AnimatorState blendTreeState = controller.layers[layer_index].stateMachine.AddState(name);
            BlendTree blendTree = new BlendTree();
            AssetDatabase.AddObjectToAsset(blendTree, controller);
            blendTree.name = name;
            blendTree.hideFlags = HideFlags.HideInHierarchy;
            blendTree.blendType = BlendTreeType.Simple1D;
            blendTree.blendParameter = blend_param;
            blendTree.useAutomaticThresholds = false;

            
            for (int i = 0; i < c; i++) {
                blendTree.AddChild(clip_id_pairs[i].clip, threshold: clip_id_pairs[i].id);
            }

            blendTreeState.motion = blendTree;

            blendTreeState.mirrorParameterActive = true;
            blendTreeState.mirrorParameter = mirrorParameter;

            blendTreeState.speedParameterActive = true;
            blendTreeState.speedParameter = speedParameter;
            
            return blendTreeState;
        }
    }
}






