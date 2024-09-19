/*******************************************************
Nom......... : FinalStackCard.cs
Role........ : Manage the stack of finals cards

Auteur...... : Edouard MORDANT
Version..... : V1.0 du 19/09/2024
********************************************************/
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FinalStackCard : MonoBehaviour, ICardContainer
{
    [SerializeField] private SymbolType _type;
    [SerializeField] private CustomEvent _eventCompleteStack;
    [SerializeField] private CustomEvent_Int _eventAddScoreFinalStack;
    [SerializeField] private Transform _parentCards;
    [SerializeField] private int index = 0;

    [SerializeField] private List<Card> _cards = new List<Card>();
    [SerializeField] public CardData _lastCardData;

    public bool AdCards => _cards.Count != 0;

    public bool IsSingleCardSelector => true;

    #region ICardContainer METHODS

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
        _eventAddScoreFinalStack.Raise(1);
        _cards.Add(selectedCard);
        selectedCard.transform.SetParent(_parentCards.transform);
        selectedCard.transform.position = _parentCards.transform.position;

        _lastCardData = selectedCard._data;
        if (flipToFront) selectedCard.FlipToFront();

        CheckEndStack();
    }

    /// <summary>
    /// Verify if the card that will be added in the column as the correct symbol and a number that's just -1 the last card of the column
    /// </summary>
    /// <param name="card"> Card to verify </param>
    /// <returns></returns>
    public bool CheckIfCardCanBeAdd(Card card)
    {
        if (_cards.Count == 0) 
        {
            bool b = card._data._number == 1;
            if (b) _type = card._data._type;
            return card._data._number == 1;
        }

        bool correctSymbol = _type == card._data._type;
        bool correctNumber = _lastCardData._number + 1 == card._data._number;

        return (_cards.Count == 0) || (correctSymbol && correctNumber);
    }

    /// <summary>
    /// Switch the last back card to the front side because no front card is remaing
    /// </summary>
    public void FlipLastBackCard() { }

    /// <summary>
    /// Remove all the card in the List
    /// </summary>
    /// <param name="selectedCard"> List of card that will be remove of the column </param>
    public void RemoveFrontCard(List<Card> selectedCard)
    {
        foreach (var card in selectedCard)
        {
            _cards.Remove(card);
        }
        if(_cards.Count != 0) _lastCardData = _cards[^1]._data;
    }

    public void ResetCard(List<Card> cards) { }

    #endregion

    #region HANDLE

    /// <summary>
    /// Verify if all the cards have been added
    /// </summary>
    private void CheckEndStack()
    {
        if (_cards.Count >= 13)
        {
            _eventCompleteStack.Raise();
        }
    }
    #endregion
}
