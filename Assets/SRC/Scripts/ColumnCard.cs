/*******************************************************
Nom ......... : ColumnCard.cs
Role ........ : Place all the cards in a the column, manage 
to sort the card in the front and back side.

Auteur ...... : Edouard MORDANT
Version ..... : V1.0 du 19/09/2024
********************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ColumnCard : MonoBehaviour, ICardContainer
{
    [Header("Events")]
    [SerializeField] private CustomEvent _eventRemoveFrontCard;

    [Header("Components")]
    [SerializeField] private RectTransform _verticalLayoutBackCardTransform;
    [SerializeField] private VerticalLayoutGroup _verticalLayoutBackCard;
    [SerializeField] private VerticalLayoutGroup _verticalLayoutFrontCard;

    [Header("Datas")]
    [SerializeField] private float _heightPerCard;
    [SerializeField] public CardData _lastCardData;

    private List<Card> _frontCards = new List<Card>();
    private List<Card> _backCards = new List<Card>();

    private Vector2 _deltaSize;
    public bool IsSingleCardSelector => false;
    public bool AdCards => _frontCards.Count == 0 && _backCards.Count == 0;
    public bool IsOnlyCard => _frontCards.Count == 1;

    #region MonoBehaviour METHODS
    private void Awake()
    {
        _eventRemoveFrontCard.handle += OnRemoveCard;

        _deltaSize = _verticalLayoutBackCardTransform.sizeDelta;
    }

    private void OnDestroy()
    {
        _eventRemoveFrontCard.handle -= OnRemoveCard;
    }
    #endregion

    #region SIZE HANLDE
    /// <summary>
    /// Edit the size of the container of back side cards to enance a good margin inbetween Front side and Back side cards
    /// </summary>
    private void UpdateSize()
    {
        _deltaSize.y = _heightPerCard * _verticalLayoutBackCardTransform.childCount;
        _verticalLayoutBackCardTransform.sizeDelta = _deltaSize;
    }
    #endregion

    #region CARD MANAGEMENT
    /// <summary>
    /// Create the amount needed of front card int the corresponding parent
    /// </summary>
    /// <param name="cardNumber"> Number of card to create </param>
    public void AddFrontCard(int cardNumber)
    {
        _frontCards = ResourcesManager.Instance.AddCard(cardNumber, true, _verticalLayoutFrontCard.transform, _verticalLayoutBackCard.transform).ToList();
        _lastCardData = _frontCards[^1]._data;
    }

    /// <summary>
    /// Create the amount needed of back card int the corresponding parent
    /// </summary>
    /// <param name="cardNumber"> Number of card to create </param>
    public void AddBackCard(int cardNumber) 
    {
        _backCards = ResourcesManager.Instance.AddCard(cardNumber, false, _verticalLayoutFrontCard.transform, _verticalLayoutBackCard.transform).ToList();
        UpdateSize();
    }

    /// <summary>
    /// When a card is remove from the front side container we flip the last card of the back side (if their is one)
    /// </summary>
    private void OnRemoveCard()
    {
        CheckIfNeedToFlipBackCard();
    }

    /// <summary>
    /// Verify if we need to Flip the last card in the BackSide container
    /// </summary>
    private void CheckIfNeedToFlipBackCard()
    {
        if (_frontCards.Count > 0 || _backCards.Count == 0) return;

        var cardToSwitch = _backCards[^1];
        _backCards.Remove(cardToSwitch);
        AddExistingCard(cardToSwitch, true);
        _lastCardData = cardToSwitch._data;
        UpdateSize();
    }

    /// <summary>
    /// Get all the card below the one selected
    /// </summary>
    /// <param name="selectedCard"> return a List of card thats below the card selected (the front side card) </param>
    /// <returns></returns>
    internal List<Card> GetCardAfterSelected(Card selectedCard)
    {
        var id = _frontCards.IndexOf(selectedCard);
        List<Card> cards = new List<Card>();
        for (int i = 0; i < _frontCards.Count; i++)
        {
            if (i < id) continue;
            cards.Add(_frontCards[i]);
        }
        return cards;
    }
    #endregion

    #region ICardContainer METHODS
    /// <summary>
    /// Remove all the card in the List
    /// </summary>
    /// <param name="selectedCard"> List of card that will be remove of the column </param>
    public void RemoveFrontCard(List<Card> selectedCard)
    {
        foreach (var card in selectedCard)
        {
            _frontCards.Remove(card);
        }

        if(_frontCards.Count > 0) _lastCardData = _frontCards[^1]._data;
    }

    /// <summary>
    /// Add a list of card that already exit in the game
    /// </summary>
    /// <param name="selectedCard"> The card to add in the column </param>
    /// <param name="flipToFront"> True : flip the card to the front side </param>
    public void AddExistingCard(List<Card> selectedCard, bool flipToFront = false)
    {
        foreach (var card in selectedCard)
        {
            AddExistingCard(card, flipToFront);
        }
    }

    /// <summary>
    /// Add a card that already exit in the game
    /// </summary>
    /// <param name="selectedCard"> The card to add in the column </param>
    /// <param name="flipToFront"> True : flip the card to the front side </param>
    public void AddExistingCard(Card selectedCard, bool flipToFront = false)
    {
        _frontCards.Add(selectedCard);
        selectedCard.transform.SetParent(_verticalLayoutFrontCard.transform);

        _lastCardData = selectedCard._data;
        if (flipToFront) selectedCard.FlipToFront();
    }

    /// <summary>
    /// Verify if the card that will be added in the column as the correct symbol and a number that's just -1 the last card of the column
    /// </summary>
    /// <param name="card"> Card to verify </param>
    /// <returns></returns>
    public bool CheckIfCardCanBeAdd(Card card)
    {
        bool correctSymbol = (card._data._type is SymbolType.ACE or SymbolType.SPADE) && (_lastCardData._type is SymbolType.HEART or SymbolType.DIAMOND)
            || (card._data._type is SymbolType.HEART or SymbolType.DIAMOND) && (_lastCardData._type is SymbolType.ACE or SymbolType.SPADE);
        bool correctNumber = _lastCardData._number -1 == card._data._number;
        return (_frontCards.Count == 0 && card._data._number == 13) || (correctSymbol && correctNumber);
    }

    /// <summary>
    /// Switch the last back card to the front side because no front card is remaing
    /// </summary>
    public void FlipLastBackCard()
    {
        if(_backCards.Count == 0 && _frontCards.Count == 0) return;

        var card = _frontCards[0];
        _backCards.Add(card); 
        _frontCards.Remove(card);
        card.transform.SetParent(_verticalLayoutBackCard.transform);

        card.FlipToBack();
        UpdateSize();
    }

    public void ResetCard(List<Card> cards) { }

    #endregion
}