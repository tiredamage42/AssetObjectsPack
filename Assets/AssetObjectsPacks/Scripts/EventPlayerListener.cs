using System.Collections.Generic;
using UnityEngine;

namespace AssetObjectsPacks {

    [RequireComponent(typeof(EventPlayer))]
    public abstract class EventPlayerListener : MonoBehaviour
    {
        protected EventPlayer eventPlayer;

        protected virtual void Awake () {
            InitializeEventPlayer();
        }
        
        protected abstract string ListenForPackName ();
        
        void InitializeEventPlayer () {
            eventPlayer = GetComponent<EventPlayer>();

            //tell the event player to call this component's "Use Asset Object" method
            //whenever it plays an event that uses the pack name
            eventPlayer.LinkAsPlayer(ListenForPackName(), UseAssetObject);
        }

        /*
            called when the attached event player plays an 
            event with the pack name this ocmponent uses
            and chooses an appropriate asset object

            endUseCallbacks should be called whenever the animation is done
        */
        protected abstract void UseAssetObject(AssetObject ao, bool asInterrupter, MiniTransform transforms, HashSet<System.Action> endUse);

    }
}
