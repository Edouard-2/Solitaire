using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Event_", menuName = "Events/CustomEvent_Int", order = 0)]
public class CustomEvent_Int : ScriptableObject
{
    public Action<int> handle;

    public void Raise(int data)
    {
        handle?.Invoke(data);
    }
}
