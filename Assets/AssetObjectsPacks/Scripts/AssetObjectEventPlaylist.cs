

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AssetObjectsPacks {
    public class AssetObjectEventPlaylist : MonoBehaviour {

        void OnDrawGizmos () {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, .5f);
        }

        [System.Serializable] public class Channel {
            public AssetObjectEvent[] events;
            public Channel(Transform t) {
                events = t.GetComponentsInChildren<AssetObjectEvent>();
            }
        }
        
        Channel[] _channels;
        public Channel[] channels {
            get {
                
                if (_channels == null || _channels.Length == 0) {

                    _channels = new Channel[transform.childCount].Generate( i => { return new Channel(transform.GetChild(i)); } );

                    //_channels = GetComponentsInChildren<AssetObjectEventPlaylistChannel>();
                }
                return _channels;
            }
        }

        // channels play events at same time, and change events at same time when ready
        // as opposed to staggered (whenever last event is done)
        public bool syncChannels; 
        //public bool interruptsOthers;
        //public int scene_weight = 0; //higher numbers override lower numbers (explosion knockdown > hit reaction)
        public bool isLooped;
        
        /*
        maybe make some non interruptable
        */
        public void InitializePerformance (List<AssetObjectEventPlayer> players, Vector3 position, Quaternion rotation, System.Action on_end_performance_callback) {

            int channel_count = channels.Length;
            int players_count = players.Count;
            if (channel_count != players_count) {
                Debug.LogError(name + " requires: " + channel_count + " players, got: " + players_count);
                return;
            }   
            for (int a = 0; a < players_count; a++) {
                for (int s = 0; s < players[a].current_playlists.Count; s++) {
                    if (players[a].current_playlists[s].playlist == this) {
                        Debug.LogError(name + " is already an active scene for: " + players[a].name);
                        return;
                    }
                }   
            }
            /*
            //maybe weight scale
            if (interruptsOthers) {
                for (int a = 0; a < players_count; a++) {
                    //get players current scenes and interrupt them
                    List<Performance> players_currrent_scenes = players[a].current_scenes;
                    for (int s = 0; s < players_currrent_scenes.Count; s++) {
                        players_currrent_scenes[s].InterruptPerformance();
                    }
                    players[a].current_scenes.Clear();
                    //players[a].InterruptAnimation();
                }
            }
             */

            Performance performance = AssetObjectsManager.GetNewPerformance();// performance_pool.GetNewObject();
            
            performance.InitializePerformance (this, position, rotation, players, on_end_performance_callback);

        }


        //instance of scenes that play out at run time
        public class Performance {

            public class PerformanceEvent {
                Vector3 initial_player_position;
                Quaternion initial_player_rotation;
                float smooth_l0, smooth_l1, smooth_l0v, smooth_l1v;
                public bool event_ready, event_active, event_playing;
                void CheckReadyTransform (AssetObjectEventPlayer player, Transform runtime_interest_transform, AssetObjectEvent ao_event) {
                    if (event_ready) 
                        return;
                    
                    smooth_l0 = Mathf.SmoothDamp(smooth_l0, 1.0f, ref smooth_l0v, ao_event.smoothPositionTime);
                    smooth_l1 = Mathf.SmoothDamp(smooth_l1, 1.0f, ref smooth_l1v, ao_event.smoothRotationTime);
                    
                    player.transform.position = Vector3.Lerp(initial_player_position, runtime_interest_transform.position, smooth_l0);
                    player.transform.rotation = Quaternion.Slerp(initial_player_rotation, runtime_interest_transform.rotation, smooth_l1);
                    
                    float threshold = .99f;
                    
                    if (smooth_l0 > threshold && smooth_l1 > threshold) {
                        player.transform.position = runtime_interest_transform.position;
                        player.transform.rotation = runtime_interest_transform.rotation;
                        event_ready = true;        
                    }
                }      
                public void InitializeEvent (AssetObjectEventPlayer player, Transform runtime_interest_transform, Transform performance_root_transform, AssetObjectEvent ao_event) {
                    event_active = true;
                    event_ready = true;
                    event_playing = false;

                    runtime_interest_transform.localPosition = ao_event.transform.localPosition;
                    //maybe zero out x and z rotation
                    runtime_interest_transform.localRotation = ao_event.transform.localRotation;
                    switch (ao_event.snapPlayerStyle) {
                        case AssetObjectEvent.SnapPlayerStyle.Snap:
                            player.transform.position = runtime_interest_transform.position;
                            player.transform.rotation = runtime_interest_transform.rotation;
                            break;
                        case AssetObjectEvent.SnapPlayerStyle.Smooth:
                            event_ready = false;
                            initial_player_position = player.transform.position;
                            initial_player_rotation = player.transform.rotation;
                            smooth_l0 = 0;
                            smooth_l1 = 0;
                            break;
                    }     
                }

                public void Play (AssetObjectEventPlayer player, Transform runtime_interest_transform, AssetObjectEvent ao_event) {
                    //Debug.Log("playing evnt!" + ao_event.name);
                    event_playing = true;
                    if (ao_event.playlist != null) {
                        ao_event.playlist.InitializePerformance(new List<AssetObjectEventPlayer>() {player}, runtime_interest_transform.position, runtime_interest_transform.rotation, OnPlaylistEnd);
                        return;
                    }
                    player.PlayEvent(ao_event, OnEventEnd);
                }


               
                
                void OnPlaylistEnd () {
                    Deactivate();
                }

                void OnEventEnd () {
                    //Debug.Log("on event end");
                    Deactivate();
                }

                void Deactivate () {
                    //player = null;
                    event_active = false;
                }

                public void UpdateEvent (AssetObjectEventPlayer player, Transform runtime_interest_transform, AssetObjectEvent ao_event) {
                    if (!event_active) 
                        return;
                    CheckReadyTransform(player, runtime_interest_transform, ao_event);
                    if (!event_ready)
                        return;
                    
                }
            }

            public class PerformanceChannel {
                public AssetObjectEventPlayer player;
                public bool channel_active;
                int event_index;
                AssetObjectEventPlaylist.Channel playlist_channel;
                public PerformanceEvent current_event = new PerformanceEvent();

                public void InitializeChannel (AssetObjectEventPlayer player, AssetObjectEventPlaylist.Channel playlist_channel, Transform performance_root_transform, Transform runtime_interest_transform) {
                    this.playlist_channel = playlist_channel;
                    this.player = player;
                    event_index = 0;
                    channel_active = true;
                    current_event.InitializeEvent(player, runtime_interest_transform, performance_root_transform, playlist_channel.events[event_index]);
                }   
                public void PlayEvent (Transform runtime_interest_transform) {
                    current_event.Play(player, runtime_interest_transform, playlist_channel.events[event_index]);   
                }
                public void UpdateChannel (Transform runtime_interest_transform) {
                    if (!channel_active) return;   
                    current_event.UpdateEvent(player, runtime_interest_transform, playlist_channel.events[event_index]);
                }
                public void OnEventEnd (Performance performance, Transform performance_root_transform, Transform runtime_interest_transform) {
                    
                    event_index++;
                    if (event_index >= playlist_channel.events.Length) {
                        OnPerformanceEnd(performance);
                        return;
                    }
                    current_event.InitializeEvent(player, runtime_interest_transform, performance_root_transform, playlist_channel.events[event_index]);
                }
                public void OnPerformanceEnd (Performance performance) {
                    if (player) {
                        player.current_playlists.Remove(performance);
                        player = null;
                    }
                    channel_active = false;
                }
            }

            public AssetObjectEventPlaylist playlist;
            int performance_key;
            public void SetPerformanceKey (int key) {
                this.performance_key = key;
            }
            Transform performance_root_transform;
            List<Transform> channel_interest_transforms = new List<Transform>();
            System.Action on_performance_done;
            List<PerformanceChannel> channels = new List<PerformanceChannel>();
            List<AssetObjectEventPlayer> orig_players;
            System.Action orig_performance_done_callback;

            public void InterruptPerformance () {
                for (int i = 0; i < channels.Count; i++) {   
                    channels[i].OnPerformanceEnd(this);
                }
                on_performance_done = null;
                AssetObjectsManager.ReturnPerformanceToPool(performance_key);
            }

            //public void ClearPerformance () {
            //    this.on_performance_done = null;
            //    this.parent_scene = null;
            //}
            public void InitializePerformance (AssetObjectEventPlaylist playlist, Vector3 position, Quaternion rotation, List<AssetObjectEventPlayer> players, System.Action on_performance_done) {
                this.on_performance_done = on_performance_done;
                this.playlist = playlist;

                if (playlist.isLooped) {
                    orig_players = players;
                    orig_performance_done_callback = on_performance_done;
                }

                if (!performance_root_transform) {
                    performance_root_transform = new GameObject("performance_root_transform").transform;
                }
                performance_root_transform.position = position;
                performance_root_transform.rotation = rotation;
                
                
                
                int channel_count = playlist.channels.Length;
                if (channels.Count != channel_count) {
                    channels.Clear ();
                    for (int i = 0; i < channel_count; i++) {
                        channels.Add(new PerformanceChannel());
                    }
                }

                if (channel_interest_transforms.Count != channel_count) {
                    if (channel_interest_transforms.Count < channel_count) {
                        int c = channel_count - channel_interest_transforms.Count;
                        for (int i = 0; i < c; i++) {
                            Transform new_trans = new GameObject("ChannelInterestTransform").transform;
                            new_trans.SetParent(performance_root_transform);
                            channel_interest_transforms.Add(new_trans);
                        }
                    }
                }

                for (int i = 0; i < channel_count; i++) {
                    players[i].current_playlists.Add(this);
                    channels[i].InitializeChannel (players[i], playlist.channels[i], performance_root_transform, channel_interest_transforms[i]);
                }
            }
            public void UpdatePerformance () {
                bool events_ready_synced = true;
                bool events_done_synced = true;
                bool all_done = true;
                int c = channels.Count;
                for (int i = 0; i < c; i++) {
                    if (!channels[i].current_event.event_ready) 
                        events_ready_synced = false;
                    if (channels[i].current_event.event_active) 
                        events_done_synced = false;
                    if (channels[i].channel_active) 
                        all_done = false;
                }
                for (int i = 0; i < c; i++) {
                    PerformanceChannel r = channels[i];
                    if (!r.current_event.event_playing && ((events_ready_synced || (!r.current_event.event_ready && !playlist.syncChannels))))
                        r.PlayEvent (channel_interest_transforms[i]);
                    if (events_done_synced || (!r.current_event.event_active && !playlist.syncChannels)) 
                        r.OnEventEnd (this, performance_root_transform, channel_interest_transforms[i]);
                    if (all_done || (!r.channel_active && !playlist.syncChannels)) 
                        channels[i].OnPerformanceEnd(this);
                }


                if (all_done) {
                    if (on_performance_done != null) {
                        on_performance_done();
                        on_performance_done = null;
                    }
                    AssetObjectsManager.ReturnPerformanceToPool(performance_key);
                    if (playlist.isLooped) {

                        playlist.InitializePerformance(orig_players, performance_root_transform.position, performance_root_transform.rotation, orig_performance_done_callback);

                    }
                    return;
                }
                for (int i = 0; i < c; i++) {
                    channels[i].UpdateChannel(channel_interest_transforms[i]);
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
   
   










