using UnityEngine;
namespace AssetObjectsPacks {
    public class SoloMute 
    {
        GUIContent muteGUI = new GUIContent("", "Mute");
        GUIContent soloGUI = new GUIContent("", "Solo");
        
        public const string soloField = "solo", muteField = "mute";

        Color32 soloOff = new Color32(84,114,87,255); 
        Color32 muteOff = new Color32(123,97,67,255);
        Color32 soloOn = Colors.green;
        Color32 muteOn = Colors.yellow; 

        public void DrawEventStateSoloMuteElement (EditorProp solodMutedList, int i) {
            EditorProp ao = solodMutedList[i];
            bool changedMute;
            bool newMute = GUIUtils.SmallToggleButton(muteGUI, ao[muteField].boolValue, muteOn, muteOff, out changedMute );
            if (changedMute) {
                ao[muteField].SetValue( newMute );
                if (newMute) ao[soloField].SetValue(false);
            }
            bool changedSolo;
            bool newSolo = GUIUtils.SmallToggleButton(soloGUI, ao[soloField].boolValue, soloOn, soloOff, out changedSolo );
            if (changedSolo) {
                ao[soloField].SetValue(newSolo);
                if (newSolo) {
                    ao[muteField].SetValue( false );
                    for (int x = 0; x < solodMutedList.arraySize; x++) {
                        if (x == i) continue;
                        solodMutedList[x][soloField].SetValue(false);
                    }
                }
            }
            /*
             */
        }
    }
}
