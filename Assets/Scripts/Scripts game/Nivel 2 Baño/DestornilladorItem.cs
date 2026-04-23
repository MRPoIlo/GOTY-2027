using UnityEngine;

public class DestornilladorItem : MonoBehaviour
{
    [SerializeField] private float rangoInteraccion = 2.5f;
    [SerializeField] private GameObject mensajeRecoger;

    private bool puedeRecoger = false;
    private Transform jugador;
    private PausaManager pausaManager;

    private void Start()
    {
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) jugador = go.transform;

        pausaManager = FindObjectOfType<PausaManager>();

        if (mensajeRecoger != null) mensajeRecoger.SetActive(false);
    }

    private void Update()
    {
        if (jugador == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);
        bool enRango = distancia <= rangoInteraccion;

        if (mensajeRecoger != null)
            mensajeRecoger.SetActive(enRango && puedeRecoger);

        if (enRango && puedeRecoger && Input.GetKeyDown(KeyCode.E))
        {
            if (pausaManager != null)
                pausaManager.tieneDestornillador = true; // ✅ ahora sí referencia de instancia

            gameObject.SetActive(false);
            if (mensajeRecoger != null) mensajeRecoger.SetActive(false);
        }
    }

    public void HabilitarRecogida()
    {
        puedeRecoger = true;
    }
}
