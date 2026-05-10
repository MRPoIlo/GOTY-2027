using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// GOTY — Nivel 6 (Sótano)
/// Fragmento de llave brillante. Al recogerlo desaparece
/// y notifica al SotanoManager.
/// </summary>
public class FragmentoLlave : MonoBehaviour, IInteractuable
{
    [Header("Identificación")]
    public int numeroFragmento = 1; // 1, 2 o 3

    [Header("Luz de atracción")]
    [SerializeField] private Light luzFragmento;
    [SerializeField] private float intensidadLuz    = 0.8f;
    [SerializeField] private float velocidadPulso   = 2f;

    [Header("Rotación")]
    [SerializeField] private float velocidadRotacion = 60f;

    [Header("Evento")]
    public UnityEvent OnRecogido;

    private bool recogido = false;
    private float tiempoBase;

    void Awake()
    {
        // Crear luz si no está asignada
        if (luzFragmento == null)
        {
            GameObject luzGO = new GameObject("LuzFragmento");
            luzGO.transform.parent        = transform;
            luzGO.transform.localPosition = Vector3.zero;
            luzFragmento                  = luzGO.AddComponent<Light>();
            luzFragmento.type             = LightType.Point;
            luzFragmento.color            = new Color(1f, 0.9f, 0.5f);
            luzFragmento.range            = 1.5f;
            luzFragmento.intensity        = intensidadLuz;
        }
    }

    void Update()
    {
        if (recogido) return;

        // Rotación constante
        transform.Rotate(Vector3.up, velocidadRotacion * Time.deltaTime);

        // Pulso de luz
        if (luzFragmento != null)
        {
            tiempoBase += Time.deltaTime * velocidadPulso;
            luzFragmento.intensity = intensidadLuz *
                (0.7f + 0.3f * Mathf.Sin(tiempoBase));
        }
    }

    // ─── IInteractuable ───────────────────────────────────────────────────────

    public void Interactuar()
    {
        if (recogido) return;
        recogido = true;

        NarracionManager.Instance?.Narrar(
            $"Un fragmento. Ya tengo {numeroFragmento} de 3.");

        OnRecogido?.Invoke();
        SotanoManager.Instance?.RegistrarFragmento(this);

        gameObject.SetActive(false);
    }

    public string ObtenerTextoAccion() => "Recoger";
    public bool   EstaActivo()         => !recogido;
}