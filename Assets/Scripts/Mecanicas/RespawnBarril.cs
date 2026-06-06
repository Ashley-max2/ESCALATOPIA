using UnityEngine;

public class RespawnBarril : MonoBehaviour
{
    [SerializeField] private GameObject prefabBarril;

    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;

    private void Start()
    {
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            Instantiate(prefabBarril, posicionInicial, rotacionInicial);
            Destroy(gameObject);
        }
    }
}