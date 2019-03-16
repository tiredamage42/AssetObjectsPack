using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AssetObjectsPacks {
    public static class Playlist {//: MonoBehaviour {


        static bool CueIsPlaylist(Cue cue) {
            //if (cue.playlist != null) {
            //    return true;
            //}
            int c = cue.transform.childCount;
            if (c == 0) {
                return false;
            }
            
            for (int i = 0; i < c; i++) {
                Transform t = cue.transform.GetChild(i);
                if (t.gameObject.activeSelf) {
                    if (t.GetComponent<Cue>() != null) {
                        return true;
                    }
                }
            }
            return false;
        }

        [System.Serializable] public class Channel {
            public Cue[] childCues;
            public Cue parentCue;

            //public bool topLevel;
            //public bool useRandomChoiceOnChild {
            //    get {
            //        return topLevel && cues[0].useRandomPlaylist;
            //    }
            //}

            public Channel(Cue parentCue) {
                this.parentCue = parentCue;
/*
                if (!ignoreTop && t.GetComponent<Cue>()) {
                    cues = new Cue[] { t.GetComponent<Cue>() };
                    topLevel = true;
                    return;
                }

 */
                Transform t = parentCue.transform;
                childCues = t.childCount.Generate( i => t.GetChild(i).GetComponent<Cue>() ).Where( cue => cue != null).ToArray();

                if (childCues.Length == 0) {
                    childCues = new Cue[] { parentCue };
                }
                //if (useRandomChoice) {
                //    cues = new Cue[] { cues.RandomChoice() };
                //}

            }
        }
        
        public static void InitializePerformance(Cue[] playlists, EventPlayer[] players, Vector3 position, Quaternion rotation, bool looped, int playerLayer, System.Action onEndPerformance) {
            int channelCount = playlists.Length;
            int playerCount = players.Length;
            if (channelCount != playerCount) {
                Debug.LogError("playlist/player coutn mismatch: playlists: " + channelCount + " players: " + playerCount);
                return;
            }               
            Performance performance = AssetObjectsManager.GetNewPerformance();

            Channel[] channels = playlists.Generate( p => new Channel(p)).ToArray();


            performance.InitializePerformance (playerLayer, playlists, channels, players, position, rotation, looped, onEndPerformance);
        }

        static void InitializePerformanceInternal(int useLayer, bool ignoreTopLevel, Cue playlist, EventPlayer player, Vector3 position, Quaternion rotation, System.Action onEndPerformance) {
            Performance performance = AssetObjectsManager.GetNewPerformance();

            performance.InitializePerformance (
                useLayer,
                new Cue[] {
                    playlist
                }, 
                new Channel[] { 
                    new Channel(
                        playlist//, 
                        //ignoreTopLevel
                    ) 
                }, 
                new EventPlayer[] { 
                    player 
                }, 
                position, rotation, false, onEndPerformance
            );
        }

        //instance of playlists that play out at run time
        public class Performance {

            public class PerformanceCue {
                public bool positionReady { get { return posReady || playImmediate; } }
                Vector3 initialPlayerPos;
                Quaternion initialPlayerRot;
                public bool isActive, isPlaying;
                float smooth_l0, smooth_l1, smooth_l0v, smooth_l1v;
                bool playImmediate, posReady;
                int layer;

                //if player has one, turn it off or smooth snap wont work
                //CharacterController overrideCharacterControl; 
                Transform interestTransform;
                EventPlayer player;
                Cue cue;

                void CheckReadyTransform (int layer, EventPlayer player, Transform interestTransform, Cue cue) {
                    if (posReady) 
                        return;
                    
                    smooth_l0 = Mathf.SmoothDamp(smooth_l0, 1.0f, ref smooth_l0v, cue.smoothPositionTime);
                    smooth_l1 = Mathf.SmoothDamp(smooth_l1, 1.0f, ref smooth_l1v, cue.smoothRotationTime);
                    
                    player.transform.position = Vector3.Lerp(initialPlayerPos, interestTransform.position, smooth_l0);
                    player.transform.rotation = Quaternion.Slerp(initialPlayerRot, interestTransform.rotation, smooth_l1);
                    
                    float threshold = .99f;
                    
                    if (smooth_l0 > threshold && smooth_l1 > threshold) {
                    
                        player.transform.position = interestTransform.position;
                        player.transform.rotation = interestTransform.rotation;

                        player.overrideMovement = false;
                        //if (overrideCharacterControl) {
                        //    overrideCharacterControl.enabled = true;
                        //}
                        posReady = true;        
                        CustomScripting.ExecuteMessageBlockStep (layer, player, cue, interestTransform.position, Cue.MessageEvent.OnSnap);

                    }
                }      
                public void InitializeCue (int layer, EventPlayer player, Transform interestTransform, Transform performanceRoot, Cue cue) {
                    this.cue = cue;
                    this.interestTransform = interestTransform;
                    this.player = player;
                    this.playImmediate = cue.playImmediate;
                    this.layer = layer;
                    
                    isActive = true;
                    posReady = true;
                    isPlaying = false;
                    //overrideCharacterControl = null;

                    interestTransform.localPosition = cue.transform.localPosition;
                    //maybe zero out x and z rotation
                    interestTransform.localRotation = cue.transform.localRotation;


                    



                    switch (cue.snapPlayerStyle) {
                        case Cue.SnapPlayerStyle.Snap:
                            player.transform.position = interestTransform.position;
                            player.transform.rotation = interestTransform.rotation;
                            break;
                        case Cue.SnapPlayerStyle.Smooth:
                            posReady = false;

                            player.overrideMovement = true;
                            //overrideCharacterControl = player.GetComponent<CharacterController>();
                            //if (overrideCharacterControl) {
                            //    overrideCharacterControl.enabled = false;
                            //}

                            initialPlayerPos = player.transform.position;
                            initialPlayerRot = player.transform.rotation;
                            smooth_l0 = smooth_l1 = 0;
                            break;
                    }     

                    CustomScripting.ExecuteMessageBlockStep (layer, player, cue, interestTransform.position, Cue.MessageEvent.OnStart);

                }

                public void Play (int layer, EventPlayer player, Transform interestTransform, Cue cue) {
                    isPlaying = true;

                    CustomScripting.ExecuteMessageBlockStep (layer, player, cue, interestTransform.position, Cue.MessageEvent.OnPlay);
                    
                    if (CueIsPlaylist(cue)) {
                       
                        //Debug.Log("playign cue playlist " + cue.name);
                        Playlist.InitializePerformanceInternal(
                            layer,
                            //cue.playlist == null,
                            true,
                            cue,
                            //cue.playlist != null ? cue.playlist : cue.transform,
                            player,
                            interestTransform.position, interestTransform.rotation, OnPlaylistEnd
                        );
                        return;
                    }
                    
                    //Debug.Log("playign cue" + cue.name);
                      
                    player.SubscribeToPlayEnd(layer, OnEventEnd);
                    player.PlayEvents_Cue(layer, cue.events, cue.overrideDuration);
                }

                void OnPlaylistEnd () {   
                    Deactivate();
                }
                void OnEventEnd (bool success) {
                    Deactivate();
                }

                void Deactivate () {

                    CustomScripting.ExecuteMessageBlockStep (layer, player, cue, interestTransform.position, Cue.MessageEvent.OnEnd);

                    cue = null;
                    player = null;
                    interestTransform = null;
                    isActive = false;

                }

                public void UpdateCue (int layer, EventPlayer player, Transform interestTransform, Cue cue) {
                    if (!isActive) 
                        return;
                    CheckReadyTransform(layer, player, interestTransform, cue);
                }
            }

            [System.Serializable] public class PerformanceChannel {
                public EventPlayer player;
                public bool isActive;
                int cueIndex, curCueRepeats;
                Playlist.Channel playlistChannel;
                public PerformanceCue currentCue = new PerformanceCue();

                public void InitializeChannel (int layer, EventPlayer player, Playlist.Channel playlistChannel, Transform performanceRoot, Transform interestTransform) {
                    this.playlistChannel = playlistChannel;
                    this.player = player;

                    if (playlistChannel.parentCue.useRandomPlaylist) {
                        List<int> enabledIndicies = new List<int>();
                        for (int i = 0; i < playlistChannel.childCues.Length; i++) {
                            if (playlistChannel.childCues[i].gameObject.activeSelf) {
                                enabledIndicies.Add(i);
                            }
                        }
                        cueIndex = enabledIndicies.RandomChoice();
                    }
                    else {
                        cueIndex = 0;   
                    }
                    while (cueIndex < playlistChannel.childCues.Length && !playlistChannel.childCues[cueIndex].gameObject.activeSelf) {      
                        Debug.Log(playlistChannel.childCues[cueIndex].name + " is inative");
                        cueIndex ++;
                    }
                    
                    curCueRepeats = 0;
                    isActive = true;
                    currentCue.InitializeCue(layer, player, interestTransform, performanceRoot, playlistChannel.childCues[cueIndex]);
                }   
                public void PlayCue (int layer, Transform interestTransform) {
                    //Debug.Log("play cue: " + playlistChannel.childCues[cueIndex].name);
                    currentCue.Play(layer, player, interestTransform, playlistChannel.childCues[cueIndex]);   
                }
                public void UpdateChannel (int layer, Transform interestTransform) {
                    if (!isActive) return;   
                    currentCue.UpdateCue(layer, player, interestTransform, playlistChannel.childCues[cueIndex]);
                }
                public void OnCueEnd (int layer, Performance performance, Transform performanceRoot, Transform interestTransform) {
                    
                    curCueRepeats++;
                    
                    if (curCueRepeats < playlistChannel.childCues[cueIndex].repeats) {
                        currentCue.InitializeCue(layer, player, interestTransform, performanceRoot, playlistChannel.childCues[cueIndex]);
                        return;
                    }

                    if (playlistChannel.parentCue.useRandomPlaylist) {



                    //}
                    //if (useRandomChoice) {
                        //using random cue in this performance
                        //Debug.Log("ending performance random choice");
                        OnPerformanceEnd(performance);
                        return;
                    

                    }

                    int l = playlistChannel.childCues.Length;
                    
                    curCueRepeats = 0;
                    cueIndex++;
                    
                    while (cueIndex < l && !playlistChannel.childCues[cueIndex].gameObject.activeSelf) {                    
                        cueIndex ++;
                    }
                    if (cueIndex >= l) {
                        //Debug.Log("ending performance");
                        OnPerformanceEnd(performance);
                        return;
                    }
                    //Debug.Log("playing cue " + playlistChannel.childCues[cueIndex].name);
                    currentCue.InitializeCue(layer, player, interestTransform, performanceRoot, playlistChannel.childCues[cueIndex]);
                }
                public void OnPerformanceEnd (Performance performance) {
                    if (player) {
                        player.current_playlists.Remove(performance);
                        player = null;
                    }
                    isActive = false;
                }
            }

            int performance_key;
            public void SetPerformanceKey (int key) {
                this.performance_key = key;
            }
            Transform performance_root_transform;
            List<Transform> interestTransforms = new List<Transform>();
            System.Action on_performance_done;
            PerformanceChannel[] channels = new PerformanceChannel[0];

            
            EventPlayer[] orig_players;
            public Cue[] playlists;
            System.Action orig_performance_done_callback;
            bool looped;
            int useLayer;

            public void InterruptPerformance () {
                for (int i = 0; i < channels.Length; i++) {   
                    channels[i].OnPerformanceEnd(this);
                }
                on_performance_done = null;
                AssetObjectsManager.ReturnPerformanceToPool(performance_key);
            }

            public void InitializePerformance (int useLayer, Cue[] playlists, Channel[] playlistChannels, EventPlayer[] players, Vector3 position, Quaternion rotation, bool looped, System.Action on_performance_done) {
                this.on_performance_done = on_performance_done;
                this.playlists = playlists;
                this.looped = looped;
                this.useLayer = useLayer;

                if (looped) {
                    orig_players = players;
                    orig_performance_done_callback = on_performance_done;
                }

                if (!performance_root_transform) {
                    performance_root_transform = new GameObject("performance_root_transform").transform;
                }
                performance_root_transform.position = position;
                performance_root_transform.rotation = rotation;
                
                int channel_count = playlistChannels.Length;
                if (channels.Length != channel_count) {
                    channels = channel_count.Generate(i => new PerformanceChannel()).ToArray();
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
                    channels[i].InitializeChannel (useLayer, players[i], playlistChannels[i], performance_root_transform, interestTransforms[i]);
                }
            }
            public void UpdatePerformance () {
                bool cuesReadySynced = true;
                bool cuesDoneSynced = true;
                bool allDone = true;
                int c = channels.Length;

                for (int i = 0; i < c; i++) {
                    PerformanceChannel channel = channels[i];

                    //check for play ready
                    if (!channel.currentCue.positionReady) {
                        cuesReadySynced = false;
                    }
                    //check for end ready
                    if (channel.currentCue.isActive || !channel.isActive) {
                        cuesDoneSynced = false;
                    }
                    //check if all ended
                    if (channel.isActive) {
                        allDone = false;
                    }
                }

                for (int i = 0; i < c; i++) {
                    PerformanceChannel channel = channels[i];
                    if (!channel.currentCue.isPlaying && cuesReadySynced)
                        channel.PlayCue (useLayer, interestTransforms[i]);
                    if (cuesDoneSynced)
                        channel.OnCueEnd (useLayer, this, performance_root_transform, interestTransforms[i]);
                }

                if (allDone) {
                    if (on_performance_done != null) {
                        on_performance_done();
                        on_performance_done = null;
                    }
                    AssetObjectsManager.ReturnPerformanceToPool(performance_key);

                    if (looped) {
                        Playlist.InitializePerformance(playlists, orig_players, performance_root_transform.position, performance_root_transform.rotation, looped, useLayer, orig_performance_done_callback);
                    }
                    return;
                }
                for (int i = 0; i < c; i++) {
                    channels[i].UpdateChannel(useLayer, interestTransforms[i]);
                }
            }
        }
         

    }



}