using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Editor script que configura automáticamente Boss2.controller
/// con los estados y transiciones necesarios para BossGroundAI.
///
/// Uso: menú Unity → Tools → Setup Boss2 Animator
///
/// Parámetros creados:
///   Speed      (Float)   — 0=Idle, 0.4=Walk, 1=Trote(Run)
///   IsClimbing (Bool)    — true mientras escala
///   IsGrounded (Bool)    — true mientras toca el suelo
///   Jump       (Trigger) — dispara la secuencia de salto
///
/// Estados y transiciones según la guía del equipo:
///   Idle ↔ Walk ↔ Trote  (por Speed, sin exit time)
///   Cualquiera → ClimbIdle (IsClimbing=true, sin exit time)
///   ClimbIdle ↔ ClimbForward (por Speed)
///   ClimbIdle/Forward → Idle (IsClimbing=false)
///   Idle/Walk/Trote → Jump_Start (Jump trigger, sin exit time)
///   Jump_Start → Jump_Loop   (has exit time = 1)
///   Jump_Loop  → Jump_End    (IsGrounded=true, sin exit time)
///   Jump_End   → Idle        (has exit time = 1)
/// </summary>
public static class BossAnimatorSetup
{
    private const string CONTROLLER_PATH = "Assets/ASSETS_ESCALATOPIA/Humanoides/NPC/Boss2/Boss2.controller";
    private const string FBX_PATH        = "Assets/ASSETS_ESCALATOPIA/Humanoides/Animaciones/Boss_Snow/AnimationSnow.fbx";

    [MenuItem("Tools/Setup Boss2 Animator")]
    public static void Setup()
    {
        // ── Cargar controller ──
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CONTROLLER_PATH);
        if (controller == null)
        {
            Debug.LogError($"[BossAnimatorSetup] No se encontró el controller en: {CONTROLLER_PATH}");
            return;
        }

        // ── Cargar clips del FBX ──
        var clips = LoadClips(FBX_PATH);
        if (clips.Count == 0)
        {
            Debug.LogError($"[BossAnimatorSetup] No se encontraron clips en: {FBX_PATH}");
            return;
        }

        var sm = controller.layers[0].stateMachine;

        // ══════════════════════════════════════════════
        //  PARÁMETROS
        // ══════════════════════════════════════════════
        EnsureParam(controller, "Speed",      AnimatorControllerParameterType.Float);
        EnsureParam(controller, "ClimbH",     AnimatorControllerParameterType.Float);  // -1=izq, 0=recto, +1=der
        EnsureParam(controller, "IsClimbing", AnimatorControllerParameterType.Bool);
        EnsureParam(controller, "IsGrounded", AnimatorControllerParameterType.Bool);
        EnsureParam(controller, "Jump",       AnimatorControllerParameterType.Trigger);
        RemoveParam(controller, "IsLevitating");

        // ══════════════════════════════════════════════
        //  ESTADOS
        // ══════════════════════════════════════════════
        var stIdle          = EnsureState(sm, "Idle",         clips, "Idle",         new Vector3(250,   0));
        var stWalk          = EnsureState(sm, "Walk",         clips, "Walk",         new Vector3(250,  80));
        var stTrote         = EnsureState(sm, "Trote",        clips, "Trote",        new Vector3(250, 160));
        var stClimbIdle     = EnsureState(sm, "ClimbIdle",    clips, "ClimbIdle",    new Vector3(560,  80));
        var stClimbForward  = EnsureState(sm, "ClimbForward", clips, "ClimbForward", new Vector3(560, 160));
        var stClimbLeft     = EnsureState(sm, "ClimbLeft",    clips, "ClimbLeft",    new Vector3(560, 240));
        var stClimbRight    = EnsureState(sm, "ClimbRight",   clips, "ClimbRight",   new Vector3(560, 320));
        var stJumpStart     = EnsureState(sm, "Jump_Start",   clips, "Jump_Start",   new Vector3(560, -80));
        var stJumpLoop      = EnsureState(sm, "Jump_Loop",    clips, "Jump_Loop",    new Vector3(560,-160));
        var stJumpEnd       = EnsureState(sm, "Jump_End",     clips, "Jump_End",     new Vector3(560,-240));

        sm.defaultState = stIdle;

        // ══════════════════════════════════════════════
        //  TRANSICIONES — Locomoción básica
        //  Sin exit time, transition duration 0.1
        // ══════════════════════════════════════════════
        // Idle → Walk
        AddTransition(stIdle, stWalk, hasExitTime: false, duration: 0.1f,
            cond: ("Speed", AnimatorConditionMode.Greater, 0.1f));

        // Walk → Idle
        AddTransition(stWalk, stIdle, hasExitTime: false, duration: 0.1f,
            cond: ("Speed", AnimatorConditionMode.Less, 0.1f));

        // Walk → Trote
        AddTransition(stWalk, stTrote, hasExitTime: false, duration: 0.1f,
            cond: ("Speed", AnimatorConditionMode.Greater, 0.7f));

        // Trote → Walk
        AddTransition(stTrote, stWalk, hasExitTime: false, duration: 0.1f,
            cond: ("Speed", AnimatorConditionMode.Less, 0.7f));

        // Idle → Trote (atajo directo)
        AddTransition(stIdle, stTrote, hasExitTime: false, duration: 0.1f,
            cond: ("Speed", AnimatorConditionMode.Greater, 0.7f));

        // ══════════════════════════════════════════════
        //  TRANSICIONES — Escalada
        // ══════════════════════════════════════════════
        // Idle/Walk/Trote → ClimbIdle al entrar en modo escalada
        foreach (var src in new[] { stIdle, stWalk, stTrote })
            AddTransition(src, stClimbIdle, hasExitTime: false, duration: 0.1f,
                cond: ("IsClimbing", AnimatorConditionMode.If, 0f));

        // ClimbIdle → ClimbForward (subiendo: Speed > 0.1 y ClimbH neutro)
        AddTransition(stClimbIdle, stClimbForward, hasExitTime: false, duration: 0.05f,
            cond: ("Speed", AnimatorConditionMode.Greater, 0.1f));

        // ClimbForward → ClimbIdle (parado)
        AddTransition(stClimbForward, stClimbIdle, hasExitTime: false, duration: 0.05f,
            cond: ("Speed", AnimatorConditionMode.Less, 0.1f));

        // ClimbIdle/Forward → ClimbLeft
        foreach (var src in new[] { stClimbIdle, stClimbForward })
            AddTransition(src, stClimbLeft, hasExitTime: false, duration: 0.05f,
                cond: ("ClimbH", AnimatorConditionMode.Less, -0.3f));

        // ClimbIdle/Forward → ClimbRight
        foreach (var src in new[] { stClimbIdle, stClimbForward })
            AddTransition(src, stClimbRight, hasExitTime: false, duration: 0.05f,
                cond: ("ClimbH", AnimatorConditionMode.Greater, 0.3f));

        // ClimbLeft/Right → ClimbIdle (cuando deja de moverse lateralmente)
        foreach (var src in new[] { stClimbLeft, stClimbRight })
        {
            AddTransition(src, stClimbIdle, hasExitTime: false, duration: 0.05f,
                cond: ("ClimbH", AnimatorConditionMode.Greater, -0.3f));
            AddTransition(src, stClimbIdle, hasExitTime: false, duration: 0.05f,
                cond: ("ClimbH", AnimatorConditionMode.Less, 0.3f));
        }

        // Todos los estados de escalar → Idle al salir de la pared
        foreach (var src in new[] { stClimbIdle, stClimbForward, stClimbLeft, stClimbRight })
            AddTransition(src, stIdle, hasExitTime: false, duration: 0.15f,
                cond: ("IsClimbing", AnimatorConditionMode.IfNot, 0f));

        // ══════════════════════════════════════════════
        //  TRANSICIONES — Salto
        //  Según la guía del equipo:
        //    Idle/Walk/Trote → Jump_Start   sin exit time
        //    Jump_Start      → Jump_Loop    has exit time = 1
        //    Jump_Loop       → Jump_End     sin exit time (condición IsGrounded)
        //    Jump_End        → Idle         has exit time = 1
        // ══════════════════════════════════════════════
        foreach (var src in new[] { stIdle, stWalk, stTrote })
            AddTransition(src, stJumpStart, hasExitTime: false, duration: 0f,
                cond: ("Jump", AnimatorConditionMode.If, 0f));

        // Jump_Start → Jump_Loop (exit time completo = 1)
        AddTransitionExitTime(stJumpStart, stJumpLoop, exitTime: 1f, duration: 0f);

        // Jump_Loop → Jump_End (IsGrounded sin exit time)
        AddTransition(stJumpLoop, stJumpEnd, hasExitTime: false, duration: 0.05f,
            cond: ("IsGrounded", AnimatorConditionMode.If, 0f));

        // Jump_End → Idle (exit time completo = 1)
        AddTransitionExitTime(stJumpEnd, stIdle, exitTime: 1f, duration: 0.1f);

        // ══════════════════════════════════════════════
        //  GUARDAR
        // ══════════════════════════════════════════════
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("[BossAnimatorSetup] ¡Controller configurado correctamente!");
    }

    // ─────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────

    static Dictionary<string, AnimationClip> LoadClips(string fbxPath)
    {
        var dict = new Dictionary<string, AnimationClip>();
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(fbxPath))
        {
            if (obj is AnimationClip clip && !clip.name.Contains("__preview__"))
                dict[clip.name] = clip;
        }
        return dict;
    }

    static void EnsureParam(AnimatorController ctrl, string name,
                             AnimatorControllerParameterType type)
    {
        foreach (var p in ctrl.parameters)
            if (p.name == name) return;
        ctrl.AddParameter(name, type);
    }

    static void RemoveParam(AnimatorController ctrl, string name)
    {
        for (int i = ctrl.parameters.Length - 1; i >= 0; i--)
            if (ctrl.parameters[i].name == name)
                ctrl.RemoveParameter(i);
    }

    static AnimatorState EnsureState(AnimatorStateMachine sm, string stateName,
                                     Dictionary<string, AnimationClip> clips,
                                     string clipName, Vector3 pos)
    {
        // Buscar estado existente
        foreach (var cs in sm.states)
            if (cs.state.name == stateName) return cs.state;

        // Crear nuevo
        var state = sm.AddState(stateName, pos);
        if (clips.TryGetValue(clipName, out var clip))
            state.motion = clip;
        else
            Debug.LogWarning($"[BossAnimatorSetup] Clip '{clipName}' no encontrado en el FBX.");

        return state;
    }

    // Transición con condición bool/float
    static void AddTransition(AnimatorState from, AnimatorState to,
                               bool hasExitTime, float duration,
                               (string name, AnimatorConditionMode mode, float threshold)? cond = null)
    {
        // Evitar duplicados
        foreach (var t in from.transitions)
            if (t.destinationState == to) return;

        var tr = from.AddTransition(to);
        tr.hasExitTime       = hasExitTime;
        tr.duration          = duration;
        tr.exitTime          = 1f;

        if (cond.HasValue)
        {
            var c = cond.Value;
            if (c.mode == AnimatorConditionMode.If || c.mode == AnimatorConditionMode.IfNot)
                tr.AddCondition(c.mode, 0f, c.name);
            else
                tr.AddCondition(c.mode, c.threshold, c.name);
        }
    }

    // Transición solo por exit time (sin condiciones)
    static void AddTransitionExitTime(AnimatorState from, AnimatorState to,
                                       float exitTime, float duration)
    {
        foreach (var t in from.transitions)
            if (t.destinationState == to) return;

        var tr = from.AddTransition(to);
        tr.hasExitTime = true;
        tr.exitTime    = exitTime;
        tr.duration    = duration;
    }
}
