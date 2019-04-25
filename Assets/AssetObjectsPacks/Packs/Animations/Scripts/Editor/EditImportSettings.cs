using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks.Animations {    
    /*
        Use to adjust import settings on multiple selected animations
    */
    public class EditImportSettings : ScriptableWizard {
        public bool lockRootHeightY = true;
        public bool lockRootPositionXZ = false;
        public bool lockRootRotation = false;
        public bool keepOriginalOrientation = true;
        public bool keepOriginalPositionXZ = true;
        public bool keepOriginalPositionY = false;
        public bool heightFromFeet = true;
        [HideInInspector] public Object[] objects;

        [MenuItem("Animations Pack/Adjust Animation Import Settings")]
        public static void CreateWizard() {
            CreateWizard(Selection.objects);
        }
        public static void CreateWizard(Object[] objects) {
            EditImportSettings w = ScriptableWizard.DisplayWizard<EditImportSettings>("Adjust Import Settings", "Adjust");
            w.objects = objects;
        }

        void OnWizardCreate() {
            for (int i = 0; i < objects.Length; i++) {
                FixSettings(AssetDatabase.GetAssetPath(objects[i]));
            }
        }
        void FixSettings (string file_path) {

            ModelImporter importer = AssetImporter.GetAtPath(file_path) as ModelImporter;
            if (importer == null) {
                Debug.LogError("Importer null!");
                return;
            }
            if (importer.animationType != ModelImporterAnimationType.Human) {
                Debug.LogError("Avatar must be human!");

                //importer.animationType = UnityEditor.ModelImporterAnimationType.Human;
                //importer.sourceAvatar = source_avatar;
            
                return;
            }

            ModelImporterClipAnimation animation = GetModelImporterClip (importer);
            animation.lockRootHeightY = lockRootHeightY;
            animation.lockRootPositionXZ = lockRootPositionXZ;
            animation.lockRootRotation = lockRootRotation;
            animation.keepOriginalOrientation = keepOriginalOrientation;
            animation.keepOriginalPositionXZ = keepOriginalPositionXZ;
            animation.keepOriginalPositionY = keepOriginalPositionY;
            animation.heightFromFeet = heightFromFeet;
            animation.loopPose = false;
            animation.loopTime = true;
            animation.loop = true;
            
            /*

                For Human types animations, you must use importer.defaultClipAnimations to get all animations.
                defaultClipAnimations is a property, so you need first assign it to a local variable. 
                then change the values and reassign the local variable to importer.clipAnimations.
            
            */
            
            importer.clipAnimations = new ModelImporterClipAnimation[] { animation };
            AssetDatabase.ImportAsset((importer.assetPath));
        
        }   
        
        static ModelImporterClipAnimation GetModelImporterClip(ModelImporter mi) {
            ModelImporterClipAnimation clip = null;

            if( mi.clipAnimations.Length == 0 ) {
                //if the animation was never manually changed and saved, we get here. Check defaultClipAnimations
                if( mi.defaultClipAnimations.Length > 0 )
                    clip = mi.defaultClipAnimations[0];
                else
                    Debug.LogError("GetModelImporterClip can't find clip information");
            }
            else
                clip = mi.clipAnimations[0];
            return clip;
        }
    }



}




