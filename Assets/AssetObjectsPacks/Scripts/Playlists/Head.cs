using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AssetObjectsPacks {

    namespace Playlists {

        public class PerformanceHead {
            static void InitializePerformanceInternal(string debugReason, int useLayer, Cue playlist, EventPlayer player, MiniTransform transforms, bool forceInterrupt, System.Action onEndPerformanceCallback) {
                Performance.playlistPerformances.GetNewPerformance().InitializePerformance (
                    false, debugReason,
                    useLayer, 
                    new Playlist.Channel[] { new Playlist.Channel(playlist, false) }, 
                    new EventPlayer[] { player }, 
                    transforms,
                    false, 
                    forceInterrupt, 
                    new System.Action[] { onEndPerformanceCallback }
                );
            }

            static void BroadcastMessageToPlayer (int layer, EventPlayer player, MiniTransform runtimeTransform, CueBehavior cueBehavior, Cue.MessageEvent messageEvent) {
            
                if (cueBehavior == null) {
                    return;
                }

                
                string stepBlock = cueBehavior.messageBlocks[(int)messageEvent];
                string logErrors = "";
                CustomScripting.ExecuteMessageBlock (layer, player, stepBlock, runtimeTransform.pos, ref logErrors);
                
                if (!logErrors.IsEmpty()) {
                    logErrors = (0, cueBehavior.name + " broadcast message " +  messageEvent.ToString()) + logErrors;
                    Debug.LogError(logErrors);
                }
            }

            
            public bool isActive, isPlaying;
            protected int layer;
            MiniTransform runtimeTransform;
            protected EventPlayer player;

            public bool positionReady { get { return posReady || playImmediate; } }
            Vector3 initialPlayerPos;
            Quaternion initialPlayerRot;
            float smooth_l0, smooth_l1, smooth_l0v, smooth_l1v;
            bool playImmediate, posReady;
            CueBehavior cueBehavior;
            Cue cue;
            Event aoEvent;
            

            void EndSnapPlayer () {
                player.transform.position = runtimeTransform.pos;
                player.transform.rotation = runtimeTransform.rot;
                player.cueMoving = false;
                posReady = true;      
            }
            void CheckReadyTransform () {
                if (posReady) 
                    return;
                
                if (cueBehavior == null) {
                    EndSnapPlayer();
                    return;
                }
                
                smooth_l0 = Mathf.SmoothDamp(smooth_l0, 1.0f, ref smooth_l0v, cueBehavior.smoothPositionTime);
                smooth_l1 = Mathf.SmoothDamp(smooth_l1, 1.0f, ref smooth_l1v, cueBehavior.smoothRotationTime);
                
                player.transform.position = Vector3.Lerp(initialPlayerPos, runtimeTransform.pos, smooth_l0);
                player.transform.rotation = Quaternion.Slerp(initialPlayerRot, runtimeTransform.rot, smooth_l1);
                
                float threshold = .99f;
                
                if (smooth_l0 > threshold && smooth_l1 > threshold) {
                    EndSnapPlayer();
                    BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnSnap);
                }
            }  


            public void UpdateHead () {
                if (!isActive) 
                    return;
                CheckReadyTransform();
            }
            void Deactivate () {

                if (cueBehavior != null) {
                    BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnEnd);
                }
                
                player.cueMoving = false;
                cueBehavior = null;
                cue = null;
                aoEvent = null;
                player = null;
                isActive = false;
            }
            
            void OnEventEnd (bool success) {
                Deactivate();
            }

            void InitializeSnap () {
                
                if (cueBehavior != null) {

                    switch (cueBehavior.snapPlayerStyle) {
                        case Cue.SnapPlayerStyle.Snap:
                            player.transform.position = runtimeTransform.pos;
                            player.transform.rotation = runtimeTransform.rot;
                            BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnSnap);
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
            }  

            void OnPlaylistEnd () {   
                Deactivate();
            }

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

            public void OnPlay (string debugReason, bool forceInterrupt) {
                isPlaying = true;

                if (cueBehavior != null) {
                    BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnPlay);
                }

                if (cue != null && CueIsPlaylist(cue)) {
                    InitializePerformanceInternal(debugReason + "/" + cue.name, layer, cue, player, runtimeTransform, forceInterrupt, OnPlaylistEnd);
                }
                else {
                    player.SubscribeToPlayEnd(layer, OnEventEnd);

                    Event[] eventsToPlay = null;
                    if (cue != null) {
                        if (cueBehavior != null) {
                            eventsToPlay = cueBehavior.events;
                        }
                    }
                    else {
                        eventsToPlay = cueBehavior != null ? cueBehavior.events : new Event[] { aoEvent };
                    }
                        
                    player.PlayEvents(runtimeTransform, eventsToPlay, layer, cueBehavior != null ? cueBehavior.overrideDuration : -1, forceInterrupt);                


                }
            }
            
            
            bool isTopLevel;
            public void OnStart (bool isTopLevel, Playlist.Channel playlistChannel, int cueIndex, int layer, EventPlayer player, MiniTransform suppliedTransform, Transform interestTransform) {
                this.cue = null;
                this.cueBehavior = null;
                this.aoEvent = null;
                this.isTopLevel = isTopLevel;

                // playing physical cue
                if (playlistChannel.cues != null) {
                    this.cue = playlistChannel.cues[cueIndex];
                    this.cueBehavior = this.cue.behavior;
                }
                // just playing cue behavior
                else if (playlistChannel.cueBehavior != null) {
                    this.cueBehavior = playlistChannel.cueBehavior;
                }
                // just playing simple event
                else {
                    this.aoEvent = playlistChannel.aoEvent;
                }

                this.player = player;
                this.layer = layer;
                
                isActive = true;
                isPlaying = false;
                
                this.playImmediate = cueBehavior != null && cueBehavior.playImmediate;
                posReady = true;
                
                // if we're loading a sub cue of a cue playlist
                if (!isTopLevel) {
                    Vector3 lPos = Vector3.zero;
                    Quaternion lRot = Quaternion.identity;
                    
                    if (cueBehavior == null) {
                        lPos = cue.transform.localPosition;
                        lRot = cue.transform.localRotation;
                    }
                    else {
                        lPos = cue.transform.localPosition + cueBehavior.positionOffset;
                        lRot = Quaternion.Euler(cue.transform.localRotation.eulerAngles + cueBehavior.rotationOffset);
                    }
                    
                    //maybe zero out x and z rotation
                    interestTransform.localRotation = lRot;
                    interestTransform.localPosition = lPos;

                    suppliedTransform = new MiniTransform(interestTransform, false);
                }

                this.runtimeTransform = suppliedTransform;

                if (cueBehavior != null) {
                    BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnStart);
                }
                
                InitializeSnap();
            }
           
        }
    }
}
