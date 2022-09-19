using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VoiceLine
{
	[TextArea(3, 20)]
	public string text = string.Empty;
	public AudioClip clip = null;
	public VoiceTimeline timeline = null;
}

[CreateAssetMenu(fileName = "VoiceLineData", menuName = "Luckshot/Voice Line Data")]
public class VoiceLineData : ScriptableObject
{
	[UnityEngine.Serialization.FormerlySerializedAs("vo")]
	public VoiceLine VO = null;
}
