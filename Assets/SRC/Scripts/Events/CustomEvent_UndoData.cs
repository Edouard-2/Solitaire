using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Event_", menuName = "Events/CustomEvent_UndoData", order = 0)]
public class CustomEvent_UndoData : ScriptableObject
{
    public Action<UndoData> handle;

    public void Raise(UndoData data)
    {
        handle?.Invoke(data);
    }
}
