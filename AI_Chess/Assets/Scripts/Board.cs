using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

static class GAME_SETTINGS
{
    public const int NUM_PLAYERS = 2;
    public const int BOARD_WIDTH = 8;
}


public class Coor
{
    // able to access with values directly
    public int x;
    public int y;
    public Coor(int xPar, int yPar)
    {
        x = xPar; y = yPar;
    }
    public Coor(Coor coor)
    {
        x = coor.x;
        y = coor.y;
    }
    public Coor() { }

    public override string ToString()
    {
        return "(" + x + "," + y + ")";
    }

    // elementwise addition
    public static Coor operator +(Coor left, Coor right)
    {
        return new Coor(left.x + right.x, left.y + right.y);
    }

    // scalar multiplication
    public static Coor operator *(Coor left, int right)
    {
        return new Coor(left.x * right, left.y * right);
    }

    public override bool Equals(System.Object obj)
    {
        Coor testcoor = obj as Coor;
        return testcoor.x == x && testcoor.y == y;
    }

    public override int GetHashCode()
    {
        return x + y + x ^ y;
    }
}

public class Board : MonoBehaviour
{
    public GameObject positionPrefab;

    // pieces on or off the board
    private List<Piece> _pieces = new List<Piece>();

    // positions on the board
    Position[,] positions = new Position[GAME_SETTINGS.BOARD_WIDTH, 
                                         GAME_SETTINGS.BOARD_WIDTH];

    // Stack of Turns made on the board
    public Stack<Turn> history = new Stack<Turn>();

    // list of players who can own pieces and control pieces
    List<Player> players = new List<Player>();

    // player whose pieces can be moved
    public Player playerTurn;

    // player who can move pieces
    public Player playerInControl;

    // pieces that are on the board
    public List<Piece> pieces
    {
        get
        {
            List<Piece> pieces = new List<Piece>();
            foreach( Piece piece in this._pieces )
            {
                if (piece.isActive)
                {
                    pieces.Add(piece);
                }
            }
            return pieces;
        }
    }


    /////////////////////////// Initialize game of chess ///////////////////////
    void Start()
    {
        // initialize players
        players.Add(new UserPlayer(0, this));
        players.Add(new AIPlayer(1, this));

        // initialize empty position objects
        for( int x = 0; x < GAME_SETTINGS.BOARD_WIDTH; x++ )
        {
            for( int y = 0; y < GAME_SETTINGS.BOARD_WIDTH; y++ )
            {
                positions[x, y] = Instantiate<GameObject>(positionPrefab,this.transform).GetComponent<Position>();
                positions[x, y].Init(this, new Coor(x, y));
            }
        }

        Player player1 = players[0];
        Player player2 = players[1];

        // initialize pieces
        AddPiece(Create<King>(new Coor(3, 0), player1));
        AddPiece(Create<Rook>(new Coor(0, 0), player1));
        AddPiece(Create<Rook>(new Coor(7, 0), player1));
        AddPiece(Create<Knight>(new Coor(1, 0), player1));
        AddPiece(Create<Knight>(new Coor(6, 0), player1));
        AddPiece(Create<Bishop>(new Coor(2, 0), player1));
        AddPiece(Create<Bishop>(new Coor(5, 0), player1));
        AddPiece(Create<Queen>(new Coor(4, 0), player1));

        AddPiece(Create<King>(new Coor(4, 7), player2));
        AddPiece(Create<Rook>(new Coor(0, 7), player2));
        AddPiece(Create<Rook>(new Coor(7, 7), player2));
        AddPiece(Create<Knight>(new Coor(1, 7), player2));
        AddPiece(Create<Knight>(new Coor(6, 7), player2));
        AddPiece(Create<Bishop>(new Coor(2, 7), player2));
        AddPiece(Create<Bishop>(new Coor(5, 7), player2));
        AddPiece(Create<Queen>(new Coor(3, 7), player2));

        for ( int pawnX = 0; pawnX < GAME_SETTINGS.BOARD_WIDTH; pawnX++ )
        {
            AddPiece(Create<Pawn>(new Coor(pawnX, 1), player1));
            AddPiece(Create<Pawn>(new Coor(pawnX, 6), player2));
        }

        // set player values
        playerTurn = players[0];
        playerInControl = players[0];
        UpdatePhysical();

        // give control to the first player
        playerTurn.OnControlStart();
    }


    /////////////////////////// Get Coordinate Info ////////////////////////////
    // returns a friend piece on testCoor, 
    // if no friend piece, returns null
    public Piece GetFriend(Coor testCoor)
    {
        Piece piece = GetPiece(testCoor);
        // fail if piece doesn't exist
        if (piece == null)
        {
            return null;
        }
        // fail if piece is not the turn's player
        if (piece.owner != playerTurn)
        {
            return null;
        }
        // otherwise, return the piece
        return piece;
    }

    // returns an enemy piece on testCoor, 
    // if no enemy piece, returns null
    public Piece GetEnemy(Coor testCoor)
    {
        Piece piece = GetPiece(testCoor);
        // fail if piece doesn't exist
        if ( piece == null )
        {
            return null;
        }
        // fail if piece is the turn's player
        if ( piece.owner == playerTurn )
        {
            return null;
        }
        return piece;
    }

    // returns a piece on testCoor, 
    // if no piece, returns null
    public Piece GetPiece(Coor testCoor)
    {
        Position pos = GetPosition(testCoor);
        // fail if the position doesn't exist
        if (pos == null)
        {
            return null;
        }
        Piece piece = pos.piece;
        // fail if piece is not active
        if( piece == null || !piece.isActive)
        {
            return null;
        }
        // otherwise, return the piece
        return piece;
    }

    // returns if testCoor is empty
    // (returns false if not on the board)
    public bool IsEmpty(Coor testCoor)
    {
        Position pos = GetPosition(testCoor);
        // fail if the position doesn't exist
        if( pos == null )
        {
            return false;
        }
        // otherwise, return the piece doesn't exist
        return pos.piece == null;
    }

    // returns a position on the board, 
    //    return null if no position at testCoor
    public Position GetPosition(Coor testCoor)
    {
        try
        {
            return positions[testCoor.x, testCoor.y];
        }
        catch(IndexOutOfRangeException)
        {
            return null;
        }
    }


    // update Monobehaviours seen by the user
    public void UpdatePhysical()
    {
        // update all pieces in the board (include inactive)
        foreach ( Piece piece in _pieces )
        {
            piece.UpdatePhysical();
        }
    }

    // pass control to the next player
    public void PassControl()
    {
        // update the physical board for the player to observe
        UpdatePhysical();

        // identify the next player in control
        playerInControl = players[(playerInControl.id + 1) % GAME_SETTINGS.NUM_PLAYERS];

        // notify the player they are in control
        players[playerInControl.id].OnControlStart();
    }

    // switch to the next player
    public void NextPlayerTurn()
    {
        int playerID = (playerTurn.id + 1) % GAME_SETTINGS.NUM_PLAYERS;
        playerTurn = players[playerID];
    }

    // switch to the previous player
    public void PrevPlayerTurn()
    {
        int playerID = playerTurn.id - 1;
        if (playerID < 0)
        {
            playerID = GAME_SETTINGS.NUM_PLAYERS - 1;
        }
        playerTurn = players[playerID];
    }


    // create object at newPieceCoor under owner's control
    private Piece CreatePiece( Coor newPieceCoor, Player owner, Piece piece )
    {
        // create an instance of the newPiecePrefab, then initialize
        piece.Init(this, newPieceCoor, players[owner.id]);

        // add to the list of pieces, start inactive
        _pieces.Add(piece);
        piece.isActive = false; // TODO: keeps piece in pieces, uses isActive to determine if still active
                                //       want to remove isActive status, handle in history

        return piece;
    }

    // create piece at newPieceCoor with the specified owner
    // PieceType is the Piece subclass to be created
    // NOTE: after if is created, must be 'AddPiece'
    //       before it becomes active on the board
    public Piece Create<PieceType>(Coor newPieceCoor, Player owner)
                     where PieceType : Piece, new()
    {
        Piece piece = new PieceType();
        return CreatePiece(newPieceCoor, owner, piece);
    }


    // destroy the specified the piece
    public void DestroyPiece( Piece piece )
    {
        RemovePiece(piece);
        _pieces.Remove(piece);
        piece.UpdatePhysical();
    }

    // add the specified piece to the board
    public void AddPiece( Piece piece )
    {
        // store the piece in its position
        GetPosition(piece.coor).piece = piece;

        // set to active
        piece.isActive = true;
    }

    // remove the specified piece from the board
    public void RemovePiece( Piece piece )
    {
        // remove the piece from its position
        GetPosition(piece.coor).piece = null;

        // set to inactive
        piece.isActive = false;
    }

    // notify player in control when a coordinate is clicked
    public void OnSelectCoordinate(Coor coor)
    {
        playerInControl.OnSelectCoordinate(coor);
    }

    public List<Turn> GetLegalTurns()
    {
        List<Turn> turns = new List<Turn>();
        foreach (Piece piece in pieces)
        {
            turns.AddRange(piece.GetLegalTurns());
        }
        return turns;
    }

    public List<Turn> GetTurns()
    {
        List<Turn> turns = new List<Turn>();
        foreach (Piece piece in pieces)
        {
            turns.AddRange(piece.GetTurns());
        }
        return turns;
    }


    ////////////////////////////// Board State /////////////////////////////////
    
    // return if the game is over
    public bool GameOver
    {
        get
        {
            return GetLegalTurns().Count == 0;
        }
    }

    // returns if any king can be taken
    public bool ValidState
    {
        get
        {
            // not two kings on the board: invalid state
            foreach (Turn turn in GetTurns())
            {
                turn.Do();
                int kingCount = 0;
                foreach (Piece piece in pieces)
                {
                    if (piece.isKing)
                    {
                        kingCount++;
                    }
                }
                turn.Undo();
                if (kingCount < GAME_SETTINGS.NUM_PLAYERS)
                {
                    return false;
                }
            }
            return true;
        }
    }

    // undoes at least one turn, ending on the controller's last turn
    // after turns are done, signals the start of that player's turn
    public void UndoButton()
    {
        if( history.Count > 0 )
        {
            history.Peek().Undo();
            while ( history.Count > 0 && 
                    !ReferenceEquals( playerInControl, playerTurn)
                  )
            {
                history.Peek().Undo();
            }
        }

        UpdatePhysical();
        playerInControl.OnControlStart();
    }

    // creates a piece menu
    public PieceMenu InstantiatePieceMenu()
    {
        PieceMenu menu = Instantiate<GameObject>(StaticPrefabs.PIECE_OPTIONS_PREFAB, this.transform).GetComponent<PieceMenu>();
        return menu;
    }

    public override string ToString()
    {
        string str = "Board Display:\n";
        foreach( Piece piece in pieces )
        {
            str += piece.ToString() + ";\n";
        }
        return str;
    }
}
