using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicaNormal;
    public AudioSource musicaPersecucion;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    private bool enPersecucion = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (musicaNormal != null)
        {
            musicaNormal.loop = true;
            musicaNormal.Play();
        }

        if (musicaPersecucion != null)
        {
            musicaPersecucion.loop = true;
            musicaPersecucion.Stop();
        }
    }

    public void CambiarAMusicaPersecucion()
    {
        if (enPersecucion) return;
        enPersecucion = true;
        StartCoroutine(FadeMusic(musicaNormal, musicaPersecucion));
    }

    public void CambiarAMusicaNormal()
    {
        if (!enPersecucion) return;
        enPersecucion = false;
        StartCoroutine(FadeMusic(musicaPersecucion, musicaNormal));
    }

    // 🔹 Nuevo método para detener toda la música
    public void DetenerTodaMusica()
    {
        if (musicaNormal != null) musicaNormal.Stop();
        if (musicaPersecucion != null) musicaPersecucion.Stop();
    }

    private IEnumerator FadeMusic(AudioSource desde, AudioSource hacia)
    {
        float tiempo = 0f;
        float volumenInicial = desde.volume;

        hacia.volume = 0f;
        hacia.Play();

        while (tiempo < fadeDuration)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / fadeDuration;
            desde.volume = Mathf.Lerp(volumenInicial, 0f, t);
            hacia.volume = Mathf.Lerp(0f, volumenInicial, t);
            yield return null;
        }

        desde.Stop();
        hacia.volume = volumenInicial;
    }
}
