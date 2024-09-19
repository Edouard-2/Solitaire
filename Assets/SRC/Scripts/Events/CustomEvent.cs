using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Event_", menuName = "Events/CustomEvent", order = 0)]
public class CustomEvent : ScriptableObject
{
    public Action handle;

    public void Raise()
    {
        handle?.Invoke();
    }
}
