using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(VoiceTimeline))]
public class VoiceTimelineDrawer : PropertyDrawer
{
	private int selectedKey = -1;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		var clipProp = property.FindPropertyRelative("clip");

		AudioClip clip = clipProp.objectReferenceValue as AudioClip;
		if(clip != null && clip.length > 0 && position.width > 0)
		{
			int controlID = GUIUtility.GetControlID(FocusType.Passive);

			float timelineWidth = position.width;
			float timelineHeight = EditorGUIUtility.singleLineHeight * 2f;

			position.height = timelineHeight;

			Texture2D waveformTex = BuildWaveformTex(clip, (int)position.width, (int)position.height);

			GUI.DrawTexture(position, waveformTex);

			Rect mouseBox = new Rect(Event.current.mousePosition, Vector2.one * 10);

			var keysProp = property.FindPropertyRelative("keys");

			Rect keyPosition = position;
			keyPosition.height = position.height * 0.8f;
			keyPosition.width = 7;
			keyPosition.y = position.y + (position.height - keyPosition.height) / 2f;

			bool rightClickedKey = false;
			bool queuedDeselect = false;

			if (selectedKey != -1)
			{
				var keyProp = keysProp.GetArrayElementAtIndex(selectedKey);

				var timeProp = keyProp.FindPropertyRelative("time");
				float normalizedX = timeProp.floatValue / clip.length;

				keyPosition.x = position.x + normalizedX * timelineWidth - keyPosition.width / 2f;

				if (Event.current.type == EventType.MouseDown &&
					position.Contains(Event.current.mousePosition) &&
					!keyPosition.Contains(Event.current.mousePosition))
				{
					queuedDeselect = true;
				}
			}

			for (int i = 0; i < keysProp.arraySize; i++)
			{
				var keyProp = keysProp.GetArrayElementAtIndex(i);

				var timeProp = keyProp.FindPropertyRelative("time");
				float time = timeProp.floatValue;

				if (time > clip.length)
					timeProp.floatValue = clip.length;

				float normalizedX = time / clip.length;
				
				keyPosition.x = position.x + normalizedX * timelineWidth - keyPosition.width/2f;

				if (i == selectedKey)
					GUI.color = Color.green;
				else
					GUI.color = Color.white;

				Texture2D tex = new Texture2D(1,1);
				tex.SetPixel(0, 0, Color.white);
				tex.Apply();

				GUI.DrawTexture(keyPosition, tex);
				GUI.color = Color.white;

				if(selectedKey == -1)
				{
					if (Event.current.button == 0 &&
						Event.current.type == EventType.MouseDown &&
						keyPosition.Contains(Event.current.mousePosition))
					{
						selectedKey = i;
						
						keyProp.isExpanded = true;
						keyProp.serializedObject.ApplyModifiedProperties();

						Event.current.Use();
						GUI.changed = true;

						EditorGUI.EndProperty();
						return;
					}
				}
				else
				{
					if (i == selectedKey)
					{
						if (Event.current.type == EventType.MouseDrag)
						{
							float normalizedMouseX = Mathf.InverseLerp(position.x, position.x + position.width, Event.current.mousePosition.x);
							keyPosition.x = normalizedMouseX * timelineWidth;
							timeProp.floatValue = normalizedMouseX * clip.length;

							timeProp.serializedObject.ApplyModifiedProperties();
							EditorGUI.EndProperty();
							return;
						}

						if (Event.current.keyCode == KeyCode.Delete)
						{
							keysProp.DeleteArrayElementAtIndex(selectedKey);

							selectedKey = -1;

							timeProp.serializedObject.ApplyModifiedProperties();

							Event.current.Use();

							GUIUtility.hotControl = controlID;
							GUIUtility.keyboardControl = 0;

							EditorGUI.EndProperty();
							return;
						}

						timeProp.serializedObject.ApplyModifiedProperties();
					}
					else
					{
						if (Event.current.type == EventType.MouseDown &&
							keyPosition.Contains(Event.current.mousePosition))
						{
							selectedKey = i;

							Event.current.Use();

							GUIUtility.hotControl = controlID;
							GUIUtility.keyboardControl = 0;

							EditorGUI.EndProperty();
							return;
						}
					}
				}

				if (Event.current.type == EventType.ContextClick &&
					keyPosition.Contains(Event.current.mousePosition))
				{
					rightClickedKey = true;
					int keyIndex = i;

					GenericMenu menu = new GenericMenu();
					menu.AddItem(new GUIContent("Delete Key"), false, () =>
					{
						keysProp.DeleteArrayElementAtIndex(keyIndex);
						keysProp.serializedObject.ApplyModifiedProperties();
					});
					menu.ShowAsContext();

					Event.current.Use();

					selectedKey = -1;
				}
			}

			position.y += position.height;

			if (queuedDeselect)
			{
				selectedKey = -1;

				Event.current.Use();

				GUIUtility.hotControl = controlID;
				GUIUtility.keyboardControl = 0;

				GUI.changed = true;
				EditorGUI.EndProperty();
				return;
			}

			if (!rightClickedKey && 
				Event.current.type == EventType.ContextClick)
			{
				Vector2 contextPos = Event.current.mousePosition;

				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("Add Key"), false, () =>
				{
					keysProp.InsertArrayElementAtIndex(0);

					keysProp.serializedObject.ApplyModifiedProperties();

					var keyProp = keysProp.GetArrayElementAtIndex(0);

					float normalizedMouseX = Mathf.InverseLerp(position.x, position.x + position.width, contextPos.x);
					keyPosition.x = normalizedMouseX * timelineWidth;

					var timeProp = keyProp.FindPropertyRelative("time");
					timeProp.floatValue = normalizedMouseX * clip.length;

					var cardKeyProp = keyProp.FindPropertyRelative("cardKey");
					cardKeyProp.stringValue = "{CardName}"; // this shouldn't be necessary hmmm

					selectedKey = 0;

					property.serializedObject.ApplyModifiedProperties();
				});
				menu.ShowAsContext();

				Event.current.Use();
				
				GUIUtility.hotControl = controlID;
				GUIUtility.keyboardControl = 0;

				EditorGUI.EndProperty();
				return;
			}

			if(selectedKey != -1)
			{
				var keyProp = keysProp.GetArrayElementAtIndex(selectedKey);

				position.height = EditorGUI.GetPropertyHeight(keyProp, true);
				EditorGUI.PropertyField(position, keyProp, new GUIContent("Keyframe"), true);

				keyProp.serializedObject.ApplyModifiedProperties();
			}
		}

		if (GUI.changed)
		{
			property.serializedObject.ApplyModifiedProperties();
		}
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float height = 0f;

		var clipProp = property.FindPropertyRelative("clip");
		AudioClip clip = clipProp.objectReferenceValue as AudioClip;
		if (clip != null)
		{
			height += EditorGUIUtility.singleLineHeight * 2f;

			if (selectedKey != -1)
			{
				var keysProp = property.FindPropertyRelative("keys");
				var keyProp = keysProp.GetArrayElementAtIndex(selectedKey);
				
				if (keyProp.isExpanded)
					height += EditorGUI.GetPropertyHeight(keyProp, true);
				else
					height += EditorGUIUtility.singleLineHeight;
			}
		}

		return height;
	}

	// https://answers.unity.com/questions/189886/displaying-an-audio-waveform-in-the-editor.html
	public Texture2D BuildWaveformTex(AudioClip audio, int width, int height) 
    {
		Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
		float[] samples = new float[audio.samples];
		float[] waveform = new float[width];

		audio.GetData(samples, 0);

		int packSize = ( audio.samples / width ) + 1;
		int s = 0;

		for (int i = 0; i < audio.samples; i += packSize) 
		{
			waveform[s] = Mathf.Abs(samples[i]);
			s++;
		}

		Color bgColor = new Color(0.3f, 0.3f, 0.3f, 1f);

		for (int x = 0; x < width; x++) 
		{
			for (int y = 0; y < height; y++) 
			{
				tex.SetPixel(x, y, bgColor);
			}
		}
 
		for (int x = 0; x < waveform.Length; x++) 
		{
			for (int y = 0; y <= waveform[x] * ((float)height * 0.95f); y++)
			{
				tex.SetPixel(x, ( height / 2 ) + y, Color.red);
				tex.SetPixel(x, ( height / 2 ) - y, Color.red);
			}
		}

		tex.Apply();
 
		return tex;
    }
}
