using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace AssetObjectsPacks {

    namespace Playlists {

        class PerformanceCoroutineHandler : MonoBehaviour { }

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
                Pool<T> performance_pool = new Pool<T>();

                public T GetNewPerformance () {
                    int new_performance_key = performance_pool.GetNewObject();
                    T p = performance_pool[new_performance_key];
                    p.SetPerformanceKey(new_performance_key);
                    return p;
                }
                public  void ReturnPerformanceToPool(int key) {
                    performance_pool.ReturnToPool(key);
                }
            }

            public static PerformancePoolHolder<Playlists.Performance> playlistPerformances = new PerformancePoolHolder<Playlists.Performance>();

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

            

            Playlist.Channel[] playlistChannels;
            
            MiniTransform suppliedTransform;

            bool isTopLevel;

            void ReturnPerformanceToPool(int key) { playlistPerformances.ReturnPerformanceToPool(key); }
            
            public void InterruptPerformance () {
                for (int i = 0; i < channels.Length; i++) {   
                    channels[i].OnPerformanceEnd(this, true);
                }
                ReturnPerformanceToPool(performanceKey);
            }

            void CallCallbacks () {
                if (onEndPerformance != null) {
                    for (int i = 0; i < onEndPerformance.Length; i++) {
                        if (onEndPerformance[i] != null) {
                            onEndPerformance[i]();
                        }
                    }
                }
            }

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

                    if (!allDone) {
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
        }
    }
}
