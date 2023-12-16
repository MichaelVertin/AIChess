using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TerrainUtils;
using UnityEngine.UIElements;


public enum BoardState
{
    GAME_OVER, INVALID, IN_PROGRESS
}

public class Position
{
    public Piece piece;
}

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
    public Coor(int xPar,int yPar)
    {
        x = xPar; y = yPar;
    }
    public Coor() { }

    public override string ToString()
    {
        return "(" + x + "," + y + ")";
    }

    // elementwise addition
    public static Coor operator+(Coor left, Coor right)
    {
        return new Coor( left.x + right.x, left.y + right.y );
    }

    // scalar multiplication
    public static Coor operator*(Coor left, int right)
    {
        return new Coor(left.x * right, left.y * right );
    }
}

public class Board : MonoBehaviour
{
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

    // state of the game (GAME_OVER, INVALID, IN_PROGRESS)
    public BoardState state
    {
        get { return BoardState.IN_PROGRESS; }
    }

    // pieces that are on the board
    public List<Piece> pieces
    {
        get
        {
            List<Piece> pieces = new List<Piece>();
            foreach( Piece piece in this._pieces )
            {
                if( piece.isActive )
                {
                    pieces.Add(piece);
                }
            }
            return pieces;
        }
    }
    
    
    void Start()
    {
        // initialize players
        for( int playerID = 0; playerID < GAME_SETTINGS.NUM_PLAYERS; playerID++ )
        {
            players.Add(new Player(playerID));
        }

        // initialize empty position objects
        for( int x = 0; x < GAME_SETTINGS.BOARD_WIDTH; x++ )
        {
            for( int y = 0; y < GAME_SETTINGS.BOARD_WIDTH; y++ )
            {
                positions[x, y] = new Position();
            }
        }


        // initialize pieces
        AddPiece(CreateBishop(new Coor(0, 0), players[0]));
        AddPiece(CreateBasic(new Coor(1, 1), players[1]));
        AddPiece(CreateBasic(new Coor(6, 6), players[0]));
        AddPiece(CreateBishop(new Coor(7, 7), players[1]));
        AddPiece(CreateBishop(new Coor(2, 0), players[0]));
        AddPiece(CreateBishop(new Coor(0, 2), players[1]));
        AddPiece(CreateBishop(new Coor(4, 4), players[0]));
        AddPiece(CreateBishop(new Coor(5, 5), players[1]));

        // set player values
        playerTurn = players[0];
        playerInControl = players[0];
        UpdatePhysical();

        // give control to the first player
        playerTurn.OnControlStart(this);
    }

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

    public void UpdatePhysical()
    {
        // update all pieces in the board (include inactive)
        foreach( Piece piece in _pieces )
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
        players[playerInControl.id].OnControlStart(this);
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
        int playerID = playerTurn.id;
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
        piece.isActive = false;

        return piece;
    }

    public Piece CreateBishop( Coor newPieceCoor, Player owner )
    {
        Piece piece = new Bishop();
        return CreatePiece(newPieceCoor, owner, piece);
    }

    public Piece CreatePawn(Coor newPieceCoor, Player owner)
    {
        Piece piece = new Pawn();
        return CreatePiece(newPieceCoor, owner, piece);
    }

    public Piece CreateBasic(Coor newPieceCoor, Player owner)
    {
        Piece piece = new Basic();
        return CreatePiece(newPieceCoor, owner, piece);
    }

    // destroy the specified the piece
    public void DestroyPiece( Piece piece )
    {
        GetPosition(piece.coor).piece = null;
        _pieces.Remove(piece);
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
}
