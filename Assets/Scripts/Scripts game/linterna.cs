using UnityEngine;

public class Flashlight : MonoBehaviour
{
    public Light LuzLinterna;
    private bool linternaBloqueada = true;

    void Start()
    {
        if (LuzLinterna != null)
            LuzLinterna.enabled = false;

        if (GameManager2.Instance != null && GameManager2.Instance.tieneLinterna)
            linternaBloqueada = false;
    }

    void Update()
    {
        if (linternaBloqueada) return;

        if (Input.GetKeyDown(KeyCode.F))
            LuzLinterna.enabled = !LuzLinterna.enabled;
    }

    public void DesbloquearLinterna()
    {
        linternaBloqueada = false;

        if (LuzLinterna != null)
            LuzLinterna.enabled = true;

        if (GameManager2.Instance != null)
            GameManager2.Instance.tieneLinterna = true;
    }
}
