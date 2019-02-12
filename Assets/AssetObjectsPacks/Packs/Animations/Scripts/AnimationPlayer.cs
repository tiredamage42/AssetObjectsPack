

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AssetObjectsPacks;

namespace AssetObjectsPacks.Animations {

    [RequireComponent(typeof(Animator))]
    public class AnimationPlayer : MonoBehaviour
    {

        public AssetObjectEventPlayer event_player;

        void InitializeEventPlayer () {
            event_player = GetComponent<AssetObjectEventPlayer>();
            event_player.SubscribeToEventVariation("Animations", OnPlayEvent);
        }

        void OnPlayEvent(AssetObject assetObject, System.Action end_event) {
            this.end_event = end_event;

            bool mirror = false;
            int mirror_mode = assetObject["Mirror"].intValue;
            float trans_speed = assetObject["Transition Time"].floatValue;
            float speed = assetObject["Speed"].floatValue;
            bool looped = assetObject["Looped"].boolValue;

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


        static readonly int p_Mirror = Animator.StringToHash( AnimationsPack.p_Mirror);
        static readonly int p_Speed = Animator.StringToHash( AnimationsPack.p_Speed);
        static readonly int p_ActiveLoopSet = Animator.StringToHash( AnimationsPack.p_ActiveLoopSet);
        static readonly int[] p_LoopIndicies = new int[] {
            Animator.StringToHash( AnimationsPack.p_Loop_Indicies[0]), 
            Animator.StringToHash( AnimationsPack.p_Loop_Indicies[1]), 
        };
        static readonly int[] p_LoopSpeeds = new int[] {
            Animator.StringToHash( AnimationsPack.p_Loop_Speeds[0]), 
            Animator.StringToHash( AnimationsPack.p_Loop_Speeds[1]), 
        };
        static readonly int[] p_LoopMirrors = new int[] {
            Animator.StringToHash( AnimationsPack.p_Loop_Mirrors[0]), 
            Animator.StringToHash( AnimationsPack.p_Loop_Mirrors[1]), 
        };
        /*
         */
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

        //Vector3 pos_movement;
        //Quaternion rot_movement;
        //void OnAnimatorMove () {
        //    pos_movement = anim.rootPosition;
        //    rot_movement = anim.rootRotation;
        //}
        /*
        void OnAnimatorMove()
 {
     _agent.velocity = _anim.deltaPosition / Time.deltaTime;
 }
        
         */

        //void FixedUpdate () {
            //transform.rotation = rot_movement;
        //}

        

        void Update () {
            //transform.position = pos_movement;



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

            bool current_is_shot = current_state.IsTag(AnimationsPack.shots_name);
            next_is_one_shot = next_state.IsTag(AnimationsPack.shots_name);

            bool in_trans = anim.IsInTransition(layer);
            if (in_trans) {
                
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

            if (do_transition) {

                Debug.Log("doing cross fade: " + anim_index);


            }
            else{
                Debug.Log("playing loop: " + anim_index);

            

            }
                
            if (do_transition) {
                active_loopset = (active_loopset + 1) % 2;
            }
            //if we're doing it in the background of a one shot just change the current active loopset

            Debug.Log("playing loop at loopset: " + active_loopset);
            anim.SetFloat(p_LoopIndicies[active_loopset], anim_index);
            anim.SetFloat(p_LoopSpeeds[active_loopset], speed);
            anim.SetBool(p_LoopMirrors[active_loopset], mirror);

            
            if (do_transition) {
                string active_loop = "Loops_" + active_loopset.ToString();
                Debug.Log(active_loop +  " active loop cross fade time");
                //InterruptAnimation();
                anim.CrossFadeInFixedTime(
                    active_loop,
                    transition_time, 
                    layer
                );
            }
            anim.SetInteger(p_ActiveLoopSet, active_loopset);
            
        }

        void PlayAnimation(int anim_index, bool mirror, float transition_time, float speed) {
            Debug.Log("playing anim " + anim_index.ToString());



            anim.SetBool(p_Mirror, mirror);
            anim.SetFloat(p_Speed, speed);

            int layer = 0;
            anim.CrossFadeInFixedTime(anim_index.ToString(), transition_time, layer);



            playing_one_shot = true;
            
        }
    }
}


