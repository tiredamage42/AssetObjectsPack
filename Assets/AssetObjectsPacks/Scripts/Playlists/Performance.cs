using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace AssetObjectsPacks {
    namespace Playlists {


        class PerformanceCoroutineHandler : MonoBehaviour {

            

        }

        
        

        //instance of playlists that play out at run time
        public class Performance {
            static PerformanceCoroutineHandler _coroutineHandler;
            static PerformanceCoroutineHandler coroutineHandler {
                get {
                    if (_coroutineHandler == null) {
                        _coroutineHandler = new GameObject("PerformanceCoroutineHandler").AddComponent<PerformanceCoroutineHandler>();
                    }
                    return _coroutineHandler;
                }
            }
            


            public class PerformancePoolHolder<T> where T : Playlists.Performance, new() {
            // List<int> active_performances = new List<int>();
            Pool<T> performance_pool = new Pool<T>();

            public T GetNewPerformance () {
                int new_performance_key = performance_pool.GetNewObject();
                // active_performances.Add(new_performance_key);
                T p = performance_pool[new_performance_key];
                p.SetPerformanceKey(new_performance_key);
                return p;
            }
            // public void UpdatePerformances () {
            //     for (int i = 0; i < active_performances.Count; i++) {
            //         performance_pool[active_performances[i]].UpdatePerformance();
            //     }
            // }
            public  void ReturnPerformanceToPool(int key) {
                performance_pool.ReturnToPool(key);
                // active_performances.Remove(key);
            }
        }


        public static PerformancePoolHolder<Playlists.Performance> playlistPerformances = new PerformancePoolHolder<Playlists.Performance>();

            public static void UpdatePerformances () {
            //    playlistPerformances.UpdatePerformances();
                // cueBehaviorPerformances.UpdatePerformances();
                // eventPerformances.UpdatePerformances();
                
                //for (int i = 0; i < active_performances.Count; i++) {
                //    performance_pool[active_performances[i]].UpdatePerformance();
                //}
            }

            public void SetPerformanceKey (int key) {
                this.performanceKey = key;
            }

            Transform rootTransform;
            protected List<Transform> interestTransforms = new List<Transform>();
            System.Action[] onEndPerformance;
            protected bool forceInterrupt;
            public bool looped;
            protected EventPlayer[] players;
            int performanceKey;
            protected int useLayer;
            protected string debugReason;

            PerformanceChannel[] channels = new PerformanceChannel[0];

            void ReturnPerformanceToPool(int key) { playlistPerformances.ReturnPerformanceToPool(key); }
            

            // per channel
            // CueBehavior[] cueBehaviors;
            // Event[] aoEvents;
            Playlist.Channel[] playlistChannels;
            // per channel

            MiniTransform suppliedTransform;


            bool isTopLevel;


            public void InterruptPerformance () {
                // PerformanceChannel[] chs = GetPerformanceChannels();
                for (int i = 0; i < channels.Length; i++) {   
                    channels[i].OnPerformanceEnd(this, true);
                }
                ReturnPerformanceToPool(performanceKey);
            }


            // protected void BaseInitialize (
            //     string debugReason, 
            //     int channelsCount,
            //     int useLayer, 
            //     EventPlayer[] players, 
            //     MiniTransform transforms,
            //     bool looped, 
            //     bool forceInterrupt, 
            //     System.Action[] onEndPerformance
            // ) {
                
                
                
            // }
            // public abstract void UpdatePerformance();
            // protected abstract void InitializeInternal();

            protected void CallCallbacks () {
                if (onEndPerformance != null) {
                    for (int i = 0; i < onEndPerformance.Length; i++) {
                        if (onEndPerformance[i] != null) {
                            onEndPerformance[i]();
                        }
                    }
                }
            }

            
           
            
            // protected override void InitializeInternal () {
            //     int l = cueBehaviors.Length;
            //     for (int i = 0; i < l; i++) {
            //         channels[i].InitializeChannel (useLayer, players[i], cueBehaviors[i], interestTransforms[i], this);
            //     }
            // }
            // protected override void InitializeInternal () {
            //     int l = aoEvents.Length;
            //     for (int i = 0; i < l; i++) {
            //         channels[i].InitializeChannel (useLayer, players[i], aoEvents[i], interestTransforms[i]);
            //     }
            // }


            public void InitializePerformance (bool isTopLevel, string debugReason, int useLayer, Playlist.Channel[] playlistChannels, EventPlayer[] players, MiniTransform transforms, bool looped, bool forceInterrupt, System.Action[] onEndPerformance) {
                this.suppliedTransform = transforms;
                this.playlistChannels = playlistChannels;
                this.isTopLevel = isTopLevel;

                this.onEndPerformance = onEndPerformance;
                this.looped = looped;
                this.useLayer = useLayer;
                this.forceInterrupt = forceInterrupt;
                this.debugReason = debugReason;
                this.players = players;


                if (forceInterrupt) {
                    for (int i = 0; i < players.Length; i++) {
                        players[i].InterruptLayer(useLayer, debugReason);
                    }
                }

                if (!rootTransform) {
                    rootTransform = new GameObject("PerformanceRoot").transform;
                }
                //maybe parent
                rootTransform.position = transforms.pos;
                rootTransform.rotation = transforms.rot;
                
                
                int channelsCount = playlistChannels.Length;
                if (interestTransforms.Count != channelsCount) {
                    if (interestTransforms.Count < channelsCount) {
                        int c = channelsCount - interestTransforms.Count;
                        for (int i = 0; i < c; i++) {
                            Transform newChannelTransform = new GameObject("ChannelRuntimeTransform").transform;
                            newChannelTransform.SetParent(rootTransform);
                            interestTransforms.Add(newChannelTransform);
                        }
                    }
                }

                for (int i = 0; i < channelsCount; i++) {
                    players[i].currentPlaylists.Add(this);
                }
                

                // BaseInitialize(debugReason, playlistChannels.Length, useLayer, players, transforms, looped, forceInterrupt, onEndPerformance);

                if (channels.Length != channelsCount) {
                    channels = channelsCount.Generate(i => new PerformanceChannel()).ToArray();
                }

                ReInitializeChannels();
                coroutineHandler.StartCoroutine(UpdatePerformance());
            
            }
                
            void ReInitializeChannels () {
                int l = playlistChannels.Length;
                for (int i = 0; i < l; i++) {
                    channels[i].InitializeChannel (isTopLevel, useLayer, players[i], playlistChannels[i], suppliedTransform, interestTransforms[i], this);
                }
            }
            
            // public void InitializePerformance (string debugReason, int useLayer, Event[] aoEvents, EventPlayer[] players, MiniTransform transforms, bool looped, bool forceInterrupt, Action[] onEndPerformance) {
                
            //     this.aoEvents = aoEvents;

            //     BaseInitialize(debugReason, aoEvents.Length, useLayer, players, transforms, looped, forceInterrupt, onEndPerformance);

            //     int channelsCount = aoEvents.Length;
            //     if (channels.Length != channelsCount) {
            //         channels = channelsCount.Generate(i => new EventChannel()).ToArray();
            //     }

            //     for (int i = 0; i < channelsCount; i++) {
            //         channels[i].InitializeChannel (useLayer, players[i], aoEvents[i], interestTransforms[i]);
            //     }
            // }
            
            // public void InitializePerformance (string debugReason, int useLayer, CueBehavior[] cueBehaviors, EventPlayer[] players, MiniTransform transforms, bool looped, bool forceInterrupt, Action[] onEndPerformance) {
                
            //     this.cueBehaviors = cueBehaviors;

            //     BaseInitialize(debugReason, cueBehaviors.Length, useLayer, players, transforms, looped, forceInterrupt, onEndPerformance);

            //     int channelsCount = cueBehaviors.Length;
            //     if (channels.Length != channelsCount) {
            //         channels = channelsCount.Generate(i => new CueBahaviorChannel()).ToArray();
            //     }

            //     for (int i = 0; i < channelsCount; i++) {
            //         channels[i].InitializeChannel (useLayer, players[i], cueBehaviors[i], interestTransforms[i], this);
            //     }
            // }


            //public void UpdatePerformance () {
            IEnumerator UpdatePerformance () {
                bool allDone = false;
                while (!allDone) {

                
                
                bool cuesReadySynced = true;
                bool cuesDoneSynced = true;
                allDone = true;
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
                        channel.PlayCue (debugReason, forceInterrupt);
                    if (cuesDoneSynced)
                        channel.OnCueEnd (isTopLevel, useLayer, this, suppliedTransform, interestTransforms[i]);
                }

                if (allDone) {
                    // CallCallbacks();
                    // if (looped) {
                    //     ReInitializeChannels();
                    // }
                    // else {
                    //     ReturnPerformanceToPool(performanceKey);
                    // }
                }
                else {
                    for (int i = 0; i < c; i++) {
                        channels[i].UpdateChannel();
                    }
                }
                yield return null;
                }

                //done with performance
                CallCallbacks();
                if (looped) {
                    ReInitializeChannels();
                    coroutineHandler.StartCoroutine(UpdatePerformance());


                }
                else {
                    ReturnPerformanceToPool(performanceKey);
                }

            }
            
            

            
            // public override void UpdatePerformance () {
            //     bool cuesReadySynced = true;
            //     bool cuesDoneSynced = true;
            //     bool allDone = true;
            //     int c = channels.Length;

            //     for (int i = 0; i < c; i++) {
            //         CueBahaviorChannel channel = channels[i];

            //         //check for play ready
            //         if (!channel.head.positionReady) {
            //             cuesReadySynced = false;
            //         }
            //         //check for end ready
            //         if (channel.head.isActive || !channel.isActive) {
            //             cuesDoneSynced = false;
            //         }
            //         //check if all ended
            //         if (channel.isActive) {
            //             allDone = false;
            //         }
            //     }

            //     for (int i = 0; i < c; i++) {
            //         CueBahaviorChannel channel = channels[i];
            //         if (!channel.head.isPlaying && cuesReadySynced)
            //             channel.PlayCue (debugReason, forceInterrupt);
            //         if (cuesDoneSynced)
            //             channel.OnCueEnd (this);
            //     }

            //     if (allDone) {
            //         OnEnd();
            //         return;
            //     }
            //     for (int i = 0; i < c; i++) {
            //         channels[i].UpdateChannel();
            //     }
            // }
            
            
            // public override void UpdatePerformance () {
            //     bool cuesDoneSynced = true;
            //     bool allDone = true;
            //     int c = channels.Length;

            //     for (int i = 0; i < c; i++) {
            //         EventChannel channel = channels[i];

            //         //check for end ready
            //         if (channel.head.isActive || !channel.isActive) {
            //             cuesDoneSynced = false;
            //         }
            //         //check if all ended
            //         if (channel.isActive) {
            //             allDone = false;
            //         }
            //     }

            //     for (int i = 0; i < c; i++) {
            //         EventChannel channel = channels[i];
            //         if (!channel.head.isPlaying)
            //             channel.PlayCue (debugReason, forceInterrupt);
            //         if (cuesDoneSynced)
            //             channel.OnCueEnd (this);
            //     }

            //     if (allDone) {
            //         OnEnd();
            //         return;
            //     }
            // }
        }
        //instance of playlists that play out at run time
        // public class PlaylistPerformance : Performance {
            
            // PlaylistChannel[] channels = new PlaylistChannel[0];

            // protected override PerformanceChannel[] GetPerformanceChannels() { return channels; }
            // protected override void ReturnPerformanceToPool(int key) { playlistPerformances.ReturnPerformanceToPool(key); }
            
            
            // Channel[] playlistChannels;
            // bool isTopLevel;
            
            // protected override void InitializeInternal () {
            //     int l = playlistChannels.Length;
            //     for (int i = 0; i < l; i++) {
            //         channels[i].InitializeChannel (isTopLevel, useLayer, players[i], playlistChannels[i], interestTransforms[i], this);
            //     }
            // }

            // public void InitializePerformance (bool isTopLevel, string debugReason, int useLayer, Channel[] playlistChannels, EventPlayer[] players, MiniTransform transforms, bool looped, bool forceInterrupt, Action[] onEndPerformance) {
                
            //     this.playlistChannels = playlistChannels;
            //     this.isTopLevel = isTopLevel;

            //     BaseInitialize(debugReason, playlistChannels.Length, useLayer, players, transforms, looped, forceInterrupt, onEndPerformance);

            //     int channelsCount = playlistChannels.Length;
            //     if (channels.Length != channelsCount) {
            //         channels = channelsCount.Generate(i => new PlaylistChannel()).ToArray();
            //     }

            //     for (int i = 0; i < channelsCount; i++) {
            //         channels[i].InitializeChannel (isTopLevel, useLayer, players[i], playlistChannels[i], interestTransforms[i], this);
            //     }
            // }
            // public override void UpdatePerformance () {
            //     bool cuesReadySynced = true;
            //     bool cuesDoneSynced = true;
            //     bool allDone = true;
            //     int c = channels.Length;

            //     for (int i = 0; i < c; i++) {
            //         PlaylistChannel channel = channels[i];

            //         //check for play ready
            //         if (!channel.head.positionReady) {
            //             cuesReadySynced = false;
            //         }
            //         //check for end ready
            //         if (channel.head.isActive || !channel.isActive) {
            //             cuesDoneSynced = false;
            //         }
            //         //check if all ended
            //         if (channel.isActive) {
            //             allDone = false;
            //         }
            //     }

            //     for (int i = 0; i < c; i++) {
            //         PlaylistChannel channel = channels[i];
            //         if (!channel.head.isPlaying && cuesReadySynced)
            //             channel.PlayCue (debugReason, forceInterrupt);
            //         if (cuesDoneSynced)
            //             channel.OnCueEnd (isTopLevel, useLayer, this, interestTransforms[i]);
            //     }

            //     if (allDone) {
            //         OnEnd();
            //         return;
            //     }
            //     for (int i = 0; i < c; i++) {
            //         channels[i].UpdateChannel();
            //     }
            // }
        // }
        // public class CueBehaviorPerformance : Performance {
            // protected override PerformanceChannel[] GetPerformanceChannels() { return channels; }
            // protected override void ReturnPerformanceToPool(int key) { cueBehaviorPerformances.ReturnPerformanceToPool(key); }
            
            
            // CueBahaviorChannel[] channels = new CueBahaviorChannel[0];
            // CueBehavior[] cueBehaviors;
            
            // protected override void InitializeInternal () {
            //     int l = cueBehaviors.Length;
            //     for (int i = 0; i < l; i++) {
            //         channels[i].InitializeChannel (useLayer, players[i], cueBehaviors[i], interestTransforms[i], this);
            //     }
            // }

            // public void InitializePerformance (string debugReason, int useLayer, CueBehavior[] cueBehaviors, EventPlayer[] players, MiniTransform transforms, bool looped, bool forceInterrupt, Action[] onEndPerformance) {
                
            //     this.cueBehaviors = cueBehaviors;

            //     BaseInitialize(debugReason, cueBehaviors.Length, useLayer, players, transforms, looped, forceInterrupt, onEndPerformance);

            //     int channelsCount = cueBehaviors.Length;
            //     if (channels.Length != channelsCount) {
            //         channels = channelsCount.Generate(i => new CueBahaviorChannel()).ToArray();
            //     }

            //     for (int i = 0; i < channelsCount; i++) {
            //         channels[i].InitializeChannel (useLayer, players[i], cueBehaviors[i], interestTransforms[i], this);
            //     }
            // }
            // public override void UpdatePerformance () {
            //     bool cuesReadySynced = true;
            //     bool cuesDoneSynced = true;
            //     bool allDone = true;
            //     int c = channels.Length;

            //     for (int i = 0; i < c; i++) {
            //         CueBahaviorChannel channel = channels[i];

            //         //check for play ready
            //         if (!channel.head.positionReady) {
            //             cuesReadySynced = false;
            //         }
            //         //check for end ready
            //         if (channel.head.isActive || !channel.isActive) {
            //             cuesDoneSynced = false;
            //         }
            //         //check if all ended
            //         if (channel.isActive) {
            //             allDone = false;
            //         }
            //     }

            //     for (int i = 0; i < c; i++) {
            //         CueBahaviorChannel channel = channels[i];
            //         if (!channel.head.isPlaying && cuesReadySynced)
            //             channel.PlayCue (debugReason, forceInterrupt);
            //         if (cuesDoneSynced)
            //             channel.OnCueEnd (this);
            //     }

            //     if (allDone) {
            //         OnEnd();
            //         return;
            //     }
            //     for (int i = 0; i < c; i++) {
            //         channels[i].UpdateChannel();
            //     }
            // }
        // }
        // public class EventPerformance : Performance {
            // protected override PerformanceChannel[] GetPerformanceChannels() { return channels; }
            // protected override void ReturnPerformanceToPool(int key) { eventPerformances.ReturnPerformanceToPool(key); }
            
            
            // EventChannel[] channels = new EventChannel[0];
            // Event[] aoEvents;
            
            // protected override void InitializeInternal () {
            //     int l = aoEvents.Length;
            //     for (int i = 0; i < l; i++) {
            //         channels[i].InitializeChannel (useLayer, players[i], aoEvents[i], interestTransforms[i]);
            //     }
            // }

            // public void InitializePerformance (string debugReason, int useLayer, Event[] aoEvents, EventPlayer[] players, MiniTransform transforms, bool looped, bool forceInterrupt, Action[] onEndPerformance) {
                
            //     this.aoEvents = aoEvents;

            //     BaseInitialize(debugReason, aoEvents.Length, useLayer, players, transforms, looped, forceInterrupt, onEndPerformance);

            //     int channelsCount = aoEvents.Length;
            //     if (channels.Length != channelsCount) {
            //         channels = channelsCount.Generate(i => new EventChannel()).ToArray();
            //     }

            //     for (int i = 0; i < channelsCount; i++) {
            //         channels[i].InitializeChannel (useLayer, players[i], aoEvents[i], interestTransforms[i]);
            //     }
            // }
            // public override void UpdatePerformance () {
            //     bool cuesDoneSynced = true;
            //     bool allDone = true;
            //     int c = channels.Length;

            //     for (int i = 0; i < c; i++) {
            //         EventChannel channel = channels[i];

            //         //check for end ready
            //         if (channel.head.isActive || !channel.isActive) {
            //             cuesDoneSynced = false;
            //         }
            //         //check if all ended
            //         if (channel.isActive) {
            //             allDone = false;
            //         }
            //     }

            //     for (int i = 0; i < c; i++) {
            //         EventChannel channel = channels[i];
            //         if (!channel.head.isPlaying)
            //             channel.PlayCue (debugReason, forceInterrupt);
            //         if (cuesDoneSynced)
            //             channel.OnCueEnd (this);
            //     }

            //     if (allDone) {
            //         OnEnd();
            //         return;
            //     }
            // }
        // }

    }
}
