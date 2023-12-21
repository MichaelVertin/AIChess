using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserPlayer : Player
{
    private Board board;

    public UserPlayer(int id) : base(id)
    {

    }

    // called when given control of the Board
    public override void OnControlStart(Board board)
    {
        this.board = board;
        List<Turn> turns = new List<Turn>();
        foreach (Piece piece in board.pieces)
        {
            turns.AddRange(piece.GetLegalTurns());
        }



        // temporary: do a random turn
        int randomIndex = Random.Range(0, turns.Count);
        turns[randomIndex].DoIfVerified();
        board.UpdatePhysical();
        OnTurnsEnd();
    }

    // called when turns ended, 
    // does a random turn before passing control
    public void OnTurnsEnd()
    {
        board.PassControl();
    }
}

