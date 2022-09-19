// Below is an example implementation used in CardCowboy
// 

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class EventVOInfo
{
	public EventData eventData = null;

	public VoiceLineData descriptionVO = null;
	public VoiceLineData introVO = null;

	public List<ActionVOInfo> actionVOInfos = new List<ActionVOInfo>();

	public VoiceLineData noActionResponseVO = null;
}

[System.Serializable]
public class ActionVOInfo
{
	public VoiceLineData promptVO = null;
	public List<VoiceLineData> responseVOs = new List<VoiceLineData>();
}

[CreateAssetMenu(fileName = "YeeHowdyVoiceDatabase", menuName = "YeeHowdy/Voice Database")]
public class YeeHowdyVoiceDatabase : ScriptableObject
{
	public List<EventVOInfo> eventVOInfos = new List<EventVOInfo>();

	public List<VoiceLineData> comboResponseVOs = new List<VoiceLineData>();

	public List<VoiceLineData> cardVOs = new List<VoiceLineData>();

#if UNITY_EDITOR
	private readonly string voRootPath = "Assets/Data/VoiceLines/";

	[Button("Populate Event Response VOs")]
	public void PopulateEventResponseVOs()
	{
		eventVOInfos.Clear();

		EventData[] eventDatas = Resources.LoadAll<EventData>("EventDatas/");
		foreach (var eventData in eventDatas)
		{
			EventVOInfo eventLineInfo = new EventVOInfo();
			eventLineInfo.eventData = eventData;

			string eventName = eventData.name;
			string eventVORootPath = voRootPath + "Events/";

			// description?
			var descriptionVO = CreateVoiceLine(string.Format("{0}_Description", eventName),
				eventVORootPath, eventData.descriptionText);

			eventLineInfo.descriptionVO = descriptionVO;
			eventData.descriptionVO = descriptionVO;

			// intro
			var introVO = CreateVoiceLine(string.Format("{0}_Intro", eventName),
				eventVORootPath, eventData.introText);

			eventLineInfo.introVO = introVO;
			eventData.introVO = introVO;

			int numAction = 0;
			foreach (var actionInfo in eventData.eventActionInfos)
			{
				ActionVOInfo actionLinesInfo = new ActionVOInfo();

				// prompt
				var promptVO = CreateVoiceLine(string.Format("{0}_Action{1}_Prompt", eventName, numAction),
					eventVORootPath, actionInfo.prompt);

				actionInfo.promptVO = promptVO;
				actionLinesInfo.promptVO = promptVO;

				int numResponse = 0;
				foreach (var responseInfo in actionInfo.responseInfos)
				{
					// response
					var responseVO = CreateVoiceLine(string.Format("{0}_Action{1}_Response{2}", eventName, numAction, numResponse),
						eventVORootPath, responseInfo.responseText);

					responseInfo.responseVO = responseVO;
					actionLinesInfo.responseVOs.Add(responseInfo.responseVO);

					numResponse++;
				}

				eventLineInfo.actionVOInfos.Add(actionLinesInfo);
				numAction++;
			}

			// no action response
			if (eventData.noActionResponse != null &&
				!string.IsNullOrEmpty(eventData.noActionResponse.responseText))
			{
				var noActionResponseVO = CreateVoiceLine(string.Format("{0}_NoActionResponse", eventName),
					eventVORootPath, eventData.noActionResponse.responseText);

				eventData.noActionResponse.responseVO = noActionResponseVO;
				eventLineInfo.noActionResponseVO = noActionResponseVO;
			}

			EditorUtility.SetDirty(eventData);
			eventVOInfos.Add(eventLineInfo);
		}
	}

	[Button("Populate Combo Response VOs")]
	public void PopulateComboResponseVOs()
	{
		CardCombineMatrix combineMatrix = AssetDatabase.LoadAssetAtPath<CardCombineMatrix>("Assets/Data/CardCombineMatrix.asset");
		if(combineMatrix != null)
		{
			comboResponseVOs.Clear();
			string comboVORootPath = voRootPath + "ComboResponses/";

			foreach (var response in combineMatrix.comboResponses)
			{
				string comboName = string.Format("{0}X{1}", response.inputCardData.name, response.inputCardData2?.name ?? "None");

				VoiceLineData responseVO = CreateVoiceLine(comboName, comboVORootPath, response.responseText);
				response.responseVO = responseVO;

				comboResponseVOs.Add(responseVO);
			}

			VoiceLineData fallbackVO = CreateVoiceLine("Fallback", comboVORootPath, combineMatrix.fallbackResponse.responseText);
			combineMatrix.fallbackResponse.responseVO = fallbackVO;

			comboResponseVOs.Add(fallbackVO);

			EditorUtility.SetDirty(combineMatrix);
		}
	}

	[Button("Populate Card VOs")]
	public void PopulateCardVOs()
	{
		cardVOs.Clear();
		string cardVORootPath = voRootPath + "Cards/";

		CardData[] cardDatas = Resources.LoadAll<CardData>("CardDatas/");
		foreach (var cardData in cardDatas)
		{
			VoiceLineData cardVO = CreateVoiceLine(cardData.name, cardVORootPath, cardData.Name);
			cardData.cardVO = cardVO;

			cardVOs.Add(cardVO);

			EditorUtility.SetDirty(cardData);
		}
	}

	public ActionResponseInfo TryFindResponse(VoiceLineData voiceLine)
	{
		foreach(var eventVOInfo in eventVOInfos)
		{
			var eventData = eventVOInfo.eventData;
			if (eventData.introVO == voiceLine)
				return null;

			if (eventData.descriptionVO == voiceLine)
				return null;

			if (eventData.noActionResponse.responseVO == voiceLine)
				return eventData.noActionResponse;

			foreach(var actionInfo in eventData.eventActionInfos)
			{
				if (actionInfo.promptVO == voiceLine)
					return null;

				foreach(var responseInfo in actionInfo.responseInfos)
				{
					if (responseInfo.responseVO == voiceLine)
						return responseInfo;
				}
			}
		}

		CardCombineMatrix combineMatrix = AssetDatabase.LoadAssetAtPath<CardCombineMatrix>("Assets/Data/CardCombineMatrix.asset");
		if (combineMatrix != null)
		{
			foreach (var comboResponse in combineMatrix.comboResponses)
			{
				if (comboResponse.responseVO == voiceLine)
					return comboResponse;
			}

			if (combineMatrix.fallbackResponse.responseVO == voiceLine)
				return combineMatrix.fallbackResponse;
		}

		return null;
	}

	private VoiceLineData CreateVoiceLine(string name, string folderPath, string text)
	{
		if (!Directory.Exists(folderPath))
			Directory.CreateDirectory(folderPath);

		string path = string.Format("{0}VO_{1}.asset", folderPath, name);

		VoiceLineData vod = AssetDatabase.LoadAssetAtPath<VoiceLineData>(path);
		if (vod == null)
		{
			vod = ScriptableObject.CreateInstance<VoiceLineData>();
			AssetDatabase.CreateAsset(vod, path);
			AssetDatabase.ImportAsset(path);
		}

		vod.name = name;

		if(vod.VO == null)
			vod.VO = new VoiceLine();

		vod.VO.text = text;

		return vod;
	}
#endif // UNITY_EDITOR
}
*/