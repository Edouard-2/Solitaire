/*******************************************************
Nom......... : DisplayStatistics.cs
Role........ : Display the current Score, Time and move made 
in the current game

Auteur...... : Edouard MORDANT
Version..... : V1.0 du 19/09/2024
********************************************************/
using TMPro;
using UnityEngine;

public class DisplayStatistics : MonoBehaviour
{
    [SerializeField] protected CustomEvent_Int _eventUpdate;
    [SerializeField] protected TextMeshProUGUI _text;
    [SerializeField] protected string _prefix;
    [SerializeField] protected string _suffix;

    #region MonoBehaviour METHODS
    private void Awake()
    {
        _eventUpdate.handle += OnUpdate;
    }

    private void OnDestroy()
    {
        _eventUpdate.handle -= OnUpdate;
    }

    #endregion

    #region HANDLE
    protected virtual void OnUpdate(int obj)
    {
        var text = $"{_prefix}{obj}{_suffix}";
        _text.text = text;
    }
    #endregion
}
