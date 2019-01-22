

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AssetObjectsPacks;

namespace AssetObjectsPacks.Animations {

    [RequireComponent(typeof(Animator))]
    public class AnimationPlayer : AssetObjectEventPlayer
    <AnimationAssetObject, AnimationEvent, AnimationEventBehavior, AnimationPlayer>
    {

        //protected override void OnUseAssetObject(AnimInstance asset_object) {
        //    Play(asset_object, false, false);
        //}


        float duration_timer, current_duration;
        Action on_end_use;
        bool in_use;


        protected override void OnUseAssetObjectHolder(AnimationEvent asset_object_holder, AnimationAssetObject chosen_obj, Action on_end_use) {
            this.current_duration = asset_object_holder.duration;
            this.on_end_use = on_end_use;
            in_use = true;
            duration_timer = 0;

            //if (current_duration < 0) {
                //on_one_shot_exit_transition_start += on_end_use;
            //}
            //else {
            //}

                    
            Play(chosen_obj, asset_object_holder.looped, false);
        }
        void UpdateUse () {
            if (!in_use || current_duration < 0) {
                return;
            }
            duration_timer += Time.deltaTime;
            if (duration_timer >= current_duration) {
                Debug.Log("end use after " + current_duration + " (s)");
                EndUse();
            }
        }
        void EndUse () {
            if (on_end_use != null) {
                //Debug.Log("On end use anim");
                on_end_use();
                on_end_use = null;
            }
            in_use = false;
        }

        void OnOneShotTransitionStart () {
            if (on_one_shot_exit_transition_start != null) {
                Debug.Log("on one shot transition_out");
                on_one_shot_exit_transition_start();
            }
            if (in_use && current_duration < 0) {
                Debug.Log("On end use anim");
                EndUse();
            }
        }
        /*
        void OnOneShotTransitionEnd () {
            if (on_one_shot_exit_transition_end != null) {
                Debug.Log("on one shot ended");
                on_one_shot_exit_transition_end();
            }
        }
        */




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
        public RuntimeAnimatorController animatorController;
        
        event Action on_one_shot_exit_transition_start;//, on_one_shot_exit_transition_end;
        //[HideInInspector] 
        public List<AnimationScene.Performance> current_scenes = new List<AnimationScene.Performance>();
        Animator anim;
        int active_loopset;
        bool playing_one_shot;
        bool end_check, was_in_transition;

        void Awake () {
            anim = GetComponent<Animator>();
            anim.avatar = transform.GetChild(0).GetComponent<Animator>().avatar;
            anim.runtimeAnimatorController = animatorController;
            anim.applyRootMotion = true;
        }

        Vector3 pos_movement;
        Quaternion rot_movement;
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



            //DebugUpdate();
            bool next_is_one_shot;
            if (OneShotEnded(out next_is_one_shot)) {
                if (!next_is_one_shot) {
                    playing_one_shot = false;
                }
                //OnOneShotTransitionEnd();
                //if (on_one_shot_exit_transition_end != null) {
                //    Debug.Log("on one shot ended");
                //    on_one_shot_exit_transition_end();
                //}
            }

            UpdateUse();
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

                            //if (on_one_shot_exit_transition_start != null) {
                            //    Debug.Log("on one shot transition_out");
                    
                            //    on_one_shot_exit_transition_start();
                            //}
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

        

        public void Play (AnimationAssetObject anim, bool loop, bool interrupt_current) {
            bool mirror = false;
            if (!loop) {
                if (anim.mirror_mode == AnimationAssetObject.MirrorMode.Random) {
                    mirror = UnityEngine.Random.value < .5f;
                }
                else if (anim.mirror_mode == AnimationAssetObject.MirrorMode.Mirror) {
                    mirror = true;
                }
            }
            Play(anim.id, interrupt_current, mirror, loop, anim.transition_speed, anim.speed);
        }

        public void Play (int anim_index, bool interrupt_current, bool mirror, bool loop, float transition_time, float speed) {
            if (loop) {
                PlayLoop(anim_index, interrupt_current, mirror, transition_time, speed);
            }
            else {
                PlayAnimation(anim_index, mirror, transition_time, speed);
            }
        }
        /*
        */

        //public void InterruptAnimation () {
        //    Debug.Log("interupting animation");
        //    anim.SetTrigger(p_AnimExit);
        //}
    
        void PlayLoop (int anim_index, bool interrupt_current, bool mirror, float transition_time, float speed) {
            Debug.Log("playing loop");

            int layer = 0;
            active_loopset = (active_loopset + 1) % 2;
            anim.SetFloat(p_LoopIndicies[active_loopset], anim_index);
            anim.SetFloat(p_LoopSpeeds[active_loopset], speed);
            anim.SetBool(p_LoopMirrors[active_loopset], mirror);
            
            if ((playing_one_shot && interrupt_current) || !playing_one_shot) {
                string active_loop = "Loops_" + active_loopset.ToString();
                Debug.Log(active_loop +  " active loop");
                //InterruptAnimation();
                anim.CrossFadeInFixedTime(
                    active_loop,
                    transition_time, 
                    layer
                );
            }
            //if we're doing it in the background of a one shot just change the active loopset
            anim.SetInteger(p_ActiveLoopSet, active_loopset);

        }

        void PlayAnimation(int anim_index, bool mirror, float transition_time, float speed) {
            Debug.Log("playing anim " + anim_index.ToString());

            anim.SetBool(p_Mirror, mirror);
            anim.SetFloat(p_Speed, speed);

            int layer = 0;
            anim.CrossFadeInFixedTime(anim_index.ToString(), transition_time, layer);



            //anim.SetInteger(p_AnimIndex, anim_index);
            //anim.SetTrigger(p_AnimTrigger);
            playing_one_shot = true;
        }
    }
}


