using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public float time = 3f;
    public Image fill;
    public bool isLoaded = false, turnOnAudio = false;
    public AudioSource audio;
    
    void OnEnable()
    {
        turnOnAudio = false;
        isLoaded = false;
        fill.fillAmount = 0f;
        if (audio != null)
        {
            audio.Stop();
        }

        StartCoroutine(Load());
    }

    IEnumerator Load()
    {
        float t = 0f;

        while (t < time)
        {
            if (t >= time * 2f / 3f && audio != null && !turnOnAudio)
            {
                audio.Play();
                turnOnAudio = true;
            }

            t += Time.deltaTime;

            fill.fillAmount = t / time;

            yield return null;
        }

        fill.fillAmount = 1f;
        isLoaded = true;
        Debug.Log("Load Done");
        this.gameObject.SetActive(false);
    }
}