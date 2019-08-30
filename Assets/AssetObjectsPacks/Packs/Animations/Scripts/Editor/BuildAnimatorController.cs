using UnityEngine;
using UnityEditor.Animations;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;


/*


REBUILD ANIMATOR AFTER ANY CHANGES TO 
    
    LOOP, LAYER

PARAMETER CHANGES

*/

namespace AssetObjectsPacks.Animations {
    struct ClipIDPair {
        public AnimationClip clip;
        public int id, layer;
        public bool looped;
        public ClipIDPair (AnimationClip clip, int id, int layer, bool looped) {
            this.clip = clip;
            this.id = id;
            this.layer = layer;
            this.looped = looped;
        }
    }
    public class AnimatorControllerBuilder : ScriptableWizard
    {
        [MenuItem("Animations/Generate Anim Controller")]
        static void CreateWizard() {
            ScriptableWizard.DisplayWizard<AnimatorControllerBuilder>("Generate Anim Controller", "Generate");
        }
        void OnWizardCreate() {
            PacksManager packs = PacksManager.instance;// AssetObjectsEditor.packManager;
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

        // [Tooltip("Use the Asset Object's 'Layer' to place it on a controller layer.\n(use if you're not planning on changing the ao's layers)")]
        // public bool staticLayers = true;

        // public bool upperLayersOnlyLoops = true;

        // public Event[] useEvents;
        public float exitTransitionDuration = .25f;
        public string animationsPackName = "Animations";
        public string saveDirectory = "Assets/";
            
        //only add used animations to controller, to avoid adding thousands of states


        HashSet<int>[] usedLoopIDsPerLayer = new HashSet<int>[CustomAnimator.MAX_ANIM_LAYERS];
        HashSet<int>[] usedShotIDsPerLayer = new HashSet<int>[CustomAnimator.MAX_ANIM_LAYERS];

        int maxLayer = 0;

        void InitializeUsedLists () {
            for (int i = 0; i < CustomAnimator.MAX_ANIM_LAYERS; i++) {
                usedLoopIDsPerLayer[i] = new HashSet<int>();
                usedShotIDsPerLayer[i] = new HashSet<int>();   
            }
        }
        
        void GetAOs(EventState state, List<AssetObject> aos){//, int packID) {
            for (int i = 0; i < state.assetObjects.Length; i++) {
                AssetObject ao = state.assetObjects[i];

                // if (ao.packID != packID)  continue;
                bool isLooped = ao["Looped"].GetValue<bool>();
                int layer = ao["Layer"].GetValue<int>();

                if (layer > maxLayer) {
                    maxLayer = layer;
                }

                if (isLooped) {
                    if (!usedLoopIDsPerLayer[layer].Contains(ao.id)) {
                        usedLoopIDsPerLayer[layer].Add(ao.id);
                        aos.Add(ao);
                    }
                }
                else {
                    if (!usedShotIDsPerLayer[layer].Contains(ao.id)) {
                        usedShotIDsPerLayer[layer].Add(ao.id);
                        aos.Add(ao);
                    }
                }
            }
        }

        IEnumerable<AssetObject> GetAllUsedAssetObjects (int packID) {
            List<AssetObject> used = new List<AssetObject>();
            
            IEnumerable<Event> allEvents = EditorUtils.GetAllAssetsOfType<Event>();
            
            foreach (var e in allEvents) {
                if (e.mainPackID == packID) {

                    for (int i = 0; i < e.allStates.Length; i++) {
                        GetAOs(e.allStates[i], used);//, packID);
                    }
                }
            }
            if (used.Count == 0) {
                Debug.LogWarning("no IDs used for " + PacksManager.ID2Name(packID) + " pack!");
            }
            return used;
        }
        

        void BuildAnimatorController (AssetObjectPack animationPack) {

            InitializeUsedLists();



            float t = Time.realtimeSinceStartup;

            // Dictionary<int, string> id2File;
            // string[] allPaths = AssetObjectsEditor.GetAllAssetObjectPaths(animationPack.dir, animationPack.extensions, true, out id2File);
            
            //filter out unused file paths
            // IEnumerable<int> used_ids;
            IEnumerable<AssetObject> usedAOs = GetAllUsedAssetObjects(PacksManager.Name2ID(animationsPackName));//, out used_ids);
            // if (used_ids.Count() == 0) return;
            // allPaths = allPaths.Where(f => used_ids.Contains(AssetObjectsEditor.GetObjectIDFromPath(f))).ToArray();

            

            Debug.Log("Time getting paths: "+(Time.realtimeSinceStartup - t));
            //t = Time.realtimeSinceStartup;


            HashSet<ClipIDPair> clipIDPairs = BuildClipIDPairs(usedAOs);//, id2File);
            
            
            // int c = allPaths.Length;
            ControllerBuild(clipIDPairs);//, c);   

            Debug.Log("Controller built at: " + saveDirectory + "AnimationsController.controller");         
        }

        HashSet<ClipIDPair> BuildClipIDPairs (IEnumerable<AssetObject> usedAOs)///, Dictionary<int, string> id2File){//string[] allPaths, int c) {
        {
            // int c = usedAOs.Count();

            // ClipIDPair[] results = new ClipIDPair[c];

            HashSet<ClipIDPair> results = new HashSet<ClipIDPair>();
            
            foreach (var usedAO in usedAOs) 
            // for (int i = 0; i < c; i++)
            {
                // string f = allPaths[i];

                // int id = usedAO.id;
                
                AnimationClip clip = usedAO.objRef as AnimationClip;// EditorUtils.GetAssetAtPath<AnimationClip>(f);
                if (clip == null) {
                    continue;
                    // Debug.LogError("Clip is null: " + f);
                    // return null;
                }
                results.Add(new ClipIDPair(clip, usedAO.id, usedAO["Layer"].GetValue<int>(), usedAO["Looped"].GetValue<bool>()));
                // results[i] = new ClipIDPair(clip, AssetObjectsEditor.GetObjectIDFromPath(f));
            }
            return results;
        }
        void AddParameters (AnimatorController controller) {

            for (int i = 0; i < CustomAnimator.mirrorParamStrings.Length; i++) {
                controller.AddParameter(CustomAnimator.mirrorParamStrings[i], AnimatorControllerParameterType.Bool);
            }
            for (int i = 0; i < CustomAnimator.speedParamStrings.Length; i++) {
                controller.AddParameter(CustomAnimator.speedParamStrings[i], AnimatorControllerParameterType.Float);
            }


            // controller.AddParameter(CustomAnimator.sMirror, AnimatorControllerParameterType.Bool);
            // controller.AddParameter(CustomAnimator.sSpeed, AnimatorControllerParameterType.Float);

            // for (int i = 0; i < CustomAnimator.sLoopIndicies.Length; i++) {
            //     controller.AddParameter(CustomAnimator.sLoopIndicies[i], AnimatorControllerParameterType.Float);
            //     controller.AddParameter(CustomAnimator.sLoopMirrors[i], AnimatorControllerParameterType.Bool);
            //     controller.AddParameter(CustomAnimator.sLoopSpeeds[i], AnimatorControllerParameterType.Float);
            // }

            for (int i = 0; i < CustomAnimator.loopIndexStrings.Length; i++) {
                controller.AddParameter(CustomAnimator.loopIndexStrings[i], AnimatorControllerParameterType.Float);
            }
            for (int i = 0; i < CustomAnimator.loopMirrorStrings.Length; i++) {
                controller.AddParameter(CustomAnimator.loopMirrorStrings[i], AnimatorControllerParameterType.Bool);
            }
            for (int i = 0; i < CustomAnimator.loopSpeedStrings.Length; i++) {
                controller.AddParameter(CustomAnimator.loopSpeedStrings[i], AnimatorControllerParameterType.Float);
            }



            for (int i = 0; i < CustomAnimator.activeLoopParamStrings.Length; i++) {
                controller.AddParameter(CustomAnimator.activeLoopParamStrings[i], AnimatorControllerParameterType.Int);
            }


            // controller.AddParameter(CustomAnimator.sActiveLoop, AnimatorControllerParameterType.Int);
        }


        void ControllerBuild (HashSet<ClipIDPair> clips)//, int c) {
        {
            //if (clips == null) return;
            // Create the animator in the project
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(saveDirectory + "AnimationsController.controller");
            
            //add parameters
            AddParameters(controller);

            float t = Time.realtimeSinceStartup;

            {
                AnimatorControllerLayer[] layers = controller.layers;
                
                AnimatorControllerLayer layer = layers[0];
                layer.iKPass = true;


                AnimatorStateMachine sm = layer.stateMachine;
                
                AnimatorState[] blendTrees = new AnimatorState[0];
                if (usedLoopIDsPerLayer[0].Count > 0) {

                    //make blend trees for looped animations (2 to smoothly transition between loops)
                    blendTrees = new AnimatorState[2];//2];
                    for (int i =0 ; i < 2; i++) 
                        blendTrees[i] = AddBlendTree(
                            controller, sm, 0, clips, 
                            CustomAnimator.loopNamesStrings[i], 
                            CustomAnimator.loopMirrorStrings[i], 
                            CustomAnimator.loopSpeedStrings[i], 
                            CustomAnimator.loopIndexStrings[i]
                        );            
                        // blendTrees[i] = AddBlendTree(controller, 0, clips, CustomAnimator.sLoopNames[i], CustomAnimator.sLoopMirrors[i], CustomAnimator.sLoopSpeeds[i], c, CustomAnimator.sLoopIndicies[i]);            
                    // ConnectBlendTrees(0, blendTrees, 0);
                }
            
                // for (int i = 0; i < c; i++) 
                //     AddState(controller, sm, 0, clips[i], blendTrees);

                foreach (var c in clips)
                    AddState(0, controller, sm, 0, c, blendTrees);

                controller.layers = layers;
            }



            Debug.Log("Time building layer 0: "+(Time.realtimeSinceStartup - t));
            t = Time.realtimeSinceStartup;
            

            for (int l = 1; l < maxLayer+1; l++) {
                controller.AddLayer("Layer"+l);

                AnimatorControllerLayer[] layers = controller.layers;
                

                AnimatorControllerLayer layer = layers[l];



                layer.iKPass = true;
                layer.defaultWeight = 1.0f;

                AnimatorStateMachine sm = layer.stateMachine;

                bool anyLoops = usedLoopIDsPerLayer[l].Count > 0;


                AnimatorState[] blendTrees = new AnimatorState[anyLoops ? CustomAnimator.BLEND_TREES_PER_LAYER + 1 : 1];
                
                // make the empty default state
                // AnimatorState state = sm.AddState(CustomAnimator.loopNamesStrings[l*3 + 0]);
                blendTrees[0] = sm.AddState("Blank");//CustomAnimator.loopNamesStrings[l*3 + 0]);
                blendTrees[0].motion = null;
                
                if (anyLoops) {

                    for (int i =0 ; i < CustomAnimator.BLEND_TREES_PER_LAYER; i++) 
                        blendTrees[i+1] = AddBlendTree(
                            controller, sm, l, clips, 
                            CustomAnimator.loopNamesStrings[l*CustomAnimator.BLEND_TREES_PER_LAYER + (i)],


                            CustomAnimator.loopMirrorStrings[l*CustomAnimator.BLEND_TREES_PER_LAYER + (i)], 
                            CustomAnimator.loopSpeedStrings[l*CustomAnimator.BLEND_TREES_PER_LAYER + (i)], 
                            //c, 
                            CustomAnimator.loopIndexStrings[l*CustomAnimator.BLEND_TREES_PER_LAYER + (i)]
                        );     

                    // ConnectBlendTrees(-1, blendTrees, l);   
                }



                // if (!upperLayersOnlyLoops) {

                // }    


                // for (int i = 0; i < c; i++) 
                //     AddState(controller, sm, l, clips[i], blendTrees);


                foreach (var c in clips)
                    AddState(-1, controller, sm, l, c, blendTrees);

                
                

                controller.layers = layers;

            }

            EditorUtility.SetDirty(controller);

            Debug.Log("Time building other layers: "+(Time.realtimeSinceStartup - t));
            


        }


        void ConnectBlendTrees (int offset, AnimatorState[] blendTrees, int layer) {

            int c = blendTrees.Length;
            for (int i = 0; i < c; i++) {

                for (int j = 0; j < c; j++) {
                    if (i == j) {
                        continue;
                    }
                    AnimatorStateTransition exit = blendTrees[i].AddTransition(blendTrees[j]);
                    //exit.AddCondition(AnimatorConditionMode.Equals, i, CustomAnimator.sActiveLoop);
                    exit.AddCondition(AnimatorConditionMode.Equals, j + offset, CustomAnimator.activeLoopParamStrings[layer]);
                    
                    exit.canTransitionToSelf = false;// false;// true;
                    exit.interruptionSource = TransitionInterruptionSource.None;// TransitionInterruptionSource.Destination;
                    exit.hasExitTime = false;
                    // exit.exitTime = exit_time;
                    exit.hasFixedDuration = true;
                    exit.duration = exitTransitionDuration;
                    
                }
            }
        }




    


        void AddState (int indexBlendTreeOffset, AnimatorController controller, AnimatorStateMachine sm, int layer, ClipIDPair clipIDPair, AnimatorState[] blendTrees) {
            if (clipIDPair.layer != layer) {
                return;
            }
            if (clipIDPair.looped){
                return;
            }
            
            
            int id = clipIDPair.id;
            AnimationClip clip = clipIDPair.clip;

            // Add the state to the state machine (The Vector3 is for positioning in the editor window)
            AnimatorState state = sm.AddState(id.ToString());//, new Vector3(300, 0, 0));
            
            state.mirrorParameterActive = true;
            state.speedParameterActive = true;

            state.mirrorParameter = CustomAnimator.mirrorParamStrings[layer];
            state.speedParameter = CustomAnimator.speedParamStrings[layer];
            // state.mirrorParameter = CustomAnimator.sMirror;
            // state.speedParameter = CustomAnimator.sSpeed;
            
                
            state.tag = CustomAnimator.sShots;
            state.motion = clip;

            float exit_time = 1.0f - (exitTransitionDuration / clip.length);

            int c = blendTrees.Length;
            for (int i = 0; i < c; i++) {
                AnimatorStateTransition exit = state.AddTransition(blendTrees[i]);
                //exit.AddCondition(AnimatorConditionMode.Equals, i, CustomAnimator.sActiveLoop);
                exit.AddCondition(AnimatorConditionMode.Equals, i + indexBlendTreeOffset, CustomAnimator.activeLoopParamStrings[layer]);
                
                exit.canTransitionToSelf = false;
                exit.hasExitTime = true;
                exit.exitTime = exit_time;
                exit.hasFixedDuration = true;
                exit.duration = exitTransitionDuration;
            }
        }

        AnimatorState AddBlendTree (AnimatorController controller, AnimatorStateMachine sm, int layer, HashSet<ClipIDPair> clipIDPairs, string name, string mirrorParam, string speedParam, string blendParam) {
            AnimatorState state = sm.AddState(name);
            BlendTree blendTree = new BlendTree();
            AssetDatabase.AddObjectToAsset(blendTree, controller);
            blendTree.name = name;
            blendTree.hideFlags = HideFlags.HideInHierarchy;
            blendTree.blendType = BlendTreeType.Simple1D;
            blendTree.blendParameter = blendParam;
            blendTree.useAutomaticThresholds = false;


            foreach (var c in clipIDPairs) {
                if (c.layer != layer) {
                    continue;
                }
                if (!c.looped){
                    continue;
                }
                
                blendTree.AddChild(c.clip, threshold: c.id);
            }
            
            // for (int i = 0; i < c; i++) blendTree.AddChild(clipIDPairs[i].clip, threshold: clipIDPairs[i].id);

            state.motion = blendTree;

            state.mirrorParameterActive = true;
            state.mirrorParameter = mirrorParam;

            state.speedParameterActive = true;
            state.speedParameter = speedParam;
            
            return state;
        }
    }
}






