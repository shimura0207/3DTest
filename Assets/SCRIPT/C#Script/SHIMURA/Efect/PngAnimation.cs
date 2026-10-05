using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PngAnimation : MonoBehaviour {
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameRate = 30f;
    [SerializeField] bool looping;
    [SerializeField] GameObject loop;

    private Coroutine animationCoroutine;
    [SerializeField] private PngAnimation CutinAnim;

    public void Play() {
        if (animationCoroutine != null) {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation() {
        float waitTime = 1f / frameRate;

        do {
            for (int i = 0; i < frames.Length; i++) {
                targetImage.sprite = frames[i];

                yield return new WaitForSeconds(waitTime);
            }

        } while (looping);

        animationCoroutine = null;
        if (loop !=null) {
            ShowCutinLoop();
        }
    }

    public void Stop() {
        if (animationCoroutine != null) {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
            
        }
        loop.SetActive(false);
    }

    void ShowCutinLoop() {
        loop.SetActive(true);
        CutinAnim.Play();
    }

    
}