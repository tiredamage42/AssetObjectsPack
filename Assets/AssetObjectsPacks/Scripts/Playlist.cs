﻿using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace AssetObjectsPacks {
    public static class Playlist {
        static bool CueIsPlaylist(Cue cue) {
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

        //using this top level system enables messaging on the root cue
        //but now syncing only works for the root cue, not just first layer or others

        [System.Serializable] public class Channel {
            public Cue[] cues;
            public bool useRandomChoice;
            public Channel(Cue parentCue, bool topLevel) {
                if (topLevel) {
                    cues = new Cue[] { parentCue };
                    return;
                }

                this.useRandomChoice = parentCue.useRandomPlaylist;
                Transform t = parentCue.transform;
                int c = t.childCount;

                if (c != 0) { 
                    cues = c.Generate( i => t.GetChild(i).GetComponent<Cue>() ).Where( cue => cue != null).ToArray();
                }
                if (c == 0 || cues.Length == 0) {
                    cues = new Cue[] { parentCue };
                }
            }
        }
        static bool CheckCounts (int channelCount, int playerCount) {
            if (channelCount != playerCount) {
                Debug.LogError("playlist/player coutn mismatch: playlists: " + channelCount + " players: " + playerCount);
                return false;
            }               
            return true;
        }
        public static void InitializePerformance(string debugReason, Cue playlists, EventPlayer players, bool looped, int playerLayer, Vector3 position, Quaternion rotation, bool forceInterrupt = true, Action onEndPerformanceCallbacks = null) {
            InitializePerformance(debugReason, new Cue[] { playlists }, new EventPlayer[] { players }, looped, playerLayer, position, rotation, forceInterrupt, new Action[] { onEndPerformanceCallbacks });
        }
        public static void InitializePerformance(string debugReason, Cue[] playlists, EventPlayer[] players, bool looped, int playerLayer, Vector3 position, Quaternion rotation, bool forceInterrupt = true, Action[] onEndPerformanceCallbacks = null) {
            if (!CheckCounts(playlists.Length, players.Length)) {
                return;
            }
            AssetObjectsManager.GetNewPerformance().InitializePerformance (debugReason,
                playerLayer, 
                playlists, 
                playlists.Generate( p => new Channel(p, true)).ToArray(), 
                players, 
                position, 
                rotation, 
                looped, 
                forceInterrupt, 
                onEndPerformanceCallbacks
            );
        }

        static void InitializePerformanceInternal(string debugReason, int useLayer, Cue playlist, EventPlayer player, Vector3 position, Quaternion rotation, bool forceInterrupt, Action onEndPerformanceCallback) {
            AssetObjectsManager.GetNewPerformance().InitializePerformance (debugReason,
                useLayer, 
                new Cue[] { playlist }, 
                new Channel[] { new Channel(playlist, false) }, 
                new EventPlayer[] { player }, 
                position, 
                rotation, 
                false, 
                forceInterrupt, 
                new Action[] { onEndPerformanceCallback }
            );
        }

        //instance of playlists that play out at run time
        public class Performance {

            public class PlaylistHead {
                public bool positionReady { get { return posReady || playImmediate; } }
                public bool isActive, isPlaying;
                Vector3 initialPlayerPos;
                Quaternion initialPlayerRot;
                float smooth_l0, smooth_l1, smooth_l0v, smooth_l1v;
                bool playImmediate, posReady;
                int layer;

                Transform runtimeTransform;
                EventPlayer player;
                Cue cue;

                void CheckReadyTransform (int layer, EventPlayer player, Transform runtimeTransform, Cue cue) {
                    if (posReady) 
                        return;
                    
                    smooth_l0 = Mathf.SmoothDamp(smooth_l0, 1.0f, ref smooth_l0v, cue.smoothPositionTime);
                    smooth_l1 = Mathf.SmoothDamp(smooth_l1, 1.0f, ref smooth_l1v, cue.smoothRotationTime);
                    
                    player.transform.position = Vector3.Lerp(initialPlayerPos, runtimeTransform.position, smooth_l0);
                    player.transform.rotation = Quaternion.Slerp(initialPlayerRot, runtimeTransform.rotation, smooth_l1);
                    
                    float threshold = .99f;
                    
                    if (smooth_l0 > threshold && smooth_l1 > threshold) {
                    
                        player.transform.position = runtimeTransform.position;
                        player.transform.rotation = runtimeTransform.rotation;

                        player.cueMoving = false;
                        posReady = true;        
                        CustomScripting.ExecuteMessageBlockStep (layer, player, cue, runtimeTransform.position, Cue.MessageEvent.OnSnap);
                    }
                }      
                public void OnStart (Cue cue, int layer, EventPlayer player, Transform runtimeTransform) {
                    this.cue = cue;
                    this.player = player;
                    this.layer = layer;
                    this.runtimeTransform = runtimeTransform;
                    this.playImmediate = cue.playImmediate;
                    
                    isActive = true;
                    posReady = true;
                    isPlaying = false;

                    runtimeTransform.localPosition = cue.transform.localPosition;
                    //maybe zero out x and z rotation
                    runtimeTransform.localRotation = cue.transform.localRotation;


                    //Debug.Log("on start message " + cue.name);
                    CustomScripting.ExecuteMessageBlockStep (layer, player, cue, runtimeTransform.position, Cue.MessageEvent.OnStart);

                    switch (cue.snapPlayerStyle) {
                        case Cue.SnapPlayerStyle.Snap:
                            player.transform.position = runtimeTransform.position;
                            player.transform.rotation = runtimeTransform.rotation;
                            CustomScripting.ExecuteMessageBlockStep (layer, player, cue, runtimeTransform.position, Cue.MessageEvent.OnSnap);
                            break;
                        case Cue.SnapPlayerStyle.Smooth:
                            posReady = false;
                            player.cueMoving = true;

                            initialPlayerPos = player.transform.position;
                            initialPlayerRot = player.transform.rotation;
                            smooth_l0 = smooth_l1 = 0;
                            break;
                    }     
                }

                public void OnPlay (string debugReason, int layer, EventPlayer player, Transform runtimeTransform, Cue cue, bool forceInterrupt) {
                    isPlaying = true;

                    CustomScripting.ExecuteMessageBlockStep (layer, player, cue, runtimeTransform.position, Cue.MessageEvent.OnPlay);
                    
                    if (CueIsPlaylist(cue)) {
        
                        //Debug.Log("playign cue playlist " + cue.name);
                        //Playlist.InitializePerformanceInternal(debugReason + "/" + cue.name, layer, cue, player, runtimeTransform.position, runtimeTransform.rotation, forceInterrupt, OnPlaylistEnd);
                        Playlist.InitializePerformanceInternal(debugReason + "/" + cue.name, layer, cue, player, runtimeTransform.position, runtimeTransform.rotation, forceInterrupt, OnPlaylistEnd);
                        
                        return;
                    }
                    
                    //Debug.Log("playign cue" + cue.name);
                      
                    player.SubscribeToPlayEnd(layer, OnEventEnd);

                    player.PlayEvents(cue.events, layer, cue.overrideDuration, forceInterrupt);
                }

                void OnPlaylistEnd () {   
                    //Debug.Log("playlist end");
                    Deactivate();
                }
                void OnEventEnd (bool success) {
                    //Debug.Log("cue end");
                    
                    Deactivate();
                }

                void Deactivate () {

                    //Debug.Log("on end message");

                    CustomScripting.ExecuteMessageBlockStep (layer, player, cue, runtimeTransform.position, Cue.MessageEvent.OnEnd);    
                    player.cueMoving = false;
                    player = null;
                    cue = null;
                    runtimeTransform = null;
                    isActive = false;

                }

                public void UpdateHead (int layer, EventPlayer player, Transform interestTransform, Cue cue) {
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
                public PlaylistHead head = new PlaylistHead();

                public void InitializeChannel (int layer, EventPlayer player, Playlist.Channel playlistChannel, Transform interestTransform) {
                    this.playlistChannel = playlistChannel;
                    this.player = player;

                    if (playlistChannel.useRandomChoice) {
                        List<int> enabledIndicies = new List<int>();
                        for (int i = 0; i < playlistChannel.cues.Length; i++) {
                            if (playlistChannel.cues[i].gameObject.activeSelf) {
                                enabledIndicies.Add(i);
                            }
                        }
                        cueIndex = enabledIndicies.RandomChoice();
                    }
                    else {
                        cueIndex = 0;   
                    }
                    while (cueIndex < playlistChannel.cues.Length && !playlistChannel.cues[cueIndex].gameObject.activeSelf) {      
                        //Debug.Log(playlistChannel.cues[cueIndex].name + " is inative");
                        cueIndex ++;
                    }
                    
                    curCueRepeats = 0;
                    isActive = true;
                    head.OnStart(playlistChannel.cues[cueIndex], layer, player, interestTransform);
                }   
                public void PlayCue (string debugReason, int layer, Transform interestTransform, bool asInterrupter) {
                    //Debug.Log("play cue: " + playlistChannel.childCues[cueIndex].name);
                    head.OnPlay(debugReason, layer, player, interestTransform, playlistChannel.cues[cueIndex], asInterrupter);   
                }
                public void UpdateChannel (int layer, Transform interestTransform) {
                    if (!isActive) return;   
                    head.UpdateHead(layer, player, interestTransform, playlistChannel.cues[cueIndex]);
                }
                public void OnCueEnd (int layer, Performance performance, Transform interestTransform) {
                    
                    curCueRepeats++;
                    
                    if (curCueRepeats < playlistChannel.cues[cueIndex].repeats) {
                        head.OnStart(playlistChannel.cues[cueIndex], layer, player, interestTransform);
                        return;
                    }

                    if (playlistChannel.useRandomChoice) {
                        OnPerformanceEnd(performance);
                        return;
                    }

                    
                    int l = playlistChannel.cues.Length;
                    
                    curCueRepeats = 0;
                    cueIndex++;
                    while (cueIndex < l && !playlistChannel.cues[cueIndex].gameObject.activeSelf) {                    
                        cueIndex ++;
                    }
                    if (cueIndex >= l) {
                        //Debug.Log("ending performance");
                        OnPerformanceEnd(performance);
                        return;
                    }
                    //Debug.Log("playing cue " + playlistChannel.childCues[cueIndex].name);
                    head.OnStart(playlistChannel.cues[cueIndex], layer, player, interestTransform);
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
            System.Action[] onEndPerformanceCallbacks;
            PerformanceChannel[] channels = new PerformanceChannel[0];
            bool forceInterrupt;

            
            EventPlayer[] orig_players;
            public Cue[] playlists;
            System.Action[] orig_performance_done_callback;
            bool looped;
            int useLayer;
            string debugReason;

            public void InterruptPerformance () {
                for (int i = 0; i < channels.Length; i++) {   
                    channels[i].OnPerformanceEnd(this);
                }
                //on_performance_done = null;
                AssetObjectsManager.ReturnPerformanceToPool(performance_key);
            }

            public void InitializePerformance (string debugReason, int useLayer, Cue[] playlists, Channel[] playlistChannels, EventPlayer[] players, Vector3 position, Quaternion rotation, bool looped, bool forceInterrupt, Action[] onEndPerformanceCallbacks) {
                this.onEndPerformanceCallbacks = onEndPerformanceCallbacks;
                this.playlists = playlists;
                this.looped = looped;
                this.useLayer = useLayer;
                this.forceInterrupt = forceInterrupt;
                this.debugReason = debugReason;

                if (forceInterrupt) {

                    for (int i = 0; i < players.Length; i++) {
                        players[i].InterruptLayer(useLayer, debugReason);
                    }
                }


                if (looped) {
                    orig_players = players;
                    orig_performance_done_callback = onEndPerformanceCallbacks;
                }

                if (!performance_root_transform) {
                    performance_root_transform = new GameObject("PerformanceRoot").transform;
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
                            Transform new_trans = new GameObject("ChannelRuntimeTransform").transform;
                            new_trans.SetParent(performance_root_transform);
                            interestTransforms.Add(new_trans);
                        }
                    }
                }

                for (int i = 0; i < channel_count; i++) {
                    players[i].current_playlists.Add(this);
                    channels[i].InitializeChannel (useLayer, players[i], playlistChannels[i], interestTransforms[i]);
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
                    if (!channel.head.positionReady) {
                        cuesReadySynced = false;
                    }
                    //check for end ready
                    if (channel.head.isActive || !channel.isActive) {
                        cuesDoneSynced = false;
                    }
                    //check if all ended
                    if (channel.isActive) {
                        allDone = false;
                    }
                }

                for (int i = 0; i < c; i++) {
                    PerformanceChannel channel = channels[i];
                    if (!channel.head.isPlaying && cuesReadySynced)
                        channel.PlayCue (debugReason, useLayer, interestTransforms[i], forceInterrupt);
                    if (cuesDoneSynced)
                        channel.OnCueEnd (useLayer, this, interestTransforms[i]);
                }

                if (allDone) {
                    if (onEndPerformanceCallbacks != null) {
                        for (int i = 0; i < onEndPerformanceCallbacks.Length; i++) {
                            if (onEndPerformanceCallbacks[i] != null) {
                                onEndPerformanceCallbacks[i]();
                            }
                        }
                    }
                    AssetObjectsManager.ReturnPerformanceToPool(performance_key);

                    if (looped) {
                        Playlist.InitializePerformance(debugReason, playlists, orig_players, looped, useLayer, performance_root_transform.position, performance_root_transform.rotation, forceInterrupt, orig_performance_done_callback);
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