using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManagerScript : MonoBehaviour {

    public string puzzleName;
    public List<GameObject> puzzlePieces = new List<GameObject>();
    public GameObject puzzleSolution;

    private Dictionary<GameObject, PuzzlePieceState> resetState = new Dictionary<GameObject, PuzzlePieceState>();

	// Use this for initialization
	void Start () {

        if(puzzleSolution == null)
        {
            Debug.Log("Puzzle Solution Missing for puzzle "+ puzzleName);
        }

		foreach(GameObject piece in puzzlePieces)
        {
            PuzzlePieceState puzzlePieceState = new PuzzlePieceState();
            puzzlePieceState.isOn = piece.activeInHierarchy;
            puzzlePieceState.position = piece.transform.position;
            puzzlePieceState.rotation = piece.transform.rotation;
            resetState.Add(piece, puzzlePieceState);
        }
	}

    public void resetPuzzle()
    {
        foreach (GameObject piece in resetState.Keys)
        {
            if(piece.GetComponent<TriggerReceiver>() != null)
            {
                piece.GetComponent<TriggerReceiver>().ResetTrigger();
            }
            else
            {
                PuzzlePieceState state;
                resetState.TryGetValue(piece, out state);
                piece.transform.position = state.position;
                piece.transform.rotation = state.rotation;
                piece.SetActive(state.isOn);
            }
        }
    }

   public bool isSolved()
    {
        return puzzleSolution.activeInHierarchy;
    }
}
