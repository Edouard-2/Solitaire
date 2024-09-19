/*******************************************************
Nom......... : UndoData.cs
Role........ : Struct with all the data needed to Undo all 
the card move

Auteur...... : Edouard MORDANT
Version..... : V1.0 du 19/09/2024
********************************************************/
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct UndoData
{
    public List<Card> _cardToMove;
    public Transform _initialParent;
    public List<Vector3> _initialPositions;

    /// <summary>
    /// The Previous Container
    /// </summary>
    public ICardContainer _containerToAddTo;

    /// <summary>
    /// The Current Container
    /// </summary>
    public ICardContainer _containerToRemoveFrom;

    public bool Undo()
    {
        if(_containerToAddTo == null && _containerToRemoveFrom == null)
        {
            GameManager.Instance.UndoStack();
            return false;
        }

        _containerToRemoveFrom.RemoveFrontCard(_cardToMove);

        for (int i = 0; i < _cardToMove.Count; i++)
        {
            _cardToMove[i].transform.DOMove(_initialPositions[i], .5f);
        }

        DOVirtual.DelayedCall(.5f, EndUndo);
        return true;
    }

    private void EndUndo()
    {
        foreach (Card card in _cardToMove)
        {
            card.transform.SetParent(_initialParent);
        }
        _containerToAddTo.AddExistingCard(_cardToMove);
        _containerToAddTo.FlipLastBackCard();
    }
}
