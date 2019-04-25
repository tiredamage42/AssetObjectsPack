using UnityEngine;
using System.Collections;
using Syd.UI;
using UnityEngine.UI;

namespace CustomInputManager.Examples
{

	/*
	
		change:
			-/+ binding (+ only if digital axis)
			invert (if gamepad axis or mouse axis or digital axis)
			sensitivity (if gamepad axis or mouse axis or digital axis)
	*/
	public class RebindInputButton : MonoBehaviour
	{
		[SerializeField] int m_bindingIndex = 0;
		[HideInInspector] public string actionName, m_controlSchemeName;
        [HideInInspector] public UIButtonProfile normalProfile, scanningProfile;
		bool m_changePositiveKey;
		UIButton[] uIButtons;

		protected bool GetValues (out InputAction inputAction, out InputBinding inputBinding) {
			inputAction = InputManager.GetAction(m_controlSchemeName, actionName);
			inputBinding = null;
			if (inputAction != null) {
				inputBinding = inputAction.GetBinding(m_bindingIndex);
			}
			if (inputAction == null || inputBinding == null) {
				Debug.LogErrorFormat("Control scheme '{0}' does not exist or input action '{1}' does not exist", m_controlSchemeName, actionName);
                return false;
			}
            return true;
		}

		public void Initialize () {
			RefreshText();

			InputBinding inputBinding;
			InputAction inputAction;
			if (!GetValues(out inputAction, out inputBinding)) {
				return;
			}

			Toggle invertToggle = GetComponentInChildren<Toggle>();
			Slider sensitivitySlider = GetComponentInChildren<Slider>();

			Text displayNameText = GetComponentInChildren<Text>();
			displayNameText.text = inputAction.displayName;
			displayNameText.color = uIButtons[0].profile.textColor;

			
			if (inputBinding.Type != InputType.DigitalAxis && inputBinding.Type != InputType.GamepadAxis && inputBinding.Type != InputType.MouseAxis) {
				invertToggle.transform.parent.gameObject.SetActive(false);
				uIButtons[1].gameObject.SetActive(false);
			}
			else {
				if (inputBinding.Type == InputType.MouseAxis) {
					uIButtons[0].gameObject.SetActive(false);
				}

				if (inputBinding.Type != InputType.DigitalAxis) {
					uIButtons[1].gameObject.SetActive(false);
				}
				else {
					uIButtons[1].onClick += OnClickPositive;
					uIButtons[1].profile = normalProfile;
				}

				invertToggle.onValueChanged.AddListener( OnInvertChanged );
				sensitivitySlider.onValueChanged.AddListener( OnSensitivityChange );
				
				invertToggle.isOn = inputBinding.Invert;
				sensitivitySlider.value = inputBinding.Sensitivity;
			}
			
			if (inputBinding.Type != InputType.MouseAxis) {
		
				uIButtons[0].onClick += OnClickNegative;
				uIButtons[0].profile = normalProfile;
			}

		}
		
		
		void Awake()
		{
			uIButtons = GetComponentsInChildren<UIButton>();
		}

		void OnClickNegative()
		{
			if(!InputManager.IsScanning)
			{
				StartCoroutine(StartInputScanDelayedNegativeOrDefault());
			}
		}
		void OnClickPositive() {
			if(!InputManager.IsScanning)
			{
				StartCoroutine(StartInputScanDelayedPositive());
			}
		}

		void OnSensitivityChange (float value) {
			InputBinding inputBinding;
			if (!GetValues(out _, out inputBinding)) {
				return;
			}
			inputBinding.Sensitivity = value;
		}

		void OnInvertChanged(bool value)
		{
			InputBinding inputBinding;
			if (!GetValues(out _, out inputBinding)) {
				return;
			}
			inputBinding.Invert = value;
		}


		public void RefreshText () {
			InputBinding inputBinding;
			if (!GetValues(out _, out inputBinding)) {
				return;
			}
			
			uIButtons[0].text.text = (inputBinding.Type != InputType.DigitalAxis ? "" : "( - ) ") + inputBinding.GetAsString(false);
			uIButtons[0].profile = normalProfile;

			if (inputBinding.Type == InputType.DigitalAxis) {
				uIButtons[1].text.text = "( + ) " + inputBinding.GetAsString(true);
				uIButtons[1].profile = normalProfile;
			}
		}

		void OnStopScan() {
			RefreshText();
			UIUtils.RestoreUIInputControl();
		}


			
		IEnumerator StartInputScanDelayedNegativeOrDefault()
		{
			InputBinding inputBinding;
			if (GetValues(out _, out inputBinding)) {

				// override ui input
				UIUtils.OverrideUIInputControl();
				
				// update button visual
				uIButtons[0].profile = scanningProfile;
				uIButtons[0].text.text = (inputBinding.Type != InputType.DigitalAxis ? "" : "( - ) ") + "...";



				yield return null; // delay before scanning

				if (inputBinding.Type == InputType.KeyButton || inputBinding.Type == InputType.DigitalAxis) {
					m_changePositiveKey = inputBinding.Type != InputType.DigitalAxis;
						
					InputManager.StartInputScan(ScanFlags.Key, HandleKeyScan, OnStopScan);	
					
				}
				else if (inputBinding.Type == InputType.GamepadAxis) {
					InputManager.StartInputScan(ScanFlags.JoystickAxis, HandleJoystickAxisScan, OnStopScan);
				}
				else if (inputBinding.Type == InputType.GamepadButton || inputBinding.Type == InputType.GamepadAnalogButton) {
					ScanFlags flags = ScanFlags.JoystickButton;
					flags |= ScanFlags.JoystickAxis;
					InputManager.StartInputScan(flags, HandleJoystickButtonScan, OnStopScan);	
				}
			}
		}
		IEnumerator StartInputScanDelayedPositive()
		{
			InputBinding inputBinding;
			if (GetValues(out _, out inputBinding)) {

				if (inputBinding.Type == InputType.DigitalAxis) {
				
					// override ui input
					UIUtils.OverrideUIInputControl();
	
					// update button visual
					uIButtons[1].profile = scanningProfile;
					uIButtons[1].text.text = "( + ) ...";

					m_changePositiveKey = true;
					
					yield return null; // delay before scanning

					InputManager.StartInputScan(ScanFlags.Key, HandleKeyScan, OnStopScan);	
				}
			}
		}


		//	When you return false you tell the InputManager that it should keep scaning for other keys
		bool HandleKeyScan(ScanResult result)
		{
			if(IsKeyValid(result.keyCode) && result.keyCode != KeyCode.None)
			{
				InputBinding inputBinding;
				GetValues(out _, out inputBinding);
				//	If the key is KeyCode.Backspace clear the current binding
				KeyCode Key = (result.keyCode == KeyCode.Backspace) ? KeyCode.None : result.keyCode;
				if(m_changePositiveKey)
					inputBinding.Positive = Key;
				else
					inputBinding.Negative = Key;
				return true;
			}
			return false;
		}


		bool IsKeyValid(KeyCode key)
		{
			bool isValid = true;
			if((int)key >= (int)KeyCode.JoystickButton0) {
				isValid = false;
			}
			else if(key == KeyCode.LeftApple || key == KeyCode.RightApple)
				isValid = false;
			else if(key == KeyCode.LeftWindows || key == KeyCode.RightWindows)
				isValid = false;

			return isValid;
		}

				
		//	When you return false you tell the InputManager that it should keep scaning for other keys
		private bool HandleJoystickButtonScan(ScanResult result)
		{
			
			if(result.ScanFlags == ScanFlags.JoystickButton)
			{
				if (result.gamepadButton != GamepadButton.None)
				{
					InputBinding inputBinding;
					GetValues(out _, out inputBinding);
					inputBinding.Type = InputType.GamepadButton;
					inputBinding.GamepadButton = result.gamepadButton;
					return true;
				}
			}
			else
			{
				if(result.gamepadAxis != GamepadAxis.None)
				{
					InputBinding inputBinding;
					GetValues(out _, out inputBinding);
					inputBinding.Type = InputType.GamepadAnalogButton;
					inputBinding.Invert = result.axisValue < 0.0f;
					inputBinding.GamepadAxis = result.gamepadAxis;
					return true;
				}
			}
			return false;
		}
		//	When you return false you tell the InputManager that it should keep scaning for other keys
		bool HandleJoystickAxisScan(ScanResult result)
		{
			if(result.gamepadAxis != GamepadAxis.None) {
				InputBinding inputBinding;
				GetValues(out _, out inputBinding);
				inputBinding.GamepadAxis = result.gamepadAxis;
				return true;
			}
			return false;
		}		
	}
}