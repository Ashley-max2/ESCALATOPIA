using System;
using UnityEngine;

/// <summary>
/// Event arguments for hook system events.
/// Implements Observer pattern for decoupled communication.
/// </summary>
public class HookEventArgs : EventArgs
{
    public IHookable HookPoint { get; set; }
    public Vector3 HookPosition { get; set; }
    public HookEventType EventType { get; set; }
}

/// <summary>
/// Types of hook events.
/// </summary>
public enum HookEventType
{
    Launched,
    Attached,
    ImpulseStarted,
    ImpulseEnded,
    Cancelled,
    Detached
}

/// <summary>
/// Delegate for hook events.
/// </summary>
public delegate void HookEventHandler(object sender, HookEventArgs e);
