using UnityEngine;

public class Instrucciones : MonoBehaviour
{

    public GameObject UIInstrucciones;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UIInstrucciones.SetActive(false);
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerScript>().hasKey = true;
            UIInstrucciones.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UIInstrucciones.SetActive(false);
        }
    }
}
