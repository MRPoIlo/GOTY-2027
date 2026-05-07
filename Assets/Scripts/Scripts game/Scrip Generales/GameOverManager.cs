using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("Transición tipo TV")]
    [SerializeField] private CanvasGroup efectoTV;   // Canvas negro con animación de fade
    [SerializeField] private float duracionTransicion = 1f;

    [Header("Escena de Game Over")]
    [SerializeField] private string escenaGameOver = "GameOverEducativo";

    public void ActivarGameOver()
    {
        StartCoroutine(TransicionGameOver());
    }

    private IEnumerator TransicionGameOver()
    {
        // 1. Fade tipo TV apagándose
        float t = 0f;
        while (t < duracionTransicion)
        {
            t += Time.deltaTime;
            efectoTV.alpha = Mathf.Lerp(0f, 1f, t / duracionTransicion);
            yield return null;
        }

        // 2. Espera 2 segundos antes de mostrar la pantalla final
        yield return new WaitForSeconds(2f);

        // 3. Cargar la escena de Game Over educativa
        SceneManager.LoadScene(escenaGameOver);
    }
}
