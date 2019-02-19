using UnityEngine;
using System;

namespace AssetObjectsPacks.Animations {

    [RequireComponent(typeof(Animator))]
    public class CustomAnimator : MonoBehaviour
    {
        public static readonly string[] sLoop_Names = new string[] {"Loops_0", "Loops_1"};
        public static readonly string[] sLoop_Indicies = new string[] {"LoopIndex_0", "LoopIndex_1"};
        public static readonly string[] sLoop_Mirrors = new string[] {"p_LoopMirror_0", "p_LoopMirror_1"};
        public static readonly string[] sLoop_Speeds = new string[] {"p_LoopSpeed_0", "p_LoopSpeed_1"};
        public const string sMirror = "Mirror";
        public const string sSpeed = "Speed";
        public const string sActiveLoopSet = "ActiveLoopSet";
        public const string sShots = "OneShots";

        static readonly int p_Mirror = Animator.StringToHash( sMirror);
        static readonly int p_Speed = Animator.StringToHash( sSpeed);
        static readonly int p_ActiveLoopSet = Animator.StringToHash( sActiveLoopSet);
        static readonly int[] p_LoopIndicies = new int[] {
            Animator.StringToHash( sLoop_Indicies[0]), 
            Animator.StringToHash( sLoop_Indicies[1]), 
        };
        static readonly int[] p_LoopSpeeds = new int[] {
            Animator.StringToHash( sLoop_Speeds[0]), 
            Animator.StringToHash( sLoop_Speeds[1]), 
        };
        static readonly int[] p_LoopMirrors = new int[] {
            Animator.StringToHash( sLoop_Mirrors[0]), 
            Animator.StringToHash( sLoop_Mirrors[1]), 
        };
        
    
        AssetObjectEventPlayer event_player;
        void InitializeEventPlayer () {
            event_player = GetComponent<AssetObjectEventPlayer>();
            event_player.SubscribeToEventVariation("Animations", OnPlayEvent);
        }


        void OnPlayEvent(AssetObject assetObject, System.Action end_event) {
            this.end_event = end_event;
            bool mirror = false;
            int mirror_mode = assetObject["Mirror"].IntValue;
            float trans_speed = assetObject["Transition"].FloatValue;
            float speed = assetObject["Speed"].FloatValue;
            bool looped = assetObject["Looped"].BoolValue;

            if (mirror_mode == 2) {
                mirror = UnityEngine.Random.value < .5f;
            }
            else if (mirror_mode == 1) {
                mirror = true;
            }
            Play(assetObject.id, false, mirror, looped, trans_speed, speed);
        }



        Action end_event;

        void OnOneShotTransitionStart () {
            if (end_event != null) {
                Debug.Log("on one shot transition start");
                end_event();
                end_event = null;
            }
        }


        
        public RuntimeAnimatorController animatorController;
        Animator anim;
        int active_loopset;
        bool playing_one_shot;
        bool end_check, was_in_transition;

        void Awake () {
            anim = GetComponent<Animator>();
            //anim.avatar = transform.GetChild(0).GetComponent<Animator>().avatar;
            anim.runtimeAnimatorController = animatorController;
            anim.applyRootMotion = true;
            InitializeEventPlayer();
        }


        void Update () {

            bool next_is_one_shot;
            if (OneShotEnded(out next_is_one_shot)) {
                if (!next_is_one_shot) {
                    playing_one_shot = false;
                }
                //OnOneShotTransitionEnd();
            }
        }

        bool OneShotEnded (out bool next_is_one_shot, int layer = 0) {
        
            AnimatorStateInfo current_state = anim.GetCurrentAnimatorStateInfo(layer);
            AnimatorStateInfo next_state = anim.GetNextAnimatorStateInfo(layer);

            bool current_is_shot = current_state.IsTag(sShots);
            next_is_one_shot = next_state.IsTag(sShots);

            if (anim.IsInTransition(layer)) {
                if (current_is_shot) {
                    if (next_state.fullPathHash != current_state.fullPathHash) {
                        if (!end_check) {
                            end_check = true;
                            OnOneShotTransitionStart();
                        }

                    }
                }
                return false;
            }
            else {
                if (end_check) {
                    end_check = false;
                    return true;
                }
                return false;
            }
        }


        public void Play (int anim_index, bool interrupt_current, bool mirror, bool loop, float transition_time, float speed) {
            if (loop) {
                PlayLoop(anim_index, interrupt_current, mirror, transition_time, speed);
            }
            else {
                PlayAnimation(anim_index, mirror, transition_time, speed);
            }
        }
        
        //public void InterruptAnimation () {
        //    Debug.Log("interupting animation");
        //    anim.SetTrigger(p_AnimExit);
        //}
    
        void PlayLoop (int anim_index, bool interrupt_current, bool mirror, float transition_time, float speed) {
            int layer = 0;

            bool do_transition = (playing_one_shot && interrupt_current) || !playing_one_shot;

            if (do_transition) active_loopset = (active_loopset + 1) % 2;
            
            //if we're doing it in the background of a one shot just change the current active loopset

            //Debug.Log("playing loop at loopset: " + active_loopset);
            anim.SetFloat(p_LoopIndicies[active_loopset], anim_index);
            anim.SetFloat(p_LoopSpeeds[active_loopset], speed);
            anim.SetBool(p_LoopMirrors[active_loopset], mirror);

            if (do_transition) {
                //InterruptAnimation();
                anim.CrossFadeInFixedTime(sLoop_Names[active_loopset], transition_time, layer);
            }
            anim.SetInteger(p_ActiveLoopSet, active_loopset);
            
        }

        void PlayAnimation(int anim_index, bool mirror, float transition_time, float speed) {

            anim.SetBool(p_Mirror, mirror);
            anim.SetFloat(p_Speed, speed);

            int layer = 0;
            anim.CrossFadeInFixedTime(anim_index.ToString(), transition_time, layer);

            playing_one_shot = true;
            
        }
    }
}


