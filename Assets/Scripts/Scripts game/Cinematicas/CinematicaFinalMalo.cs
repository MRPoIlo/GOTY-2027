using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CinematicaFinalMalo : MonoBehaviour
{
    public Image panelImage;
    public Sprite[] panelsFinalMalo;
    public float panelDuration = 4f;
    public float fadeDuration = 1f;
    public GameObject botonSalir;
    public AudioSource musicaFinalMalo; // 🎵 referencia al AudioSource

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = panelImage.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panelImage.gameObject.AddComponent<CanvasGroup>();

        if (botonSalir != null)
            botonSalir.SetActive(false);
    }

    void Start()
    {
        // Reproducir música al iniciar la cinemática
        if (musicaFinalMalo != null)
            musicaFinalMalo.Play();

        StartCoroutine(PlayCinematic());
    }

    IEnumerator PlayCinematic()
    {
        for (int i = 0; i < panelsFinalMalo.Length - 1; i++)
        {
            panelImage.sprite = panelsFinalMalo[i];
            yield return StartCoroutine(FadeIn());
            yield return new WaitForSeconds(panelDuration - (2 * fadeDuration));
            yield return StartCoroutine(FadeOut());
        }

        panelImage.sprite = panelsFinalMalo[panelsFinalMalo.Length - 1];
        yield return StartCoroutine(FadeIn());

        if (botonSalir != null)
            botonSalir.SetActive(true);

        Debug.Log("Final malo terminado, esperando interacción del jugador.");
    }

    IEnumerator FadeIn()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }
    }
}
