using UnityEngine;

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
            eventPlayer.SubscribeToEventPlay("Animations", UseAssetObject);
        }

        /*
            when this component is in charge of determining the end of the event played
            (when it's the main event on a cue, and the asset object is not timed)
            it has to call the supplied callback to let the player know the event has ended

            in htis case, when the animation is done
        */
        System.Action endEvent;


        /*
            called when the attached event player plays an 
            "Animations" event and chooses an appropriate asset object
        
            if the endEvent callback is not null, then this component 
            is in charge of deciding when the 'event' is done.  
        
            in this case, this component tracks when the animator is exiting an animation
            that has been played
        */
        void UseAssetObject(AssetObject assetObject, System.Action endEvent) {
            this.endEvent = endEvent;

            //get asset object parameter values
            int animID = assetObject.id;
            float transition = assetObject["Transition"].FloatValue;
            float speed = assetObject["Speed"].FloatValue;
            bool looped = assetObject["Looped"].BoolValue;
            int mirror = assetObject["Mirror"].IntValue;

            
            Play(animID, false, (mirror == 2) ? Random.value < .5f : mirror == 1, looped, transition, speed);
        }

        // let the player know the event is done if this component is keeping track of that
        void TriggerEndEvent () {
            if (endEvent == null) return;
            endEvent();
            endEvent = null;
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
        bool playingOneShot, endCheck;

        void Awake () {
            anim = GetComponent<Animator>();
            anim.applyRootMotion = true;
            InitializeEventPlayer();
        }
        void Update () {
            CheckOneShotEndings();
        }

        //doing it after transition end looks janky
        void OnOneShotExitTransitionStart () {
            TriggerEndEvent();
        }
        void OnOneShotTransitionEnd() {

        }

    
        void Play (int id, bool interrupt, bool mirror, bool loop, float transition, float speed) {
            if (loop) PlayLoop(id, interrupt, mirror, transition, speed);
            else PlayOneShot(id, mirror, transition, speed);
        }
        void PlayLoop (int id, bool interrupt, bool mirror, float transition, float speed, int layer = 0) {

            bool do_transition = (playingOneShot && interrupt) || !playingOneShot;
            
            if (do_transition) activeLoops = (activeLoops + 1) % 2;
            //if we're doing it in the background of a one shot just change the current active loopset

            //Debug.Log("playing loop at loopset: " + activeLoops);
            anim.SetFloat(pLoopIndicies[activeLoops], id);
            anim.SetFloat(pLoopSpeeds[activeLoops], speed);
            anim.SetBool(pLoopMirrors[activeLoops], mirror);

            if (do_transition) anim.CrossFadeInFixedTime(sLoopNames[activeLoops], transition, layer);
            
            anim.SetInteger(pActiveLoopSet, activeLoops);
        }
            

        void PlayOneShot(int id, bool mirror, float transition, float speed, int layer = 0) {

            anim.SetBool(pMirror, mirror);
            anim.SetFloat(pSpeed, speed);

            anim.CrossFadeInFixedTime(id.ToString(), transition, layer);

            playingOneShot = true;
        }

        //check when a non looped animation is starting its exit transition and ending its exit transition
        void CheckOneShotEndings (int layer = 0) {
        
            AnimatorStateInfo current_state = anim.GetCurrentAnimatorStateInfo(layer);
            AnimatorStateInfo next_state = anim.GetNextAnimatorStateInfo(layer);

            bool current_is_shot = current_state.IsTag(sShots);
            bool next_is_one_shot = next_state.IsTag(sShots);

            if (anim.IsInTransition(layer)) {
                if (current_is_shot) {
                    if (next_state.fullPathHash != current_state.fullPathHash) {
                        if (!endCheck) {
                            endCheck = true;
                            OnOneShotExitTransitionStart();
                        }
                    }
                }
            }
            else {
                if (endCheck) {
                    endCheck = false;
                    if (!next_is_one_shot) playingOneShot = false;
                    OnOneShotTransitionEnd();
                }
            }
        }
    }
}


