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
            PacksManager packs = AssetObjectsEditor.packManager;
            if (packs == null) {
                Debug.LogError("Couldnt find pack manager");
                return;
            }
            
            if (!saveDirectory.EndsWith("/")) saveDirectory += "/";

            bool foundPack = false;
            for (int i = 0; i < packs.packs.Length; i++) {
                if( packs.packs[i].name == animationsPackName) {
                    BuildAnimatorController(packs.packs[i]);
                    foundPack = true;
                    break;
                }
            }
            if (!foundPack) {
                Debug.LogError("Couldnt find pack named: " + animationsPackName);
            }
        }

        [Header("HIGHLY RECOMMENDED")]
        [Tooltip("Use only the animations specified in cue lists in the project, setting to false can cause performance issues")]
        public bool usedOnly = true;
        public float exitTransitionDuration = .25f;
        public string animationsPackName = "Animations";
        public string saveDirectory = "Assets/";
            
        //only add used animations to controller, to avoid adding thousands of states
        

        void BuildAnimatorController (AssetObjectPack animationPack) {
            string[] allPaths = AssetObjectsEditor.GetAllAssetObjectPaths(animationPack.dir, animationPack.extensions, true);

            //filter out unused file paths
            if (usedOnly) {
                int[] used_ids = AssetObjectsEditor.GetAllUsedIDs(animationsPackName);
                if (used_ids.Length == 0) return;
                allPaths = allPaths.Where(f => used_ids.Contains(AssetObjectsEditor.GetObjectIDFromPath(f))).ToArray();
            } 
            
            int c = allPaths.Length;
            ControllerBuild(BuildClipIDPairs(allPaths, c), c);   

            Debug.Log("Controller built at: " + saveDirectory + "AnimationsController.controller");         
        }

        ClipIDPair[] BuildClipIDPairs (string[] allPaths, int c) {
            ClipIDPair[] results = new ClipIDPair[c];
            for (int i = 0; i < c; i++) {
                string f = allPaths[i];
                
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
            controller.AddParameter(CustomAnimator.sMirror, AnimatorControllerParameterType.Bool);
            controller.AddParameter(CustomAnimator.sSpeed, AnimatorControllerParameterType.Float);

            for (int i = 0; i < CustomAnimator.sLoopIndicies.Length; i++) {
                controller.AddParameter(CustomAnimator.sLoopIndicies[i], AnimatorControllerParameterType.Float);
                controller.AddParameter(CustomAnimator.sLoopMirrors[i], AnimatorControllerParameterType.Bool);
                controller.AddParameter(CustomAnimator.sLoopSpeeds[i], AnimatorControllerParameterType.Float);
            }
            controller.AddParameter(CustomAnimator.sActiveLoop, AnimatorControllerParameterType.Int);
        }
        void ControllerBuild (ClipIDPair[] clips, int c) {
            if (clips == null) return;
            // Create the animator in the project
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(saveDirectory + "AnimationsController.controller");
            
            //add parameters
            AddParameters(controller);

            //make blend trees for looped animations (2 to smoothly transition between loops)
            AnimatorState[] blendTrees = new AnimatorState[2];
            for (int i =0 ; i < 2; i++) blendTrees[i] = AddBlendTree(controller, 0, clips, CustomAnimator.sLoopNames[i], CustomAnimator.sLoopMirrors[i], CustomAnimator.sLoopSpeeds[i], c, CustomAnimator.sLoopIndicies[i]);            
            for (int i = 0; i < c; i++) AddState(controller, 0, clips[i], blendTrees);
        }


        void AddState (AnimatorController controller, int layer, ClipIDPair clipIDPair, AnimatorState[] blendTrees) {
            int id = clipIDPair.id;
            AnimationClip clip = clipIDPair.clip;

            AnimatorStateMachine sm = controller.layers[layer].stateMachine;
            
            // Add the state to the state machine (The Vector3 is for positioning in the editor window)
            AnimatorState state = sm.AddState(id.ToString());//, new Vector3(300, 0, 0));
            
            state.mirrorParameterActive = true;
            state.mirrorParameter = CustomAnimator.sMirror;

            state.speedParameterActive = true;
            state.speedParameter = CustomAnimator.sSpeed;
                
            state.tag = CustomAnimator.sShots;
            state.motion = clip;

            float exit_time = 1.0f - (exitTransitionDuration / clip.length);

            int c = blendTrees.Length;
            for (int i = 0; i < c; i++) {
                AnimatorStateTransition exit = state.AddTransition(blendTrees[i]);
                exit.AddCondition(AnimatorConditionMode.Equals, i, CustomAnimator.sActiveLoop);
                exit.canTransitionToSelf = false;
                exit.hasExitTime = true;
                exit.exitTime = exit_time;
                exit.hasFixedDuration = true;
                exit.duration = exitTransitionDuration;
            }
        }

        AnimatorState AddBlendTree (AnimatorController controller, int layer, ClipIDPair[] clipIDPairs, string name, string mirrorParam, string speedParam, int c, string blendParam) {
            AnimatorState state = controller.layers[layer].stateMachine.AddState(name);
            BlendTree blendTree = new BlendTree();
            AssetDatabase.AddObjectToAsset(blendTree, controller);
            blendTree.name = name;
            blendTree.hideFlags = HideFlags.HideInHierarchy;
            blendTree.blendType = BlendTreeType.Simple1D;
            blendTree.blendParameter = blendParam;
            blendTree.useAutomaticThresholds = false;
            
            for (int i = 0; i < c; i++) blendTree.AddChild(clipIDPairs[i].clip, threshold: clipIDPairs[i].id);

            state.motion = blendTree;

            state.mirrorParameterActive = true;
            state.mirrorParameter = mirrorParam;

            state.speedParameterActive = true;
            state.speedParameter = speedParam;
            
            return state;
        }
    }
}






