using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private Vector3 lastRespawnPosition;
    private bool hasCheckpoint = false;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip respawnSound;

    private void Start()
    {
        // Default to starting position
        lastRespawnPosition = transform.position;
        hasCheckpoint = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("respawn"))
        {
            SetCheckpoint(other.transform.position);
        }
    }

    public void SetCheckpoint(Vector3 position)
    {
        lastRespawnPosition = position;
        hasCheckpoint = true;
        Debug.Log($"Checkpoint actualizado a: {position}");
    }

    public void Respawn()
    {
        if (!hasCheckpoint) return;

        // Reset positions
        transform.position = lastRespawnPosition;
        
        // Reset Physics
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Notify DeadManager if exists (compatibility with legacy)
        DeadManager dm = GetComponent<DeadManager>();
        if (dm != null)
        {
            dm.RespawnPlayer();
        }

        // Reset Hook if needed
        HookSystem hs = GetComponentInChildren<HookSystem>();
        if (hs != null)
        {
            hs.CancelHook();
        }
        
        // Play Sound
        if (audioSource != null && respawnSound != null)
        {
            audioSource.PlayOneShot(respawnSound);
        }

        Debug.Log("Jugador reapareció");
    }
}
