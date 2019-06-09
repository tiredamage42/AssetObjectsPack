
using UnityEngine;
using System;

namespace CustomInputManager
{
	public enum ButtonState { Pressed, JustPressed, Released, JustReleased }
	public enum InputType
	{
		KeyButton, MouseAxis, DigitalAxis,
		GamepadButton, GamepadAnalogButton, GamepadAxis
	}

	[Serializable] public class InputBinding
	{

		public static readonly string[] mouseAxisNames = new string[] { 
			"Mouse X", "Mouse Y", "Mouse Scroll" 
		};
		string rawMouseAxisName { get { return string.Concat("mouse_axis_", mouseAxis); } } 

		public string GetAsString(bool usePositive) {

			switch (m_type) {

				case InputType.KeyButton:
					return m_positive.ToString();
				case InputType.MouseAxis:
					return mouseAxisNames[mouseAxis];
				case InputType.DigitalAxis:
					return usePositive ? m_positive.ToString() : m_negative.ToString();
				case InputType.GamepadButton:
					return m_gamepadButton.ToString();
				case InputType.GamepadAnalogButton:
					return (Invert ? "-" : "+") + GamepadAxis.ToString();
				case InputType.GamepadAxis:
					return GamepadAxis.ToString();
			}
			return "ERROR";
		}


		public const float AXIS_NEUTRAL = 0.0f;
		public const float AXIS_POSITIVE = 1.0f;
		public const float AXIS_NEGATIVE = -1.0f;
		public const int MAX_MOUSE_AXES = 3;
		public const int MAX_JOYSTICK_AXES = 28;
        public const int MAX_JOYSTICK_BUTTONS = 20;
        public const int MAX_JOYSTICKS = 11;


		[SerializeField] private KeyCode m_positive;
		[SerializeField] private KeyCode m_negative;
		[SerializeField] private float m_deadZone;
		[SerializeField] private float m_gravity;
		[SerializeField] private float m_sensitivity;
		[SerializeField] private bool m_snap;
		[SerializeField] private bool m_invert;
		[SerializeField] private InputType m_type;
		[SerializeField] private int mouseAxis;
		[SerializeField] private GamepadAxis m_gamepadAxis;
		[SerializeField] private GamepadButton m_gamepadButton;

		
		float digitalAxisValue;
		ButtonState[] analogButtonStates = new ButtonState[MAX_JOYSTICKS];

		public KeyCode Positive
		{
			get { return m_positive; }
			set { m_positive = value; }
		}
		public KeyCode Negative
		{
			get { return m_negative; }
			set { m_negative = value; }
		}
		public float DeadZone
		{
			get { return m_deadZone; }
			set { m_deadZone = Mathf.Max(value, 0.0f); }
		}
		public float Gravity
		{
			get { return m_gravity; }
			set { m_gravity = Mathf.Max(value, 0.0f); }
		}
		public float Sensitivity
		{
			get { return m_sensitivity; }
			set { m_sensitivity = Math.Max(value, 0.0f); }
		}
		public bool Snap
		{
			get { return m_snap; }
			set { m_snap = value; }
		}
		public bool Invert
		{
			get { return m_invert; }
			set { m_invert = value; }
		}
		public InputType Type
		{
			get { return m_type; }
			set { m_type = value; }
		}
		public int MouseAxis
		{
			get { return mouseAxis; }
			set { mouseAxis = Mathf.Clamp(value, 0, MAX_MOUSE_AXES - 1); }
		}
		public GamepadButton GamepadButton
		{
			get { return m_gamepadButton; }
			set { m_gamepadButton = value; }
		}
		public GamepadAxis GamepadAxis
		{
			get { return m_gamepadAxis; }
			set { m_gamepadAxis = value; }
		}

		public InputBinding()
		{
			m_positive = KeyCode.None;
			m_negative = KeyCode.None;
			m_type = InputType.KeyButton;
			m_gravity = 1.0f;
			m_sensitivity = 1.0f;
		}

		void ResetAnalogButtonStates () {
			for (int i = 0; i < MAX_JOYSTICKS; i++) {
				analogButtonStates[i] = ButtonState.Released;
			}
		}

		void Reset()
		{
			if (m_type == InputType.DigitalAxis) {
				digitalAxisValue = AXIS_NEUTRAL;
			}
			if (m_type == InputType.GamepadAnalogButton) {
				ResetAnalogButtonStates();
			}
		}
		public void Initialize()
		{
			Reset();
		}
		public void Update(float deltaTime)
		{
			if(m_type == InputType.DigitalAxis)
			{
				UpdateDigitalAxisValue(deltaTime);
			}
			
			if(m_type == InputType.GamepadAnalogButton)
			{
				UpdateAnalogButtonValue();
			}
		}

		private void UpdateDigitalAxisValue(float deltaTime)
		{
			bool posDown = Input.GetKey(m_positive);
			bool negDown = Input.GetKey(m_negative);
			
			if (posDown && negDown) return;
			
			if(posDown) {
				if(digitalAxisValue < AXIS_NEUTRAL && m_snap)
					digitalAxisValue = AXIS_NEUTRAL;
				
				digitalAxisValue += m_sensitivity * deltaTime;
				
				if(digitalAxisValue > AXIS_POSITIVE)
					digitalAxisValue = AXIS_POSITIVE;
			}
			else if(negDown)
			{
				if(digitalAxisValue > AXIS_NEUTRAL && m_snap)
					digitalAxisValue = AXIS_NEUTRAL;
				
				digitalAxisValue -= m_sensitivity * deltaTime;
				
				if(digitalAxisValue < AXIS_NEGATIVE)
					digitalAxisValue = AXIS_NEGATIVE;
			}
			else
			{
				if(digitalAxisValue < AXIS_NEUTRAL)
				{
					digitalAxisValue += m_gravity * deltaTime;
					if(digitalAxisValue > AXIS_NEUTRAL)
						digitalAxisValue = AXIS_NEUTRAL;	
				}
				else if(digitalAxisValue > AXIS_NEUTRAL)
				{
					digitalAxisValue -= m_gravity * deltaTime;
					if(digitalAxisValue < AXIS_NEUTRAL)
						digitalAxisValue = AXIS_NEUTRAL;
				}
			}
		}

		private void UpdateAnalogButtonValue()
		{

			for (int playerID = 0; playerID < MAX_JOYSTICKS; playerID++) {

				float axis = InputManager.Gamepad.GetAxis(m_gamepadAxis, playerID);

				axis = m_invert ? -axis : axis;

				if(axis > m_deadZone)
				{
					if(analogButtonStates[playerID] == ButtonState.Released || analogButtonStates[playerID] == ButtonState.JustReleased)
						analogButtonStates[playerID] = ButtonState.JustPressed;
					else if(analogButtonStates[playerID] == ButtonState.JustPressed)
						analogButtonStates[playerID] = ButtonState.Pressed;
				}
				else
				{
					if(analogButtonStates[playerID] == ButtonState.Pressed || analogButtonStates[playerID] == ButtonState.JustPressed)
						analogButtonStates[playerID] = ButtonState.JustReleased;
					else if(analogButtonStates[playerID] == ButtonState.JustReleased)
						analogButtonStates[playerID] = ButtonState.Released;
				}
			}
		}



		#region  GETTERS
		public bool AnyInput (int playerID)
		{
				switch(m_type)
				{
				case InputType.KeyButton:
					return Input.GetKey(m_positive);
				
				case InputType.GamepadAnalogButton:
					return analogButtonStates[playerID] == ButtonState.Pressed || analogButtonStates[playerID] == ButtonState.JustPressed;
				
				case InputType.GamepadButton:
					return InputManager.Gamepad.GetButton(m_gamepadButton, playerID);
				
				case InputType.GamepadAxis:
					return Mathf.Abs(InputManager.Gamepad.GetAxisRaw(m_gamepadAxis, playerID)) >= 1.0f;
					
				case InputType.DigitalAxis:
					return Mathf.Abs(digitalAxisValue) >= 1.0f;

				case InputType.MouseAxis:
					return Mathf.Abs(Input.GetAxisRaw(rawMouseAxisName)) >= 1.0f;

				default:
					return false;
				}
		}
		
		public float? GetAxis(int playerID)
		{
			float? axis = null;
			if(m_type == InputType.DigitalAxis)
			{
				axis = m_invert ? -digitalAxisValue : digitalAxisValue;
			}
			else if(m_type == InputType.MouseAxis)
			{
				axis = Input.GetAxis(rawMouseAxisName) * m_sensitivity;
				axis = m_invert ? -axis : axis;
			}
			else if(m_type == InputType.GamepadAxis)
			{
				axis = InputManager.Gamepad.GetAxis(m_gamepadAxis, playerID);
				if(Mathf.Abs(axis.Value) < m_deadZone)
				{
					axis = AXIS_NEUTRAL;
				}
				axis = Mathf.Clamp(axis.Value * m_sensitivity, -1, 1);
				axis = m_invert ? -axis : axis;
			}

			if(axis.HasValue && Mathf.Abs(axis.Value) <= 0.0f)
				axis = null;
			
			return axis;
		}

		///<summary>
		///	Returns raw input with no sensitivity or smoothing applyed.
		/// </summary>
		public float? GetAxisRaw(int playerID)
		{
			float? axis = null;

			if(m_type == InputType.DigitalAxis)
			{
				if(Input.GetKey(m_positive))
				{
					axis = m_invert ? -AXIS_POSITIVE : AXIS_POSITIVE;
				}
				else if(Input.GetKey(m_negative))
				{
					axis = m_invert ? -AXIS_NEGATIVE : AXIS_NEGATIVE;
				}
			}
			else if(m_type == InputType.MouseAxis)
			{
				axis = Input.GetAxisRaw(rawMouseAxisName);
				axis = m_invert ? -axis : axis;
			}
			else if(m_type == InputType.GamepadAxis)
			{
				axis = InputManager.Gamepad.GetAxisRaw(m_gamepadAxis, playerID);
				axis = m_invert ? -axis : axis;
			}

			if(axis.HasValue && Mathf.Abs(axis.Value) <= 0.0f)
				axis = null;

			return axis;
		}

		public bool? GetButton(int playerID)
		{
			bool? value = null;

			if(m_type == InputType.KeyButton)
				value = Input.GetKey(m_positive);
			
			else if(m_type == InputType.GamepadButton)
				value = InputManager.Gamepad.GetButton(m_gamepadButton, playerID);
			
			else if( m_type == InputType.GamepadAnalogButton)
				value = analogButtonStates[playerID] == ButtonState.Pressed || analogButtonStates[playerID] == ButtonState.JustPressed;
			
			if(value.HasValue && !value.Value)
				value = null;

			return value;
		}

		public bool? GetButtonDown(int playerID)
		{
			bool? value = null;

			if(m_type == InputType.KeyButton)
				value = Input.GetKeyDown(m_positive);
			
			else if(m_type == InputType.GamepadButton)
				value = InputManager.Gamepad.GetButtonDown(m_gamepadButton, playerID);
			
			else if( m_type == InputType.GamepadAnalogButton)
				value = analogButtonStates[playerID] == ButtonState.JustPressed;
			
			if(value.HasValue && !value.Value)
				value = null;

			return value;
		}

		public bool? GetButtonUp(int playerID)
		{
			bool? value = null;

			if(m_type == InputType.KeyButton)
				value = Input.GetKeyUp(m_positive);
			
			else if(m_type == InputType.GamepadButton)
				value = InputManager.Gamepad.GetButtonUp(m_gamepadButton, playerID);
			
			else if( m_type == InputType.GamepadAnalogButton)
				value = analogButtonStates[playerID] == ButtonState.JustReleased;
			
			if(value.HasValue && !value.Value)
				value = null;

			return value;
		}
		#endregion

		
		public static InputBinding Duplicate(InputBinding source)
		{
			InputBinding duplicate = new InputBinding();
			duplicate.Copy(source);
			return duplicate;
		}
		public void Copy(InputBinding source)
		{
			m_positive = source.m_positive;
			m_negative = source.m_negative;

			m_deadZone = source.m_deadZone;
			m_gravity = source.m_gravity;
			m_sensitivity = source.m_sensitivity;
			m_snap = source.m_snap;
			m_invert = source.m_invert;
			m_type = source.m_type;

			mouseAxis = source.mouseAxis;
			m_gamepadAxis = source.m_gamepadAxis;
			m_gamepadButton = source.m_gamepadButton;
		}
	}
}