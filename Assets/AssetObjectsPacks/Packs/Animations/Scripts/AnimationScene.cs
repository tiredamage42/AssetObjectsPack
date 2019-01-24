

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AssetObjectsPacks.Animations {


   


    /*
    
    concern for aiming ?

    cue behaviors : (updates)
        behavior for slerping to look rotation of cues interest transform or transform of scene

        behavior for ai pathfinding ?
            update beh {
                when actor get close to cue's interest trnsform, move it to the next waypoint on path
            }
        
        animation scenes:
            script:
                class that contains the roles and cue lists for the scene

            performances:
                each performance is a runtime instance of the scene playing out (so the scene can be used multiple times)


        useage:
            make animation scene prefab as a template, then initiate that scene at (with actors, position, rotation, on scene end callback)
    */

    


    public class AnimationScene : MonoBehaviour {

        void OnDrawGizmos () {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, .5f);
        }
        
        Role[] _roles;
        public Role[] roles {
            get {
                if (_roles == null || _roles.Length == 0) {
                    _roles = GetComponentsInChildren<Role>();
                }
                return _roles;
            }
        }



        // roles play animations at same time, and change cues at same time when ready
        // as opposed to staggered (whenever last animation is done)
        public bool syncRoles; //roles get END scene callback at the same time as opposed to whenever their cue's are done
        public bool interruptsOthers;
        //public int scene_weight = 0; //higher numbers override lower numbers (explosion knockdown > hit reaction)
        public bool isLooped;
        
        /*
        maybe make some non interruptable
        */
        public void InitializePerformance (List<AnimationPlayer> actors, Vector3 position, Quaternion rotation, System.Action on_end_performance_callback) {

            int role_count = roles.Length;
            int actors_count = actors.Count;
            if (role_count != actors_count) {
                Debug.LogError(name + " requires: " + role_count + " actors, got: " + actors_count);
                return;
            }   
            for (int a = 0; a < actors_count; a++) {
                for (int s = 0; s < actors[a].current_scenes.Count; s++) {
                    if (actors[a].current_scenes[s].parent_scene == this) {
                        Debug.LogError(name + " is already an active scene for: " + actors[a].name);
                        return;
                    }
                }   
            }
            //maybe weight scale
            if (interruptsOthers) {
                for (int a = 0; a < actors_count; a++) {
                    //get actors current scenes and interrupt them
                    List<Performance> actors_currrent_scenes = actors[a].current_scenes;
                    for (int s = 0; s < actors_currrent_scenes.Count; s++) {
                        actors_currrent_scenes[s].InterruptPerformance();
                    }
                    actors[a].current_scenes.Clear();
                    //actors[a].InterruptAnimation();
                }
            }

            Performance performance = AnimationsPack.PerformanceManager.GetNewPerformance();// performance_pool.GetNewObject();
            
            performance.InitializePerformance (this, position, rotation, actors, on_end_performance_callback);

        }


        //instance of scenes that play out at run time
        public class Performance {

            public class PerformanceCue {
                Vector3 initial_actor_position;
                Quaternion initial_actor_rotation;
                float smooth_l0, smooth_l1, smooth_l0v, smooth_l1v;//, duration_timer;
                public bool cue_ready, cue_active, cue_playing;
                AnimationPlayer actor;
                void CheckReadyTransform (Transform runtime_interest_transform, AnimationEvent script_cue) {
                    if (cue_ready) 
                        return;
                    
                    smooth_l0 = Mathf.SmoothDamp(smooth_l0, 1.0f, ref smooth_l0v, script_cue.smoothPositionTime);
                    smooth_l1 = Mathf.SmoothDamp(smooth_l1, 1.0f, ref smooth_l1v, script_cue.smoothRotationTime);
                    
                    actor.transform.position = Vector3.Lerp(initial_actor_position, runtime_interest_transform.position, smooth_l0);
                    actor.transform.rotation = Quaternion.Slerp(initial_actor_rotation, runtime_interest_transform.rotation, smooth_l1);
                    
                    float threshold = .99f;
                    
                    if (smooth_l0 > threshold && smooth_l1 > threshold) {
                        actor.transform.position = runtime_interest_transform.position;
                        actor.transform.rotation = runtime_interest_transform.rotation;
                        cue_ready = true;        
                    }
                }      
                public void InitializeCue (AnimationPlayer actor, Transform runtime_interest_transform, Transform performance_root_transform, AnimationEvent script_cue) {
                    cue_active = true;
                    cue_ready = true;
                    cue_playing = false;

                    runtime_interest_transform.localPosition = script_cue.transform.localPosition;
                    //maybe zero out x and z rotation
                    runtime_interest_transform.localRotation = script_cue.transform.localRotation;
                    switch (script_cue.snapActorStyle) {
                        case AnimationEvent.SnapActorStyle.Snap:
                            actor.transform.position = runtime_interest_transform.position;
                            actor.transform.rotation = runtime_interest_transform.rotation;
                            break;
                        case AnimationEvent.SnapActorStyle.Smooth:
                            cue_ready = false;
                            initial_actor_position = actor.transform.position;
                            initial_actor_rotation = actor.transform.rotation;
                            smooth_l0 = 0;
                            smooth_l1 = 0;
                            break;
                    }     
                }

                public void Play (AnimationPlayer actor, Transform runtime_interest_transform, AnimationEvent script_cue) {
                    Debug.Log("playing cue!" + script_cue.name);
                    cue_playing = true;
                    if (script_cue.animationScene != null) {
                        script_cue.animationScene.InitializePerformance(new List<AnimationPlayer>() {actor}, runtime_interest_transform.position, runtime_interest_transform.rotation, OnSceneEnd);
                        return;
                    }
                    /*
                    if (script_cue.duration < 0) {
                        actor.on_one_shot_exit_transition_start += OnAnimationEnd;
                    }
                    else {
                        duration_timer = 0;
                    }
                     */
                    actor.PlayEvent(script_cue, OnAnimationEnd);
                }


               
                
                void OnSceneEnd () {
                    //Debug.Log("cue on scne ened");
                    DeactivateCue();
                }

                void OnAnimationEnd () {
                    //Debug.Log("Cue On Animation End");
                    //actor.on_one_shot_exit_transition_start -= OnAnimationEnd;
                    DeactivateCue();
                }

                void DeactivateCue () {
                    actor = null;
                    cue_active = false;
                    //duration_timer = 0;
                }

                public void UpdateCue (AnimationPlayer actor, Transform runtime_interest_transform, AnimationEvent script_cue) {
                    if (!cue_active) 
                        return;
                    CheckReadyTransform(runtime_interest_transform, script_cue);
                    if (!cue_ready)
                        return;
                    this.actor = actor;
                    //if (script_cue.duration >= 0) {
                    //    duration_timer += Time.deltaTime;
                    //    if (duration_timer >= script_cue.duration) {

                    //        Debug.Log("duration up!");
                    //        DeactivateCue();
                    //    }
                    //}
                    if (cue_active) {
                        //update behaviors
                        for (int i = 0; i < script_cue.behaviors.Length; i++) {
                            ((AnimationEventBehavior)script_cue.behaviors[i]).UpdateBehavior(this, actor);
                        }
                    }
                }
            }

            public class PerformanceRole {
                public AnimationPlayer actor;
                public bool role_active;
                int cue_index;
                Role script_role;

                public PerformanceCue current_cue = new PerformanceCue();
                public void InitializeRole (AnimationPlayer actor, Role script_role, Transform performance_root_transform, Transform runtime_interest_transform) {
                    this.script_role = script_role;
                    this.actor = actor;
                    cue_index = 0;
                    role_active = true;
                    current_cue.InitializeCue(actor, runtime_interest_transform, performance_root_transform, script_role.cues[cue_index]);
                }   
                public void PlayCue (Transform runtime_interest_transform) {
                    current_cue.Play(actor, runtime_interest_transform, script_role.cues[cue_index]);   
                }
                public void UpdateRole (Transform runtime_interest_transform) {
                    if (!role_active) return;   
                    current_cue.UpdateCue(actor, runtime_interest_transform, script_role.cues[cue_index]);
                }
                public void OnCueEnd (Performance performance, Transform performance_root_transform, Transform runtime_interest_transform) {
                    
                    cue_index++;
                    if (cue_index >= script_role.cues.Length) {
                        OnPerformanceEnd(performance);
                        return;
                    }
                    current_cue.InitializeCue(actor, runtime_interest_transform, performance_root_transform, script_role.cues[cue_index]);
                }
                public void OnPerformanceEnd (Performance performance) {
                    if (actor) {
                        actor.current_scenes.Remove(performance);
                        actor = null;
                    }
                    role_active = false;
                }
            }

            public AnimationScene parent_scene;
            int performance_key;
            public void SetPerformanceKey (int key) {
                this.performance_key = key;
            }
            Transform performance_root_transform;
            List<Transform> role_interest_transforms = new List<Transform>();
            System.Action on_performance_done;
            List<PerformanceRole> roles = new List<PerformanceRole>();
            List<AnimationPlayer> orig_actors_list;
            System.Action orig_performance_done_callback;

            public void InterruptPerformance () {
                for (int i = 0; i < roles.Count; i++) {   
                    roles[i].OnPerformanceEnd(this);
                }
                on_performance_done = null;
                AnimationsPack.PerformanceManager.ReturnPerformanceToPool(performance_key);
            }

            //public void ClearPerformance () {
            //    this.on_performance_done = null;
            //    this.parent_scene = null;
            //}
            public void InitializePerformance (AnimationScene parent_scene, Vector3 position, Quaternion rotation, List<AnimationPlayer> actors, System.Action on_performance_done) {
                this.on_performance_done = on_performance_done;
                this.parent_scene = parent_scene;

                if (parent_scene.isLooped) {
                    orig_actors_list = actors;
                    orig_performance_done_callback = on_performance_done;
                }

                if (!performance_root_transform) {
                    performance_root_transform = new GameObject("performance_root_transform").transform;
                }
                performance_root_transform.position = position;
                performance_root_transform.rotation = rotation;
                
                
                
                int role_count = parent_scene.roles.Length;
                if (roles.Count != role_count) {
                    roles.Clear ();
                    for (int i = 0; i < role_count; i++) {
                        roles.Add(new PerformanceRole());
                    }
                }

                if (role_interest_transforms.Count != role_count) {
                    if (role_interest_transforms.Count < role_count) {
                        int c = role_count - role_interest_transforms.Count;
                        for (int i = 0; i < c; i++) {
                            Transform new_trans = new GameObject("RoleInterestTransform").transform;
                            new_trans.SetParent(performance_root_transform);
                            role_interest_transforms.Add(new_trans);
                        }
                    }
                }

                for (int i = 0; i < role_count; i++) {
                    actors[i].current_scenes.Add(this);
                    roles[i].InitializeRole (actors[i], parent_scene.roles[i], performance_root_transform, role_interest_transforms[i]);
                }
            }
            public void UpdatePerformance () {
                bool cues_ready_synced = true;
                bool cues_done_synced = true;
                bool all_done = true;
                int c = roles.Count;
                for (int i = 0; i < c; i++) {
                    if (!roles[i].current_cue.cue_ready) 
                        cues_ready_synced = false;
                    if (roles[i].current_cue.cue_active) 
                        cues_done_synced = false;
                    if (roles[i].role_active) 
                        all_done = false;
                }
                for (int i = 0; i < c; i++) {
                    PerformanceRole r = roles[i];
                    if (!r.current_cue.cue_playing && ((cues_ready_synced || (!r.current_cue.cue_ready && !parent_scene.syncRoles))))
                        r.PlayCue (role_interest_transforms[i]);
                    if (cues_done_synced || (!r.current_cue.cue_active && !parent_scene.syncRoles)) 
                        r.OnCueEnd (this, performance_root_transform, role_interest_transforms[i]);
                    if (all_done || (!r.role_active && !parent_scene.syncRoles)) 
                        roles[i].OnPerformanceEnd(this);
                }


                if (all_done) {
                    if (on_performance_done != null) {
                        on_performance_done();
                        on_performance_done = null;
                    }
                    AnimationsPack.PerformanceManager.ReturnPerformanceToPool(performance_key);
                    if (parent_scene.isLooped) {

                        parent_scene.InitializePerformance(orig_actors_list, performance_root_transform.position, performance_root_transform.rotation, orig_performance_done_callback);

                    }
                    return;
                }
                for (int i = 0; i < c; i++) {
                    roles[i].UpdateRole(role_interest_transforms[i]);
                }
            }
        }
    }
}

                

        

        





        
    
            /*
                jump points have their own scene
            */
        

        //anim in charge of face direction and move towards waypoint

        //ai in charge of stance (with different animation behavior variations)

/*

        public class CharacterAnimator : MonoBehaviour {
            //do root movement stuff with char controller component

            public Vector3 target_point;
            public Vector3 interest_point;
            public float min_strafe_travel_dist = 1;
        
            int GetDirection (bool allow_strafe) {
                if (!allow_strafe) {
                    return 0;
                }

                Vector3 pos = transform.position;

                Vector3 dir_to_dest = target_point - pos;
                dir_to_dest.y = 0;
                if (dir_to_dest.magnitude < min_strafe_travel_dist) {
                    return 0;
                }

                Debug.DrawRay(pos, dir_to_dest, Color.blue, dir_to_dest.magnitude);

                Vector3 dir_to_interest_point = interest_point - pos;
                dir_to_interest_point.y = 0;

                Debug.DrawRay(pos, dir_to_interest_point, Color.red, dir_to_interest_point.magnitude);

                float angle = Vector3.Angle(dir_to_interest_point, dir_to_dest);
                if (angle <= 45 || angle >= 135) {
                    //angle is too acute or obtuse between interest (enemy point) and destination
                    //for strafing
                    Debug.LogError ("angle is too acute or obtuse");
                    Debug.Break ();
                    return 0;
                }

                Vector3 dir_to_dest_perp = Vector3.Cross(dir_to_dest.normalized, Vector3.up);
                //dir_to_dest_perp.y = 0;
                Debug.DrawRay(pos, dir_to_dest_perp.normalized, Color.green, dir_to_dest_perp.magnitude);
                angle = Vector3.Angle(dir_to_dest_perp, dir_to_interest_point);
                if (angle <= 45) {
                    Debug.LogError ("strafing left towards destination");
                    Debug.Break ();
                    return 2;
                }
                else {
                    Debug.LogError ("strafing right towards destination");   
                    Debug.Break ();
                    return 1;
                }
            }
        }
        */
        /*
            figure out different variants of strafe / fwd during same go to animation scene
        */
   
   










