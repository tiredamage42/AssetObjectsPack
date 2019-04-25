using UnityEngine;
using System.Collections.Generic;

namespace AssetObjectsPacks.Animations {

    [RequireComponent(typeof(Animator))]
    public class CustomAnimator : EventPlayerListener
    {
        protected override string ListenForPackName() {
            return "Animations";
        }

        /*
            callbacks to call when one shot animation is done

            (per animator layer)
        */

        HashSet<System.Action>[] endUseCallbacksPerLayer = new HashSet<System.Action>[MAX_ANIM_LAYERS];
        // HashSet<System.Action> endUseCallbacks;



        /*
            called when the attached event player plays an 
            "Animations" event and chooses an appropriate asset object

            endUseCallbacks should be called whenever the animation is done
        
            in this case, this component tracks when the animator is exiting an animation
            that has been played
        */
        protected override void UseAssetObject(AssetObject assetObject, bool asInterrupter, MiniTransform transforms, HashSet<System.Action> endUseCallbacks) {
            bool looped = assetObject["Looped"].GetValue<bool>();
            int layer = assetObject["Layer"].GetValue<int>();
            
            if (!looped) {
                if (this.endUseCallbacksPerLayer[layer] != null && this.endUseCallbacksPerLayer[layer].Count != 0) {
                    // Debug.Log("clearing old callbacks");
                    this.endUseCallbacksPerLayer[layer].Clear();
                }
                this.endUseCallbacksPerLayer[layer] = endUseCallbacks;

                // if (this.endUseCallbacks != null && this.endUseCallbacks.Count != 0) {
                //     Debug.Log("clearing old callbacks");
                //     this.endUseCallbacks.Clear();
                // }
                // this.endUseCallbacks = endUseCallbacks;
            }
            
            //get asset object parameter values
            int mirrorMode = assetObject["Mirror"].GetValue<int>();  
            bool mirror = (mirrorMode == 2) ? Random.value < .5f : mirrorMode == 1;          
            float timeOffset = assetObject["TimeOffset"].GetValue<float>();

            AnimationClip clip = assetObject.objRef as AnimationClip;// ((AnimationClip)assetObject.objRef);

            if (timeOffset != 0) {
                if (clip != null) {
                    timeOffset = timeOffset * clip.length;
                }
                else {
                    timeOffset = 0;
                }

            } 

            Play(
                clip,
                assetObject.id, 
                asInterrupter, 
                mirror, 
                looped, //if null clip and 
                assetObject["Transition"].GetValue<float>(), 
                assetObject["Speed"].GetValue<float>(), 
                layer, 
                timeOffset
            );

            if (looped) {
                if (assetObject.objRef != null) {

                    Debug.Log("playing loop: " + assetObject.objRef.name);
                }

                //BroadcastEndUse(); //dont leave loops hanging
            }
            else {

                lastPlayed = assetObject.objRef.name;
                Debug.Log("playing: " + assetObject.objRef.name);

            }
        }
        string lastPlayed;

        // let the player know the event is done
        void BroadcastEndUse (int forLayer) {
                  //          Debug.Log("stopped anim " + lastPlayed);
                            
            // if (endUseCallbacks != null) {
                            

            //     if (endUseCallbacks.Count != 0) {
            //         //Debug.Log("call callbacks");
            //     }
            //     foreach (var endUse in endUseCallbacks) {
            //         endUse();    
            //     }
            //     endUseCallbacks.Clear();
            // }
            if (endUseCallbacksPerLayer[forLayer] != null) {
                            

                if (endUseCallbacksPerLayer[forLayer].Count != 0) {
                    //Debug.Log("call callbacks");
                }
                foreach (var endUse in endUseCallbacksPerLayer[forLayer]) {
                    endUse();    
                }
                endUseCallbacksPerLayer[forLayer].Clear();
            }
        }
        

        public const string sShots = "OneShots";
        public const int MAX_ANIM_LAYERS = 3;

        static string[] BuildLayerParameters (string baseParameter) {
            string[] r = new string[MAX_ANIM_LAYERS];
            for (int i = 0; i < MAX_ANIM_LAYERS; i++) {
                r[i] = baseParameter + "_" + i.ToString();
            }
            return r;
        }
        static string[] BuildLayerParametersLoops (string baseParameter) {
            // each layer has 3 possible loops, except base layer which has just two
            // 0, 1, 2(empty on layers above 0)
            int amount = (MAX_ANIM_LAYERS * 3);

            string[] r = new string[amount];
            int layer = 0;
            for (int i = 0; i < amount; i+=3) {
                r[i+0] = baseParameter + "0_" + layer.ToString();
                r[i+1] = baseParameter + "1_" + layer.ToString();
                r[i+2] = baseParameter + "2_" + layer.ToString();
                layer++;
            }
            return r;
        }

        static string[] BuildLayerParametersLoops2 (string baseParameter) {
            // each layer has 3 possible loops, except base layer which has just two
            // 0, 1, 2(empty on layers above 0)

            // speed, mirror and index params only need to be created for the first two though
            int amount = (MAX_ANIM_LAYERS * 2);

            string[] r = new string[amount];
            int layer = 0;
            for (int i = 0; i < amount; i+=2) {
                r[i+0] = baseParameter + "0_" + layer.ToString();
                r[i+1] = baseParameter + "1_" + layer.ToString();
                layer++;
            }
            return r;
        }

        static int[] StringToHash(string[] strings) {

            int l = strings.Length;

            int[] r = new int[l];
            for (int i = 0; i < l; i++) {
                r[i] = Animator.StringToHash( strings[i] );
            }
            return r;
        }

        public static readonly string[] mirrorParamStrings = BuildLayerParameters("Mirror");
        public static readonly string[] speedParamStrings = BuildLayerParameters("Speed");
        public static readonly string[] activeLoopParamStrings = BuildLayerParameters("ActiveLoop");
        
        public static readonly string[] loopNamesStrings = BuildLayerParametersLoops("Loops");
        public static readonly string[] loopIndexStrings = BuildLayerParametersLoops2("Index");
        public static readonly string[] loopMirrorStrings = BuildLayerParametersLoops2("Mirror");
        public static readonly string[] loopSpeedStrings = BuildLayerParametersLoops2("Speed");
        

        // public const string sMirror = "Mirror", sSpeed = "Speed", sActiveLoop = "ActiveLoop";
        // public static readonly string[] sLoopNames = new string[] {"Loops0", "Loops1"};
        // public static readonly string[] sLoopIndicies = new string[] {"Index0", "Index1"};
        // public static readonly string[] sLoopMirrors = new string[] {"Mirror0", "Mirror1"};
        // public static readonly string[] sLoopSpeeds = new string[] {"Speed0", "Speed1"};


        // static readonly int pMirror = Animator.StringToHash( sMirror );
        // static readonly int pSpeed = Animator.StringToHash( sSpeed);
        // static readonly int pActiveLoopSet = Animator.StringToHash( sActiveLoop );
        // static readonly int[] pLoopIndicies = new int[] { Animator.StringToHash( sLoopIndicies[0]), Animator.StringToHash( sLoopIndicies[1]), };
        // static readonly int[] pLoopSpeeds = new int[] { Animator.StringToHash( sLoopSpeeds[0]), Animator.StringToHash( sLoopSpeeds[1]), };
        // static readonly int[] pLoopMirrors = new int[] { Animator.StringToHash( sLoopMirrors[0]), Animator.StringToHash( sLoopMirrors[1]), };
        

        static readonly int[] pLMirror = StringToHash(mirrorParamStrings);
        static readonly int[] pLSpeed = StringToHash( speedParamStrings);
        static readonly int[] pLActiveLoopSet = StringToHash( activeLoopParamStrings );
        static readonly int[] pLLoopIndicies = StringToHash( loopIndexStrings );
        static readonly int[] pLLoopMirrors = StringToHash( loopMirrorStrings );
        static readonly int[] pLLoopSpeeds = StringToHash( loopSpeedStrings );
        


        //custom animation component that controls a Unity Animator with a runtime animator controller
        //set up by the wizard included in this package
        Animator anim;

        // int activeLoops;
        int[] activeLayerLoops = new int[MAX_ANIM_LAYERS];


        //bool playingOneShot, endTransitionStartCheck;
        bool[] playingOneShots = new bool[MAX_ANIM_LAYERS], endTransitionStartChecks = new bool[MAX_ANIM_LAYERS];

        protected override void Awake () {
            base.Awake();
            anim = GetComponent<Animator>();
            
        }
        void LateUpdate () {
            CheckOneShotEndings();
        }

        void Play (AnimationClip clip, int id, bool interrupt, bool mirror, bool loop, float transition, float speed, int layer, float timeOffset) {
            if (loop || (clip == null && layer > 0)) PlayLoop(clip, id, interrupt, mirror, transition, speed, layer, timeOffset);
            else PlayOneShot(id, mirror, transition, speed, layer, timeOffset);
        }
        void PlayLoop (AnimationClip clip, int id, bool interrupt, bool mirror, float transition, float speed, int layer, float timeOffset) {
            bool goToBlankState = clip == null && layer > 0;

            if (goToBlankState){
                Debug.Log("playing blank state layre" + layer);
            }

            
            bool doTransition = (playingOneShots[layer] && interrupt) || !playingOneShots[layer];
            // bool doTransition = (playingOneShot && interrupt) || !playingOneShot;
            
            if (goToBlankState) {

                doTransition = activeLayerLoops[layer] != 0;
                activeLayerLoops[layer] = 0;

            }
            else {

                if (doTransition) {
                    if (!goToBlankState) {

                        if (layer > 0) {
                            activeLayerLoops[layer] = activeLayerLoops[layer] == 1 ? 2 : 1;
                        }
                        else {
                            activeLayerLoops[layer] = activeLayerLoops[layer] == 1 ? 0 : 1;
                        }
                    }
                }
            }


            int activeLoop = activeLayerLoops[layer];



            // if (doTransition) {
            //     activeLoops = (activeLoops + 1) % 2;
            // }
            
            //else if we're doing it in the background of a one shot 
            //just change the parameters for the current active loopset


            if (!goToBlankState) {

                int activeSet = activeLoop;
                if (layer > 0) {
                    activeSet -= 1;
                }
                //anim.SetFloat(pLoopIndicies[activeLoops], id);
                anim.SetFloat(pLLoopIndicies[2*layer + activeSet], id);
                
                anim.SetFloat(pLLoopSpeeds[2*layer + activeSet], speed);
                anim.SetBool(pLLoopMirrors[2*layer + activeSet], mirror);

                // anim.SetFloat(pLoopSpeeds[activeLoops], speed);
                // anim.SetBool(pLoopMirrors[activeLoops], mirror);
            }

            //if transitioning, crossfade to the new active loopset
            if (doTransition) {

                Debug.LogError("playing loop transition on layer " + layer);
                string loopName = loopNamesStrings[3*layer + activeLoop];
                // anim.CrossFadeInFixedTime(sLoopNames[activeLoops], transition, layer, timeOffset);
                anim.CrossFadeInFixedTime(loopName, transition, layer, timeOffset);
            }
            //set active loopset
            anim.SetInteger(pLActiveLoopSet[layer], activeLayerLoops[layer]);



        }
            
        void PlayOneShot(int id, bool mirror, float transition, float speed, int layer, float timeOffset) {
            
            Debug.Log("playing one shot layer: " + layer + " trans:: " + transition);
            
            //set mirror ansd speed parameters
            anim.SetBool(pLMirror[layer], mirror);
            anim.SetFloat(pLSpeed[layer], speed);

            // if (timeOffset != 0) {
            //     Debug.Log("timeoffset " + timeOffset);
            // }
            
            //non looped states are named as their ids
            anim.CrossFadeInFixedTime(id.ToString(), transition, layer, timeOffset);
            
            playingOneShots[layer] = true;
            lastOneShotPlayTimes[layer] = Time.time;
            lastOneshotTransitionTimes[layer] = transition;
            endTransitionStartChecks[layer] = false;
        }
        //float lastOneShotPlayTime, lastOneshotTransitionTime;
        float[] lastOneShotPlayTimes = new float[MAX_ANIM_LAYERS], lastOneshotTransitionTimes = new float[MAX_ANIM_LAYERS];

        //doing it after transition end looks janky
        void OnOneShotExitTransitionStart (int layer) {
            BroadcastEndUse(layer);
        }
        void OnOneShotTransitionEnd(int layer) {

        }

        //check when a non looped animation is starting and ending its exit transition
        void CheckOneShotEndings (int layer = 0) {
            if (!playingOneShots[layer]) return;
            //if (!playingOneShot) return;
        
            AnimatorStateInfo nextState = anim.GetNextAnimatorStateInfo(layer);

            bool nextIsOneShot = nextState.IsTag(sShots);

            bool introTransition = Time.time - lastOneShotPlayTimes[layer] <= lastOneshotTransitionTimes[layer] + .1f;
            if (introTransition) return;

            //if in transition
            if (anim.IsInTransition(layer)){
                if (!endTransitionStartChecks[layer]){
                    OnOneShotExitTransitionStart(layer);
                    endTransitionStartChecks[layer] = true;
                }
            }
            else {
            
                //not in transition, but was exiting, so transition is done
                if (endTransitionStartChecks[layer]) {
                    //check if we're out of a one shot animation
                    if (!nextIsOneShot) {
                        playingOneShots[layer] = false;
                    }
                    OnOneShotTransitionEnd(layer);

                    endTransitionStartChecks[layer] = false;
                }
            }
        }
    }
}


