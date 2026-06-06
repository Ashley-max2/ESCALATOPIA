using System.Collections;
using UnityEngine;

/// <summary>
/// Adjuntar a cualquier prop crítico (p.ej. barril que contiene una llave).
/// Si cae al agua o a una zona de peligro, lo devuelve a su posición inicial
/// para que el juego nunca quede bloqueado.
/// </summary>
public class EssentialPropRespawner : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Etiqueta del trigger de peligro (agua, etc.)")]
    [SerializeField] private string hazardTag = "Water";

    [Tooltip("Coordenada Y mínima como red de seguridad (si no hay trigger de agua).")]
    [SerializeField] private float fallDeathY = -20f;

    [Tooltip("Segundos de espera antes de reaparecer (para que el efecto sea visible).")]
    [SerializeField] private float respawnDelay = 1.5f;

    private Vector3 _spawnPosition;
    private Quaternion _spawnRotation;
    private bool _isRespawning;

    private void Start()
    {
        _spawnPosition = transform.position;
        _spawnRotation = transform.rotation;
    }

    private void Update()
    {
        if (!_isRespawning && transform.position.y < fallDeathY)
            StartCoroutine(RespawnRoutine());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isRespawning && other.CompareTag(hazardTag))
            StartCoroutine(RespawnRoutine());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_isRespawning && collision.gameObject.CompareTag(hazardTag))
            StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        _isRespawning = true;
        yield return new WaitForSeconds(respawnDelay);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        transform.position = _spawnPosition;
        transform.rotation = _spawnRotation;

        if (rb != null)
            rb.isKinematic = false;

        _isRespawning = false;
    }
}
