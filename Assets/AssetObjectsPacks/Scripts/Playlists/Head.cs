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

            //static void BroadcastMessageToPlayer (int layer, EventPlayer player, Transform runtimeTransform, CueBehavior cueBehavior, Cue.MessageEvent messageEvent) {
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
            // protected Transform runtimeTransform;
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

            
            

            // protected void InitializeBehaviorTrack (CueBehavior cueBehavior) {
            //     this.cueBehavior = cueBehavior;
            //     this.playImmediate = cueBehavior != null && cueBehavior.playImmediate;
            //     posReady = true;
            // }  
                


            // public abstract void OnPlay (string debugReason, bool forceInterrupt);


            public void UpdateHead () {
                if (!isActive) 
                    return;
                CheckReadyTransform();
            }
            void Deactivate () {

                if (cueBehavior != null) {
                    // Debug.Log("on end message");
                    
                    BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnEnd);
                }
                
                player.cueMoving = false;
                // base.Deactivate();
                cueBehavior = null;
                cue = null;
                aoEvent = null;
                player = null;
                // runtimeTransform = null;
                isActive = false;
            }
            // protected virtual void Deactivate () {
            //     //Debug.Log("on end message");
            // }
            // protected override void Deactivate () {
            //     base.Deactivate();
            // }


            void OnEventEnd (bool success) {
                // Debug.Log("on event end");
                Deactivate();
            }


            protected void InitializeSnap () {
                
                if (cueBehavior != null) {

                    //Debug.Log("on start " + layer + " " + cue.name);
                    // BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnStart);
                
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
                Debug.Log("playlist end");
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

                // if (!isTopLevel) {
                //     suppliedTransform = new MiniTransform(runTimeTransform, false);
                // }


                if (cueBehavior != null) {
                    BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnPlay);
                }

                
                if (cue != null && CueIsPlaylist(cue)) {
                    Debug.Log("playign cue playlist " + cue.name);
                    //InitializePerformanceInternal(debugReason + "/" + cue.name, layer, cue, player, new MiniTransform(runtimeTransform, false), forceInterrupt, OnPlaylistEnd);
                    InitializePerformanceInternal(debugReason + "/" + cue.name, layer, cue, player, runtimeTransform, forceInterrupt, OnPlaylistEnd);
                }
                else {
                    //Debug.Log("playign cue" + cueBehavior.name);
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
                        
                    //player.PlayEvents(new MiniTransform(runtimeTransform, false), eventsToPlay, layer, cueBehavior != null ? cueBehavior.overrideDuration : -1, forceInterrupt);                
                    player.PlayEvents(runtimeTransform, eventsToPlay, layer, cueBehavior != null ? cueBehavior.overrideDuration : -1, forceInterrupt);                


                }
            }
            // public void OnStart (Event aoEvent, int layer, EventPlayer player, Transform runtimeTransform) {
            //     BaseOnStart(layer, player, runtimeTransform);
            //     this.aoEvent = aoEvent;   
            // }
            // public void OnStart (CueBehavior cueBehavior, int layer, EventPlayer player, Transform runtimeTransform) {   
            //     BaseOnStart(layer, player, runtimeTransform);
            //     InitializeBehaviorTrack(cueBehavior);
            //     InitializeSnap();                
            // }


            
            bool isTopLevel;
            public void OnStart (bool isTopLevel, Playlist.Channel playlistChannel, int cueIndex, int layer, EventPlayer player, MiniTransform suppliedTransform, Transform interestTransform) {
                this.cue = null;
                this.cueBehavior = null;
                this.aoEvent = null;
                this.isTopLevel = isTopLevel;



                // playing physical cue
                if (playlistChannel.cues != null) {
                // if (cueIndex >= 0) {
                    this.cue = playlistChannel.cues[cueIndex];
                    this.cueBehavior = this.cue.behavior;
                }
                // just playing cue behavior
                else if (playlistChannel.cueBehavior != null) {
                //else if (cueIndex == -1) {
                
                    this.cueBehavior = playlistChannel.cueBehavior;
                }
                // just playing simple event
                else {// if (cueIndex == -2) {

                    this.aoEvent = playlistChannel.aoEvent;
                }

                // this.cue = cue;
                
                this.player = player;
                this.layer = layer;
                // this.runtimeTransform = runtimeTransform;
                
                isActive = true;
                isPlaying = false;

                // Vector3 lPos = Vector3.zero;
                // Quaternion lRot = Quaternion.identity;
                
                // runtimeTransform.localPosition = Vector3.zero;
                // //maybe zero out x and z rotation
                // runtimeTransform.localRotation = Quaternion.identity;
                
                
                // InitializeBehaviorTrack(cue.behavior);
                // this.cueBehavior = cue.behavior;
                
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

                    DebugTransform.instance.transform.position = interestTransform.position;
                    // Debug.Break();
                    suppliedTransform = new MiniTransform(interestTransform, false);
                    
                }

                this.runtimeTransform = suppliedTransform;

                if (cueBehavior != null) {

                    //Debug.Log("on start " + layer + " " + cue.name);
                    BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnStart);
                }
                

                InitializeSnap();

                // return suppliedTransform;
            }
            

            
            // public override void OnPlay (string debugReason, bool forceInterrupt) {
            //     isPlaying = true;
            //     BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnPlay);
            //     PlayBehavior(forceInterrupt);
            // }
            // public override void OnPlay (string debugReason, bool forceInterrupt) {
            //     isPlaying = true;
            //     //Debug.Log("playign cue" + aoEvent.name);
            //     player.SubscribeToPlayEnd(layer, OnEventEnd);
            //     player.PlayEvents(new MiniTransform(runtimeTransform, false), new Event[] { aoEvent }, layer, -1, forceInterrupt);
            // }

            
        }
        // public abstract class BehaviorTrackerHead : PerformanceHead {
            // public bool positionReady { get { return posReady || playImmediate; } }
            // Vector3 initialPlayerPos;
            // Quaternion initialPlayerRot;
            // float smooth_l0, smooth_l1, smooth_l0v, smooth_l1v;
            // bool playImmediate, posReady;
            // protected CueBehavior cueBehavior;

            // void EndSnapPlayer () {
            //     player.transform.position = runtimeTransform.position;
            //     player.transform.rotation = runtimeTransform.rotation;
            //     player.cueMoving = false;
            //     posReady = true;      
            // }
            // void CheckReadyTransform () {
            //     if (posReady) 
            //         return;
                
            //     if (cueBehavior == null) {
            //         EndSnapPlayer();
            //     }
                
            //     smooth_l0 = Mathf.SmoothDamp(smooth_l0, 1.0f, ref smooth_l0v, cueBehavior.smoothPositionTime);
            //     smooth_l1 = Mathf.SmoothDamp(smooth_l1, 1.0f, ref smooth_l1v, cueBehavior.smoothRotationTime);
                
            //     player.transform.position = Vector3.Lerp(initialPlayerPos, runtimeTransform.position, smooth_l0);
            //     player.transform.rotation = Quaternion.Slerp(initialPlayerRot, runtimeTransform.rotation, smooth_l1);
                
            //     float threshold = .99f;
                
            //     if (smooth_l0 > threshold && smooth_l1 > threshold) {
            //         EndSnapPlayer();
            //         BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnSnap);
            //     }
            // }  

            // protected void InitializeBehaviorTrack (CueBehavior cueBehavior) {
            //     this.cueBehavior = cueBehavior;
            //     this.playImmediate = cueBehavior != null && cueBehavior.playImmediate;
            //     posReady = true;
                
            // }  

            // protected void InitializeSnap () {
            //     //Debug.Log("on start " + layer + " " + cue.name);
            //     BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnStart);
                
            //     if (cueBehavior != null) {

            //         switch (cueBehavior.snapPlayerStyle) {
            //             case Cue.SnapPlayerStyle.Snap:
            //                 player.transform.position = runtimeTransform.position;
            //                 player.transform.rotation = runtimeTransform.rotation;
            //                 BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnSnap);
            //                 break;
            //             case Cue.SnapPlayerStyle.Smooth:
            //                 posReady = false;
            //                 player.cueMoving = true;

            //                 initialPlayerPos = player.transform.position;
            //                 initialPlayerRot = player.transform.rotation;
            //                 smooth_l0 = smooth_l1 = 0;
            //                 break;
            //         }
            //     }

            // }  
            // public void UpdateHead () {
            //     if (!isActive) 
            //         return;
            //     CheckReadyTransform();
            // }
            // protected override void Deactivate () {
            //     BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnEnd);
            //     player.cueMoving = false;
            //     base.Deactivate();
            //     cueBehavior = null;
            //     //Debug.Log("on end message");
            // }

            // protected void PlayBehavior (bool forceInterrupt) {
            //     //Debug.Log("playign cue" + cueBehavior.name);
            //     player.SubscribeToPlayEnd(layer, OnEventEnd);
            //     player.PlayEvents(new MiniTransform(runtimeTransform, false), cueBehavior != null ? cueBehavior.events : null, layer, cueBehavior != null ? cueBehavior.overrideDuration : -1, forceInterrupt);
            // }
                
        // }

        // public class PlaylistHead : BehaviorTrackerHead {
            // Cue cue;
            
            // public void OnStart (bool isTopLevel, Cue cue, int layer, EventPlayer player, Transform runtimeTransform) {
                
            //     this.cue = cue;
            //     BaseOnStart(layer, player, runtimeTransform);
            //     InitializeBehaviorTrack(cue.behavior);
                
            //     Vector3 lPos = Vector3.zero;
            //     Quaternion lRot = Quaternion.identity;
                
            //     if (!isTopLevel) {
            //         if (cueBehavior == null) {
            //             lPos = cue.transform.localPosition;
            //             lRot = cue.transform.localRotation;
            //         }
            //         else {
            //             lPos = cue.transform.localPosition + cueBehavior.positionOffset;
            //             lRot = Quaternion.Euler(cue.transform.localRotation.eulerAngles + cueBehavior.rotationOffset);
            //         }
            //     }
            //     //maybe zero out x and z rotation
            //     runtimeTransform.localRotation = lRot;
            //     runtimeTransform.localPosition = lPos;

            //     InitializeSnap();
            // }

            // public override void OnPlay (string debugReason, bool forceInterrupt) {
            //     isPlaying = true;

            //     BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnPlay);
                
            //     if (CueIsPlaylist(cue)) {
            //         Debug.Log("playign cue playlist " + cue.name);
            //         Playlist.InitializePerformanceInternal(debugReason + "/" + cue.name, layer, cue, player, new MiniTransform(runtimeTransform, false), forceInterrupt, OnPlaylistEnd);
            //     }
            //     else {
            //         PlayBehavior(forceInterrupt);
            //     }
            // }
            // void OnPlaylistEnd () {   
            //     Debug.Log("playlist end");
            //     Deactivate();
            // }
            

            // protected override void Deactivate () {
            //     base.Deactivate();
            //     cue = null;
            // }
        // }
        // public class CueBehaviorHead : BehaviorTrackerHead {
            
            // public void OnStart (CueBehavior cueBehavior, int layer, EventPlayer player, Transform runtimeTransform) {   
            //     BaseOnStart(layer, player, runtimeTransform);
            //     InitializeBehaviorTrack(cueBehavior);
            //     InitializeSnap();                
            // }
            // public override void OnPlay (string debugReason, bool forceInterrupt) {
            //     isPlaying = true;
            //     BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnPlay);
            //     PlayBehavior(forceInterrupt);
            // }
        // }
        // public class EventHead : PerformanceHead {
            // Event aoEvent;
            // public void OnStart (Event aoEvent, int layer, EventPlayer player, Transform runtimeTransform) {
            //     BaseOnStart(layer, player, runtimeTransform);
            //     this.aoEvent = aoEvent;   
            // }
            // public override void OnPlay (string debugReason, bool forceInterrupt) {
            //     isPlaying = true;
            //     //Debug.Log("playign cue" + aoEvent.name);
            //     player.SubscribeToPlayEnd(layer, OnEventEnd);
            //     player.PlayEvents(new MiniTransform(runtimeTransform, false), new Event[] { aoEvent }, layer, -1, forceInterrupt);
            // }
        // }

}
}
