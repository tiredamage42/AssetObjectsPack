using UnityEngine;
using System.Collections.Generic;

namespace AssetObjectsPacks.Animations {

    /*
        playlists / cues:
            have more control over player positioning / syncing multiple events on different players

            Create a playlist:

                Add a playlist component to an empty gameobject

                add an empty gameObject child to this object for each player to be synced:

                    e.g. two characters shaking hands would be two empty gameObjects as children

                under each 'player child' add GameObjects with a Cue component attached

                each cue can hold several events
                    for example when playing an animation and audio clip for that cue
                    the first event in the array is the main one that can potentially decide when 
                    the event is done (and the cue)





            to play a playlist at runtime:
                get a reference to the playlist component ( can be a prefab )

                then call:
                    InitializePerformance(
                        new EventPlayer[] { player },  // the players to use for the playlist
                        transform.position, transform.rotation, //position and rotation to start the performance
                        OnPerformanceEndCallback //performance end callback
                    );




        players:
            play events, 

            asset object per event chosen based on parameters matching asset object conditions
            
            when multiple events played, first is in charge of deciding when the event is ending 
            (unless asset object timed)


            player then calls whichever component has subscribed to whenever an event is played
            that corresponds to the event's pack type


            to subscribe to events being played:

                get a reference to the event player, then call:

                    SubscribeToEventPlay ( { pack type name } , { callback when event is played and asset object chosen } )

                the callback supplied should have parameters:
                    
                    AssetObject assetObject :   the asset object chosen for the event,
                    
                    System.Action endEvent  :   a callback for when the assetObject is done playing
                                                if it's not null, then the component is in charge
                                                of deciding when the asset object (and event) are done

                the callback is now called whenever the player plays events of the pack type specified



        events:
        pack manager


        custom parameters

        maybe animation download?



            

                                                
                

            
    
    */

    [RequireComponent(typeof(Animator))]
    public class CustomAnimator : MonoBehaviour
    {
        //call on awake
        void InitializeEventPlayer () {
            //get the event player
            EventPlayer eventPlayer = GetComponent<EventPlayer>();

            //tell the event player to call this component's "Use Asset Object" method
            //whenever it plays an event that uses the "Animations" pack
            eventPlayer.LinkAsPlayer("Animations", UseAssetObject);
        }

        /*
            callbacks to call when one shot animation is done
        */
        HashSet<System.Action> endUseCallbacks;

        /*
            called when the attached event player plays an 
            "Animations" event and chooses an appropriate asset object

            endUseCallbacks should be called whenever the animation is done
        
            in this case, this component tracks when the animator is exiting an animation
            that has been played
        */
        void UseAssetObject(AssetObject assetObject, bool asInterrupter, HashSet<System.Action> endUseCallbacks) {
            //if (this.endUseCallbacks != null) {
            //    this.endUseCallbacks.Clear();
            //}
            this.endUseCallbacks = endUseCallbacks;

            //Debug.Log("using ao");
            

            //get asset object parameter values
            int mirrorMode = assetObject["Mirror"].GetValue<int>();  
            bool mirror = (mirrorMode == 2) ? Random.value < .5f : mirrorMode == 1;          
            float timeOffset = assetObject["TimeOffset"].GetValue<float>();
            if (timeOffset != 0) {
                timeOffset = timeOffset * ((AnimationClip)assetObject.objRef).length;
            } 
            bool looped = assetObject["Looped"].GetValue<bool>();

            Play(
                assetObject.id, 
                asInterrupter, 
                mirror, 
                looped, 
                assetObject["Transition"].GetValue<float>(), 
                assetObject["Speed"].GetValue<float>(), 
                assetObject["Layer"].GetValue<int>(), 
                timeOffset
            );

            if (looped) {
                BroadcastEndUse(); //dont leave loops hanging
            }
        }

        // let the player know the event is done
        void BroadcastEndUse () {
            //Debug.Log("end animation");
            if (endUseCallbacks != null) {
                foreach (var endUse in endUseCallbacks) {
                    Debug.Log("callign callbacks");
                    endUse();    
                }
                endUseCallbacks.Clear();
            }
        }
        

        public const string sMirror = "Mirror", sSpeed = "Speed", sActiveLoop = "ActiveLoop", sShots = "OneShots";
        public static readonly string[] sLoopNames = new string[] {"Loops0", "Loops1"};
        public static readonly string[] sLoopIndicies = new string[] {"Index0", "Index1"};
        public static readonly string[] sLoopMirrors = new string[] {"Mirror0", "Mirror1"};
        public static readonly string[] sLoopSpeeds = new string[] {"Speed0", "Speed1"};
        static readonly int pMirror = Animator.StringToHash( sMirror );
        static readonly int pSpeed = Animator.StringToHash( sSpeed);
        static readonly int pActiveLoopSet = Animator.StringToHash( sActiveLoop );
        static readonly int[] pLoopIndicies = new int[] { Animator.StringToHash( sLoopIndicies[0]), Animator.StringToHash( sLoopIndicies[1]), };
        static readonly int[] pLoopSpeeds = new int[] { Animator.StringToHash( sLoopSpeeds[0]), Animator.StringToHash( sLoopSpeeds[1]), };
        static readonly int[] pLoopMirrors = new int[] { Animator.StringToHash( sLoopMirrors[0]), Animator.StringToHash( sLoopMirrors[1]), };
        

        //custom animation component that controls a Unity Animator with a runtime animator controller
        //set up by the wizard included in this package
        Animator anim;
        int activeLoops;
        bool playingOneShot, endTransitionStartCheck;

        void Awake () {
            anim = GetComponent<Animator>();
            InitializeEventPlayer();
        }
        void Update () {
            CheckOneShotEndings();
        }

        void Play (int id, bool interrupt, bool mirror, bool loop, float transition, float speed, int layer, float timeOffset) {
            if (loop) PlayLoop(id, interrupt, mirror, transition, speed, layer, timeOffset);
            else PlayOneShot(id, mirror, transition, speed, layer, timeOffset);
        }
        void PlayLoop (int id, bool interrupt, bool mirror, float transition, float speed, int layer, float timeOffset) {
            //Debug.Log("playing loop");
            bool doTransition = (playingOneShot && interrupt) || !playingOneShot;
                
            if (doTransition) {
                activeLoops = (activeLoops + 1) % 2;
            }
            //else if we're doing it in the background of a one shot 
            //just change the parameters for the current active loopset

            anim.SetFloat(pLoopIndicies[activeLoops], id);
            anim.SetFloat(pLoopSpeeds[activeLoops], speed);
            anim.SetBool(pLoopMirrors[activeLoops], mirror);

            //if transitioning, crossfade to the new active loopset
            if (doTransition) {
                anim.CrossFadeInFixedTime(sLoopNames[activeLoops], transition, layer, timeOffset);
            }
            //set active loopset
            anim.SetInteger(pActiveLoopSet, activeLoops);
        }
            
        void PlayOneShot(int id, bool mirror, float transition, float speed, int layer, float timeOffset) {
            //set mirror ansd speed parameters
            anim.SetBool(pMirror, mirror);
            anim.SetFloat(pSpeed, speed);
            
            //non looped states are named as their ids
            anim.CrossFadeInFixedTime(id.ToString(), transition, layer, timeOffset);
            
            playingOneShot = true;
        }

        //doing it after transition end looks janky
        void OnOneShotExitTransitionStart () {
            BroadcastEndUse();
        }
        void OnOneShotTransitionEnd() {

        }


        //check when a non looped animation is starting its exit transition and ending its exit transition
        void CheckOneShotEndings (int layer = 0) {
        
            AnimatorStateInfo currentState = anim.GetCurrentAnimatorStateInfo(layer);
            AnimatorStateInfo nextState = anim.GetNextAnimatorStateInfo(layer);

            bool currentIsOneShot = currentState.IsTag(sShots);
            bool nextIsOneShot = nextState.IsTag(sShots);

            //if in transition
            if (anim.IsInTransition(layer)) {
                if (currentIsOneShot) {
                    //currently exiting a one shot
                    if (nextState.fullPathHash != currentState.fullPathHash) {
                        //make sure this only happens one frame
                        if (!endTransitionStartCheck) {
                            OnOneShotExitTransitionStart();
                            endTransitionStartCheck = true;
                        }
                    }
                }
            }
            else {
                //not in transition, but was exiting, so transition is done
                if (endTransitionStartCheck) {
                    
                    //check if we're out of a one shot animation
                    if (!nextIsOneShot) {
                        playingOneShot = false;
                    }

                    OnOneShotTransitionEnd();
                    endTransitionStartCheck = false;
                }
            }
        }
    }
}


