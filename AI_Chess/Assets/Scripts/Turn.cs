using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


///////////////////////////////////////// PieceType ////////////////////////////
// workaround to use Type in a template
// Calls board.Create when PieceType.create is called
public abstract class PieceType
{
    public readonly Coor coor;
    public readonly Player owner;
    public readonly Board board;
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


// PieceMovement:
// Holds data to move piece from departure to destination
// does not provide error handling
public class PieceMovement
{
    private Piece movingPiece;
    private Coor destination;
    private Coor departure;
    private Board board;
    public PieceMovement(Board board, Piece movingPiece, Coor departure, Coor destination)
    {
        this.movingPiece = movingPiece;
        this.destination = destination;
        this.departure = departure;
        this.board = board;
    }

    public void Do()
    {
        // move the moving piece to endCoor
        board.GetPosition(departure).piece = null;
        movingPiece.coor = destination;
        board.GetPosition(destination).piece = movingPiece;
        movingPiece.turnCount++;
    }

    public void Undo()
    {
        // move the moving piece to startCoor
        board.GetPosition(destination).piece = null;
        movingPiece.coor = departure;
        board.GetPosition(departure).piece = movingPiece;
        movingPiece.turnCount--;
    }
}

public class Turn
{
    private Board board;
    private Turn prevTurn;

    // list of movements
    private List<PieceMovement> movements = new List<PieceMovement>();

    // piecesRemoved/Created during the turn
    private List<Piece> piecesRemoved = new List<Piece>();
    private List<PieceType> piecesCreated = new List<PieceType>();


    ////////////////////////////////// creating Turn ///////////////////////////
    public Turn( Board board )
    {
        this.board = board;
        if(this.board.history.Count == 0)
        {
            prevTurn = null;
        }
        else
        {
            prevTurn = this.board.history.Peek();
        }
    }

    // marks the specified piece to be removed during the turn
    public void RemovePiece(Piece piece)
    {
        piecesRemoved.Add(piece);
        checkForCoordinateIdentifier(piece.coor);
    }

    // marks the specified piece to be added during the turn
    public void AddPiece(PieceType piece)
    {
        piecesCreated.Add(piece);
        checkForCoordinateIdentifier(piece.coor);
    }

    // moves piece from startCoor to endCoor
    public void AddMovement(Piece piece, Coor startCoor, Coor endCoor)
    {
        movements.Add(new PieceMovement(board, piece, startCoor, endCoor));
        checkForCoordinateIdentifier(startCoor);
        checkForCoordinateIdentifier(endCoor);
    }


    //////////////////////////// perform turns /////////////////////////////////
    // does the turn
    // NOTE: does not ensure the turn is legal
    //       (may make the king vulnerable)
    public void Do()
    {
        if(this.board.history.Count != 0)
        {
            if (!ReferenceEquals(this.board.history.Peek(), prevTurn))
            {
                Debug.LogError("WARNING: another turn was done since this turn was created");
            }
        }

        // remove/create pieces if applicable
        foreach ( Piece pieceRemoved in piecesRemoved )
        {
            board.RemovePiece(pieceRemoved);
        }


        foreach (PieceType pieceCreated in piecesCreated)
        {
            pieceCreated.CreateAndAdd();
        }

        // do all the movements
        foreach(PieceMovement movement in  movements)
        {
            movement.Do();
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

        // undo all the movements
        foreach (PieceMovement movement in movements)
        {
            movement.Undo();
        }

        // remove/create pieces
        foreach (PieceType pieceCreated in piecesCreated)
        {
            pieceCreated.Uncreate();
        }
        foreach (Piece pieceRemoved in piecesRemoved)
        {
            board.AddPiece(pieceRemoved);
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
        isValid = board.ValidState;
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
        if (!board.ValidState )
        {
            Undo();
            return false;
        }
        return true;
    }

    ///////////////////////// coordinate identifiers ///////////////////////////
    private Coor _mainStartCoor = null;
    private Coor _mainEndCoor = null;

    // mainStart/EndCoor are determined by the first 
    //   two coordinates that are referenced in 
    //   creation methods
    public Coor mainStartCoor
    {  get { return _mainStartCoor; } }

    public Coor mainEndCoor 
    { get { return _mainEndCoor; } }



    // checks for testCoor will be used as a main coordiante, 
    // assigns to mainStartCoor and mainEndCoor as needed
    private void checkForCoordinateIdentifier( Coor testCoor )
    {
        // mainStartCoor will be the first checked coordinate
        if( _mainStartCoor == null )
        {
            _mainStartCoor = testCoor;
        }
        // mainEndCoor will be the second checked coordinate
        else if( _mainEndCoor == null )
        {
            _mainEndCoor = testCoor;
        }
        // any other coordinate is not a main coordinate
    }

    public override string ToString()
    {
        return mainStartCoor + " -> " + mainEndCoor;
    }
}
