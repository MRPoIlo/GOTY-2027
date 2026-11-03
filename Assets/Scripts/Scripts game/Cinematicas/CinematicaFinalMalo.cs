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
        // Modo exposición: logro del final y borrado inmediato del progreso
        LogrosManager.Instance?.IniciarFinal("final_malo", "Llegó al final malo");

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

        // Modo exposición: lista de logros conseguidos (el progreso ya se borró al entrar).
        // Antes se deja leer el último panel, que lleva el mensaje final del juego.
        if (LogrosManager.Instance != null)
        {
            yield return new WaitForSeconds(panelDuration + 1f);
            LogrosManager.Instance.MostrarResumenFinal();
            yield return new WaitForSeconds(2f);
        }

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
