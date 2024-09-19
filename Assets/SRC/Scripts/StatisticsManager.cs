/*******************************************************
Nom......... : StastisticManager.cs
Role........ : Manager that handle all the statistics

Auteur...... : Edouard MORDANT
Version..... : V1.0 du 19/09/2024
********************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatisticsManager : MonoBehaviour
{
    [SerializeField] private CustomEvent_GameState _eventOnStateChange;

    [SerializeField] private CustomEvent _eventUndoScore;
    [SerializeField] private CustomEvent _addCardMoveCount;
    [SerializeField] private CustomEvent_Int _moveCardToFinalStack;
    [SerializeField] private CustomEvent_Int _moveCardToArray;

    [SerializeField] private CustomEvent_Int _eventScoreUpdate;
    [SerializeField] private CustomEvent_Int _eventTimeUpdate;
    [SerializeField] private CustomEvent_Int _eventCardMoveCountUpdate;
    [SerializeField] private int _scoreIfTimeMax;
    [SerializeField] private int _scoreIfTimeMin;

    /// <summary>
    /// its 59 minutes and 59 secondes all in seconds
    /// </summary>
    private int MAX_TIME = 900;

    private string KEY_SCORE = "SCORE";
    private string KEY_TIME = "TIME";
    private string KEY_MOVE = "MOVE";

    private int _pointForStackToArray = 10;
    private int _pointForArrayToFinalStack = 20;

    private int _countCardMove;
    private int _timeInSecond;
    private int _score;

    private List<int> _undoScore = new List<int>();

    private WaitForSeconds _waitForOneSecond = new WaitForSeconds(1);

    private Coroutine _coroutineTime;

    #region MonoBehaviour METHODS
    private void Awake()
    {
        _eventUndoScore.handle += UndoScore;
        _moveCardToFinalStack.handle += AddScroreFinalStack;
        _moveCardToArray.handle += AddScroreArray;
        _addCardMoveCount.handle += AddMoveCardCount;
        _eventOnStateChange.handle += OnStateChange;
    }

    private void Start()
    {
        _eventScoreUpdate.Raise(0);
        _eventTimeUpdate.Raise(0);
        _eventCardMoveCountUpdate.Raise(0);
    }

    private void OnDestroy()
    {
        _eventUndoScore.handle -= UndoScore;
        _moveCardToFinalStack.handle -= AddScroreFinalStack;
        _moveCardToArray.handle -= AddScroreArray;
        _addCardMoveCount.handle -= AddMoveCardCount;
        _eventOnStateChange.handle -= OnStateChange;
    }
    #endregion

    #region GAME STATE
    /// <summary>
    /// Set the state of the statistics
    /// </summary>
    /// <param name="obj"></param>
    private void OnStateChange(GameManager.GAME_STATE obj)
    {
        switch (obj)
        {
            case GameManager.GAME_STATE.HOME:
                StopAllStatistics();
                break;
            case GameManager.GAME_STATE.PLAYING:
                StartAllStatistics();
                break;
            case GameManager.GAME_STATE.WIN:
                StopAllStatistics();
                CalculWinScore();
                SaveStats();
                break;
            case GameManager.GAME_STATE.LOOSE:
                StopAllStatistics();
                break;
        }
    }
    #endregion

    #region CALCUL STATS

    private void AddScroreFinalStack(int times)
    {
        AddScore(_pointForArrayToFinalStack * times);
    }

    private void AddScroreArray(int times)
    {
        AddScore(_pointForStackToArray * times);
    }

    private void AddScore(int points)
    {
        AddUndoScore(points);
        _score += points;
        _eventScoreUpdate.Raise(_score);
    }

    private void AddUndoScore(int points)
    {
        _undoScore.Add(points);
        if (_undoScore.Count > 10) _undoScore.RemoveAt(0);
    }

    private void AddMoveCardCount()
    {
        _countCardMove++;
        _eventCardMoveCountUpdate.Raise(_countCardMove);
    }
    private void CalculWinScore()
    {
        // Calculate score with time
        var scoreToAdd = Mathf.Min(Remap(_timeInSecond, 0, MAX_TIME, _scoreIfTimeMin, _scoreIfTimeMax), _scoreIfTimeMin);
        _score += scoreToAdd;
        _eventScoreUpdate.Raise(_score);
    }
    #endregion

    #region SAVE
    private void SaveStats()
    {
        if (PlayerPrefs.GetInt(KEY_SCORE) > _score) return;

        PlayerPrefs.SetInt(KEY_SCORE, _score);
        PlayerPrefs.SetInt(KEY_TIME, _timeInSecond);
        PlayerPrefs.SetInt(KEY_MOVE, _countCardMove);
    }
#endregion

    #region UNDO
    private void UndoScore()
    {
        _score -= _undoScore.Last();
        _undoScore.RemoveAt(_undoScore.Count - 1);
        _eventScoreUpdate.Raise(_score);
    }
    #endregion

    #region HANDLE STATISTICS
    private void StartAllStatistics()
    {
        StartTimer();
    }

    private void StopAllStatistics()
    {
        StopTimer();
        _countCardMove = 0;
        _score = 0;
    }

    private void StartTimer()
    {
        _coroutineTime = StartCoroutine(Time_Coroutine());
    }

    private void StopTimer()
    {
        if (_coroutineTime == null) return;
        StopCoroutine(_coroutineTime);
    }
    private int Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return (int)(toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin));
    }

    IEnumerator Time_Coroutine()
    {
        while (true)
        {
            _timeInSecond++;
            _eventTimeUpdate.Raise(_timeInSecond);
            yield return _waitForOneSecond;
        }
    }

    #endregion
}