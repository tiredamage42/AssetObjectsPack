using UnityEngine;

namespace Game {        
    [CreateAssetMenu()]
    public class GameSettingsHolder : ScriptableObject
    {
        public GameSettings[] gameSettings;

        public T GetGameSettings<T> () where T : GameSettings {
            for (int i = 0; i < gameSettings.Length; i++) {
                T asType = gameSettings[i] as T;
                if (asType != null) {
                    return asType;
                }
            }
            Debug.LogError("Couldnt find game settings type: " + typeof(T).ToString());
            return null;
        }
    }
    public abstract class GameSettings : ScriptableObject
    {
        public static T GetGameSettings<T> () where T : GameSettings {

            GameSettingsHolderSceneRef scenRefInstance = GameSettingsHolderSceneRef.instance;
            if (scenRefInstance == null) 
                return null;
            
            if (scenRefInstance.gameSettings == null) 
                return null;
            
            return scenRefInstance.gameSettings.GetGameSettings<T>();
        }
    }
}
