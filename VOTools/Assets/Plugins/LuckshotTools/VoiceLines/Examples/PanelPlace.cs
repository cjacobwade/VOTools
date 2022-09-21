// This is an example of how to play Voice Line Datas in-game.
// In this case the call to AudioManager.Instance.PlaySFX is spawning a prefab with an AudioSource
// and then setting the VO.clip as it's clip to be played.

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class PanelPlace : PanelBase
{
    [SerializeField]
    private TextMeshProUGUI
        title = null,
        description = null;

    [SerializeField]
    private SuperTextMesh enterButtonText = null;

    [SerializeField]
    private float showTime = 0f;

    private bool showing = false;
    private Transform target = null;

    [SerializeField]
    private AudioSFX voSFX = null;

    private AudioSFX activeVOSFX = null;
    private Coroutine voSequenceRoutine = null;

    [SerializeField]
    private float multiVOPadding = 0.15f;

    protected override void Awake()
    {
        base.Awake();

        rectTransform.localScale = Vector2.zero;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if(showing && target != null)
        {
            var pos = CamRig.Instance.WorldToCanvasPos(target.position);
            rectTransform.anchoredPosition = pos;
        }
    }

    public void Show(Place place)
    {
        title.text = place.EventData.displayName;

        if (description != null)
            description.text = place.EventData.descriptionText;
        
        if (enterButtonText != null)
            enterButtonText.text = "<w>" + place.EventData.enterText;

        gameObject.SetActive(true);
        showing = true;
        target = place.transform;

        var pos = CamRig.Instance.WorldToCanvasPos(target.position);

        rectTransform.anchoredPosition = pos;
        rectTransform.DOKill();
        rectTransform.DOScale(Vector2.one, showTime)
            .OnComplete(() =>
            {
                ClearActiveVOSFX();
                PlayVOLines(place.EventData.nameVO, place.EventData.descriptionVO);
            });
    }

    public void Close()
    {
        ClearActiveVOSFX();

        rectTransform.DOKill();
        rectTransform.DOScale(Vector2.zero, showTime).OnComplete(() =>
        {
           gameObject.SetActive(false);
           showing = false;
           rectTransform.localScale = Vector2.zero;
       });
    }

    private void ClearActiveVOSFX()
    {
        if (activeVOSFX != null)
            Destroy(activeVOSFX.gameObject);

        if (voSequenceRoutine != null)
            StopCoroutine(voSequenceRoutine);
    }

    private void PlayVOLine(VoiceLineData lineData)
    {
        if (lineData != null &&
            lineData.VO != null &&
            lineData.VO.clip != null)
        {
            ClearActiveVOSFX();
            activeVOSFX = AudioManager.Instance.PlayAudioSFX(voSFX, lineData.VO.clip);
        }
    }

    private float PlayVOLines(params VoiceLineData[] lineDatas)
    {
        if (voSequenceRoutine != null)
            StopCoroutine(voSequenceRoutine);

        voSequenceRoutine = StartCoroutine(PlayVOLines_Async(lineDatas));

        float totalVOLength = 0f;
        foreach (var lineData in lineDatas)
        {
            if (lineData.VO != null &&
                lineData.VO.clip != null)
            {
                totalVOLength += lineData.VO.clip.length;
            }

            totalVOLength += multiVOPadding;
        }

        return totalVOLength;
    }

    private IEnumerator PlayVOLines_Async(params VoiceLineData[] lineDatas)
    {
        foreach (var lineData in lineDatas)
        {
            if (lineData != null &&
                lineData.VO != null &&
                lineData.VO.clip != null)
            {
                List<VoiceTimelineKey> keys = lineData.VO.timeline.keys;
                keys.Sort((x, y) => x.time.CompareTo(y.time));

                PlayVOLine(lineData);

                float clipLength = 0f;
                if (activeVOSFX != null)
                    clipLength = activeVOSFX.clips[0].length;

                yield return new WaitForSeconds(clipLength + multiVOPadding); // add a bit of space between clips
            }
        }

        voSequenceRoutine = null;
    }
}
*/
