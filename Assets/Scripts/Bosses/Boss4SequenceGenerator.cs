using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generador de secuencias predefinidas para Boss4
/// Este script ayuda a crear secuencias optimizadas para diferentes niveles
/// </summary>
public class Boss4SequenceGenerator : MonoBehaviour
{
    [Header("Sequence Configuration")]
    [SerializeField] private Boss4Professional boss4;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform goalPoint;
    [SerializeField] private List<Transform> keyPoints = new List<Transform>();

    [Header("Generated Sequences")]
    [SerializeField] private List<ClimbingSequence> generatedSequences = new List<ClimbingSequence>();

    /// <summary>
    /// Genera una secuencia rápida y directa
    /// </summary>
    [ContextMenu("Generate Fast Sequence")]
    public void GenerateFastSequence()
    {
        ClimbingSequence sequence = new ClimbingSequence
        {
            sequenceName = "Fast Route",
            description = "Ruta directa optimizada para velocidad máxima"
        };

        float currentTime = 0f;

        // Ejemplo de secuencia rápida
        // Sprint inicial
        sequence.steps.Add(new SequenceStep
        {
            actionType = SequenceActionType.Sprint,
            targetPosition = startPoint.position + startPoint.forward * 10f,
            duration = 1.5f,
            startTime = currentTime,
            requiresPrecision = false
        });
        currentTime += 1.5f;

        // Hook a punto medio
        if (keyPoints.Count > 0)
        {
            sequence.steps.Add(new SequenceStep
            {
                actionType = SequenceActionType.Hook,
                targetPosition = keyPoints[0].position,
                duration = 1.2f,
                startTime = currentTime,
                requiresPrecision = true
            });
            currentTime += 1.2f;
        }

        // Escalada rápida
        sequence.steps.Add(new SequenceStep
        {
            actionType = SequenceActionType.Climb,
            targetPosition = Vector3.zero, // Se determina dinámicamente
            duration = 2f,
            startTime = currentTime,
            requiresPrecision = false
        });
        currentTime += 2f;

        // Sprint final
        sequence.steps.Add(new SequenceStep
        {
            actionType = SequenceActionType.Sprint,
            targetPosition = goalPoint.position,
            duration = 2f,
            startTime = currentTime,
            requiresPrecision = false
        });

        generatedSequences.Add(sequence);
        Debug.Log($"Secuencia rápida generada con {sequence.steps.Count} pasos");
    }

    /// <summary>
    /// Genera una secuencia conservadora
    /// </summary>
    [ContextMenu("Generate Conservative Sequence")]
    public void GenerateConservativeSequence()
    {
        ClimbingSequence sequence = new ClimbingSequence
        {
            sequenceName = "Conservative Route",
            description = "Ruta segura con descansos estratégicos"
        };

        float currentTime = 0f;

        // Movimiento inicial cauteloso
        sequence.steps.Add(new SequenceStep
        {
            actionType = SequenceActionType.Move,
            targetPosition = startPoint.position + startPoint.forward * 5f,
            duration = 2f,
            startTime = currentTime,
            requiresPrecision = false
        });
        currentTime += 2f;

        // Escalada gradual
        sequence.steps.Add(new SequenceStep
        {
            actionType = SequenceActionType.Climb,
            targetPosition = Vector3.zero,
            duration = 3f,
            startTime = currentTime,
            requiresPrecision = false
        });
        currentTime += 3f;

        // Descanso estratégico
        sequence.steps.Add(new SequenceStep
        {
            actionType = SequenceActionType.Rest,
            targetPosition = Vector3.zero,
            duration = 2f,
            startTime = currentTime,
            requiresPrecision = false
        });
        currentTime += 2f;

        // Continuar escalada
        sequence.steps.Add(new SequenceStep
        {
            actionType = SequenceActionType.Climb,
            targetPosition = Vector3.zero,
            duration = 3f,
            startTime = currentTime,
            requiresPrecision = false
        });
        currentTime += 3f;

        // Movimiento final
        sequence.steps.Add(new SequenceStep
        {
            actionType = SequenceActionType.Move,
            targetPosition = goalPoint.position,
            duration = 2f,
            startTime = currentTime,
            requiresPrecision = true
        });

        generatedSequences.Add(sequence);
        Debug.Log($"Secuencia conservadora generada con {sequence.steps.Count} pasos");
    }

    /// <summary>
    /// Genera una secuencia basada en los puntos clave
    /// </summary>
    [ContextMenu("Generate From Key Points")]
    public void GenerateFromKeyPoints()
    {
        if (keyPoints.Count == 0)
        {
            Debug.LogWarning("No hay puntos clave definidos");
            return;
        }

        ClimbingSequence sequence = new ClimbingSequence
        {
            sequenceName = "Key Points Route",
            description = "Ruta siguiendo puntos clave predefinidos"
        };

        float currentTime = 0f;

        // Desde inicio al primer punto clave
        sequence.steps.Add(new SequenceStep
        {
            actionType = SequenceActionType.Move,
            targetPosition = keyPoints[0].position,
            duration = 2f,
            startTime = currentTime,
            requiresPrecision = false
        });
        currentTime += 2f;

        // Entre puntos clave
        for (int i = 0; i < keyPoints.Count - 1; i++)
        {
            Vector3 currentPos = keyPoints[i].position;
            Vector3 nextPos = keyPoints[i + 1].position;
            float distance = Vector3.Distance(currentPos, nextPos);
            float heightDiff = nextPos.y - currentPos.y;

            // Determinar acción según geometría
            SequenceActionType actionType;
            float duration;

            if (heightDiff > 3f)
            {
                // Gran diferencia de altura: usar gancho o escalar
                if (distance > 5f)
                {
                    actionType = SequenceActionType.Hook;
                    duration = 1.5f;
                }
                else
                {
                    actionType = SequenceActionType.Climb;
                    duration = 2.5f;
                }
            }
            else if (distance > 8f)
            {
                // Larga distancia horizontal: sprint
                actionType = SequenceActionType.Sprint;
                duration = 1.8f;
            }
            else
            {
                // Movimiento normal
                actionType = SequenceActionType.Move;
                duration = 1.5f;
            }

            sequence.steps.Add(new SequenceStep
            {
                actionType = actionType,
                targetPosition = nextPos,
                duration = duration,
                startTime = currentTime,
                requiresPrecision = true
            });

            currentTime += duration;

            // Descanso cada 3 puntos
            if ((i + 1) % 3 == 0 && i < keyPoints.Count - 2)
            {
                sequence.steps.Add(new SequenceStep
                {
                    actionType = SequenceActionType.Rest,
                    targetPosition = nextPos,
                    duration = 1.5f,
                    startTime = currentTime,
                    requiresPrecision = false
                });
                currentTime += 1.5f;
            }
        }

        // Último tramo a la meta
        sequence.steps.Add(new SequenceStep
        {
            actionType = SequenceActionType.Sprint,
            targetPosition = goalPoint.position,
            duration = 2f,
            startTime = currentTime,
            requiresPrecision = false
        });

        generatedSequences.Add(sequence);
        Debug.Log($"Secuencia de puntos clave generada con {sequence.steps.Count} pasos");
    }

    /// <summary>
    /// Genera una secuencia mostrando todas las capacidades
    /// </summary>
    [ContextMenu("Generate Showcase Sequence")]
    public void GenerateShowcaseSequence()
    {
        ClimbingSequence sequence = new ClimbingSequence
        {
            sequenceName = "Showcase Route",
            description = "Demuestra todas las habilidades del profesional"
        };

        float currentTime = 0f;

        // 1. Sprint inicial
        sequence.steps.Add(new SequenceStep
        {
            actionType = SequenceActionType.Sprint,
            targetPosition = startPoint.position + Vector3.forward * 8f,
            duration = 1.2f,
            startTime = currentTime,
            requiresPrecision = false
        });
        currentTime += 1.2f;

        // 2. Wall Jump
        sequence.steps.Add(new SequenceStep
        {
            actionType = SequenceActionType.WallJump,
            targetPosition = Vector3.zero,
            duration = 0.5f,
            startTime = currentTime,
            requiresPrecision = true
        });
        currentTime += 0.5f;

        // 3. Hook a punto alto
        if (keyPoints.Count > 0)
        {
            sequence.steps.Add(new SequenceStep
            {
                actionType = SequenceActionType.Hook,
                targetPosition = keyPoints[0].position + Vector3.up * 5f,
                duration = 1.5f,
                startTime = currentTime,
                requiresPrecision = true
            });
            currentTime += 1.5f;
        }

        // 4. Escalada precisa
        sequence.steps.Add(new SequenceStep
        {
            actionType = SequenceActionType.Climb,
            targetPosition = Vector3.zero,
            duration = 2f,
            startTime = currentTime,
            requiresPrecision = true
        });
        currentTime += 2f;

        // 5. Descanso breve
        sequence.steps.Add(new SequenceStep
        {
            actionType = SequenceActionType.Rest,
            targetPosition = Vector3.zero,
            duration = 1f,
            startTime = currentTime,
            requiresPrecision = false
        });
        currentTime += 1f;

        // 6. Sprint final
        sequence.steps.Add(new SequenceStep
        {
            actionType = SequenceActionType.Sprint,
            targetPosition = goalPoint.position,
            duration = 2f,
            startTime = currentTime,
            requiresPrecision = false
        });

        generatedSequences.Add(sequence);
        Debug.Log($"Secuencia showcase generada con {sequence.steps.Count} pasos");
    }

    /// <summary>
    /// Aplica la secuencia generada al Boss4
    /// </summary>
    [ContextMenu("Apply Sequences To Boss")]
    public void ApplySequencesToBoss()
    {
        if (boss4 == null)
        {
            Debug.LogError("Boss4 no asignado");
            return;
        }

        if (generatedSequences.Count == 0)
        {
            Debug.LogWarning("No hay secuencias generadas. Genera una primero.");
            return;
        }

        // Nota: Esto requeriría hacer las secuencias públicas en Boss4
        // o añadir un método público para asignarlas
        Debug.Log($"Aplicando {generatedSequences.Count} secuencias al Boss4");
        
        // En un editor personalizado, esto se haría automáticamente
        // Por ahora, copia manualmente desde generatedSequences
    }

    /// <summary>
    /// Limpia las secuencias generadas
    /// </summary>
    [ContextMenu("Clear Generated Sequences")]
    public void ClearGeneratedSequences()
    {
        generatedSequences.Clear();
        Debug.Log("Secuencias limpiadas");
    }

    /// <summary>
    /// Visualiza las secuencias en el Scene view
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Dibujar puntos clave
        if (keyPoints.Count > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < keyPoints.Count; i++)
            {
                if (keyPoints[i] == null) continue;

                Gizmos.DrawWireSphere(keyPoints[i].position, 0.5f);
                
                // Conectar con líneas
                if (i < keyPoints.Count - 1 && keyPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(keyPoints[i].position, keyPoints[i + 1].position);
                }
            }
        }

        // Dibujar secuencias generadas
        foreach (ClimbingSequence seq in generatedSequences)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < seq.steps.Count; i++)
            {
                if (seq.steps[i].targetPosition == Vector3.zero) continue;

                Gizmos.DrawSphere(seq.steps[i].targetPosition, 0.3f);
                
                if (i < seq.steps.Count - 1 && seq.steps[i + 1].targetPosition != Vector3.zero)
                {
                    Gizmos.DrawLine(seq.steps[i].targetPosition, seq.steps[i + 1].targetPosition);
                }
            }
        }
    }
}
