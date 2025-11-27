using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHookable
{
    bool IsValidTarget { get; }
    Vector3 HookPoint { get; }
    void OnHookAttach();
    void OnHookDetach();
}
