using UnityEngine;

public class llave : MonoBehaviour
{
    public GameObject UIKey;
    AudioSource audio;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            audio.Play();
            other.gameObject.GetComponent<PlayerScript>().hasKey = true;
            UIKey.SetActive(true);
            Destroy(gameObject, 5f);
        }
    }
}
