using UnityEngine;

public class CallFinalEvent : MonoBehaviour
{
    public AudioSource[] audios;

    public void ChangeFinalScene()
    {
        foreach (var audio in audios)
        {
            audio.Stop();
        }
    }
}
