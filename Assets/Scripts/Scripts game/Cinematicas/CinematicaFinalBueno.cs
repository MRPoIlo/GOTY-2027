using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CinematicaFinalBueno : MonoBehaviour
{
    public Image panelImage;
    public Sprite[] panelsFinalBueno;
    public float panelDuration = 4f;
    public float fadeDuration = 1f;
    public GameObject botonSalir;
    public AudioSource musicaFinalBueno; // 🎵 referencia al AudioSource

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
        if (musicaFinalBueno != null)
            musicaFinalBueno.Play();

        StartCoroutine(PlayCinematic());
    }

    IEnumerator PlayCinematic()
    {
        for (int i = 0; i < panelsFinalBueno.Length - 1; i++)
        {
            panelImage.sprite = panelsFinalBueno[i];
            yield return StartCoroutine(FadeIn());
            yield return new WaitForSeconds(panelDuration - (2 * fadeDuration));
            yield return StartCoroutine(FadeOut());
        }

        panelImage.sprite = panelsFinalBueno[panelsFinalBueno.Length - 1];
        yield return StartCoroutine(FadeIn());

        if (botonSalir != null)
            botonSalir.SetActive(true);

        Debug.Log("Final bueno terminado, esperando interacción del jugador.");
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
