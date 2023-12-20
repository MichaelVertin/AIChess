using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;


/*
 * Player for testing moves
 * Does and undoes every possible move, 
 *   when done, passes control to the next player
 */
public class TesterPlayer : Player
{
    private Board board;
    private List<Turn> turns; // contains list of possible moves
    private int turnIndex; // used to iterate

    public TesterPlayer(int id) : base(id)
    {
        
    }

    // called when given control of the Board
    public override void OnControlStart(Board board)
    {
        this.board = board;
        turns = new List<Turn>();
        foreach( Piece piece in board.pieces )
        {
            turns.AddRange(piece.GetLegalTurns());
        }
        turnIndex = 0;
        board.TesterPlayerInitialize(this);
        board.TesterPlayerDoTurn();
    }

    // if another turn remains, do the turn, then undo the turn
    public bool DoTurn()
    {
        if( turnIndex >= turns.Count )
        {
            return false;
        }
        turns[turnIndex].DoIfVerified();
        board.UpdatePhysical();
        return true;
    }

    // undoes the last turn,
    // swaps to the next turn, 
    // contiues to do that turn
    public bool UndoTurn()
    {
        turns[turnIndex].Undo();
        turnIndex++;
        board.UpdatePhysical();
        return true;
    }

    // called when turns ended, 
    // does a random turn before passing control
    public void OnTurnsEnd()
    {
        int randomIndex = Random.Range(0, turns.Count);
        turns[randomIndex].DoIfVerified();
        board.UpdatePhysical();
        board.Invoke("PassControl", 1f);
    }
}
