/*******************************************************
Nom......... : Map.cs
Role........ : All the data needed for the managers to 
handle the game

Auteur...... : Edouard MORDANT
Version..... : V1.0 du 19/09/2024
********************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField] public ColumnCard[] _columns;
    [SerializeField] public FinalStackCard[] _finalStack;
    [SerializeField] public StackCard _stack;
    [SerializeField] public Transform _parentWhenSelected;
    [SerializeField] public Transform _movablePartWhenCardSelected;
}
