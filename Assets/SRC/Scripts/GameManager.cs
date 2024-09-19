/*******************************************************
Nom......... : GameManager.cs
Role........ : Manage all the game with the GAME_STATE

Auteur...... : Edouard MORDANT
Version..... : V1.0 du 19/09/2024
********************************************************/
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GAME_STATE
    {
        HOME,
        PLAYING,
        WIN,
        LOOSE,
    }

    public static GameManager Instance;

    [Header("Events")]
    [SerializeField] private CustomEvent_UndoData _eventAddUndo;
    [SerializeField] private CustomEvent_GameState _eventOnStateChange;
    [SerializeField] private CustomEvent _eventFinalStackComplete;
    [SerializeField] private CustomEvent _eventUndoScore;
    [SerializeField] private CustomEvent _eventMoveCard;
    [SerializeField] private CustomEvent _eventRaiseUndoStack;
    [SerializeField] private CustomEvent _eventActiveMainMenu;
    [SerializeField] private CustomEvent _eventActivePlaying;
    [SerializeField] private CustomEvent _eventActiveLoose;
    [SerializeField] private CustomEvent _eventRaiseUndo;
    [SerializeField] private CustomEvent _eventRemoveFrontCard;

    [Header("GameStates")]
    [SerializeField] private GameObject _mainMenuParent;
    [SerializeField] private GameObject _gameParent;
    [SerializeField] private GameObject _winParent;
    [SerializeField] private GameObject _looseParent;

    [Header("Other")]
    [SerializeField] private float _undoMoveCount;

    private List<UndoData> _undoCommandList = new List<UndoData>();

    private ColumnCard[] _columns;
    private StackCard _stack;
    public Map _map;

    private int _finalStackComplete;

    private GAME_STATE _state;

    /// <summary>
    /// Handle all the game management when the state is edit
    /// </summary>
    public GAME_STATE State {
        get { return _state; }
        set
        {
            switch (value)
            {
                case GAME_STATE.HOME:
                    _finalStackComplete = 0;
                    _undoCommandList.Clear();
                    DesactiveMap();
                    _looseParent.SetActive(false);
                    _winParent.SetActive(false);
                    _mainMenuParent.SetActive(true);
                    break;
                case GAME_STATE.PLAYING:
                    _mainMenuParent.SetActive(false);
                    SpawnMap();
                    break;
                case GAME_STATE.WIN:
                    _winParent.SetActive(true);
                    break;
                case GAME_STATE.LOOSE:
                    _looseParent.SetActive(true);
                    break;
            }
            _eventOnStateChange?.Raise(value);
            _state = value;
        }
    }

    #region MonoBehaviour METHODS
    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 144;

        //_eventRemoveFrontCard.handle += CheckIfLose;
        _eventFinalStackComplete.handle += OnFinalStackComplete;
        _eventActiveLoose.handle += OnLoose;
        _eventActivePlaying.handle += OnPlay;
        _eventActiveMainMenu.handle += OnMainMenu;
        _eventAddUndo.handle += OnAddUndo;
        _eventRaiseUndo.handle += OnUndo;
    }

    private void Start()
    {
        State = GAME_STATE.HOME;
    }

    private void OnDestroy()
    {
        //_eventRemoveFrontCard.handle += CheckIfLose;
        _eventFinalStackComplete.handle += OnFinalStackComplete;
        _eventActiveLoose.handle -= OnLoose;
        _eventActivePlaying.handle -= OnPlay;
        _eventActiveMainMenu.handle -= OnMainMenu;
        _eventAddUndo.handle -= OnAddUndo;
        _eventRaiseUndo.handle -= OnUndo;
    }

    #endregion
    
    #region INITIALIZE

    // Call to setup the stack of cards
    private void InitializeGameStack()
    {
        _stack.AddBackCard(24);
    }

    // Call to setup the board
    private void InitializeGameBoard()
    {
        int numberOfCard = 1;
        foreach (var column in _columns)
        {
            column.AddBackCard(numberOfCard - 1);
            column.AddFrontCard(1);
            numberOfCard++;
        }
    }
    /// <summary>
    /// Spawn the map
    /// </summary>
    private void SpawnMap()
    {
        _map = Instantiate(ResourcesManager.Instance._mapPrefab, _gameParent.transform.position, Quaternion.identity, _gameParent.transform);
        _columns = _map._columns;
        _stack = _map._stack;

        CursorCardManager.Instance._movablepartWhenCartSelected = _map._movablePartWhenCardSelected;
        CursorCardManager.Instance._parentWhenSelected = _map._parentWhenSelected;

        // Call to start the game Setup
        InitializeGameBoard();
        InitializeGameStack();
    }


    #endregion

    #region STATE
    [ContextMenu("WIN")]
    private void Win()
    {
        State = GAME_STATE.WIN;
    }

    [ContextMenu("LOOSE")]
    private void OnLoose()
    {
        State = GAME_STATE.LOOSE;
    }

    private void OnMainMenu()
    {
        State = GAME_STATE.HOME;
    }

    private void OnPlay()
    {
        State = GAME_STATE.PLAYING;
    }

    private void DesactiveMap()
    {
        if (_map == null) return;
        Destroy(_map.gameObject);
    }

    /// <summary>
    /// Verify if all the FinalStacks are full of cards
    /// </summary>
    private void OnFinalStackComplete()
    {
        _finalStackComplete++;
        if (_finalStackComplete >= 4)
        {
            Win();
        }
    }

    #endregion

    #region UNDO
    /// <summary>
    /// Manage the undo control
    /// </summary>
    /// <param name="undoData">UndoData used to handle the undo</param>
    private void OnAddUndo(UndoData undoData)
    {
        _undoCommandList.Add(undoData);
        if(_undoCommandList.Count > _undoMoveCount)
        {
            _undoCommandList.RemoveAt(0);
        }
    }

    /// <summary>
    /// Raised when an undo is made
    /// </summary>
    private void OnUndo()
    {
        if (_undoCommandList.Count == 0) return;

        _eventMoveCard.Raise();

        var undo = _undoCommandList[^1];
        var canRemoveScore = undo.Undo();
        if(canRemoveScore) _eventUndoScore.Raise();
        _undoCommandList.RemoveAt(_undoCommandList.Count - 1);
        _eventRemoveFrontCard.Raise();
    }

    /// <summary>
    /// Handle a Stack Undo
    /// </summary>
    internal void UndoStack()
    {
        _eventRaiseUndoStack.Raise();
    }

    #endregion

    #region TRY TO CHECK IF THE PLAYER LOOSE
    /*
    public struct CardDataArray
    {
        public bool _isOnlyFrontCard;
        public CardData? _data;
    }

    private void CheckIfLose()
    {
        var columns = _map._columns;

        var allLastCards = new List<CardData?>();

        for (int i = 0; i < columns.Length; i++)
        {
            var column = columns[i];
            Debug.Log(column._lastCardData._number + " " + column._lastCardData._type);
            if (column.AdCards == false) allLastCards.Add(null);
            else allLastCards.Add(columns[i]._lastCardData);
        }


        var allLastCardsArray = new List<CardDataArray>();

        for (int i = 0; i < allLastCards.Count; i++)
        {
            var column = columns[i];
            var data = new CardDataArray() { _isOnlyFrontCard = column.IsOnlyCard };

            if (column.AdCards == false) data._data = null;
            else data._data = columns[i]._lastCardData;

            allLastCardsArray.Add(data);
        }

        // Array with Array
        foreach (var item in allLastCardsArray)
        {
            foreach (var item2 in allLastCardsArray)
            {
                if (CheckIfCardCanBeAddInBetween(item, item2))
                {
                    return;
                }
            }
        }

        // Array Stack
        var tempAllList = new List<Card>();

        tempAllList.AddRange(_map._stack._cardsBack);
        tempAllList.AddRange(_map._stack._cardsSelectable);
        tempAllList.AddRange(_map._stack._cardsWaiting);


        foreach (var item in tempAllList)
        {
            foreach (var item2 in allLastCards)
            {
                if (CheckIfCardCanBeAddInBetween(item, item2))
                {
                    return;
                }
            }
        }

        tempAllList.AddRange(allLastCards);

        // Array Stack FinalStack

        var finalStacks = _map._finalStack;
        var allFinalStackCards = new List<CardData>();

        for (int i = 0; i < finalStacks.Length; i++)
        {
            var stack = finalStacks[i];
            if (stack.AdCards == false) continue;
            else allFinalStackCards.Add(stack._lastCardData);
        }


        foreach (var item in tempAllList)
        {
            foreach (var item2 in allFinalStackCards)
            {
                if (CheckIfCardCanBeAddForTheEnd(item, item2))
                {
                    return;
                }
            }
            //Debug.Log(item._data._number + " " + item._data._type);
        }

        OnLoose();
    }

    public bool CheckIfCardCanBeAddInBetween(Card card, CardData? card2)
    {
        if (card2 == null) return card._data._number == 13 || card._data._number == 1;
        if (card._data._number == 1 || card2.Value._number == 1) return true;

        bool correctSymbol = ((card._data._type is SymbolType.ACE or SymbolType.SPADE) && (card2.Value._type is SymbolType.HEART or SymbolType.DIAMOND))
            || ((card._data._type is SymbolType.HEART or SymbolType.DIAMOND) && (card2.Value._type is SymbolType.ACE or SymbolType.SPADE));
        bool correctNumber = card2.Value._number - 1 == card._data._number;

        return (correctSymbol && correctNumber);
    }

    public bool CheckIfCardCanBeAddInBetween(CardDataArray card, CardDataArray card2)
    {
        if (card._data == null && card2._data == null) return false;

        if (card._data == null) return card2._data.Value._number == 13 || card2._data.Value._number == 1;
        if (card2._data == null) return card._data.Value._number == 13 || card._data.Value._number == 1;

        if (card._data.Value._number == 1 || card2._data.Value._number == 1) return true;

        bool correctSymbol = ((card._data.Value._type is SymbolType.ACE or SymbolType.SPADE) && (card2._data.Value._type is SymbolType.HEART or SymbolType.DIAMOND))
            || ((card._data.Value._type is SymbolType.HEART or SymbolType.DIAMOND) && (card2._data.Value._type is SymbolType.ACE or SymbolType.SPADE));
        bool correctNumber = card2._data.Value._number - 1 == card._data.Value._number;

        return (correctSymbol && correctNumber) && card._isOnlyFrontCard;
    }

    private bool CheckIfCardCanBeAddForTheEnd(Card card, CardData card2)
    {
        bool correctSymbol = card._data._type == card2._type;
        bool correctNumber = card2._number + 1 == card._data._number;

        return (correctSymbol && correctNumber);
    }
    */

    #endregion
}