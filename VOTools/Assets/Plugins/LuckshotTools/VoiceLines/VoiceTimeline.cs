using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VoiceTimeline
{
	public AudioClip clip = null;
	public List<VoiceTimelineKey> keys = new List<VoiceTimelineKey>(); 
}
