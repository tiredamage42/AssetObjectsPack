


/*

	actions:
		bindings
	
	jump
		gampad a
		space bar

	fire
		trigger right
		enter


*/



using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

namespace CustomInputManager
{
	public class SaveData
    {
        public List<ControlScheme> ControlSchemes;
        public string[] playerSchemes;
        
		public SaveData()
		{
			ControlSchemes = new List<ControlScheme>();
            playerSchemes = new string[InputManager.maxPlayers];	
		}
    }
	
	public partial class InputManager : MonoBehaviour
	{

		#region  EDITORSTUFF
		public string GetPlayerDefault(int playerIndex) {
			return playerSchemesDefaults[playerIndex];
		}
		public void SetPlayerDefault(int playerIndex, string value) {
			playerSchemesDefaults[playerIndex] = value;
		}
		#endregion


		public const int maxPlayers = InputBinding.MAX_JOYSTICKS;


		
		public GamepadHandler gamepad;

		public static GamepadHandler Gamepad {
			get {
				return m_instance.gamepad;
			}
		}

		[SerializeField] private List<ControlScheme> m_controlSchemes = new List<ControlScheme>();
		[SerializeField] string[] playerSchemesDefaults = new string[maxPlayers];
		
		
		ControlScheme[] playerSchemes = new ControlScheme[maxPlayers];
		
		private ScanService m_scanService;
		private static InputManager m_instance;
		
		private Dictionary<string, ControlScheme> m_schemeLookup;
		private Dictionary<string, ControlScheme> m_schemeLookupByID;
		private Dictionary<string, Dictionary<string, InputAction>> m_actionLookup;


		public List<ControlScheme> ControlSchemes
		{
			get { return m_controlSchemes; }
		}

		private void Awake()
		{
			if(m_instance == null)
			{
				m_instance = this;
				m_scanService = new ScanService();
				m_schemeLookup = new Dictionary<string, ControlScheme>();
				m_schemeLookupByID = new Dictionary<string, ControlScheme>();
				m_actionLookup = new Dictionary<string, Dictionary<string, InputAction>>();

				Initialize();
				gamepad.Awake(this);

				// try and load custom runtime bindings
				Load();

				// Lock or unlock the cursor.
				// Cursor.lockState = m_LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
				// Cursor.visible = !m_LockCursor;


			}
			else
			{
				Debug.LogWarning("You have multiple InputManager instances in the scene!", gameObject);
				Destroy(this);
			}
		}



		private void OnDestroy()
		{
			if(m_instance == this)
			{
				m_instance = null;
			}
		}

		private void Initialize()
		{
			m_schemeLookup.Clear();
			m_actionLookup.Clear();

			for (int i = 0; i < maxPlayers; i++) {
				playerSchemes[i] = null;
			}

			if(m_controlSchemes.Count == 0)
				return;

			PopulateLookupTables();

			for (int i = 0; i < maxPlayers; i++) {
				if(!string.IsNullOrEmpty(playerSchemesDefaults[i]) && m_schemeLookupByID.ContainsKey(playerSchemesDefaults[i]))
				{
					playerSchemes[i] = m_schemeLookupByID[playerSchemesDefaults[i]];
				}
				else {
					if (i == 0) {
						if(m_controlSchemes.Count > 0)
							playerSchemes[i] = m_controlSchemes[0];
					}
				}
			}

			foreach(ControlScheme scheme in m_controlSchemes)
			{
				scheme.Initialize();
			}

			Input.ResetInputAxes();
		}

		private void PopulateLookupTables()
		{
			m_schemeLookup.Clear();
			m_schemeLookupByID.Clear();
			foreach(ControlScheme scheme in m_controlSchemes)
			{
				m_schemeLookup[scheme.Name] = scheme;
				m_schemeLookupByID[scheme.UniqueID] = scheme;
			}

			m_actionLookup.Clear();
			foreach(ControlScheme scheme in m_controlSchemes)
			{
				m_actionLookup[scheme.Name] = scheme.GetActionLookupTable();
			}
		}

		private void Update()
		{

			gamepad.OnUpdate(Time.unscaledDeltaTime);

			for (int i = 0; i < maxPlayers; i++) {
				if(playerSchemes[i] != null)
				{
					playerSchemes[i].Update(Time.unscaledDeltaTime);
				}
			}
				
			if(m_scanService.IsScanning)
			{
				m_scanService.Update(Time.unscaledTime, KeyCode.Escape, 5.0f);
			}

		}
		private int? IsControlSchemeInUse(string name)
		{
			for (int i = 0; i < maxPlayers; i++) {
				if(playerSchemes[i] != null && playerSchemes[i].Name == name)
					return i;
			}
			return null;
		}

		public void SetSaveData(SaveData saveData)
		{
			if(saveData != null)
			{
				m_controlSchemes = saveData.ControlSchemes;
				playerSchemesDefaults = saveData.playerSchemes;
			}
		}

		public SaveData GetSaveData()
		{
			SaveData d = new SaveData();
			d.ControlSchemes = m_controlSchemes;
			d.playerSchemes = playerSchemesDefaults;
			return d;
		}


		#region [Static Interface]
		

		public static bool IsScanning
		{
			get { return m_instance.m_scanService.IsScanning; }
		}

		public static ControlScheme GetControlScheme(int playerID) {
			return m_instance.playerSchemes[playerID];
		}

		/// <summary>
		/// Returns true if any axis of any active control scheme is receiving input.
		/// </summary>
		public static bool AnyInput()
		{
			for (int i = 0; i < maxPlayers; i++) {
				if (AnyInput(m_instance.playerSchemes[i], i)) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns true if any axis of the control scheme is receiving input.
		/// </summary>
		public static bool AnyInput(int playerID)
		{
			return AnyInput(m_instance.playerSchemes[playerID], playerID);
		}

		/// <summary>
		/// Returns true if any axis of the specified control scheme is receiving input.
		/// </summary>
		public static bool AnyInput(string schemeName, int playerID)
		{
			ControlScheme scheme;
			if(m_instance.m_schemeLookup.TryGetValue(schemeName, out scheme))
			{
				return scheme.AnyInput(playerID);
			}

			return false;
		}

		private static bool AnyInput(ControlScheme scheme, int playerID)
		{
			if(scheme != null)
				return scheme.AnyInput(playerID);
			return false;
		}

		/// <summary>
		/// Resets the internal state of the input manager.
		/// </summary>
		public static void Reinitialize()
		{
			m_instance.Initialize();
		}

		/// <summary>
		/// Changes the active control scheme.
		/// </summary>
		public static void SetControlScheme(string name, int playerID)
		{
			int? playerWhoUsesControlScheme = m_instance.IsControlSchemeInUse(name);

			if(playerWhoUsesControlScheme.HasValue && playerWhoUsesControlScheme.Value != playerID)
			{
				Debug.LogErrorFormat("The control scheme named \'{0}\' is already being used by player {1}", name, playerWhoUsesControlScheme.Value.ToString());
				return;
			}

			if(playerWhoUsesControlScheme.HasValue && playerWhoUsesControlScheme.Value == playerID) {
				Debug.LogWarning("player " + playerID + " is already using scheme: " + name);
				return;
			}

			ControlScheme controlScheme = null;
			if(m_instance.m_schemeLookup.TryGetValue(name, out controlScheme))
			{
				controlScheme.Initialize();
				m_instance.playerSchemes[playerID] = controlScheme;
			}
			else
			{
				Debug.LogError(string.Format("A control scheme named \'{0}\' does not exist", name));
			}
		}

		public static ControlScheme GetControlScheme(string name)
		{
			ControlScheme scheme = null;
			if(m_instance.m_schemeLookup.TryGetValue(name, out scheme))
				return scheme;

			return null;
		}

		public static InputAction GetAction(string controlSchemeName, string actionName)
		{
			Dictionary<string, InputAction> table;
			if(m_instance.m_actionLookup.TryGetValue(controlSchemeName, out table))
			{
				InputAction action;
				if(table.TryGetValue(actionName, out action))
					return action;
			}
			return null;
		}

		public static InputAction GetAction(int playerID, string actionName)
		{
			var scheme = m_instance.playerSchemes[playerID];
			if(scheme == null)
				return null;

			Dictionary<string, InputAction> table;
			if(m_instance.m_actionLookup.TryGetValue(scheme.Name, out table))
			{
				InputAction action;
				if(table.TryGetValue(actionName, out action))
					return action;
			}

			return null;
		}

		public static ControlScheme CreateControlScheme(string name)
		{
			if(m_instance.m_schemeLookup.ContainsKey(name))
			{
				Debug.LogError(string.Format("A control scheme named \'{0}\' already exists", name));
				return null;
			}

			ControlScheme scheme = new ControlScheme(name);
			m_instance.m_controlSchemes.Add(scheme);
			m_instance.m_schemeLookup[name] = scheme;
			m_instance.m_actionLookup[name] = new Dictionary<string, InputAction>();

			return scheme;
		}

		/// <summary>
		/// Deletes the specified control scheme. If the speficied control scheme is
		/// active for any player then the active control scheme for the respective player will be set to null.
		/// </summary>
		public static bool DeleteControlScheme(string name)
		{
			ControlScheme scheme = GetControlScheme(name);
			if(scheme == null)
				return false;

			m_instance.m_actionLookup.Remove(name);
			m_instance.m_schemeLookup.Remove(name);
			m_instance.m_controlSchemes.Remove(scheme);

			for (int i = 0; i < maxPlayers; i++) {
				if(m_instance.playerSchemes[i].Name == scheme.Name)
					m_instance.playerSchemes[i] = null;
			}
				
			return true;
		}


		/// <summary>
		/// Creates an uninitialized input action. It's your responsability to configure it properly.
		/// </summary>
		public static InputAction CreateEmptyAction(string controlSchemeName, string actionName)
		{
			ControlScheme scheme = GetControlScheme(controlSchemeName);
			if(scheme == null)
			{
				Debug.LogError(string.Format("A control scheme named \'{0}\' does not exist", controlSchemeName));
				return null;
			}
			if(m_instance.m_actionLookup[controlSchemeName].ContainsKey(actionName))
			{
				Debug.LogError(string.Format("The control scheme named {0} already contains an action named {1}", controlSchemeName, actionName));
				return null;
			}

			InputAction action = scheme.CreateNewAction(actionName);

			m_instance.m_actionLookup[controlSchemeName][actionName] = action;
			
			// InputBinding binding = action.CreateNewBinding();
			// binding.Type = InputType.MouseAxis;
			// binding.MouseAxis = axis;
			// binding.Sensitivity = sensitivity;
			// action.Initialize();
			return action;
		}

		public static bool DeleteAction(string controlSchemeName, string actionName)
		{
			ControlScheme scheme = GetControlScheme(controlSchemeName);
			InputAction action = GetAction(controlSchemeName, actionName);
			if(scheme != null && action != null)
			{
				m_instance.m_actionLookup[scheme.Name].Remove(action.Name);
				scheme.DeleteAction(action);
				return true;
			}
			return false;
		}

		public static void StartInputScan(ScanFlags scanFlags, ScanHandler scanHandler, System.Action onScanEnd)
		{
			m_instance.m_scanService.Start(Time.unscaledTime, scanFlags, scanHandler, onScanEnd);
		}
		public static void StopInputScan()
		{
			m_instance.m_scanService.Stop();
		}

		

		
		/// <summary>
		/// Saves the control schemes in an XML file, in Application.persistentDataPath.
		/// </summary>
		public static void SaveCustomBindings()
		{
			Save(Application.persistentDataPath + "/InputManagerOverride.xml");
		}

		/// <summary>
		/// Saves the control schemes in the XML format, at the specified location.
		/// </summary>
		public static void Save(string filePath)
		{
			Save(new InputSaverXML(filePath));
		}

		public static void Save(InputSaverXML inputSaver)
		{
			if(inputSaver != null)
			{
				inputSaver.Save(m_instance.GetSaveData());
			}
			else
			{
				Debug.LogError("InputSaver is null. Cannot save control schemes.");
			}
		}



		/// <summary>
		/// Loads the control schemes from an XML file, from Application.persistentDataPath.
		/// </summary>
		public static void Load()
		{
			Load(Application.persistentDataPath + "/InputManagerOverride.xml");
		}

		/// <summary>
		/// Loads the control schemes saved in the XML format, from the specified location.
		/// </summary>
		public static void Load(string filePath)
		{
			#if UNITY_WINRT && !UNITY_EDITOR
			if(UnityEngine.Windows.File.Exists(filePath))
#else
			if(System.IO.File.Exists(filePath))
#endif
			{

				Load(new InputLoaderXML(filePath));
			}
		}

		public static void Load(InputLoaderXML inputLoader)
		{
			if(inputLoader != null)
			{
				m_instance.SetSaveData(inputLoader.Load());
				m_instance.Initialize();
			}
			else
			{
				Debug.LogError("InputLoader is null. Cannot load control schemes.");
			}
		}

		#endregion
	}
}
