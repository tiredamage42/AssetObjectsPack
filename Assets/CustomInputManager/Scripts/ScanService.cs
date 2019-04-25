
using UnityEngine;

namespace CustomInputManager
{
	public enum ScanFlags { None = 0, Key = 1 << 1, JoystickButton = 1 << 2, JoystickAxis = 1 << 3, MouseAxis = 1 << 4 }
    
    public struct ScanResult {
		public ScanFlags ScanFlags;
		public KeyCode keyCode;
		public int mouseAxis;
		public GamepadButton gamepadButton;
		public GamepadAxis gamepadAxis;
		public float axisValue;

		public static ScanResult Empty (ScanFlags scanResult = ScanFlags.None){
			ScanResult result = new ScanResult();
			result.ScanFlags = scanResult;
			result.keyCode = KeyCode.None;
			result.mouseAxis = -1;
			result.gamepadButton = GamepadButton.None;
			result.gamepadAxis = GamepadAxis.None;
			result.axisValue = 0.0f;
			return result;
		}

		public static ScanResult KeyScanResult(KeyCode keyCode) {
			ScanResult result = Empty(ScanFlags.Key);
			result.keyCode = keyCode;
			return result;
		}
		public static ScanResult MouseScanResult(int mouseAxis) {
			ScanResult result = Empty(ScanFlags.MouseAxis);
			result.mouseAxis = mouseAxis;
			return result;
		}
		public static ScanResult GamepadButtonResult(GamepadButton gamepadButton){
			ScanResult result = Empty(ScanFlags.JoystickButton);
			result.gamepadButton = gamepadButton;
			return result;
		}
		public static ScanResult GamepadAxisResult(GamepadAxis gamepadAxis, float axisValue){
			ScanResult result = Empty(ScanFlags.JoystickAxis);
			result.gamepadAxis = gamepadAxis;
			result.axisValue = axisValue;
			return result;
		}
	}

	/// <summary>
	/// Encapsulates a method that takes one parameter(the scan result) and returns 'true' if
	/// the scan result is accepted or 'false' if it isn't.
	/// </summary>
	public delegate bool ScanHandler(ScanResult result);

	public class ScanService
	{
		ScanHandler m_scanHandler;
		ScanFlags scanFlags;
		System.Action onScanEnd;

		float m_scanStartTime;
		KeyCode[] m_keys;
		string[] m_rawMouseAxes;
		
		public bool IsScanning { get; private set; }

		public ScanService()
		{
			m_rawMouseAxes = new string[InputBinding.MAX_MOUSE_AXES];
			for(int i = 0; i < m_rawMouseAxes.Length; i++)
				m_rawMouseAxes[i] = string.Concat("mouse_axis_", i);
			
			m_keys = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
			IsScanning = false;
		}

		public void Start(float startTime, ScanFlags scanFlags, ScanHandler scanHandler, System.Action onScanEnd)
		{
			if(IsScanning)
				Stop();

			m_scanStartTime = startTime;
			this.scanFlags = scanFlags;
			this.onScanEnd = onScanEnd;
			m_scanHandler = scanHandler;

			IsScanning = true;
		}

		public void Stop()
		{
			if(IsScanning)
			{
				IsScanning = false;
				m_scanHandler(ScanResult.Empty());
				EndScan();
			}
		}

		bool EndScan() {
			m_scanHandler = null;
			if (onScanEnd != null) {
				onScanEnd();
				onScanEnd = null;
			}
			return true;
		}

		public void Update(float gameTime, KeyCode cancelScanKey, float maxScanTime)
		{
			float timeout = gameTime - m_scanStartTime;

			// or on cancel
			if(Input.GetKeyDown(cancelScanKey) || timeout >= maxScanTime)
			{
				Stop();
				return;
			}

			bool success = false;
			if(HasFlag(ScanFlags.Key))
				success = ScanKey();
			if(!success && HasFlag(ScanFlags.JoystickButton))
				success = ScanJoystickButton();
			if(!success && HasFlag(ScanFlags.JoystickAxis))
				success = ScanJoystickAxis();
			if(!success && HasFlag(ScanFlags.MouseAxis))
				success = ScanMouseAxis();
	
			IsScanning = !success;
		}

		bool ScanKey()
		{
			int max = (int)KeyCode.JoystickButton0;


			for(int i = 0; i < m_keys.Length; i++)
			{
				KeyCode k = m_keys[i];
				if((int)k >= max)
					break;

				
				if(Input.GetKeyDown(k))
				{
					if(m_scanHandler(ScanResult.KeyScanResult(k)))
					{
					
						return EndScan();
					}
				}
			}

			return false;
		}

		bool ScanJoystickButton()
		{
			int gamepadButtons = 14;
			for(int i = 0; i < gamepadButtons; i++)
			{
				GamepadButton button = (GamepadButton)i;
				for (int x = 0; x < InputBinding.MAX_JOYSTICKS; x++) {

					if(InputManager.Gamepad.GetButtonDown(button, x))
					{
						if(m_scanHandler(ScanResult.GamepadButtonResult(button)))
						{
							return EndScan();
						}
					}
				}
			}
			return false;
		}

		bool ScanJoystickAxis()
		{
			int axes = 8;
			for(int i = 0; i < axes; i++) 
			{
				GamepadAxis axis = (GamepadAxis)i;
				for (int x = 0; x < InputBinding.MAX_JOYSTICKS; x++) {
					float axisRaw = InputManager.Gamepad.GetAxisRaw(axis, x);
					if(Mathf.Abs(axisRaw) >= 1.0f)
					{
						if(m_scanHandler(ScanResult.GamepadAxisResult(axis, axisRaw)))
						{
							return EndScan();
						}
					}
				}
			}
			return false;
		}

		bool ScanMouseAxis()
		{
			for(int i = 0; i < m_rawMouseAxes.Length; i++)
			{
				if(Mathf.Abs(Input.GetAxis(m_rawMouseAxes[i])) > 0.0f)
				{
					if(m_scanHandler(ScanResult.MouseScanResult(i)))
					{
						return EndScan();
					}
				}
			}
			return false;
		}

		bool HasFlag(ScanFlags flag)
		{
			return ((int)scanFlags & (int)flag) != 0;
		}
	}
}
