/*******************************************************
Nom......... : DisplayHighScore.cs
Role........ : Display the HighScore in the games

Auteur...... : Edouard MORDANT
Version..... : V1.0 du 19/09/2024
********************************************************/
using System;
using TMPro;
using UnityEngine;

public class DisplayHighScore : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private string _preffix;
    [SerializeField] private string _playerPrefName;
    [SerializeField] private bool _isTime;

    #region MonoBehaviour METHODS
    private void OnEnable()
    {
        UpdateScore();
    }
    #endregion

    #region HANDLE

    /// <summary>
    /// Update the text with the Int data
    /// </summary>
    private void UpdateScore()
    {
        var data = PlayerPrefs.GetInt(_playerPrefName);
        var str = $"{_preffix}{data}";
        if(_isTime) str = $"{_preffix}{TimeSpan.FromSeconds(data).ToString(@"mm\:ss")}";

        _text.text = str;
    }
    #endregion
}