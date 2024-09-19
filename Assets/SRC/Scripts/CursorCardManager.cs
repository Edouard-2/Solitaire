/*******************************************************
Nom......... : CursorCardManager.cs
Role........ : Manage all the inputs of the cards. 
(the buttons are not manage in this script)

Auteur...... : Edouard MORDANT
Version..... : V1.0 du 19/09/2024
********************************************************/

using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CursorCardManager : MonoBehaviour
{

    public static CursorCardManager Instance = null;
    [Header("Events")]
    [SerializeField] private CustomEvent_Int _eventAddScore;
    [SerializeField] private CustomEvent _eventMoveCard;
    [SerializeField] private CustomEvent _eventRemoveFrontCard;
    [SerializeField] private CustomEvent _eventStackShowNextCard;
    [SerializeField] private CustomEvent_UndoData _eventAddUndo;

    [Header("Components")]
    [SerializeField] public GraphicRaycaster _raycaster;
    [SerializeField] private EventSystem _eventSystem;
    [SerializeField] public Transform _parentWhenSelected;
    [SerializeField] public Transform _movablepartWhenCartSelected;
    [SerializeField] private bool _useOffset;
    private PointerEventData _pointerEventData;

    private bool _canMove;

    private PIA _inputs;

    private ICardContainer _initialICardContainer;
    private Transform _selectedCardParent;
    private Transform _selectedCardTransform;
    private List<Card> _selectedCard = new List<Card>();
    private List<Vector3> _listPosition = new List<Vector3>();

    #region MonoBehaviour METHODS
    private void Awake()
    {
        Instance = this;

        _inputs = new PIA();
        _inputs.Enable();

        _inputs.Game.Click.started += OnBeginDrag;
        _inputs.Game.Drag.performed += OnDrag;
        _inputs.Game.Click.canceled += OnEndDrag;

        //Set up the new Pointer Event
        _pointerEventData = new PointerEventData(_eventSystem);
    }

    private void OnDestroy()
    {
        _inputs.Game.Drag.started -= OnBeginDrag;
        _inputs.Game.Drag.performed -= OnDrag;
        _inputs.Game.Click.canceled -= OnEndDrag;
    }

    #endregion

    #region DRAG HANDLE
    /// <summary>
    /// Call when drag input start
    /// </summary>
    /// <param name="obj">Input Contect with all the input data</param>
    private void OnBeginDrag(InputAction.CallbackContext obj)
    {
        if(GameManager.Instance.State != GameManager.GAME_STATE.PLAYING)return;
        _listPosition = new List<Vector3>();

        //Set the Pointer Event Position to that of the game object
        _pointerEventData.position = Touchscreen.current.position.value;

        //Create a list of Raycast Results
        List<RaycastResult> results = new List<RaycastResult>();
        
        _raycaster.Raycast(_pointerEventData, results);

        if (results.Count == 0 || results[0].gameObject == null) return;

        _selectedCardTransform = results[0].gameObject.transform;
        var selectedCard = _selectedCardTransform.GetComponent<Card>();
        if (selectedCard == null || !selectedCard.IsFrontCard)
        {
            CheckIfStackCard(results.Find(x => x.gameObject.name.Equals("Card Back Placement")).gameObject);
            return;
        }

        _canMove = true;
        _initialICardContainer = FindCardContainer();

        if(_initialICardContainer == null)
        {
            _canMove = false;
            return;
        }

        _selectedCardParent = _selectedCardTransform.parent;

        // Get All cart selected And add them in the new parent
        GetAllCardSelected(_initialICardContainer, selectedCard);

        _selectedCardTransform.SetParent(_parentWhenSelected);
    }

    /// <summary>
    /// Call when drag input is on going
    /// </summary>
    /// <param name="obj">Input Contect with all the input data</param>
    private void OnDrag(InputAction.CallbackContext obj)
    {
        if(GameManager.Instance.State != GameManager.GAME_STATE.PLAYING)return;
        if (!_canMove) return;

        _movablepartWhenCartSelected.position = obj.ReadValue<Vector2>(); 
    }

    /// <summary>
    /// Call when drag input ended
    /// </summary>
    /// <param name="obj">Input Contect with all the input data</param>
    private void OnEndDrag(InputAction.CallbackContext obj)
    {
        if (GameManager.Instance.State != GameManager.GAME_STATE.PLAYING) return;
        if(!_canMove) return;

        foreach (var card in _selectedCard)
        {
            card.Diselected();
        }

        var container = FindCardContainer();

        SetCardPosition(container);

        _selectedCardTransform = null;
        _selectedCard = null;
        _canMove = false;
    }

    #endregion

    #region CARD MANAGEMENT
    /// <summary>
    /// Verify if the selected object is a StackCard
    /// </summary>
    /// <param name="go">The object to verify</param>
    private void CheckIfStackCard(GameObject go)
    {
        if (go == null || go.transform.parent.GetComponent<StackCard>() == null) return;
        _eventStackShowNextCard.Raise();
        _eventAddUndo.Raise(new UndoData() { _containerToAddTo = null, _containerToRemoveFrom = null, });
    }

    /// <summary>
    /// Get all the card below the selected card
    /// </summary>
    /// <param name="initialColumn">Container to get all the card</param>
    /// <param name="selectedCard">Initial Card to use</param>
    private void GetAllCardSelected(ICardContainer initialColumn, Card selectedCard)
    {
        if (initialColumn.IsSingleCardSelector)
        {
            _selectedCard = new List<Card>();
            _selectedCard.Add(selectedCard);
            _listPosition.Add(selectedCard.transform.position);
            return;
        }

        var cards = ((ColumnCard)initialColumn).GetCardAfterSelected(selectedCard);

        // Ajouter dans _cards
        _selectedCard = new List<Card>(cards);

        foreach (var card in _selectedCard)
        {
            card.Selected();
            _listPosition.Add(card.transform.position);
            card.transform.SetParent(_parentWhenSelected);
        }
    }

    /// <summary>
    /// Set the selected card (inside _selectedCard) in the container column
    /// </summary>
    /// <param name="column">Container in wich the cards need to be add</param>
    private void SetCardPosition(ICardContainer column)
    {
        // Return in the initial column
        if(column == null || column.CheckIfCardCanBeAdd(_selectedCard[0]) == false)
        {
            foreach (var card in _selectedCard)
            {
                card.transform.SetParent(_selectedCardParent);
            }
            _initialICardContainer.ResetCard(_selectedCard);
            return;
        }

        // Go to a new column

        _eventMoveCard.Raise();
        AddUndo(column);

        _eventAddScore.Raise(_selectedCard.Count);

        _initialICardContainer.RemoveFrontCard(_selectedCard);
        column.AddExistingCard(_selectedCard);
        _eventRemoveFrontCard.Raise();
    }

    /// <summary>
    /// Search for a container in the Mouse position
    /// </summary>
    /// <returns> The selected container or null if no container have been found</returns>
    private ICardContainer FindCardContainer()
    {
        _pointerEventData.position = Touchscreen.current.position.value;
        List<RaycastResult> results = new List<RaycastResult>();
        _raycaster.Raycast(_pointerEventData, results);
        foreach (var ray in results)
        {
            if (ray.gameObject.TryGetComponent(out ICardContainer column))
            {
                return column;
            }
        }
        return null;
    }
    #endregion

    #region UNDO
    /// <summary>
    /// Notify that a movement have been done so add a possible undo
    /// </summary>
    /// <param name="newContainer"> Container </param>
    private void AddUndo(ICardContainer newContainer)
    {
        var undoData = new UndoData();

        // Get initial ICardContainer
        undoData._containerToAddTo = _initialICardContainer;
        // Get current ICardContainer
        undoData._containerToRemoveFrom= newContainer;
        // Get initial parent
        undoData._initialParent = _selectedCardParent;
        // Get initial Position
        undoData._initialPositions = _listPosition;
        // Get list cards move
        undoData._cardToMove = _selectedCard;

        _eventAddUndo.Raise(undoData);
    }
    #endregion
}