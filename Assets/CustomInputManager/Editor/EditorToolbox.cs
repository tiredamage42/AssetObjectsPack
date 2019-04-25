using UnityEngine;
using UnityEditor;
using System.IO;
using CustomInputManager;
using System;
using UnityInputConverter;


namespace CustomInputManagerEditor.IO
{
	public static class EditorToolbox
	{
		private static string m_snapshotFile;
        private static string[] m_buttonNames, m_axisNames;
		
        public static string[] GenerateJoystickButtonNames()
        {
            if(m_buttonNames == null || m_buttonNames.Length != InputBinding.MAX_JOYSTICK_BUTTONS)
            {
                m_buttonNames = new string[InputBinding.MAX_JOYSTICK_BUTTONS];
                for(int i = 0; i < InputBinding.MAX_JOYSTICK_BUTTONS; i++)
                {
                    m_buttonNames[i] = "Joystick Button " + i;
                }
            }

            return m_buttonNames;
        }

		public static string[] GenerateJoystickAxisNames()
		{
			if(m_axisNames == null || m_axisNames.Length != InputBinding.MAX_JOYSTICK_AXES)
			{
				m_axisNames = new string[InputBinding.MAX_JOYSTICK_AXES];
				for(int i = 0; i < InputBinding.MAX_JOYSTICK_AXES; i++)
				{
					if(i == 0)
						m_axisNames[i] = "X";
					else if(i == 1)
						m_axisNames[i] = "Y";
					else if(i == 2)
						m_axisNames[i] = "3rd axis (Joysticks and Scrollwheel)";
					else if(i == 21)
						m_axisNames[i] = "21st axis (Joysticks)";
					else if(i == 22)
						m_axisNames[i] = "22nd axis (Joysticks)";
					else if(i == 23)
						m_axisNames[i] = "23rd axis (Joysticks)";
					else
						m_axisNames[i] = string.Format("{0}th axis (Joysticks)", i + 1);
				}
			}

			return m_axisNames;
		}


		public static bool CanLoadSnapshot()
		{
			if(m_snapshotFile == null)
			{
				m_snapshotFile = Path.Combine(Application.temporaryCachePath, "input_config.xml");
			}
			
			return File.Exists(m_snapshotFile);
		}
		
		public static void CreateSnapshot(InputManager inputManager)
		{
			if(m_snapshotFile == null)
			{
				m_snapshotFile = Path.Combine(Application.temporaryCachePath, "input_config.xml");
			}

			InputSaverXML inputSaver = new InputSaverXML(m_snapshotFile);
			inputSaver.Save(inputManager.GetSaveData());
		}
		
		public static void LoadSnapshot(InputManager inputManager)
		{
			if(!CanLoadSnapshot())
				return;

			InputLoaderXML inputLoader = new InputLoaderXML(m_snapshotFile);
			inputManager.SetSaveData(inputLoader.Load());
		}
		
		public static void ShowStartupWarning()
		{
			string key = string.Concat(PlayerSettings.companyName, ".", PlayerSettings.productName, ".InputManager.StartupWarning");
			
			if(!EditorPrefs.GetBool(key, false))
			{
				string message = "In order to use the InputManager plugin you need to overwrite your project's input settings. Your old input axes will be exported to a file which can be imported at a later time from the File menu.\n\nDo you want to overwrite the input settings now?\nYou can always do it later from the File menu.";
				if(EditorUtility.DisplayDialog("Warning", message, "Yes", "No"))
				{
					if(OverwriteProjectSettings())
						EditorPrefs.SetBool(key, true);
				}
			}
		}
		
		public static bool OverwriteProjectSettings()
		{
			int length = Application.dataPath.LastIndexOf('/');
			string projectSettingsFolder = string.Concat(Application.dataPath.Substring(0, length), "/ProjectSettings");
			string inputManagerPath = string.Concat(projectSettingsFolder, "/InputManager.asset");

			if(!Directory.Exists(projectSettingsFolder))
			{
				EditorUtility.DisplayDialog("Error", "Unable to get the correct path to the ProjectSetting folder.", "OK");
				return false;
			}

			if(!EditorUtility.DisplayDialog("Warning", "You chose not to export your old input settings. They will be lost forever. Are you sure you want to continue?", "Yes", "No"))
				return false;
				
			InputConverter inputConverter = new InputConverter();
			
			inputConverter.GenerateDefaultUnityInputManager(inputManagerPath);

			EditorUtility.DisplayDialog("Success", "The input settings have been successfully replaced.\n\nYou might need to minimize and restore Unity to reimport the new settings.", "OK");

			return true;
		}

		public static Texture2D GetUnityIcon(string name)
		{
			return EditorGUIUtility.Load(name + ".png") as Texture2D;
		}
		public static Texture2D GetCustomIcon(string name)
		{
			return Resources.Load<Texture2D>(name) as Texture2D;
		}
	}

	public class KeyCodeField
	{
		private string m_controlName, m_keyString;
		private bool m_isEditing;

		public KeyCodeField()
		{
			m_controlName = Guid.NewGuid().ToString("N");
			m_keyString = "";
			m_isEditing = false;
		}

		public KeyCode OnGUI(string label, KeyCode key)
		{
			GUI.SetNextControlName(m_controlName);
			bool hasFocus = (GUI.GetNameOfFocusedControl() == m_controlName);
			if(!m_isEditing && hasFocus)
			{
				m_keyString = key == KeyCode.None ? "" : KeyCodeConverter.KeyToString(key);
			}

			m_isEditing = hasFocus;
			if(m_isEditing)
			{
				m_keyString = EditorGUILayout.TextField(label, m_keyString);
			}
			else
			{
				EditorGUILayout.TextField(label, key == KeyCode.None ? "" : KeyCodeConverter.KeyToString(key));
			}

			if(m_isEditing && Event.current.type == EventType.KeyUp)
			{
				key = KeyCodeConverter.StringToKey(m_keyString);
				if(key == KeyCode.None)
				{
					m_keyString = "";
				}
				else
				{
					m_keyString = KeyCodeConverter.KeyToString(key);
				}
				m_isEditing = false;
			}

			return key;
		}

		public void Reset()
		{
			m_keyString = "";
			m_isEditing = false;
		}
	}
}
