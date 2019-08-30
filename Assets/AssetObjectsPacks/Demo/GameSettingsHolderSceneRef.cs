using UnityEngine;

namespace Game {

    public class GameSettingsHolderSceneRef : MonoBehaviour {
        public GameSettingsHolder gameSettings;
        static GameSettingsHolderSceneRef _i;
        public static GameSettingsHolderSceneRef instance {
            get {
                if (_i == null) {
                    _i = GameObject.FindObjectOfType<GameSettingsHolderSceneRef>();
                    if (_i == null) {
                        Debug.LogError("No GameSettingsHolderSceneRef instance in the scene");
                    }
                    else {
                        if (_i.gameSettings == null) {
                            Debug.LogError("GameSettingsHolderSceneRef instance game settings is null");
                        }
                    }
                }
                return _i;
            }
        }
    }
}