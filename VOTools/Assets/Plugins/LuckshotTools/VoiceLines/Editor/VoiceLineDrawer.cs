using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomPropertyDrawer(typeof(VoiceLine))]
public class VoiceLineDrawer : PropertyDrawer
{
	private bool recordingVO = false;
	private System.DateTime recordStartTime = default;
	private AudioClip recordClip = null;

	private static int micDeviceIndex = 0;

	private float prevPreviewTime = 0f;
	private AudioSource previewSource = null;
	private AudioSource insertSource = null;

	private float maxRecordTime = 60f;

	private bool insertPlaying = false;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		CreatePreviewSourcesIfNeeded();

		VoiceLineData containerVOD = property.serializedObject.targetObject as VoiceLineData;

		VoiceLine voiceLine = EditorUtils.GetTargetObjectOfProperty(property) as VoiceLine;
		if (voiceLine == null)
			return;

		var textProp = property.FindPropertyRelative("text");
		string prevText = textProp.stringValue;

		position.height = EditorGUI.GetPropertyHeight(textProp);

		EditorGUI.PropertyField(position, textProp);

		if (textProp.stringValue != prevText)
		{
			Debug.LogError("Do not make changes to text in voice lines. This is for reference only!");
			textProp.stringValue = prevText;
		}

		position.y += position.height;
		position.height = EditorGUIUtility.singleLineHeight;

		var clipProp = property.FindPropertyRelative("clip");
		AudioClip clip = clipProp.objectReferenceValue as AudioClip;
		if (clip != null)
			EditorGUI.PropertyField(position, clipProp, new GUIContent(string.Format("{0} ({1:0.00}s)", clipProp.displayName, clip.length)));
		else
			EditorGUI.PropertyField(position, clipProp);

		position.y += EditorGUIUtility.singleLineHeight;

		var timelineProp = property.FindPropertyRelative("timeline");
		position.height = EditorGUI.GetPropertyHeight(timelineProp);
		EditorGUI.PropertyField(position, timelineProp, true);

		Rect timelinePosition = new Rect(position.position, position.size);

		position.y += position.height;
		position.height = EditorGUIUtility.singleLineHeight;

		if (Microphone.devices.Length == 0)
		{
			GUI.Label(position, "No Mic Found.");
			micDeviceIndex = 0;
		}
		else
		{
			bool wasEnabled = GUI.enabled;
			GUI.enabled = !recordingVO;

			micDeviceIndex = EditorGUI.Popup(position, "Recording device", micDeviceIndex, Microphone.devices);
			
			GUI.enabled = wasEnabled;

			if (!recordingVO)
			{
				position.y += EditorGUIUtility.singleLineHeight;

				if (GUI.Button(position, "Record VO"))
				{
					recordClip = Microphone.Start(Microphone.devices[micDeviceIndex], false, (int)maxRecordTime, 44100);
					if (recordClip != null)
					{
						recordStartTime = System.DateTime.Now;
						recordingVO = true;
					}
					else
					{
						Microphone.End(null);
					}

					return;
				}
			}
		}

		if (recordingVO)
		{
			float elapsedTime = (float)(System.DateTime.Now - recordStartTime).TotalSeconds;

			position.y += EditorGUIUtility.singleLineHeight;

			GUI.color = Color.red;
			if (elapsedTime >= maxRecordTime || GUI.Button(position, "Stop Recording"))
			{
				GUI.color = Color.white;

				recordingVO = false;

				if (recordClip != null)
				{
					Debug.Log(Microphone.IsRecording(null));

					Microphone.End(null);

					if (elapsedTime > 0f)
					{
						AudioClip trimmedClip = recordClip.TrimSilence();

						byte[] bytes = WavUtility.FromAudioClip(trimmedClip);

						string path = AssetDatabase.GetAssetPath(containerVOD);
						path = path.Replace(".asset", ".wav");

						File.WriteAllBytes(path, bytes);

						AssetDatabase.ImportAsset(path);

						AudioClip onDiskClip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
						voiceLine.clip = onDiskClip;
						voiceLine.timeline.clip = onDiskClip;

						EditorUtility.SetDirty(containerVOD);

						AssetDatabase.SaveAssets();
					}
				}
			}

			GUI.color = Color.white;
		}
		else
		{
			if (voiceLine.clip != null)
			{
				int controlID = GUIUtility.GetControlID(FocusType.Passive);

				if (Event.current.type == EventType.MouseDown &&
					timelinePosition.Contains(Event.current.mousePosition))
				{
					float normalizedTime = Mathf.InverseLerp(timelinePosition.x,
						timelinePosition.x + timelinePosition.width,
						Event.current.mousePosition.x);

					PlayPreviewSFX(voiceLine.clip, normalizedTime * voiceLine.clip.length);

					Event.current.Use();

					GUIUtility.hotControl = controlID;
					GUIUtility.keyboardControl = 0;
				}

				position.y += EditorGUIUtility.singleLineHeight;

				if (previewSource.isPlaying || insertPlaying)
				{
					if (insertPlaying && !insertSource.isPlaying)
					{
						insertPlaying = false;
						previewSource.UnPause();
					}

					var keysProp = timelineProp.FindPropertyRelative("keys");
					for(int i =0; i < keysProp.arraySize; i++)
					{
						var keyProp = keysProp.GetArrayElementAtIndex(i);
						var timeProp = keyProp.FindPropertyRelative("time");

						if(	prevPreviewTime < timeProp.floatValue &&
							previewSource.time > timeProp.floatValue)
						{
							var insertClipProp = keyProp.FindPropertyRelative("insertClip");

							AudioClip insertClip = insertClipProp.objectReferenceValue as AudioClip;
							if (insertClip != null)
								PlayInsertSFX(insertClip);
						}
					}

					prevPreviewTime = previewSource.time;

					float normalizedTime = previewSource.time / previewSource.clip.length;

					Rect playheadPosition = new Rect(timelinePosition.position, new Vector2(2, EditorGUIUtility.singleLineHeight * 2f));
					playheadPosition.x += timelinePosition.width * normalizedTime;

					GUI.color = Color.green;
					GUI.Box(playheadPosition, "");

					if (GUI.Button(position, "Stop Preview"))
					{
						previewSource.Stop();
						insertSource.Stop();

						insertPlaying = false;
					}

					GUI.color = Color.white;
				}
				else
				{
					if (GUI.Button(position, "Play Preview"))
					{
						PlayPreviewSFX(voiceLine.clip);
					}
				}
			}
		}
	}

	private void CreatePreviewSourcesIfNeeded()
	{
		if (previewSource == null)
		{
			previewSource = new GameObject("PreviewSource").AddComponent<AudioSource>();
			previewSource.gameObject.hideFlags = HideFlags.HideAndDontSave;
		}

		if (insertSource == null)
		{
			insertSource = new GameObject("CardSource").AddComponent<AudioSource>();
			insertSource.gameObject.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	private void PlayPreviewSFX(AudioClip clip, float startTime = 0f)
	{
		prevPreviewTime = 0f;

		previewSource.clip = clip;
		previewSource.time = startTime;

		previewSource.Stop();
		previewSource.Play();
	}

	private void PlayInsertSFX(AudioClip clip)
	{
		previewSource.Pause();

		if (clip != null)
		{
			insertSource.clip = clip;
			insertSource.Play();

			insertPlaying = true;
		}
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float height = EditorGUIUtility.singleLineHeight;
		
		var textProp = property.FindPropertyRelative("text");
		if (textProp != null)
			height += EditorGUI.GetPropertyHeight(textProp);

		var timelineProp = property.FindPropertyRelative("timeline");
		if(timelineProp != null)
			height += EditorGUI.GetPropertyHeight(timelineProp, true);

		if (Microphone.devices.Length == 0)
		{
			height += EditorGUIUtility.singleLineHeight; // mic devices
		}
		else
		{
			height += EditorGUIUtility.singleLineHeight;

			if (!recordingVO)
				height += EditorGUIUtility.singleLineHeight;
		}

		if (recordingVO)
		{
			height += EditorGUIUtility.singleLineHeight;
		}
		else
		{
			VoiceLine voiceLine = EditorUtils.GetTargetObjectOfProperty(property) as VoiceLine;
			if (voiceLine == null)
				return height;

			if (voiceLine.clip != null)
				height += EditorGUIUtility.singleLineHeight;
		}

		return height;
	}
}
