using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



// workaround for not being able to use Type in a template
// Calls board.Create when PieceType.create is called
public abstract class PieceType
{
    protected Coor coor;
    protected Player owner;
    protected Board board;
    protected Piece piece = null;

    public PieceType(Coor coor, Player owner, Board board)
    {
        this.coor = coor;
        this.owner = owner;
        this.board = board;
    }

    public void CreateAndAdd()
    {
        Create();
        board.AddPiece(piece);
    }
    protected abstract void Create();
    public void Uncreate()
    {
        board.DestroyPiece(piece);
    }
}

public class KingType : PieceType
{
    public KingType(Coor coor, Player owner, Board board) : base(coor, owner, board) { }
    protected override void Create()
    { piece = board.Create<King>(coor, owner); }
}
public class PawnType : PieceType
{
    public PawnType(Coor coor, Player owner, Board board) : base(coor, owner, board) { }
    protected override void Create()
    { piece = board.Create<Pawn>(coor, owner); }
}
public class RookType : PieceType
{
    public RookType(Coor coor, Player owner, Board board) : base(coor, owner, board) { }
    protected override void Create()
    { piece = board.Create<Rook>(coor, owner); }
}
public class BishopType : PieceType
{
    public BishopType(Coor coor, Player owner, Board board) : base(coor, owner, board) { }
    protected override void Create()
    { piece = board.Create<Bishop>(coor, owner); }
}
public class QueenType : PieceType
{
    public QueenType(Coor coor, Player owner, Board board) : base(coor, owner, board) { }
    protected override void Create()
    { piece = board.Create<Queen>(coor, owner); }
}
public class KnightType : PieceType
{
    public KnightType(Coor coor, Player owner, Board board) : base(coor, owner, board) { }
    protected override void Create()
    { piece = board.Create<Knight>(coor, owner); }
}

public class PieceMovement
{
    public Piece movingPiece;
    public Coor destination;
    public Coor departure;
    public PieceMovement( Piece movingPiece, Coor departure, Coor destination )
    {
        this.movingPiece = movingPiece;
        this.destination = destination;
        this.departure = departure;
    }
}

public class Turn
{
    private Board board;

    // list of movements
    private List<PieceMovement> movements = new List<PieceMovement>();

    // piecesRemoved/Created during the turn
    private List<Piece> piecesRemoved = new List<Piece>();
    private List<PieceType> piecesCreated = new List<PieceType>();

    public Turn( Board board )
    {
        this.board = board;
    }

    // marks the specified piece to be removed during the turn
    public void RemovePiece(Piece piece)
    {
        piecesRemoved.Add(piece);
    }

    // marks the specified piece to be added during the turn
    public void AddPiece(PieceType piece)
    {
        piecesCreated.Add(piece);
    }

    // moves piece from startCoor to endCoor
    public void AddMovement(Piece piece, Coor startCoor, Coor endCoor)
    {
        movements.Add(new PieceMovement(piece, startCoor, endCoor));
    }

    // remove all movement requests
    public void IgnoreMovement()
    {
        movements = new List<PieceMovement>();
    }

    // does the turn
    private void Do()
    {
        // remove/create pieces if applicable
        foreach( Piece pieceRemoved in piecesRemoved )
        {
            board.RemovePiece(pieceRemoved);
        }

        foreach (PieceType pieceCreated in piecesCreated)
        {
            pieceCreated.CreateAndAdd();
        }

        foreach(PieceMovement movement in  movements)
        {
            // move the moving piece to endCoor
            board.GetPosition(movement.departure).piece = null;
            movement.movingPiece.coor = movement.destination;
            board.GetPosition(movement.destination).piece = movement.movingPiece;
            movement.movingPiece.turnCount++;
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

        foreach (PieceMovement movement in movements)
        {
            // move the moving piece to endCoor
            board.GetPosition(movement.destination).piece = null;
            movement.movingPiece.coor = movement.departure;
            board.GetPosition(movement.departure).piece = movement.movingPiece;
            movement.movingPiece.turnCount--;
        }

        // remove/create pieces
        foreach (Piece pieceRemoved in piecesRemoved)
        {
            board.AddPiece(pieceRemoved);
        }
        foreach (PieceType pieceCreated in piecesCreated)
        {
            pieceCreated.Uncreate();
        }

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
