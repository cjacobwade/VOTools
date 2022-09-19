using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VoiceLineData))]
public class VoiceLineDataEditor : Editor
{
	public override bool RequiresConstantRepaint()
	{ return true; }

	public override void OnInspectorGUI()
	{
		GUILayout.BeginHorizontal();

		if (GUILayout.Button("Prev") || 
			(Event.current.type == EventType.KeyDown &&
			Event.current.keyCode == KeyCode.LeftArrow))
		{
			ChangeSelection(-1);
			
		}
		else if(GUILayout.Button("Next") || 
			(Event.current.type == EventType.KeyDown && 
			Event.current.keyCode == KeyCode.RightArrow))
		{
			ChangeSelection(1);
		}

		GUILayout.EndHorizontal();

		base.OnInspectorGUI();
	}

	private void ChangeSelection(int change)
	{
		VoiceLineData vod = target as VoiceLineData;

		string path = AssetDatabase.GetAssetPath(target as VoiceLineData);
		path = path.Replace("Assets/", "");
		path = path.Replace(target.name + ".asset", "");

		VoiceLineData[] voiceLineDatas = EditorUtils.LoadAllAssetsAtPath<VoiceLineData>(path);
		int vodIndex = System.Array.IndexOf(voiceLineDatas, vod);

		vodIndex = (int)Mathf.Repeat(vodIndex + change, voiceLineDatas.Length);
		Selection.activeObject = voiceLineDatas[vodIndex];

		Event.current.Use();
	}
}
