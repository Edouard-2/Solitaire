/*******************************************************
Nom ......... : Card.cs
Role ........ : Manage the card visual for each card as
indepandent objects.

Auteur ...... : Edouard MORDANT
Version ..... : V1.0 du 19/09/2024
********************************************************/

using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Image _cardImage;
    [SerializeField] private Image _smallSymbol;
    [SerializeField] private Image _bigSymbol;
    [SerializeField] private Image _number;
    [SerializeField] private Image _backCard;
    [SerializeField] private Image _frontCard;
    [SerializeField] private Animator _animator;

    [Header("Statistics")]
    [SerializeField] private bool _isFrontCard;
    [SerializeField] private float _durationFlipCardAnimation = 1;
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Color _diselectedColor;

    [Header("Data")]
    [SerializeField] public CardData _data;

    private int _hashFlipBack = Animator.StringToHash("FlipBack");
    private int _hashFlipFront = Animator.StringToHash("FlipFront");
    private int _hashFront = Animator.StringToHash("Idle Front");
    private int _hashBack = Animator.StringToHash("Idle Back");

    public bool IsFrontCard => _isFrontCard;
    public bool IsInteractable 
    {
        get { return _cardImage.raycastTarget; }

        set { _cardImage.raycastTarget = value; }
    }

    #region INITIALIZATION
    /// <summary>
    /// Initialize the card with the corresponding sprites and put the card in front or back side
    /// </summary>
    /// <param name="data"> Data used for the sprite request </param>
    /// <param name="isFrontCard"> True : the card is in front side </param>
    public void Initialize(CardData data, bool isFrontCard)
    {
        _data = data;
        var resourcesManager = ResourcesManager.Instance;
        var symbol = resourcesManager.GetSymbol(data._type);
        var number = resourcesManager.GetNumber(data);

        _bigSymbol.sprite = symbol;
        _smallSymbol.sprite = symbol;
        _number.sprite = number;

        _isFrontCard = isFrontCard;

        SetCorrectSide(_isFrontCard, false);
    }
    #endregion

    #region SELECTION
    /// <summary>
    /// The card is selected
    /// </summary>
    internal void Selected()
    {
        _cardImage.color = _selectedColor;
    }

    /// <summary>
    /// The card is Diselected
    /// </summary>
    internal void Diselected()
    {
        _cardImage.color = _diselectedColor;
    }
#endregion

    #region SIDE HANDLE
    /// <summary>
    /// Switch side of the card
    /// </summary>
    /// <param name="isFrontCard"> True : the card is in front side </param>
    /// <param name="hasTransition"> True : will play an animation to switch side. Else it will switch instantly</param>
    public void SetCorrectSide(bool isFrontCard, bool hasTransition = true)
    {
        _isFrontCard = isFrontCard;
        _animator.SetBool(_hashFlipFront, _isFrontCard);
        _animator.SetBool(_hashFlipBack, !_isFrontCard);

        if (!hasTransition)
        {
            _animator.Play(isFrontCard ? _hashFront : _hashBack);
        }
    }

    /// <summary>
    /// Flip the card in the front side
    /// </summary>
    internal void FlipToFront()
    {
        _isFrontCard = true;
        SetCorrectSide(_isFrontCard);
    }

    /// <summary>
    /// Flip the card in the back side
    /// </summary>
    internal void FlipToBack()
    {
        _isFrontCard = false;
        SetCorrectSide(_isFrontCard);
    }

    #endregion

    #region DEBUG

    /// <summary>
    /// Debug the Switch Side Animation
    /// </summary>
    [ContextMenu("Switch Side")]
    private void SwitchSide()
    {
        SetCorrectSide(!_isFrontCard);
    }

    #endregion
}