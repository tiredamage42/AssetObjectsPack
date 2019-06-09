
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CustomInputManager
{
	[Serializable]
	public class InputAction
	{
		public const int MAX_BINDINGS = 16;
		[SerializeField] private string m_name;
		[SerializeField] private List<InputBinding> m_bindings;

		[SerializeField] private bool m_rebindable;
		[SerializeField] private string m_displayName;
		


		public ReadOnlyCollection<InputBinding> Bindings
		{
			get { return m_bindings.AsReadOnly(); }
		}

		public bool rebindable
		{
			get { return m_rebindable; }
			set
			{
				m_rebindable = value;
			}
		}
		public string displayName
		{
			get { return string.IsNullOrEmpty( m_displayName ) ? Name : m_displayName; }
			set
			{
				m_displayName = value;
			}
		}

		public string Name
		{
			get { return m_name; }
			set
			{
				if(Application.isPlaying)
					Debug.LogWarning("You should not change the name of an input action at runtime");
				else 
					m_name = value;
			}
		}

		public bool AnyInput(int playerID)
		{
			foreach(var binding in m_bindings) {
				if(binding.AnyInput(playerID)) return true;
			}
			return false;
		}
		
		public InputAction(string name) : this(name, name, false) {
			
		}
		
		public InputAction(string name, string displayName, bool rebindable)
		{
			m_name = name;
			m_displayName = displayName;
			m_rebindable = rebindable;
			m_bindings = new List<InputBinding>();
		}
		
		public void Initialize()
		{
			foreach(var binding in m_bindings)
			{
				binding.Initialize();
			}
		}
		
		public void Update(float deltaTime)
		{
			foreach(var binding in m_bindings)
			{
				binding.Update(deltaTime);
			}
		}
		
		public float GetAxis(int playerID)
		{
			float? value = null;
			foreach(var binding in m_bindings)
			{
				value = binding.GetAxis(playerID);
				if(value.HasValue)
					break;
			}
			return value ?? InputBinding.AXIS_NEUTRAL;
		}

		///<summary>
		///	Returns raw input with no sensitivity or smoothing applyed.
		/// </summary>
		public float GetAxisRaw(int playerID)
		{
			float? value = null;
			foreach(var binding in m_bindings)
			{
				value = binding.GetAxisRaw(playerID);
				if(value.HasValue)
					break;
			}
			return value ?? InputBinding.AXIS_NEUTRAL;
		}
		
		public bool GetButton(int playerID)
		{
			bool? value = null;
			foreach(var binding in m_bindings)
			{
				value = binding.GetButton(playerID);
				if(value.HasValue)
					break;
			}

			return value ?? false;
		}
		
		public bool GetButtonDown(int playerID)
		{
			bool? value = null;
			foreach(var binding in m_bindings)
			{
				value = binding.GetButtonDown(playerID);
				if(value.HasValue)
					break;
			}

			return value ?? false;
		}
		
		public bool GetButtonUp(int playerID)
		{
			bool? value = null;
			foreach(var binding in m_bindings)
			{
				value = binding.GetButtonUp(playerID);
				if(value.HasValue)
					break;
			}
			return value ?? false;
		}

		public InputBinding GetBinding(int index)
		{
			if(index >= 0 && index < m_bindings.Count)
				return m_bindings[index];

			return null;
		}

		public InputBinding CreateNewBinding()
		{
			if(m_bindings.Count < MAX_BINDINGS)
			{
				InputBinding binding = new InputBinding();
				m_bindings.Add(binding);

				return binding;
			}

			return null;
		}

		public InputBinding CreateNewBinding(InputBinding source)
		{
			if(m_bindings.Count < MAX_BINDINGS)
			{
				InputBinding binding = InputBinding.Duplicate(source);
				m_bindings.Add(binding);

				return binding;
			}

			return null;
		}

		public InputBinding InsertNewBinding(int index)
		{
			if(m_bindings.Count < MAX_BINDINGS)
			{
				InputBinding binding = new InputBinding();
				m_bindings.Insert(index, binding);

				return binding;
			}

			return null;
		}

		public InputBinding InsertNewBinding(int index, InputBinding source)
		{
			if(m_bindings.Count < MAX_BINDINGS)
			{
				InputBinding binding = InputBinding.Duplicate(source);
				m_bindings.Insert(index, binding);

				return binding;
			}

			return null;
		}

		public void DeleteBinding(int index)
		{
			if(index >= 0 && index < m_bindings.Count)
				m_bindings.RemoveAt(index);
		}

		public void SwapBindings(int fromIndex, int toIndex)
		{
			if(fromIndex >= 0 && fromIndex < m_bindings.Count && toIndex >= 0 && toIndex < m_bindings.Count)
			{
				var temp = m_bindings[toIndex];
				m_bindings[toIndex] = m_bindings[fromIndex];
				m_bindings[fromIndex] = temp;
			}
		}

		public void Copy(InputAction source)
		{

			m_name = source.m_name;
			m_displayName = source.m_displayName;
			m_rebindable = source.m_rebindable;

			m_bindings.Clear();
			foreach(var binding in source.m_bindings)
			{
				m_bindings.Add(InputBinding.Duplicate(binding));
			}
		}

		public static InputAction Duplicate(InputAction source)
		{
			return Duplicate(source.m_name, source);
		}

		public static InputAction Duplicate(string name, InputAction source)
		{
			InputAction duplicate = new InputAction("_");

			duplicate.Copy (source);
			return duplicate;
		}
	}
}