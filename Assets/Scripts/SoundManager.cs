using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("효과음 AudioSource")]
    [SerializeField] private AudioSource sfxSource;

    [Header("버튼 클릭 효과음")]
    [SerializeField] private AudioClip buttonClickClip;

    public void PlayButtonClick()
    {
        if (sfxSource == null)
        {
            Debug.LogWarning("SFX AudioSource가 연결되지 않았습니다.");
            return;
        }

        if (buttonClickClip == null)
        {
            Debug.LogWarning("Button Click AudioClip이 연결되지 않았습니다.");
            return;
        }

        sfxSource.PlayOneShot(buttonClickClip);
    }
}