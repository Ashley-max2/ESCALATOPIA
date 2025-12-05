using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HookImpulseConfig", menuName = "Hook/ImpulseConfig")]
public class HookImpulseConfig : ScriptableObject
{
    [Header("Impulso inicial")]
    [Tooltip("Velocidad inicial hacia el hook al enganchar")]
    public float initialImpulseSpeed = 30f;

    [Tooltip("Duración del impulso inicial en segundos")]
    public float impulseDuration = 0.25f;

    [Header("Arrastre después del impulso")]
    [Tooltip("Si true, después del impulso sigue arrastrando hacia el hook")]
    public bool continuePullAfterImpulse = true;

    [Tooltip("Velocidad de arrastre después del impulso")]
    public float pullSpeed = 20f;

    [Header("Ajustes de distancia")]
    [Tooltip("Distancia mínima para considerar que hemos llegado al hook")]
    public float stopDistance = 1f;

    [Tooltip("¿Reactivar gravedad al llegar?")]
    public bool enableGravityOnStop = true;

    [Header("Tipo de movimiento")]
    [Tooltip("Si true, se usa velocity directa. Si false, se usa AddForce (Acceleración).")]
    public bool useVelocity = true;

    [Tooltip("Factor de fuerza si se usa AddForce")]
    public float forceMultiplier = 50f;
}