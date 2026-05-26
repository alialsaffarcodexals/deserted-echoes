// ------------------------------------------------------------
// SlidePuzzlePiece.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Ali Alsaffar
// Created: 26/5/2026
// Description: A single sliding tile. Reports clicks to its SlidePuzzle owner.
// ------------------------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class SlidePuzzlePiece : MonoBehaviour, IPointerClickHandler
{
    private SlidePuzzle puzzle;

    public RectTransform Rect { get; private set; }

    // Cell this tile belongs to in the finished picture.
    public int HomeIndex { get; private set; }

    // Cell this tile currently occupies.
    public int CurrentIndex { get; set; }

    public void Setup(SlidePuzzle owner, int homeIndex)
    {
        puzzle = owner;
        HomeIndex = homeIndex;
        CurrentIndex = homeIndex;
        Rect = (RectTransform)transform;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (puzzle != null)
            puzzle.TryMove(this);
    }
}
