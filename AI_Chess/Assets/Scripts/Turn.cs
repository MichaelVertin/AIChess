using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Turn
{
    private Board board;

    // start/end coordinate of movingPiece
    public readonly Coor startCoor, endCoor;
    private Piece movingPiece = null;

    // pieceRemoved/Created during the turn
    private Piece pieceRemoved = null, pieceCreated = null;

    public Turn( Board boardPar, Coor startCoorPar, Coor endCoorPar, Piece piece)
    {
        this.board = boardPar;
        this.startCoor = startCoorPar;
        this.endCoor = endCoorPar;
        this.movingPiece = piece;
    }

    // marks the specified piece to be removed during the turn
    public void RemovePiece(Piece piece)
    {
        pieceRemoved = piece;
    }
    // marks the specified piece to be added during the turn
    public void AddPiece(Piece piece)
    {
        pieceCreated = piece;
    }

    // does the turn
    private void Do()
    {
        // fail if movingPiece is not at the start
        if( !ReferenceEquals(board.GetPosition(startCoor).piece, this.movingPiece) )
        {
            Debug.Log("Attempted to Do turn when the movingPiece is no longer at startCoor");
            return;
        }

        // remove/create pieces if applicable
        if (pieceRemoved != null)
            { board.RemovePiece(pieceRemoved); }
        if (pieceCreated != null)
            { board.AddPiece(pieceCreated); }

        if (!ReferenceEquals(pieceRemoved, movingPiece))
        {
            // move the moving piece to endCoor
            movingPiece.coor = endCoor;
            board.GetPosition(endCoor).piece = movingPiece;
            board.GetPosition(startCoor).piece = null;
            movingPiece.turnCount++;
        }

        // add the turn to the board's history
        board.history.Push(this);

        // select the next player
        board.NextPlayerTurn();
    }

    // undoes the turn
    public void Undo()
    {
        // fail if the last turn was not this
        if( !ReferenceEquals(board.history.Peek(),this))
        {
            Debug.Log("Attempted to Undo a turn that was not the last made");
            return;
        }
        // otherwise, assume top turn is this, 
        //    remove this from history
        board.history.Pop();

        // fail if movingPiece is not at the endCoor
        if (!ReferenceEquals(board.GetPosition(endCoor).piece, this.movingPiece))
        {
            Debug.Log("Attempted to Undo turn when the movingPiece is no longer at endCoor");
            return;
        }

        // move the movingPiece back to startCoor
        if( !ReferenceEquals(movingPiece,pieceRemoved))
        {
            movingPiece.coor = startCoor;
            board.GetPosition(startCoor).piece = movingPiece;
            board.GetPosition(endCoor).piece = null;
            movingPiece.turnCount--;
        }

        // remove/create pieces if applicable
        if (pieceRemoved != null)
            { board.AddPiece(pieceRemoved); }
        if (pieceCreated != null)
            { board.DestroyPiece(pieceCreated); }

        // select the previous player
        board.PrevPlayerTurn();
    }

    // returns if the Board's state will become INVALID
    //     when the turn is done
    public bool Verify()
    {
        // do the turn, check for a valid state, undo the turn
        bool isValid;
        Do();
        isValid = board.state != BoardState.INVALID;
        Undo();
        return isValid;
    }

    // does the turn if it does not create an invalid state
    //     returns if the turn was done
    public bool DoIfVerified()
    {
        // do the turn
        Do();

        // undo the turn if invalid
        if( board.state == BoardState.INVALID )
        {
            Undo();
            return false;
        }
        return true;
    }
}
