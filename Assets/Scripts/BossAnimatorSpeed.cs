using UnityEngine;

public class BossAnimatorSpeed : MonoBehaviour
{
    public Animator animator;        // Referencia al Animator
    public string speedParam = "Speed"; // Nombre del parámetro float en el Animator

    private Vector3 lastPosition;
    private float speed;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        // Calcular velocidad (distancia / tiempo)
        speed = (transform.position - lastPosition).magnitude / Time.deltaTime;

        // Actualizar el parámetro del Animator
        animator.SetFloat(speedParam, speed);

        // Guardar posición para el siguiente frame
        lastPosition = transform.position;
    }
}