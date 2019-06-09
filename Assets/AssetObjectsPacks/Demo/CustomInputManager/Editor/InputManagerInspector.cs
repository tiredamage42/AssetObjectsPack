
using UnityEngine;
using UnityEditor;
using CustomInputManager;

namespace CustomInputManagerEditor.IO
{
	[CustomEditor(typeof(InputManager))]
	public class InputManagerInspector : Editor
	{
		private const int BUTTON_HEIGHT = 35;
		private InputManager m_inputManager;
		private SerializedProperty m_playerDefaults;
		private GUIContent m_createSnapshotInfo;
		private string[] m_controlSchemeNames;

		private void OnEnable()
		{
			m_inputManager = target as InputManager;

			m_playerDefaults = serializedObject.FindProperty("playerSchemesDefaults");
			m_createSnapshotInfo = new GUIContent("Create\nSnapshot", "Creates a snapshot of your input configurations which can be restored at a later time(when you exit play-mode for example)");
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			serializedObject.Update();
			UpdateControlSchemeNames();

			EditorGUILayout.Space();

			for (int i = 0; i < InputManager.maxPlayers; i++) {
				DrawControlSchemeDropdown(m_playerDefaults.GetArrayElementAtIndex(i), i);
			}

			
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			GUI.enabled = !InputEditor.IsOpen;
			if(GUILayout.Button("Input\nEditor", GUILayout.Height(BUTTON_HEIGHT)))
			{
				InputEditor.OpenWindow(m_inputManager);
			}
			GUI.enabled = true;
			if(GUILayout.Button(m_createSnapshotInfo, GUILayout.Height(BUTTON_HEIGHT)))
			{
				EditorToolbox.CreateSnapshot(m_inputManager);
			}
			GUI.enabled = EditorToolbox.CanLoadSnapshot();
			if(GUILayout.Button("Restore\nSnapshot", GUILayout.Height(BUTTON_HEIGHT)))
			{
				EditorToolbox.LoadSnapshot(m_inputManager);
			}
			GUI.enabled = true;
			GUILayout.EndHorizontal();

			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(serializedObject.targetObject);
			// UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(m_inputManager.gameObject.scene);
			
		}

		private void UpdateControlSchemeNames()
		{
			if(m_controlSchemeNames == null || (m_controlSchemeNames.Length - 1 != m_inputManager.ControlSchemes.Count))
			{
				m_controlSchemeNames = new string[m_inputManager.ControlSchemes.Count + 1];
			}

			m_controlSchemeNames[0] = "None";
			for(int i = 1; i < m_controlSchemeNames.Length; i++)
			{
				m_controlSchemeNames[i] = m_inputManager.ControlSchemes[i - 1].Name;
			}
		}

		private void DrawControlSchemeDropdown(SerializedProperty item, int playerID)
		{
			int index = FindIndexOfControlScheme(item.stringValue);
			index = EditorGUILayout.Popup("Player " + playerID + " Default", index, m_controlSchemeNames);

			if(index > 0)
			{
				item.stringValue = m_inputManager.ControlSchemes[index - 1].UniqueID;
			}
			else
			{
				item.stringValue = null;
			}
		}

		private int FindIndexOfControlScheme(string id)
		{
			if(string.IsNullOrEmpty(id))
				return 0;

			for(int i = 0; i < m_inputManager.ControlSchemes.Count; i++)
			{
				if(m_inputManager.ControlSchemes[i].UniqueID == id)
					return i + 1;
			}

			return 0;
		}
	}
}