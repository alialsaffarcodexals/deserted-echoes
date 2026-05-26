// ------------------------------------------------------------
// SlidePuzzle.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Ali Alsaffar
// Created: 26/5/2026
// Description: Scarab slide puzzle. Tiles slide into the single empty cell;
//              when every tile is back in its home cell the puzzle is solved
//              and the "solved" object is revealed.
// ------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlidePuzzle : MonoBehaviour
{
    [Header("Grid layout")]
    [SerializeField] private int columns = 3;
    [SerializeField] private int rows = 3;
    [Tooltip("Anchored position of the top-left cell, relative to the tiles' parent.")]
    [SerializeField] private Vector2 topLeftCell = new Vector2(-60f, 98f);
    [Tooltip("Distance between cell centres (x = right, y = down).")]
    [SerializeField] private Vector2 cellSpacing = new Vector2(123f, 115f);

    [Header("Tiles in reading order (top-left first). The last grid cell stays empty.")]
    [SerializeField] private List<Image> tiles = new List<Image>();

    [Header("Result")]
    [SerializeField] private GameObject puzzleSolvedObject;
    [Tooltip("Optional: hidden when the puzzle is solved.")]
    [SerializeField] private GameObject puzzleUnsolvedObject;

    [Header("Behaviour")]
    [SerializeField] private int shuffleMoves = 80;
    [SerializeField] private float slideDuration = 0.12f;
    [SerializeField] private bool shuffleOnStart = true;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip slideClip;
    [SerializeField] private AudioClip invalidClip;
    [SerializeField] private AudioClip solvedClip;

    private SlidePuzzlePiece[] grid; // length columns*rows, null = empty cell
    private int emptyIndex;
    private bool busy;
    private bool solved;

    private int CellCount => columns * rows;
    private int Col(int index) => index % columns;
    private int Row(int index) => index / columns;

    private void Start()
    {
        BuildGrid();
        if (shuffleOnStart)
            Shuffle();
    }

    private void BuildGrid()
    {
        grid = new SlidePuzzlePiece[CellCount];
        for (int i = 0; i < tiles.Count && i < CellCount; i++)
        {
            Image img = tiles[i];
            if (img == null) continue;

            img.raycastTarget = true;
            SlidePuzzlePiece piece = img.GetComponent<SlidePuzzlePiece>();
            if (piece == null)
                piece = img.gameObject.AddComponent<SlidePuzzlePiece>();

            piece.Setup(this, i);
            grid[i] = piece;
            piece.Rect.anchoredPosition = CellPosition(i);
        }

        emptyIndex = CellCount - 1; // last cell is the gap
        grid[emptyIndex] = null;
    }

    private Vector2 CellPosition(int index)
    {
        return topLeftCell + new Vector2(Col(index) * cellSpacing.x, -Row(index) * cellSpacing.y);
    }

    // Performs a series of random valid slides so the board is always solvable.
    public void Shuffle()
    {
        if (grid == null) BuildGrid();

        solved = false;
        if (puzzleSolvedObject != null) puzzleSolvedObject.SetActive(false);

        int previousEmpty = -1;
        for (int n = 0; n < shuffleMoves; n++)
        {
            List<int> options = Neighbours(emptyIndex);
            options.RemoveAll(c => c == previousEmpty); // don't instantly undo the last move
            if (options.Count == 0) options = Neighbours(emptyIndex);

            int pick = options[Random.Range(0, options.Count)];
            previousEmpty = emptyIndex;
            SwapWithEmpty(pick, false);
        }

        if (IsSolved())
        {
            List<int> options = Neighbours(emptyIndex);
            SwapWithEmpty(options[Random.Range(0, options.Count)], false);
        }
    }

    private List<int> Neighbours(int index)
    {
        List<int> list = new List<int>(4);
        if (Col(index) > 0) list.Add(index - 1);
        if (Col(index) < columns - 1) list.Add(index + 1);
        if (Row(index) > 0) list.Add(index - columns);
        if (Row(index) < rows - 1) list.Add(index + columns);
        return list;
    }

    private bool AreAdjacent(int a, int b)
    {
        return Mathf.Abs(Col(a) - Col(b)) + Mathf.Abs(Row(a) - Row(b)) == 1;
    }

    public void TryMove(SlidePuzzlePiece piece)
    {
        if (busy || solved || piece == null) return;

        if (!AreAdjacent(piece.CurrentIndex, emptyIndex))
        {
            PlayClip(invalidClip);
            return;
        }

        SwapWithEmpty(piece.CurrentIndex, true);
    }

    private void SwapWithEmpty(int pieceIndex, bool animate)
    {
        SlidePuzzlePiece piece = grid[pieceIndex];
        if (piece == null) return;

        int target = emptyIndex;
        grid[target] = piece;
        grid[pieceIndex] = null;
        emptyIndex = pieceIndex;
        piece.CurrentIndex = target;

        Vector2 destination = CellPosition(target);
        if (animate)
        {
            PlayClip(slideClip);
            StartCoroutine(SlideTo(piece, destination));
        }
        else
        {
            piece.Rect.anchoredPosition = destination;
        }
    }

    private IEnumerator SlideTo(SlidePuzzlePiece piece, Vector2 destination)
    {
        busy = true;
        Vector2 start = piece.Rect.anchoredPosition;
        float t = 0f;
        while (t < slideDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = slideDuration <= 0f ? 1f : Mathf.Clamp01(t / slideDuration);
            piece.Rect.anchoredPosition = Vector2.Lerp(start, destination, k);
            yield return null;
        }
        piece.Rect.anchoredPosition = destination;
        busy = false;
    }

    private bool IsSolved()
    {
        for (int i = 0; i < grid.Length; i++)
        {
            SlidePuzzlePiece p = grid[i];
            if (p != null && p.HomeIndex != i)
                return false;
        }
        return true;
    }

    // Hooked to the "Solve" button. Reveals the win state only when the picture
    // is correctly assembled; otherwise plays the error cue so the player retries.
    public void CheckSolution()
    {
        if (busy || solved) return;

        if (IsSolved())
            OnSolved();
        else
            PlayClip(invalidClip);
    }

    private void OnSolved()
    {
        solved = true;
        PlayClip(solvedClip);
        if (puzzleSolvedObject != null) puzzleSolvedObject.SetActive(true);
        if (puzzleUnsolvedObject != null) puzzleUnsolvedObject.SetActive(false);
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}
