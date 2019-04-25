using System;
using System.IO;
using System.Xml;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace CustomInputManager
{
	public class InputLoaderXML 
	{
		private string m_filename;
		private Stream m_inputStream;
		private TextReader m_textReader;

		public InputLoaderXML(string filename)
		{
			if(filename == null)
				throw new ArgumentNullException("filename");
			
			m_filename = filename;
			m_inputStream = null;
			m_textReader = null;
		}

		
		public InputLoaderXML(TextReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException("reader");
			
			m_filename = null;
			m_inputStream = null;
			m_textReader = reader;
		}

		private XmlDocument CreateXmlDocument()
		{
			if(m_filename != null)
			{
				using(StreamReader reader = File.OpenText(m_filename))
				{
					XmlDocument doc = new XmlDocument();
					doc.Load(reader);

					return doc;
				}
			}
			else if(m_inputStream != null)
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(m_inputStream);

				return doc;
			}
			else if(m_textReader != null)
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(m_textReader);

				return doc;
			}

			return null;
		}

		public SaveData Load()
		{
			XmlDocument doc = CreateXmlDocument();
			if(doc != null)
			{
				return Load_V2(doc);
			}

			return new SaveData();
		}

		public ControlScheme Load(string schemeName)
		{
			XmlDocument doc = CreateXmlDocument();
			if(doc != null)
			{
				return Load_V2(doc, schemeName);
			}
			return null;
		}

		#region [V2]
		private SaveData Load_V2(XmlDocument doc)
		{
			SaveData saveData = new SaveData();
			var root = doc.DocumentElement;

			for (int i = 0; i < InputManager.maxPlayers; i++) {
				saveData.playerSchemes[i] = ReadNode(SelectSingleNode(root, "PlayerScheme"+i));
			}
				
			var schemeNodes = SelectNodes(root, "ControlScheme");
			foreach(XmlNode node in schemeNodes)
			{
				saveData.ControlSchemes.Add(ReadControlScheme_V2(node));
			}

			return saveData;
		}

		private ControlScheme Load_V2(XmlDocument doc, string schemeName)
		{
			if(string.IsNullOrEmpty(schemeName))
				return null;

			ControlScheme scheme = null;
			var schemeNodes = SelectNodes(doc.DocumentElement, "ControlScheme");
			foreach(XmlNode node in schemeNodes)
			{
				if(ReadAttribute(node, "name") == schemeName)
				{
					scheme = ReadControlScheme_V2(node);
					break;
				}
			}

			return scheme;
		}

		private ControlScheme ReadControlScheme_V2(XmlNode node)
		{
			string name = ReadAttribute(node, "name", "Unnamed Control Scheme");
			string id = ReadAttribute(node, "id", null);
			ControlScheme scheme = new ControlScheme(name);
			scheme.UniqueID = id ?? ControlScheme.GenerateUniqueID();

			var actionNodes = SelectNodes(node, "Action");
			foreach(XmlNode child in actionNodes)
			{
				ReadInputAction_V2(scheme, child);
			}

			return scheme;
		}

		private void ReadInputAction_V2(ControlScheme scheme, XmlNode node)
		{
			string name = ReadAttribute(node, "name", "Unnamed Action");
			string displayName = ReadAttribute(node, "displayName", name);
			bool rebindable =  ReadAsBool(node.FirstChild);

			InputAction action = scheme.CreateNewAction(name, displayName, rebindable);
			var bindingNodes = SelectNodes(node, "Binding");
			foreach(XmlNode child in bindingNodes)
			{
				ReadInputBinding_V2(action, child);
			}
		}

		static KeyCode StringToKey(string value) {
			return StringToEnum(value, KeyCode.None);
		}
		static InputType StringToInputType(string value) {
			return StringToEnum(value, InputType.KeyButton);
		}
		static GamepadButton StringToGamepadButton(string value) {
			return StringToEnum(value, GamepadButton.None);
		}
		static GamepadAxis StringToGamepadAxis(string value) {
			return StringToEnum(value, GamepadAxis.None);
		}

		static T StringToEnum<T>(string value, T defValue)
		{
			if(string.IsNullOrEmpty(value))
				return defValue;
			
			try {
				return (T)Enum.Parse(typeof(T), value, true);
			}
			catch {
				return defValue;
			}
		}


		private void ReadInputBinding_V2(InputAction action, XmlNode node)
		{
			InputBinding binding = action.CreateNewBinding();
			foreach(XmlNode child in node.ChildNodes)
			{
				switch(child.LocalName)
				{
				case "Positive":
					binding.Positive = StringToKey(child.InnerText);
					break;
				case "Negative":
					binding.Negative = StringToKey(child.InnerText);
					break;
				case "DeadZone":
					binding.DeadZone = ReadAsFloat(child);
					break;
				case "Gravity":
					binding.Gravity = ReadAsFloat(child, 1.0f);
					break;
				case "Sensitivity":
					binding.Sensitivity = ReadAsFloat(child, 1.0f);
					break;
				case "Snap":
					binding.Snap = ReadAsBool(child);
					break;
				case "Invert":
					binding.Invert = ReadAsBool(child);
					break;
				case "Type":
					binding.Type = StringToInputType(child.InnerText);
					break;
				case "Axis":
					binding.MouseAxis = ReadAsInt(child);
					break;
				case "GamepadButton":
					binding.GamepadButton = StringToGamepadButton(child.InnerText);
					break;
				case "GamepadAxis":
					binding.GamepadAxis = StringToGamepadAxis(child.InnerText);
					break;
				}
			}
		}
		#endregion

		
		#region [Helper]
		private XmlNode SelectSingleNode(XmlNode parent, string name)
		{
#if UNITY_WSA && !UNITY_EDITOR
			return parent.ChildNodes.Cast<XmlNode>().First(nd => nd.LocalName == name);
#else
			return parent.SelectSingleNode(name);
#endif
		}

		private IEnumerable<XmlNode> SelectNodes(XmlNode parent, string name)
		{
#if UNITY_WSA && !UNITY_EDITOR
			return parent.ChildNodes.Cast<XmlNode>().Where(nd => nd.LocalName == name);
#else
			return parent.SelectNodes(name).Cast<XmlNode>();
#endif
		}

		private string ReadAttribute(XmlNode node, string attribute, string defValue = null)
		{
			if(node.Attributes[attribute] != null)
				return node.Attributes[attribute].InnerText;

			return defValue;
		}

		private string ReadNode(XmlNode node, string defValue = null)
		{
			return node != null ? node.InnerText : defValue;
		}

		private int ReadAsInt(XmlNode node, int defValue = 0)
		{
			int value = 0;
			if(int.TryParse(node.InnerText, out value))
				return value;

			return defValue;
		}

		private float ReadAsFloat(XmlNode node, float defValue = 0.0f)
		{
			float value = 0;
			if(float.TryParse(node.InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
				return value;

			return defValue;
		}

		private bool ReadAsBool(XmlNode node, bool defValue = false)
		{
			bool value = false;
			if(bool.TryParse(node.InnerText, out value))
				return value;

			return defValue;
		}
#endregion
	}
}
