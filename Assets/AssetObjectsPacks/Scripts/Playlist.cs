using System.Collections.Generic;
using UnityEngine;
namespace AssetObjectsPacks {
    public class Playlist : MonoBehaviour {

        void OnDrawGizmos () {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, .5f);
        }

        [System.Serializable] public class Channel {
            public Cue[] cues;
            public Channel(Transform t) {
                cues = t.GetComponentsInChildren<Cue>();
            }
        }
        
        Channel[] _channels;
        public Channel[] channels {
            get {
                if (_channels == null || _channels.Length == 0) _channels = new Channel[transform.childCount].Generate( i => { return new Channel(transform.GetChild(i)); } );
                return _channels;
            }
        }

        // channels play cues at same time, and change cues at same time when ready
        // as opposed to staggered (whenever last cue is done)
        public bool syncChannels; 
        //public bool interruptsOthers;
        //public int scene_weight = 0; //higher numbers override lower numbers (explosion knockdown > hit reaction)
        public bool isLooped;
        

        /*
        maybe make some non interruptable
        */
        public void InitializePerformance (EventPlayer[] players, Vector3 position, Quaternion rotation, System.Action onEndPerformance) {

            int channelCount = channels.Length;
            int playerCount = players.Length;
            if (channelCount != playerCount) {
                Debug.LogError(name + " requires: " + channelCount + " players, got: " + playerCount);
                return;
            }   
            for (int a = 0; a < playerCount; a++) {
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

            Performance performance = AssetObjectsManager.GetNewPerformance();
            
            performance.InitializePerformance (this, position, rotation, players, onEndPerformance);

        }


        //instance of playlists that play out at run time
        public class Performance {

            public class PerformanceCue {
                Vector3 initialPlayerPos;
                Quaternion initialPlayerRot;
                float smooth_l0, smooth_l1, smooth_l0v, smooth_l1v;
                public bool playerPositionReady, isActive, isPlaying;
                void CheckReadyTransform (EventPlayer player, Transform interestTransform, Cue cue) {
                    if (playerPositionReady) 
                        return;
                    
                    smooth_l0 = Mathf.SmoothDamp(smooth_l0, 1.0f, ref smooth_l0v, cue.smoothPositionTime);
                    smooth_l1 = Mathf.SmoothDamp(smooth_l1, 1.0f, ref smooth_l1v, cue.smoothRotationTime);
                    
                    player.transform.position = Vector3.Lerp(initialPlayerPos, interestTransform.position, smooth_l0);
                    player.transform.rotation = Quaternion.Slerp(initialPlayerRot, interestTransform.rotation, smooth_l1);
                    
                    float threshold = .99f;
                    
                    if (smooth_l0 > threshold && smooth_l1 > threshold) {
                        player.transform.position = interestTransform.position;
                        player.transform.rotation = interestTransform.rotation;
                        playerPositionReady = true;        
                    }
                }      
                public void InitializeCue (EventPlayer player, Transform interestTransform, Transform performanceRoot, Cue cue) {
                    isActive = true;
                    playerPositionReady = true;
                    isPlaying = false;

                    interestTransform.localPosition = cue.transform.localPosition;
                    //maybe zero out x and z rotation
                    interestTransform.localRotation = cue.transform.localRotation;
                    switch (cue.snapPlayerStyle) {
                        case Cue.SnapPlayerStyle.Snap:
                            player.transform.position = interestTransform.position;
                            player.transform.rotation = interestTransform.rotation;
                            break;
                        case Cue.SnapPlayerStyle.Smooth:
                            playerPositionReady = false;
                            initialPlayerPos = player.transform.position;
                            initialPlayerRot = player.transform.rotation;
                            smooth_l0 = smooth_l1 = 0;
                            break;
                    }     
                }

                public void Play (EventPlayer player, Transform interestTransform, Cue cue) {
                    //Debug.Log("playing cue!" + cue.name);
                    isPlaying = true;

                    if (cue.sendMessage != "") {
                        Debug.Log("sending message");
                        player.SendMessage(cue.sendMessage, interestTransform, SendMessageOptions.RequireReceiver);
                    }
                    if (cue.playlist != null) {
                        cue.playlist.InitializePerformance(new EventPlayer[] {player}, interestTransform.position, interestTransform.rotation, OnPlaylistEnd);
                        return;
                    }

                    player.PlayEvents(cue.events, OnEventEnd);
                }


               
                
                void OnPlaylistEnd () {
                    Deactivate();
                }

                void OnEventEnd () {
                    //Debug.Log("on cue end");
                    Deactivate();
                }

                void Deactivate () {
                    //player = null;
                    isActive = false;
                }

                public void UpdateCue (EventPlayer player, Transform interestTransform, Cue cue) {
                    if (!isActive) 
                        return;
                    CheckReadyTransform(player, interestTransform, cue);
                    //if (!playerReady)
                    //    return;
                    
                }
            }

            [System.Serializable] public class PerformanceChannel {
                public EventPlayer player;
                public bool isActive;
                int cueIndex;
                Playlist.Channel playlistChannel;
                public PerformanceCue currentCue = new PerformanceCue();

                public void InitializeChannel (EventPlayer player, Playlist.Channel playlistChannel, Transform performanceRoot, Transform interestTransform) {
                    this.playlistChannel = playlistChannel;
                    this.player = player;
                    cueIndex = 0;
                    isActive = true;
                    currentCue.InitializeCue(player, interestTransform, performanceRoot, playlistChannel.cues[cueIndex]);
                }   
                public void PlayCue (Transform interestTransform) {
                    currentCue.Play(player, interestTransform, playlistChannel.cues[cueIndex]);   
                }
                public void UpdateChannel (Transform interestTransform) {
                    if (!isActive) return;   
                    currentCue.UpdateCue(player, interestTransform, playlistChannel.cues[cueIndex]);
                }
                public void OnCueEnd (Performance performance, Transform performanceRoot, Transform interestTransform) {
                    
                    cueIndex++;
                    if (cueIndex >= playlistChannel.cues.Length) {
                        OnPerformanceEnd(performance);
                        return;
                    }
                    currentCue.InitializeCue(player, interestTransform, performanceRoot, playlistChannel.cues[cueIndex]);
                }
                public void OnPerformanceEnd (Performance performance) {
                    if (player) {
                        player.current_playlists.Remove(performance);
                        player = null;
                    }
                    isActive = false;
                }
            }

            public Playlist playlist;
            int performance_key;
            public void SetPerformanceKey (int key) {
                this.performance_key = key;
            }
            Transform performance_root_transform;
            List<Transform> interestTransforms = new List<Transform>();
            System.Action on_performance_done;
            PerformanceChannel[] channels = new PerformanceChannel[0];
            EventPlayer[] orig_players;
            System.Action orig_performance_done_callback;

            public void InterruptPerformance () {
                for (int i = 0; i < channels.Length; i++) {   
                    channels[i].OnPerformanceEnd(this);
                }
                on_performance_done = null;
                AssetObjectsManager.ReturnPerformanceToPool(performance_key);
            }

            //public void ClearPerformance () {
            //    this.on_performance_done = null;
            //    this.parent_scene = null;
            //}
            public void InitializePerformance (Playlist playlist, Vector3 position, Quaternion rotation, EventPlayer[] players, System.Action on_performance_done) {
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
                if (channels.Length != channel_count) {
                    channels = new PerformanceChannel[channel_count].Generate( i => new PerformanceChannel() );
                }

                if (interestTransforms.Count != channel_count) {
                    if (interestTransforms.Count < channel_count) {
                        int c = channel_count - interestTransforms.Count;
                        for (int i = 0; i < c; i++) {
                            Transform new_trans = new GameObject("ChannelInterestTransform").transform;
                            new_trans.SetParent(performance_root_transform);
                            interestTransforms.Add(new_trans);
                        }
                    }
                }

                for (int i = 0; i < channel_count; i++) {
                    players[i].current_playlists.Add(this);
                    channels[i].InitializeChannel (players[i], playlist.channels[i], performance_root_transform, interestTransforms[i]);
                }
            }
            public void UpdatePerformance () {
                bool cuesReadySynced = true;
                bool cuesDoneSynced = true;
                bool allDone = true;
                int c = channels.Length;
                for (int i = 0; i < c; i++) {

                    PerformanceChannel playerChannel = channels[i];
                    if (!playerChannel.currentCue.playerPositionReady) 
                        cuesReadySynced = false;
                    if (playerChannel.currentCue.isActive) 
                        cuesDoneSynced = false;
                    if (playerChannel.isActive) 
                        allDone = false;
                }
                for (int i = 0; i < c; i++) {
                    PerformanceChannel playerChannel = channels[i];
                    if (!playerChannel.currentCue.isPlaying && ((cuesReadySynced || (!playerChannel.currentCue.playerPositionReady && !playlist.syncChannels))))
                        playerChannel.PlayCue (interestTransforms[i]);
                    if (cuesDoneSynced || (!playerChannel.currentCue.isActive && !playlist.syncChannels)) 
                        playerChannel.OnCueEnd (this, performance_root_transform, interestTransforms[i]);
                    if (allDone || (!playerChannel.isActive && !playlist.syncChannels)) 
                        channels[i].OnPerformanceEnd(this);
                }


                if (allDone) {
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
                    channels[i].UpdateChannel(interestTransforms[i]);
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
   
   










