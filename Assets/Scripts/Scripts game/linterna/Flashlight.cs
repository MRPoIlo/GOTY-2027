using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [SerializeField] private Light LuzLinterna;
    private bool linternaBloqueada = true;
    private bool encendida = false;

    private PausaManager pausaManager;

    void Start()
    {
        if (LuzLinterna != null)
            LuzLinterna.enabled = false;

        if (GameManager2.Instance != null && GameManager2.Instance.tieneLinterna)
            linternaBloqueada = false;

        pausaManager = FindObjectOfType<PausaManager>();
    }

    void Update()
    {
        if (pausaManager != null)
        {
            if (pausaManager.juegoPausado || pausaManager.EnOpciones)
                return;
        }

        if (linternaBloqueada) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            encendida = !encendida;

            if (LuzLinterna != null)
                LuzLinterna.enabled = encendida;
        }
    }

    public void DesbloquearLinterna()
    {
        linternaBloqueada = false;

        if (LuzLinterna != null)
        {
            encendida = true;
            LuzLinterna.enabled = true;
        }

        if (GameManager2.Instance != null)
            GameManager2.Instance.tieneLinterna = true;
    }
}
