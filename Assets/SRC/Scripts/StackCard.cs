/*******************************************************
Nom......... : StackCard.cs
Role........ : Manage the stack of all the remaiding card

Auteur...... : Edouard MORDANT
Version..... : V1.0 du 19/09/2024
********************************************************/
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class StackCard : MonoBehaviour, ICardContainer
{
    [SerializeField] private CustomEvent _eventActiveLose;
    [SerializeField] private CustomEvent _eventMoveCard;
    [SerializeField] private CustomEvent _eventRaiseUndoStack;
    [SerializeField] private CustomEvent _eventShowNextCard;
    [SerializeField] private TextMeshProUGUI _numberCardBackSide;
    [SerializeField] private Transform[] _positionFrontCards;
    [SerializeField] private Transform _parentBackCards;
    [SerializeField] private Transform _parentFrontCards;

    [SerializeField] private float _durationMoveCard;

    [SerializeField] public List<Card> _cardsBack = new List<Card>();
    [SerializeField] public List<Card> _cardsSelectable = new List<Card>();
    [SerializeField] public List<Card> _cardsWaiting = new List<Card>();

    [SerializeField] private bool _interactible = true;
    [SerializeField] private int _indexCard;

    [SerializeField] private int _currentCardSiblingIndex;

    private Tween _tweenRefill;

    public bool IsSingleCardSelector => true;

    #region MonoBehaviour METHODS
    private void Awake()
    {
        _interactible = true;
        _eventRaiseUndoStack.handle += OnUndo;
        _eventShowNextCard.handle += OnNextCard;
    }
    
    private void OnDestroy()
    {
        _eventRaiseUndoStack.handle -= OnUndo;
        _eventShowNextCard.handle -= OnNextCard;
    }

    #endregion

    #region ICardContainer METHODS

    /// <summary>
    /// NOT USED IN THIS SCRIPT
    /// </summary>
    public bool CheckIfCardCanBeAdd(Card card)
    {
        return false;
    }


    /// <summary>
    /// Add a list of card that already exit in the game
    /// </summary>
    /// <param name="selectedCard"> The card to add in the column </param>
    /// <param name="flipToFront"> True : flip the card to the front side </param>
    public void AddExistingCard(List<Card> cards, bool flipToFront = false)
    {
        foreach (var card in cards)
        {
            AddExistingCard(card, flipToFront);
        }
    }

    /// <summary>
    /// Add a card that already exit in the game
    /// </summary>
    /// <param name="selectedCard"> The card to add in the column </param>
    /// <param name="flipToFront"> True : flip the card to the front side </param>
    public void AddExistingCard(Card cards, bool flipToFront = false)
    {
        if (_cardsSelectable.Count >= 3)
        {
            _cardsWaiting.Add(_cardsSelectable.Last());
            _cardsSelectable.Remove(_cardsSelectable.Last());
        }

        _cardsSelectable = _cardsSelectable.Prepend(cards).ToList();
        var indexSibling = _cardsSelectable[Mathf.Min(_cardsSelectable.IndexOf(cards) + 1, _cardsSelectable.Count - 1)].transform.GetSiblingIndex() + 1;
        if (_cardsSelectable.Count == 1) indexSibling = 0;
        cards.transform.SetSiblingIndex(indexSibling);
        UpdatePositionSelected();
    }

    /// <summary>
    /// Remove all the card in the List
    /// </summary>
    /// <param name="selectedCard"> List of card that will be remove of the column </param>
    public void RemoveFrontCard(List<Card> cards)
    {
        var cardToRemove = cards[0];

        // Remove Selected[0]
        _cardsSelectable.Remove(cardToRemove);

        if (_cardsWaiting.Count != 0)
        {
            // Prepend Waiting.Last() to Selected
            _cardsWaiting.Last().FlipToFront();
            _cardsSelectable.Add(_cardsWaiting.Last());
            _cardsWaiting.Remove(_cardsWaiting.Last());
        }

        if (_cardsSelectable.Count != 0)
        {
            _cardsSelectable[0].IsInteractable = true;
            _currentCardSiblingIndex = _cardsSelectable[0].transform.GetSiblingIndex();
        }

        // Move everyCard
        UpdatePositionSelected();
    }

    /// <summary>
    /// Reset the sibling index of the cards to enhance a goof overlaping due to the hierarchy priority
    /// </summary>
    /// <param name="cards"></param>
    public void ResetCard(List<Card> cards)
    {
        var card = _cardsSelectable[0];
        var indexSibling = _cardsSelectable[Mathf.Min(_cardsSelectable.IndexOf(card) + 1, _cardsSelectable.Count - 1)].transform.GetSiblingIndex() + 1;
        if (_cardsSelectable.Count == 1) indexSibling = 0;
        card.transform.SetSiblingIndex(indexSibling);
        //card.transform.SetSiblingIndex(_currentCardSiblingIndex);
        card.transform.position = _positionFrontCards[0].position;
    }

    /// <summary>
    /// Switch the last back card to the front side because no front card is remaing
    /// </summary>
    public void FlipLastBackCard()
    {
        if (_cardsWaiting.Count == 0) return;
        _cardsWaiting.Last().FlipToBack();
    }

    #endregion

    #region CARD MANAGEMENT

    /// <summary>
    /// When the player click on the stack it give another card
    /// </summary>
    private void OnNextCard()
    {
        if(_interactible == false) return;
        _interactible = false;
        
        _eventMoveCard.Raise();

        if (_cardsBack.Count == 0)
        {
            RefillEveryCard();
            return;
        }

        var card = _cardsBack[^1];

        if (_cardsSelectable.Count >= 3)
        {
            _cardsWaiting.Add(_cardsSelectable.Last());
            _cardsSelectable.Remove(_cardsSelectable.Last());
        }

        _cardsSelectable = _cardsSelectable.Prepend(card).ToList();
        _cardsBack.Remove(card);

        TranslateAllSelectableCards();

        UpdateText();
    }

    /// <summary>
    /// When no card is remaining in the stack, refill it with all the card as it was in the start of the game
    /// </summary>
    private void RefillEveryCard()
    {
        _interactible = false;
        _indexCard = 0;

        var countSelected = _cardsSelectable.Count;
        var countWaiting = _cardsWaiting.Count;

        for (int i = 0; i < countWaiting; i++)
        {
            var card = _cardsWaiting[0];
            card.transform.DOMoveX(_parentBackCards.position.x, _durationMoveCard).SetDelay(.3f);
            card.SetCorrectSide(false, false);
            _cardsBack.Add(card);
            _cardsWaiting.RemoveAt(0);
        }

        for (int i = 0; i < countSelected; i++)
        {
            var card = _cardsSelectable.Last();
            card.transform.DOMoveX(_parentBackCards.position.x, _durationMoveCard).SetDelay(.1f * i);
            card.SetCorrectSide(false,false);
            _cardsBack.Add(card);
            _cardsSelectable.Remove(card);
        }

        _tweenRefill = DOVirtual.DelayedCall(_durationMoveCard + .5f, () => 
        { 
            _tweenRefill = null; 
            _interactible = true; 
        });

        _cardsBack.Reverse();
        UpdateText();
    }

    /// <summary>
    /// Create all the cards at the begining of the game
    /// </summary>
    /// <param name="numberCard"></param>
    internal void AddBackCard(int numberCard)
    {
        _cardsBack = ResourcesManager.Instance.AddCard(numberCard, false, _parentFrontCards, _parentBackCards).ToList();
        _cardsBack.Reverse();
    }

    /// <summary>
    /// Update the text of card amount left to reveal
    /// </summary>
    private void UpdateText()
    {
        _numberCardBackSide.text = _cardsBack.Count.ToString();
    }
#endregion

    #region CARD MOVEMENT

    /// <summary>
    /// Increase the card index and move the _selectedCard to the targets position
    /// </summary>
    private void TranslateAllSelectableCards()
    {
        _indexCard++;
        _cardsSelectable[0].FlipToFront();
        _cardsSelectable[0].IsInteractable = true;
        _currentCardSiblingIndex = _cardsSelectable[0].transform.GetSiblingIndex();
        UpdatePositionSelected();
    }

    /// <summary>
    /// Handle the position of the _selectedCard
    /// </summary>
    private void UpdatePositionSelected()
    {
        for (int i = 0; i < _cardsSelectable.Count; i++)
        {
            var card = _cardsSelectable[i];

            card.transform.DOMoveX(_positionFrontCards[i].position.x, _durationMoveCard);
            if (i == 0) continue;

            card.IsInteractable = false;
        }

        DOVirtual.DelayedCall(_durationMoveCard, () =>
        {
            if (_tweenRefill == null) _interactible = true;
        });
    }
    #endregion

    #region UNDO
    /// <summary>
    /// Handle the undo
    /// </summary>
    private void OnUndo()
    {
        if (_cardsWaiting.Count != 0)
        {
            _cardsSelectable.Add(_cardsWaiting.Last());
            _cardsWaiting.Remove(_cardsWaiting.Last());
            _cardsSelectable.Last().FlipToFront();
        }

        // Undo quand tout à été remis au debut
        if(_cardsSelectable.Count == 0 && _cardsWaiting.Count == 0)
        {
            UndoTheReset();
            return;
        }

        NormalUndo();
    }

    /// <summary>
    /// Put the first card of _selectCard back in the stack
    /// </summary>
    private void NormalUndo()
    {
        var card = _cardsSelectable[0];
        card.SetCorrectSide(false,false);
        card.IsInteractable = false;
        card.transform.DOMoveX(_parentBackCards.position.x, _durationMoveCard);

        _cardsBack.Add(_cardsSelectable[0]);
        _cardsSelectable.RemoveAt(0);

        UpdatePositionSelected();
        UpdateText();

        if (_cardsSelectable.Count == 0) return;
        _cardsSelectable[0].IsInteractable = true;
    }
    
    /// <summary>
    /// Undo the refill of all the card in the stack
    /// </summary>
    private void UndoTheReset()
    {
        _cardsSelectable.Clear();

        for (int i = 0; i < 3; i++)
        {
            var card = _cardsBack[0];
            _cardsSelectable.Add(card);
            _cardsBack.Remove(card);
        }

        var count = _cardsBack.Count;

        for (int i = 0; i < count; i++)
        {
            var card = _cardsBack[0];
            _cardsWaiting = _cardsWaiting.Prepend(card).ToList();
            card.transform.DOMoveX(_positionFrontCards[2].position.x, _durationMoveCard);
            card.SetCorrectSide(false, false);
            _cardsBack.RemoveAt(0);
        }

        for (int i = 0; i < _cardsSelectable.Count; i++)
        {
            var card = _cardsSelectable[i];
            card.transform.DOMoveX(_positionFrontCards[i].position.x, _durationMoveCard);
            card.SetCorrectSide(true, false);
            card.IsInteractable = false;
        }

        _cardsBack.Clear();
        _cardsSelectable[0].IsInteractable = true;
        UpdateText();
    }
    #endregion
}