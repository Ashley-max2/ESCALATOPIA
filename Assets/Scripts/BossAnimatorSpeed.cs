using UnityEngine;

public class BossAnimatorSpeed : MonoBehaviour
{
    public Animator animator;        // Referencia al Animator
    public string speedParam = "Speed"; // Nombre del par�metro float en el Animator

    private Vector3 lastPosition;
    private float speed;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        speed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;

        if (animator == null) return;

        // Actualiza el parámetro sea Float o Bool (evita error de tipo)
        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            if (p.name != speedParam) continue;
            if (p.type == AnimatorControllerParameterType.Float)
                animator.SetFloat(speedParam, speed);
            else if (p.type == AnimatorControllerParameterType.Bool)
                animator.SetBool(speedParam, speed > 0.1f);
            break;
        }
    }
}