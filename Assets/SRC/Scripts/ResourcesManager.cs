/*******************************************************
Nom......... : ResourcesManager.cs
Role........ : All resources aldready in game needed to 
create or use objects

Auteur...... : Edouard MORDANT
Version..... : V1.0 du 19/09/2024
********************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum SymbolType
{
    ACE,
    SPADE,
    HEART,
    DIAMOND
}

[Serializable]
public struct CardData
{
    public CardData(int number, SymbolType type)
    {
        _number = number;
        _type = type;
    }

    public int _number;
    public SymbolType _type;
}

public class ResourcesManager : MonoBehaviour
{
    public static ResourcesManager Instance;

    [SerializeField] public Map _mapPrefab;
    [SerializeField] public CustomEvent_GameState _onStateChange;

    [SerializeField, Tooltip("Be sure that all the numbers are in order")] private Sprite[] _numberBlack;
    [SerializeField, Tooltip("Be sure that all the numbers are in order")] private Sprite[] _numberRed;

    [SerializeField] private Sprite _symbolAce;
    [SerializeField] private Sprite _symbolSpade;
    [SerializeField] private Sprite _symbolHeart;
    [SerializeField] private Sprite _symbolDiamond;

    [SerializeField] public Card CardPrefab;

    [SerializeField] public List<CardData> _allCardsData;
    [SerializeField] public List<CardData> _tempAllCardsData;

    private Sprite[] _symbols;

    #region MonoBehaviour METHODS
    private void Awake()
    {
        Instance = this;

        _symbols = new Sprite[]{ _symbolAce, _symbolSpade, _symbolHeart, _symbolDiamond };

        _onStateChange.handle += OnGameChange;

        CreateAllCards();
    }

    private void OnDestroy()
    {
        _onStateChange.handle -= OnGameChange;
    }
    #endregion

    #region CARD MANAGEMENT
    /// <summary>
    /// Create all the cards use in the game
    /// </summary>
    private void CreateAllCards()
    {
        int symbol = 0;
        for (int i = 0; i < 52; i++)
        {
            if (i % 13 == 0) symbol++;
            CardData cardData = new CardData(i%13+1, (SymbolType)(symbol-1));
            _allCardsData.Add(cardData);
        }

        InitializeTempsCards();
    }

    private void InitializeTempsCards()
    {
        _tempAllCardsData = new List<CardData>(_allCardsData);
    }

    /// <summary>
    /// Return an IEnumerable of Card instanciate in the conrespondent parent according to the side of the card
    /// </summary>
    /// <param name="cardNumber">Number of card to instanciate</param>
    /// <param name="frontCard">True : The card is in front side. Else it's backside</param>
    /// <param name="parentFrontCard">Parent in wich all the FrontCard will spawn</param>
    /// <param name="parentBackCard">Parent in wich all the BackCard will spawn</param>
    /// <returns></returns>
    public IEnumerable<Card> AddCard(int cardNumber, bool frontCard, Transform parentFrontCard, Transform parentBackCard)
    {
        var cards = new Card[cardNumber];
        var parent = frontCard ? parentFrontCard : parentBackCard;

        for (int i = 0; i < cardNumber; i++)
        {
            var card = Instantiate(CardPrefab, parent.transform);
            card.Initialize(GetCardData(), frontCard);
            cards[i] = card;
        }
        return cards;
    }
    #endregion

    #region CARD DATA
    /// <summary>
    /// Return a random CardData
    /// </summary>
    /// <returns></returns>
    internal CardData GetCardData()
    {
        int id = Random.Range(0, _tempAllCardsData.Count);
        var cardData = _tempAllCardsData[id];
        _tempAllCardsData.RemoveAt(id);
        return cardData;
    }
    /// <summary>
    /// Return the sprite of the corresponding symbol
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public Sprite GetSymbol(SymbolType type)
    {
        switch (type)
        {
            case SymbolType.ACE:
                return _symbolAce;
            case SymbolType.SPADE:
                return _symbolSpade;
            case SymbolType.HEART:
                return _symbolHeart;
            case SymbolType.DIAMOND:
                return _symbolDiamond;
            default:
                return _symbolAce;
        }
    }

    /// <summary>
    /// Return the sprite of the corresponding number according the the card type
    /// </summary>
    /// <param name="data"> CardData </param>
    /// <returns></returns>
    public Sprite GetNumber(CardData data)
    {
        var array = data._type is SymbolType.ACE or SymbolType.SPADE ? _numberBlack : _numberRed;
        return array[data._number - 1];
    }
    #endregion

    #region HANDLE
    /// <summary>
    /// Reset when the game state change
    /// </summary>
    /// <param name="obj"></param>
    private void OnGameChange(GameManager.GAME_STATE obj)
    {
        if (obj != GameManager.GAME_STATE.HOME) return;

        InitializeTempsCards();
    }
    #endregion
}