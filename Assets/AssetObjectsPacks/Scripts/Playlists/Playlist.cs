using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;



namespace AssetObjectsPacks {
    public static class Playlist {

        // static void BroadcastMessageToPlayer (int layer, EventPlayer player, Transform runtimeTransform, CueBehavior cueBehavior, Cue.MessageEvent messageEvent) {
        //     if (cueBehavior == null) {
        //         return;
        //     }
        //     string stepBlock = cueBehavior.messageBlocks[(int)messageEvent];
        //     string logErrors = "";
        //     CustomScripting.ExecuteMessageBlock (layer, player, stepBlock, runtimeTransform.position, ref logErrors);
            
        //     if (!logErrors.IsEmpty()) {
        //         logErrors = (0, cueBehavior.name + " broadcast message " +  messageEvent.ToString()) + logErrors;
        //         Debug.LogError(logErrors);
        //     }
        // }


/*
        static Dictionary<int, Cue> cuePool = new Dictionary<int, Cue>();
        static Cue GetCue (Cue baseCue) {
            int id = baseCue.GetInstanceID();
            Cue cue;
            if (!cuePool.TryGetValue(id, out cue)) {
                cue = GameObject.Instantiate(baseCue.gameObject).GetComponent<Cue>();
                cuePool[id] = cue;
            }
            return cue;
        }
 */

        // class PerformancePoolHolder<T> where T : Playlist.Performance, new() {
        //     List<int> active_performances = new List<int>();
        //     Pool<T> performance_pool = new Pool<T>();

        //     public T GetNewPerformance () {
        //         int new_performance_key = performance_pool.GetNewObject();
        //         active_performances.Add(new_performance_key);
        //         T p = performance_pool[new_performance_key];
        //         p.SetPerformanceKey(new_performance_key);
        //         return p;
        //     }
        //     public void UpdatePerformances () {
        //         for (int i = 0; i < active_performances.Count; i++) {
        //             performance_pool[active_performances[i]].UpdatePerformance();
        //         }
        //     }
        //     public  void ReturnPerformanceToPool(int key) {
        //         performance_pool.ReturnToPool(key);
        //         active_performances.Remove(key);
        //     }
        // }


        // static PerformancePoolHolder<Playlist.Performance> playlistPerformances = new PerformancePoolHolder<Playlist.Performance>();
        // static PerformancePoolHolder<CueBehaviorPerformance> cueBehaviorPerformances = new PerformancePoolHolder<CueBehaviorPerformance>();
        // static PerformancePoolHolder<EventPerformance> eventPerformances = new PerformancePoolHolder<EventPerformance>();


        //static List<int> active_performances = new List<int>();
        //static Pool<Performance> performance_pool = new Pool<Performance>();
        /*
        static Performance GetNewPerformance () {
            int new_performance_key = performance_pool.GetNewObject();
            active_performances.Add(new_performance_key);
            Performance p = performance_pool[new_performance_key];
            p.SetPerformanceKey(new_performance_key);
            return p;
        }
         */

        // public static void UpdatePerformances () {
        //     playlistPerformances.UpdatePerformances();
            // cueBehaviorPerformances.UpdatePerformances();
            // eventPerformances.UpdatePerformances();
            
            //for (int i = 0; i < active_performances.Count; i++) {
            //    performance_pool[active_performances[i]].UpdatePerformance();
            //}
        // }
/*
        static void ReturnPerformanceToPool(int key) {
            performance_pool.ReturnToPool(key);
            active_performances.Remove(key);
        }

 */
        // static bool CueIsPlaylist(Cue cue) {
        //     int c = cue.transform.childCount;
        //     if (c == 0) {
        //         return false;
        //     }
        //     for (int i = 0; i < c; i++) {
        //         Transform t = cue.transform.GetChild(i);
        //         if (t.gameObject.activeSelf) {
        //             if (t.GetComponent<Cue>() != null) {
        //                 return true;
        //             }
        //         }
        //     }
        //     return false;
        // }

        //using this top level system enables messaging on the root cue
        //but now syncing only works for the root cue, not just first layer or others

        [System.Serializable] public class Channel {
            public Cue[] cues;
            public CueBehavior cueBehavior;
            public Event aoEvent;

            public bool useRandomChoice;
            // public int subCount;

            public Channel(CueBehavior cueBehavior) {
                cues = null;
                this.cueBehavior = cueBehavior;
                aoEvent = null;
            }
            public Channel(Event aoEvent) {
                cues = null;
                this.aoEvent = aoEvent;
                cueBehavior = null;
            }


            public Channel(Cue parentCue, bool topLevel) {
                if (topLevel) {
                    cues = new Cue[] { parentCue };
                    // subCount = 1;
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

                // subCount = cues.Length;
            }
        }

        static bool CheckCounts (int channelCount, int playerCount) {
            if (channelCount != playerCount) {
                Debug.LogError("playlist/player coutn mismatch: playlists: " + channelCount + " players: " + playerCount);
                return false;
            }               
            return true;
        }


        public static void InitializePerformance(string debugReason, CueBehavior playlists, EventPlayer players, bool looped, int playerLayer, MiniTransform transforms, bool forceInterrupt = true, Action onEndPerformanceCallbacks = null) {
            InitializePerformance(debugReason, new CueBehavior[] { playlists }, new EventPlayer[] { players }, looped, playerLayer, transforms, forceInterrupt, new Action[] { onEndPerformanceCallbacks });
        }
        public static void InitializePerformance(string debugReason, CueBehavior[] playlists, EventPlayer[] players, bool looped, int playerLayer, MiniTransform transforms, bool forceInterrupt = true, Action[] onEndPerformanceCallbacks = null) {
            if (!CheckCounts(playlists.Length, players.Length)) {
                return;
            }
            Playlists.Performance.playlistPerformances.GetNewPerformance().InitializePerformance (
                true, 
                
                debugReason,
                playerLayer,
                playlists.Generate( p => new Channel(p)).ToArray(),
                players, 
                transforms,
                looped, 
                forceInterrupt, 
                onEndPerformanceCallbacks
            );
        }

        
        public static void InitializePerformance(string debugReason, Event playlists, EventPlayer players, bool looped, int playerLayer, MiniTransform transforms, bool forceInterrupt = true, Action onEndPerformanceCallbacks = null) {
            InitializePerformance(debugReason, new Event[] { playlists }, new EventPlayer[] { players }, looped, playerLayer, transforms, forceInterrupt, new Action[] { onEndPerformanceCallbacks });
        }
        public static void InitializePerformance(string debugReason, Event[] playlists, EventPlayer[] players, bool looped, int playerLayer, MiniTransform transforms, bool forceInterrupt = true, Action[] onEndPerformanceCallbacks = null) {
            if (!CheckCounts(playlists.Length, players.Length)) {
                return;
            }
            Playlists.Performance.playlistPerformances.GetNewPerformance().InitializePerformance (
                true, 
                debugReason,
                playerLayer,
                playlists.Generate( p => new Channel(p)).ToArray(),
                players, 
                transforms,
                looped, 
                forceInterrupt, 
                onEndPerformanceCallbacks
            );
        }















        public static void InitializePerformance(string debugReason, Cue playlists, EventPlayer players, bool looped, int playerLayer, MiniTransform transforms, bool forceInterrupt = true, Action onEndPerformanceCallbacks = null) {
            InitializePerformance(debugReason, new Cue[] { playlists }, new EventPlayer[] { players }, looped, playerLayer, transforms, forceInterrupt, new Action[] { onEndPerformanceCallbacks });
        }
        public static void InitializePerformance(string debugReason, Cue[] playlists, EventPlayer[] players, bool looped, int playerLayer, MiniTransform transforms, bool forceInterrupt = true, Action[] onEndPerformanceCallbacks = null) {
            if (!CheckCounts(playlists.Length, players.Length)) {
                return;
            }
            Playlists.Performance.playlistPerformances.GetNewPerformance().InitializePerformance (
                true, 
                debugReason,
                playerLayer, 
                playlists.Generate( p => new Channel(p, true)).ToArray(), 
                players, 
                transforms,
                looped, 
                forceInterrupt, 
                onEndPerformanceCallbacks
            );
        }

        /// used for sub cues
        
        // static void InitializePerformanceInternal(string debugReason, int useLayer, Cue playlist, EventPlayer player, MiniTransform transforms, bool forceInterrupt, Action onEndPerformanceCallback) {
        //     playlistPerformances.GetNewPerformance().InitializePerformance (false, debugReason,
        //         useLayer, 
        //         new Channel[] { new Channel(playlist, false) }, 
        //         new EventPlayer[] { player }, 
        //         transforms,
        //         false, 
        //         forceInterrupt, 
        //         new Action[] { onEndPerformanceCallback }
        //     );
        // }

        










































        // //instance of playlists that play out at run time
        // public abstract class Performance {

        //     public void SetPerformanceKey (int key) {
        //         this.performanceKey = key;
        //     }

        //     Transform rootTransform;
        //     protected List<Transform> interestTransforms = new List<Transform>();
        //     Action[] onEndPerformance;
        //     protected bool forceInterrupt;
        //     public bool looped;
        //     protected EventPlayer[] players;
        //     int performanceKey;
        //     protected int useLayer;
        //     protected string debugReason;

        //     protected abstract PerformanceChannel[] GetPerformanceChannels ();
        //     protected abstract void ReturnPerformanceToPool(int key);
        //     public void InterruptPerformance () {
        //         PerformanceChannel[] chs = GetPerformanceChannels();
        //         for (int i = 0; i < chs.Length; i++) {   
        //             chs[i].OnPerformanceEnd(this, true);
        //         }
        //         ReturnPerformanceToPool(performanceKey);
        //     }


        //     protected void BaseInitialize (
        //         string debugReason, 
        //         int channelsCount,
        //         int useLayer, 
        //         EventPlayer[] players, 
        //         MiniTransform transforms,
        //         bool looped, 
        //         bool forceInterrupt, 
        //         Action[] onEndPerformance
        //     ) {
                
        //         this.onEndPerformance = onEndPerformance;
        //         this.looped = looped;
        //         this.useLayer = useLayer;
        //         this.forceInterrupt = forceInterrupt;
        //         this.debugReason = debugReason;
        //         this.players = players;
                
        //         if (forceInterrupt) {
        //             for (int i = 0; i < players.Length; i++) {
        //                 players[i].InterruptLayer(useLayer, debugReason);
        //             }
        //         }

        //         if (!rootTransform) {
        //             rootTransform = new GameObject("PerformanceRoot").transform;
        //         }
        //         rootTransform.position = transforms.pos;
        //         rootTransform.rotation = transforms.rot;
                
                
        //         if (interestTransforms.Count != channelsCount) {
        //             if (interestTransforms.Count < channelsCount) {
        //                 int c = channelsCount - interestTransforms.Count;
        //                 for (int i = 0; i < c; i++) {
        //                     Transform newChannelTransform = new GameObject("ChannelRuntimeTransform").transform;
        //                     newChannelTransform.SetParent(rootTransform);
        //                     interestTransforms.Add(newChannelTransform);
        //                 }
        //             }
        //         }

        //         for (int i = 0; i < channelsCount; i++) {
        //             players[i].currentPlaylists.Add(this);
        //         }
        //     }
        //     public abstract void UpdatePerformance();
        //     protected abstract void InitializeInternal();

        //     protected void CallCallbacks () {
        //         if (onEndPerformance != null) {
        //             for (int i = 0; i < onEndPerformance.Length; i++) {
        //                 if (onEndPerformance[i] != null) {
        //                     onEndPerformance[i]();
        //                 }
        //             }
        //         }
        //     }

        //     protected void OnEnd () {
        //         CallCallbacks();
        //         if (looped) {
        //             InitializeInternal();
        //         }
        //         else {
        //             ReturnPerformanceToPool(performanceKey);
        //         }
        //     }
        // }
        // //instance of playlists that play out at run time
        // public class PlaylistPerformance : Performance {
            
        //     PlaylistChannel[] channels = new PlaylistChannel[0];

        //     protected override PerformanceChannel[] GetPerformanceChannels() { return channels; }
        //     protected override void ReturnPerformanceToPool(int key) { playlistPerformances.ReturnPerformanceToPool(key); }
            
            
        //     Channel[] playlistChannels;
        //     bool isTopLevel;
            
        //     protected override void InitializeInternal () {
        //         int l = playlistChannels.Length;
        //         for (int i = 0; i < l; i++) {
        //             channels[i].InitializeChannel (isTopLevel, useLayer, players[i], playlistChannels[i], interestTransforms[i], this);
        //         }
        //     }

        //     public void InitializePerformance (bool isTopLevel, string debugReason, int useLayer, Channel[] playlistChannels, EventPlayer[] players, MiniTransform transforms, bool looped, bool forceInterrupt, Action[] onEndPerformance) {
                
        //         this.playlistChannels = playlistChannels;
        //         this.isTopLevel = isTopLevel;

        //         BaseInitialize(debugReason, playlistChannels.Length, useLayer, players, transforms, looped, forceInterrupt, onEndPerformance);

        //         int channelsCount = playlistChannels.Length;
        //         if (channels.Length != channelsCount) {
        //             channels = channelsCount.Generate(i => new PlaylistChannel()).ToArray();
        //         }

        //         for (int i = 0; i < channelsCount; i++) {
        //             channels[i].InitializeChannel (isTopLevel, useLayer, players[i], playlistChannels[i], interestTransforms[i], this);
        //         }
        //     }
        //     public override void UpdatePerformance () {
        //         bool cuesReadySynced = true;
        //         bool cuesDoneSynced = true;
        //         bool allDone = true;
        //         int c = channels.Length;

        //         for (int i = 0; i < c; i++) {
        //             PlaylistChannel channel = channels[i];

        //             //check for play ready
        //             if (!channel.head.positionReady) {
        //                 cuesReadySynced = false;
        //             }
        //             //check for end ready
        //             if (channel.head.isActive || !channel.isActive) {
        //                 cuesDoneSynced = false;
        //             }
        //             //check if all ended
        //             if (channel.isActive) {
        //                 allDone = false;
        //             }
        //         }

        //         for (int i = 0; i < c; i++) {
        //             PlaylistChannel channel = channels[i];
        //             if (!channel.head.isPlaying && cuesReadySynced)
        //                 channel.PlayCue (debugReason, forceInterrupt);
        //             if (cuesDoneSynced)
        //                 channel.OnCueEnd (isTopLevel, useLayer, this, interestTransforms[i]);
        //         }

        //         if (allDone) {
        //             OnEnd();
        //             return;
        //         }
        //         for (int i = 0; i < c; i++) {
        //             channels[i].UpdateChannel();
        //         }
        //     }
        // }
        // public class CueBehaviorPerformance : Performance {
        //     protected override PerformanceChannel[] GetPerformanceChannels() { return channels; }
        //     protected override void ReturnPerformanceToPool(int key) { cueBehaviorPerformances.ReturnPerformanceToPool(key); }
            
            
        //     CueBahaviorChannel[] channels = new CueBahaviorChannel[0];
        //     CueBehavior[] cueBehaviors;
            
        //     protected override void InitializeInternal () {
        //         int l = cueBehaviors.Length;
        //         for (int i = 0; i < l; i++) {
        //             channels[i].InitializeChannel (useLayer, players[i], cueBehaviors[i], interestTransforms[i], this);
        //         }
        //     }

        //     public void InitializePerformance (string debugReason, int useLayer, CueBehavior[] cueBehaviors, EventPlayer[] players, MiniTransform transforms, bool looped, bool forceInterrupt, Action[] onEndPerformance) {
                
        //         this.cueBehaviors = cueBehaviors;

        //         BaseInitialize(debugReason, cueBehaviors.Length, useLayer, players, transforms, looped, forceInterrupt, onEndPerformance);

        //         int channelsCount = cueBehaviors.Length;
        //         if (channels.Length != channelsCount) {
        //             channels = channelsCount.Generate(i => new CueBahaviorChannel()).ToArray();
        //         }

        //         for (int i = 0; i < channelsCount; i++) {
        //             channels[i].InitializeChannel (useLayer, players[i], cueBehaviors[i], interestTransforms[i], this);
        //         }
        //     }
        //     public override void UpdatePerformance () {
        //         bool cuesReadySynced = true;
        //         bool cuesDoneSynced = true;
        //         bool allDone = true;
        //         int c = channels.Length;

        //         for (int i = 0; i < c; i++) {
        //             CueBahaviorChannel channel = channels[i];

        //             //check for play ready
        //             if (!channel.head.positionReady) {
        //                 cuesReadySynced = false;
        //             }
        //             //check for end ready
        //             if (channel.head.isActive || !channel.isActive) {
        //                 cuesDoneSynced = false;
        //             }
        //             //check if all ended
        //             if (channel.isActive) {
        //                 allDone = false;
        //             }
        //         }

        //         for (int i = 0; i < c; i++) {
        //             CueBahaviorChannel channel = channels[i];
        //             if (!channel.head.isPlaying && cuesReadySynced)
        //                 channel.PlayCue (debugReason, forceInterrupt);
        //             if (cuesDoneSynced)
        //                 channel.OnCueEnd (this);
        //         }

        //         if (allDone) {
        //             OnEnd();
        //             return;
        //         }
        //         for (int i = 0; i < c; i++) {
        //             channels[i].UpdateChannel();
        //         }
        //     }
        // }
        // public class EventPerformance : Performance {
        //     protected override PerformanceChannel[] GetPerformanceChannels() { return channels; }
        //     protected override void ReturnPerformanceToPool(int key) { eventPerformances.ReturnPerformanceToPool(key); }
            
            
        //     EventChannel[] channels = new EventChannel[0];
        //     Event[] aoEvents;
            
        //     protected override void InitializeInternal () {
        //         int l = aoEvents.Length;
        //         for (int i = 0; i < l; i++) {
        //             channels[i].InitializeChannel (useLayer, players[i], aoEvents[i], interestTransforms[i]);
        //         }
        //     }

        //     public void InitializePerformance (string debugReason, int useLayer, Event[] aoEvents, EventPlayer[] players, MiniTransform transforms, bool looped, bool forceInterrupt, Action[] onEndPerformance) {
                
        //         this.aoEvents = aoEvents;

        //         BaseInitialize(debugReason, aoEvents.Length, useLayer, players, transforms, looped, forceInterrupt, onEndPerformance);

        //         int channelsCount = aoEvents.Length;
        //         if (channels.Length != channelsCount) {
        //             channels = channelsCount.Generate(i => new EventChannel()).ToArray();
        //         }

        //         for (int i = 0; i < channelsCount; i++) {
        //             channels[i].InitializeChannel (useLayer, players[i], aoEvents[i], interestTransforms[i]);
        //         }
        //     }
        //     public override void UpdatePerformance () {
        //         bool cuesDoneSynced = true;
        //         bool allDone = true;
        //         int c = channels.Length;

        //         for (int i = 0; i < c; i++) {
        //             EventChannel channel = channels[i];

        //             //check for end ready
        //             if (channel.head.isActive || !channel.isActive) {
        //                 cuesDoneSynced = false;
        //             }
        //             //check if all ended
        //             if (channel.isActive) {
        //                 allDone = false;
        //             }
        //         }

        //         for (int i = 0; i < c; i++) {
        //             EventChannel channel = channels[i];
        //             if (!channel.head.isPlaying)
        //                 channel.PlayCue (debugReason, forceInterrupt);
        //             if (cuesDoneSynced)
        //                 channel.OnCueEnd (this);
        //         }

        //         if (allDone) {
        //             OnEnd();
        //             return;
        //         }
        //     }
        // }



        // public abstract class PerformanceHead {
        //     public bool isActive, isPlaying;
        //     protected int layer;
        //     protected Transform runtimeTransform;
        //     protected EventPlayer player;
            
        //     public void BaseOnStart (int layer, EventPlayer player, Transform runtimeTransform) {
                
        //         this.player = player;
        //         this.layer = layer;
        //         this.runtimeTransform = runtimeTransform;
                
        //         isActive = true;
        //         isPlaying = false;

        //         Vector3 lPos = Vector3.zero;
        //         Quaternion lRot = Quaternion.identity;
                
        //         runtimeTransform.localPosition = Vector3.zero;
        //         //maybe zero out x and z rotation
        //         runtimeTransform.localRotation = Quaternion.identity;
        //     }

        //     public abstract void OnPlay (string debugReason, bool forceInterrupt);



        //     protected void OnEventEnd (bool success) {
        //         //Debug.Log("cue end");
        //         Deactivate();
        //     }

        //     protected virtual void Deactivate () {
        //         //Debug.Log("on end message");
        //         player = null;
        //         runtimeTransform = null;
        //         isActive = false;
        //     }
        // }
        // public abstract class BehaviorTrackerHead : PerformanceHead {
        //     public bool positionReady { get { return posReady || playImmediate; } }
        //     Vector3 initialPlayerPos;
        //     Quaternion initialPlayerRot;
        //     float smooth_l0, smooth_l1, smooth_l0v, smooth_l1v;
        //     bool playImmediate, posReady;
        //     protected CueBehavior cueBehavior;

        //     void EndSnapPlayer () {
        //         player.transform.position = runtimeTransform.position;
        //         player.transform.rotation = runtimeTransform.rotation;
        //         player.cueMoving = false;
        //         posReady = true;      
        //     }
        //     void CheckReadyTransform () {
        //         if (posReady) 
        //             return;
                
        //         if (cueBehavior == null) {
        //             EndSnapPlayer();
        //         }
                
        //         smooth_l0 = Mathf.SmoothDamp(smooth_l0, 1.0f, ref smooth_l0v, cueBehavior.smoothPositionTime);
        //         smooth_l1 = Mathf.SmoothDamp(smooth_l1, 1.0f, ref smooth_l1v, cueBehavior.smoothRotationTime);
                
        //         player.transform.position = Vector3.Lerp(initialPlayerPos, runtimeTransform.position, smooth_l0);
        //         player.transform.rotation = Quaternion.Slerp(initialPlayerRot, runtimeTransform.rotation, smooth_l1);
                
        //         float threshold = .99f;
                
        //         if (smooth_l0 > threshold && smooth_l1 > threshold) {
        //             EndSnapPlayer();
        //             BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnSnap);
        //         }
        //     }  

        //     protected void InitializeBehaviorTrack (CueBehavior cueBehavior) {
        //         this.cueBehavior = cueBehavior;
        //         this.playImmediate = cueBehavior != null && cueBehavior.playImmediate;
        //         posReady = true;
                
        //     }  

        //     protected void InitializeSnap () {
        //         //Debug.Log("on start " + layer + " " + cue.name);
        //         BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnStart);
                
        //         if (cueBehavior != null) {

        //             switch (cueBehavior.snapPlayerStyle) {
        //                 case Cue.SnapPlayerStyle.Snap:
        //                     player.transform.position = runtimeTransform.position;
        //                     player.transform.rotation = runtimeTransform.rotation;
        //                     BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnSnap);
        //                     break;
        //                 case Cue.SnapPlayerStyle.Smooth:
        //                     posReady = false;
        //                     player.cueMoving = true;

        //                     initialPlayerPos = player.transform.position;
        //                     initialPlayerRot = player.transform.rotation;
        //                     smooth_l0 = smooth_l1 = 0;
        //                     break;
        //             }
        //         }

        //     }  
        //     public void UpdateHead () {
        //         if (!isActive) 
        //             return;
        //         CheckReadyTransform();
        //     }
        //     protected override void Deactivate () {
        //         BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnEnd);
        //         player.cueMoving = false;
        //         base.Deactivate();
        //         cueBehavior = null;
        //         //Debug.Log("on end message");
        //     }

        //     protected void PlayBehavior (bool forceInterrupt) {
        //         //Debug.Log("playign cue" + cueBehavior.name);
        //         player.SubscribeToPlayEnd(layer, OnEventEnd);
        //         player.PlayEvents(new MiniTransform(runtimeTransform, false), cueBehavior != null ? cueBehavior.events : null, layer, cueBehavior != null ? cueBehavior.overrideDuration : -1, forceInterrupt);
        //     }
                
        // }

        // public class PlaylistHead : BehaviorTrackerHead {
        //     Cue cue;
            
        //     public void OnStart (bool isTopLevel, Cue cue, int layer, EventPlayer player, Transform runtimeTransform) {
                
        //         this.cue = cue;
        //         BaseOnStart(layer, player, runtimeTransform);
        //         InitializeBehaviorTrack(cue.behavior);
                
        //         Vector3 lPos = Vector3.zero;
        //         Quaternion lRot = Quaternion.identity;
                
        //         if (!isTopLevel) {
        //             if (cueBehavior == null) {
        //                 lPos = cue.transform.localPosition;
        //                 lRot = cue.transform.localRotation;
        //             }
        //             else {
        //                 lPos = cue.transform.localPosition + cueBehavior.positionOffset;
        //                 lRot = Quaternion.Euler(cue.transform.localRotation.eulerAngles + cueBehavior.rotationOffset);
        //             }
        //         }
        //         //maybe zero out x and z rotation
        //         runtimeTransform.localRotation = lRot;
        //         runtimeTransform.localPosition = lPos;

        //         InitializeSnap();
        //     }

        //     public override void OnPlay (string debugReason, bool forceInterrupt) {
        //         isPlaying = true;

        //         BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnPlay);
                
        //         if (CueIsPlaylist(cue)) {
        //             Debug.Log("playign cue playlist " + cue.name);
        //             Playlist.InitializePerformanceInternal(debugReason + "/" + cue.name, layer, cue, player, new MiniTransform(runtimeTransform, false), forceInterrupt, OnPlaylistEnd);
        //         }
        //         else {
        //             PlayBehavior(forceInterrupt);
        //         }
        //     }
        //     void OnPlaylistEnd () {   
        //         Debug.Log("playlist end");
        //         Deactivate();
        //     }
            

        //     protected override void Deactivate () {
        //         base.Deactivate();
        //         cue = null;
        //     }
        // }
        // public class CueBehaviorHead : BehaviorTrackerHead {
            
        //     public void OnStart (CueBehavior cueBehavior, int layer, EventPlayer player, Transform runtimeTransform) {   
        //         BaseOnStart(layer, player, runtimeTransform);
        //         InitializeBehaviorTrack(cueBehavior);
        //         InitializeSnap();                
        //     }
        //     public override void OnPlay (string debugReason, bool forceInterrupt) {
        //         isPlaying = true;
        //         BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnPlay);
        //         PlayBehavior(forceInterrupt);
        //     }
        // }
        // public class EventHead : PerformanceHead {
        //     Event aoEvent;
        //     public void OnStart (Event aoEvent, int layer, EventPlayer player, Transform runtimeTransform) {
        //         BaseOnStart(layer, player, runtimeTransform);
        //         this.aoEvent = aoEvent;   
        //     }
        //     public override void OnPlay (string debugReason, bool forceInterrupt) {
        //         isPlaying = true;
        //         //Debug.Log("playign cue" + aoEvent.name);
        //         player.SubscribeToPlayEnd(layer, OnEventEnd);
        //         player.PlayEvents(new MiniTransform(runtimeTransform, false), new Event[] { aoEvent }, layer, -1, forceInterrupt);
        //     }
        // }





    //     public abstract class PerformanceChannel {
    //         public bool isActive;
    //         public EventPlayer player;

    //         protected abstract PerformanceHead GetPerformanceHead();
                
    //         public void OnPerformanceEnd (Performance performance, bool force) {
    //             if (!performance.looped || force) {
    //                 if (player) {
    //                     player.currentPlaylists.Remove(performance);
    //                     player = null;
    //                 }
    //             }
    //             isActive = false;
    //         }
    //         public void PlayCue (string debugReason, bool asInterrupter) {
    //             //Debug.Log("play cue: " + playlistChannel.childCues[cueIndex].name);
    //             GetPerformanceHead().OnPlay(debugReason, asInterrupter);   
    //         }
            
    //     }

    //     public class PlaylistChannel : PerformanceChannel {
    //         int cueIndex, curCueRepeats;
    //         Playlist.Channel playlistChannel;
    //         public PlaylistHead head = new PlaylistHead();

    //         protected override PerformanceHead GetPerformanceHead() { return head; }

    //         public void InitializeChannel (bool isTopLevel, int layer, EventPlayer player, Playlist.Channel playlistChannel, Transform interestTransform, Performance performance) {
    //             this.playlistChannel = playlistChannel;
    //             this.player = player;

    //             //only if it has subcues
    //             if (playlistChannel.useRandomChoice) {
    //                 List<int> enabledIndicies = new List<int>();
    //                 for (int i = 0; i < playlistChannel.cues.Length; i++) {
    //                     if (playlistChannel.cues[i].gameObject.activeSelf) {
    //                         enabledIndicies.Add(i);
    //                     }
    //                     i++;
    //                 }
    //                 cueIndex = enabledIndicies.RandomChoice();
    //             }
    //             else {
    //                 cueIndex = 0;   
    //             }

    //             if (!isTopLevel) {
    //                 if (SkipAheadPastDisabledCues(performance)) {
    //                     return;
    //                 }
    //             }
    //             curCueRepeats = 0;
    //             isActive = true;
    //             head.OnStart(isTopLevel, playlistChannel.cues[cueIndex], layer, player, interestTransform);
    //         }   
    //         public void UpdateChannel () {
    //             if (!isActive) return;   
    //             head.UpdateHead();
    //         }
    //         bool SkipAheadPastDisabledCues (Performance performance) {

    //             int l = playlistChannel.cues.Length;
    //             while (cueIndex < l && !playlistChannel.cues[cueIndex].gameObject.activeSelf) {                    
    //                 cueIndex ++;
    //             }
    //             if (cueIndex >= l) {
    //                 //Debug.Log("ending performance");
    //                 OnPerformanceEnd(performance, false);
    //                 return true;
    //             }
    //             return false;
    //         }
            
    //         public void OnCueEnd (bool isTopLevel, int layer, Performance performance, Transform interestTransform) {
                
    //             curCueRepeats++;

    //             Cue cueAtIndex = playlistChannel.cues[cueIndex];
                
    //             if (curCueRepeats < cueAtIndex.repeats) {
    //                 head.OnStart(isTopLevel, cueAtIndex, layer, player, interestTransform);
    //                 return;
    //             }
                
    //             if (playlistChannel.useRandomChoice) {
    //                 OnPerformanceEnd(performance, false);
    //                 return;
    //             }

    //             int l = playlistChannel.subCount;
                
    //             curCueRepeats = 0;
    //             cueIndex++;

    //             if (SkipAheadPastDisabledCues(performance)) {
    //                 return;
    //             }

    //             //Debug.Log("playing cue " + playlistChannel.childCues[cueIndex].name);
    //             head.OnStart(isTopLevel, playlistChannel.cues[cueIndex], layer, player, interestTransform);
    //         }
    //     }


    //     public class CueBahaviorChannel : PerformanceChannel {
    //         public CueBehaviorHead head = new CueBehaviorHead();
    //         protected override PerformanceHead GetPerformanceHead() { return head; }


    //         public void InitializeChannel (int layer, EventPlayer player, CueBehavior cueBehavior, Transform interestTransform, Performance performance) {
    //             this.player = player;
    //             isActive = true;
    //             head.OnStart(cueBehavior, layer, player, interestTransform);
    //         }   
    //         public void UpdateChannel () {
    //             if (!isActive) return;   
    //             head.UpdateHead();
    //         }
    //         public void OnCueEnd (Performance performance) {   
    //             OnPerformanceEnd(performance, false);
    //         }
    //     }
    //     public class EventChannel : PerformanceChannel {
    //         public EventHead head = new EventHead();
    //         protected override PerformanceHead GetPerformanceHead() { return head; }

    //         public void InitializeChannel (int layer, EventPlayer player, Event aoEvent, Transform interestTransform) {
    //             this.player = player;
    //             isActive = true;
    //             head.OnStart(aoEvent, layer, player, interestTransform);
    //         }   
    //         public void OnCueEnd (Performance performance) {   
    //             OnPerformanceEnd(performance, false);
    //         }
    //     }
    // }
}
}