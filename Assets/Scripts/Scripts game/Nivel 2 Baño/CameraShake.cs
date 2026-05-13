using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Coroutine currentShake;

    private Vector3 originalPos;

    [Header("Cooldown narración")]
    [SerializeField] private float cooldownNarracion = 8f;

    private bool puedeNarrar = true;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        originalPos = transform.localPosition;
    }

    // 🔥 IMPORTANTE:
    // duration primero
    // magnitude segundo

    public void StartShake(
        float duration,
        float magnitude)
    {
        if (currentShake != null)
        {
            StopCoroutine(currentShake);

            transform.localPosition =
                originalPos;
        }

        currentShake =
            StartCoroutine(
                Shake(duration, magnitude)
            );
    }

    private IEnumerator Shake(
        float duration,
        float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x =
                Random.Range(-1f, 1f)
                * magnitude;

            float y =
                Random.Range(-1f, 1f)
                * magnitude;

            transform.localPosition =
                originalPos +
                new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition =
            originalPos;

        currentShake = null;
    }

    // ─────────────────────────────────────
    // NARRACIÓN CONTROLADA
    // ─────────────────────────────────────

    public bool PuedeNarrar()
    {
        return puedeNarrar;
    }

    public void ActivarCooldownNarracion()
    {
        StartCoroutine(
            CooldownNarracion()
        );
    }

    private IEnumerator CooldownNarracion()
    {
        puedeNarrar = false;

        yield return new WaitForSeconds(
            cooldownNarracion
        );

        puedeNarrar = true;
    }
}