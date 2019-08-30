using UnityEngine;

namespace AssetObjectsPacks.Playlists {
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
            if (cueBehavior == null) 
                return;

            string stepBlock = cueBehavior.messageBlocks[(int)messageEvent];
            string logErrors = "";
            CustomScripting.ExecuteMessageBlock (layer, player, stepBlock, runtimeTransform.pos, ref logErrors);
            
            if (!logErrors.IsEmpty()) {
                logErrors = (0, cueBehavior.name + " broadcast message " +  messageEvent.ToString()) + logErrors;
                Debug.LogError(logErrors);
            }
        }

        public bool isActive, isPlaying;
        int layer;
        MiniTransform runtimeTransform, initialPlayerTransform;
        EventPlayer player;
        bool playImmediate, posReady;
        float smooth_l0, smooth_l1, smooth_l0v, smooth_l1v;
        CueBehavior cueBehavior;
        public bool positionReady { get { return posReady || playImmediate; } }
        
        void EndSnapPlayer () {
            runtimeTransform.SetTransform(player.transform);        
            player.cueMoving = false;
            posReady = true;      
        }
        void CheckReadyTransform (CueBehavior cueBehavior) {
            if (posReady) 
                return;
            
            if (cueBehavior == null) {
                EndSnapPlayer();
                return;
            }
            
            smooth_l0 = Mathf.SmoothDamp(smooth_l0, 1.0f, ref smooth_l0v, cueBehavior.smoothPositionTime);
            smooth_l1 = Mathf.SmoothDamp(smooth_l1, 1.0f, ref smooth_l1v, cueBehavior.smoothRotationTime);
            
            player.transform.position = Vector3.Lerp(initialPlayerTransform.pos, runtimeTransform.pos, smooth_l0);
            player.transform.rotation = Quaternion.Slerp(initialPlayerTransform.rot, runtimeTransform.rot, smooth_l1);
            
            const float threshold = .99f;
            
            if (smooth_l0 > threshold && smooth_l1 > threshold) {
                EndSnapPlayer();
                BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnSnap);
            }
        }  


        public void UpdateHead (CueBehavior cueBehavior) {
            if (!isActive) 
                return;
            CheckReadyTransform( cueBehavior);
        }

        void OnEventEnd (bool success) {
            Deactivate();
        }
        void OnPlaylistEnd () {   
            Deactivate();
        }
        void Deactivate () {
            BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnEnd);    
            player.cueMoving = false;
            cueBehavior = null;
            player = null;
            isActive = false;
        }
        static bool CueIsPlaylist(Cue cue) {
            int c = cue.transform.childCount;
            if (c == 0) return false;
            
            for (int i = 0; i < c; i++) {
                Transform t = cue.transform.GetChild(i);
                if (t.gameObject.activeSelf && t.GetComponent<Cue>() != null) {
                    return true;
                }
            }
            return false;
        }

        public void OnPlay (string debugReason, bool forceInterrupt, Cue cue, CueBehavior cueBehavior, Event[] aoEvents) {
            isPlaying = true;

            BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnPlay);
            
            if (cue != null && CueIsPlaylist(cue)) {
                InitializePerformanceInternal(debugReason + " / " + cue.name, layer, cue, player, runtimeTransform, forceInterrupt, OnPlaylistEnd);
            }
            else {

                Event[] eventsToPlay = cueBehavior != null ? cueBehavior.events : aoEvents;
                string eventsName = cue != null ? cue.name : (cueBehavior != null ? cueBehavior.name : aoEvents[0].name);
                    
                player.SubscribeToPlayEnd(layer, OnEventEnd);
                player.PlayEvents(debugReason, eventsName, runtimeTransform, eventsToPlay, layer, cueBehavior != null ? cueBehavior.overrideDuration : -1, forceInterrupt);                
            }
        }
        
        public void OnStart (bool isTopLevel, Playlist.Channel playlistChannel, Cue cue, CueBehavior cueBehavior, Event[] aoEvents, int layer, EventPlayer player, MiniTransform suppliedTransform)
        {

            this.cueBehavior = cueBehavior;
            this.player = player;
            this.layer = layer;
            this.playImmediate = cueBehavior != null && cueBehavior.playImmediate;
            
            isActive = true;
            posReady = true;
            isPlaying = false;
            
            // if we're loading a sub cue of a cue playlist
            if (!isTopLevel) {

                Vector3 posOffset = cueBehavior != null ? cueBehavior.positionOffset : Vector3.zero;
                Vector3 rotOffset = cueBehavior != null ? cueBehavior.rotationOffset : Vector3.zero;
                
                Vector3 lPos = cue.transform.localPosition;
                if (posOffset != Vector3.zero) lPos += posOffset;
                
                Quaternion lRot = cue.transform.localRotation;
                if (rotOffset != Vector3.zero) lRot = Quaternion.Euler(lRot.eulerAngles + rotOffset);
            
                suppliedTransform = new MiniTransform(suppliedTransform.pos + (suppliedTransform.rot * lPos), suppliedTransform.rot * lRot);
            }
            
            this.runtimeTransform = suppliedTransform;

            BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnStart);
            
            InitializeSnap();
        }

        void InitializeSnap () {
            
            if (cueBehavior == null) 
                return;
            
            switch (cueBehavior.snapPlayerStyle) {
                case Cue.SnapPlayerStyle.Snap:
                    runtimeTransform.SetTransform(player.transform);
                    BroadcastMessageToPlayer (layer, player, runtimeTransform, cueBehavior, Cue.MessageEvent.OnSnap);
                    break;
                case Cue.SnapPlayerStyle.Smooth:
                    posReady = false;
                    player.cueMoving = true;
                    initialPlayerTransform.CopyTransform(player.transform);
                    smooth_l0 = smooth_l1 = 0;
                    break;
            }
        }  
    }
}