using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Event_", menuName = "Events/CustomEvent_GameState", order = 0)]
public class CustomEvent_GameState : ScriptableObject
{
    public Action<GameManager.GAME_STATE> handle;

    public void Raise(GameManager.GAME_STATE data)
    {
        handle?.Invoke(data);
    }
}
