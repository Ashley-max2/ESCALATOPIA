using UnityEngine;
using UnityEngine.Playables;

public class PiezaMecanismoA : MonoBehaviour
{
    public PlayableDirector timeline;

    void OnEnable()
    {
        // Se ejecuta cuando el objeto pasa a activo
        if (timeline != null)
            timeline.Play();
    }
}