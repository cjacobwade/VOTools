using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;
using Object = UnityEngine.Object;
using System.IO;

public static class EditorUtils
{
	public static T[] LoadAllAssetsAtPath<T>(string path, bool recursive = false) where T : UnityEngine.Object
	{
		List<T> tCollection = new List<T>();

		string[] fileEntries = null;

		string directory = Application.dataPath + "/" + path;

		if (recursive)
			fileEntries = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
		else
			fileEntries = Directory.GetFiles(directory);

		foreach (string fileName in fileEntries)
		{
			int index = fileName.LastIndexOf("/");
			string localPath = "Assets/" + path;

			if (index > 0)
				localPath += fileName.Substring(index);

			T tObject = AssetDatabase.LoadAssetAtPath<T>(localPath);
			if (tObject != null)
				tCollection.Add(tObject);
		}

		return tCollection.ToArray();
	}

	public static T CreateOrReplaceAsset<T>(T asset, string path) where T : Object
	{
		T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);

		if (existingAsset == null)
		{
			AssetDatabase.CreateAsset(asset, path);
			existingAsset = asset;
		}
		else
		{
			EditorUtility.CopySerialized(asset, existingAsset);
		}

		return existingAsset;
	}

	public static object GetTargetObjectOfProperty(SerializedProperty prop)
	{
		var path = prop.propertyPath.Replace(".Array.data[", "[");
		object obj = prop.serializedObject.targetObject;
		var elements = path.Split('.');
		foreach (var element in elements)
		{
			if (element.Contains("["))
			{
				var elementName = element.Substring(0, element.IndexOf("["));
				var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
				obj = GetValue(obj, elementName, index);
			}
			else
			{
				obj = GetValue(obj, element);
			}
		}
		return obj;
	}

	public static Type GetTypeOfProperty(SerializedProperty prop)
	{
		object obj = GetTargetObjectOfProperty(prop);
		if (obj != null)
			return obj.GetType();

		return null;
	}

	public static object SetTargetObjectOfProperty(SerializedProperty prop, object value)
	{
		var path = prop.propertyPath.Replace(".Array.data[", "[");
		object obj = prop.serializedObject.targetObject;
		var elements = path.Split('.');
		foreach (var element in elements.Take(elements.Length - 1))
		{
			if (element.Contains("["))
			{
				var elementName = element.Substring(0, element.IndexOf("["));
				var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
				obj = GetValue(obj, elementName, index);
			}
			else
			{
				obj = GetValue(obj, element);
			}
		}

		if (ReferenceEquals(obj, null)) return null;

		try
		{
			var element = elements.Last();

			if (element.Contains("["))
			{
				var tp = obj.GetType();
				var elementName = element.Substring(0, element.IndexOf("["));
				var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
				var field = tp.GetField(elementName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				var arr = field.GetValue(obj) as System.Collections.IList;
				arr[index] = value;
			}
			else
			{
				var tp = obj.GetType();
				var field = tp.GetField(element, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if (field != null)
					field.SetValue(obj, value);
			}

			prop.serializedObject.Update(); // this way of setting the value means we definitely want this
			return obj;
		}
		catch
		{
			return null;
		}
	}

	private static object GetValue(object source, string name)
	{
		if (source == null)
			return null;

		var type = source.GetType();
		while (type != null)
		{
			var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			if (field != null)
				return field.GetValue(source);

			var property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
			if (property != null)
				return property.GetValue(source, null);

			type = type.BaseType;
		}
		return null;
	}

	private static object GetValue(object source, string name, int index)
	{
		var enumerable = GetValue(source, name) as IEnumerable;
		if (enumerable == null) return null;
		var enm = enumerable.GetEnumerator();

		for (int i = 0; i <= index; i++)
		{
			if (!enm.MoveNext()) return null;
		}
		return enm.Current;
	}

	public static void SetPropertyValue(SerializedProperty prop, object value)
	{
		if (prop == null) throw new ArgumentNullException("prop");

		switch (prop.propertyType)
		{
			case SerializedPropertyType.Integer:
				prop.intValue = Convert.ToInt32(value);
				break;
			case SerializedPropertyType.Boolean:
				prop.boolValue = Convert.ToBoolean(value);
				break;
			case SerializedPropertyType.Float:
				prop.floatValue = Convert.ToSingle(value);
				break;
			case SerializedPropertyType.String:
				prop.stringValue = Convert.ToString(value);
				break;
			case SerializedPropertyType.Color:
				prop.colorValue = (Color)value;
				break;
			case SerializedPropertyType.ObjectReference:
				prop.objectReferenceValue = value as UnityEngine.Object;
				break;
			case SerializedPropertyType.LayerMask:
				prop.intValue = (value is LayerMask) ? ((LayerMask)value).value : Convert.ToInt32(value);
				break;
			case SerializedPropertyType.Enum:
				prop.enumValueIndex = Convert.ToInt32(value);
				break;
			case SerializedPropertyType.Vector2:
				prop.vector2Value = (Vector2)value;
				break;
			case SerializedPropertyType.Vector3:
				prop.vector3Value = (Vector3)value;
				break;
			case SerializedPropertyType.Vector4:
				prop.vector4Value = (Vector4)value;
				break;
			case SerializedPropertyType.Rect:
				prop.rectValue = (Rect)value;
				break;
			case SerializedPropertyType.ArraySize:
				prop.arraySize = Convert.ToInt32(value);
				break;
			case SerializedPropertyType.Character:
				prop.intValue = Convert.ToChar(value);
				break;
			case SerializedPropertyType.AnimationCurve:
				prop.animationCurveValue = value as AnimationCurve;
				break;
			case SerializedPropertyType.Bounds:
				prop.boundsValue = (Bounds)value;
				break;
			case SerializedPropertyType.Gradient:
				throw new System.InvalidOperationException("Can not handle Gradient types.");
		}
	}

	public static object GetPropertyValue(SerializedProperty prop)
	{
		if (prop == null) throw new ArgumentNullException("prop");

		switch (prop.propertyType)
		{
			case SerializedPropertyType.Integer:
				return prop.intValue;
			case SerializedPropertyType.Boolean:
				return prop.boolValue;
			case SerializedPropertyType.Float:
				return prop.floatValue;
			case SerializedPropertyType.String:
				return prop.stringValue;
			case SerializedPropertyType.Color:
				return prop.colorValue;
			case SerializedPropertyType.ObjectReference:
				return prop.objectReferenceValue;
			case SerializedPropertyType.LayerMask:
				return (LayerMask)prop.intValue;
			case SerializedPropertyType.Enum:
				return prop.enumValueIndex;
			case SerializedPropertyType.Vector2:
				return prop.vector2Value;
			case SerializedPropertyType.Vector3:
				return prop.vector3Value;
			case SerializedPropertyType.Vector4:
				return prop.vector4Value;
			case SerializedPropertyType.Rect:
				return prop.rectValue;
			case SerializedPropertyType.ArraySize:
				return prop.arraySize;
			case SerializedPropertyType.Character:
				return (char)prop.intValue;
			case SerializedPropertyType.AnimationCurve:
				return prop.animationCurveValue;
			case SerializedPropertyType.Bounds:
				return prop.boundsValue;
			case SerializedPropertyType.Gradient:
				throw new System.InvalidOperationException("Can not handle Gradient types.");
		}

		return null;
	}
}
