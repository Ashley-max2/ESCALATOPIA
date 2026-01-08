using UnityEngine;

public class BonfireCheckpoint : MonoBehaviour
{
    [Header("Checkpoint")]
    public Transform spawnPoint;
    public ParticleSystem fireEffects;
    public Light fireLight;
    
    private bool isActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            ActivateCheckpoint();
            // Notify GameManager or Player Respawn system
            PlayerRespawn respawn = other.GetComponent<PlayerRespawn>();
            if (respawn != null)
            {
                respawn.SetCheckpoint(spawnPoint != null ? spawnPoint.position : transform.position);
            }
        }
    }

    private void ActivateCheckpoint()
    {
        isActivated = true;
        Debug.Log("Bonfire Lit!");
        
        if (fireEffects != null) fireEffects.Play();
        if (fireLight != null) fireLight.enabled = true;
    }
}
