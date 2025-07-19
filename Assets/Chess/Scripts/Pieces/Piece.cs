using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.VisualScripting;
using UnityEngine;

public enum ON_CONTACT_ENEMY
{
    EXCLUDE, REMOVE_ENEMY
}

public abstract class Piece
{
    protected Board board;
    public Coor coor; // position of piece on Board
    public bool isActive; // is the piece on the board? 
    public Player_Chess owner; // owner of the piece
    private GameObject physicalGO = null; // reference to physical piece 
                                          //     seen by user
    public int turnCount = 0;

    // prefab to create physicalGO
    public abstract GameObject physicalPrefab
    {
        get;
    }

    public abstract float Value
    {
        get;
    }

    public void Init( Board board, Coor coor, Player_Chess owner )
    {
        this.board = board;
        this.coor = coor;
        this.owner = owner;
        isActive = false;
    }



    /////////////////////// accessing turns of piece ///////////////////////////
    // return all turns that involve this
    // NOTE: some turns may not be legal
    public List<Turn> GetTurns(bool requirePlayerTurn = true)
    {
        // fail if not active or not owner's turn
        if ( !isActive )
        {
            return new List<Turn>();
        }

        if (requirePlayerTurn && !ReferenceEquals(board.playerTurn, owner))
        {
            return new List<Turn>();
        }

        return GetPieceTurns();
    }

    // method where subclass will inform Piece how it works
    public abstract List<Turn> GetPieceTurns();

    public virtual bool isKing
    {
        get { return false; }
    }

    // return legal turns that involve this
    public List<Turn> GetLegalTurns(bool requirePlayerTurn = true)
    {
        List<Turn> turns = new List<Turn>();

        // only select turns that can be verified
        foreach( Turn turn in GetTurns(requirePlayerTurn) )
        {
            if( turn.Verify() )
            {
                turns.Add( turn );
            }
        }
        // return all available turns
        return turns;
    }

    // update physical piece seen by user
    public void UpdatePhysical()
    {
        // calculate scale and piece offset
        float scale = 1f / GAME_SETTINGS.BOARD_WIDTH;
        float offset = -GAME_SETTINGS.BOARD_WIDTH / 2 + .5f;

        // destroy the previous physical GameObject
        if ( physicalGO != null)
        {
            Board.Destroy(physicalGO);
            physicalGO = null;
        }

        // create the updated physical GameObject
        // no physical object if inactive
        if( !isActive )
        {
            return;
        }

        // instantiate the physical object
        physicalGO = Board.Instantiate<GameObject>(physicalPrefab,board.transform);
        
        // calculate the position
        Vector2 position = new Vector2((float)coor.x, (float)coor.y);
        position.x += offset;
        position.y += offset;
        physicalGO.transform.position = position;

        // calculate the scale
        physicalGO.transform.localScale = new Vector2(scale,scale);
    }




    ////////////////////// tools to generate Turns /////////////////////////////
    
    // returns list of turns resulting from moving from start
    //                                to start+stepSize for each iteration
    // contactEnemyCode.EXCLUDE: stop before enemy
    // contactEnemyCode.REMOVE_ENEMY: destroy and include enemy position
    protected List<Turn> GetTurnsByLinearMovement(Coor start, Coor stepSize,
                   ON_CONTACT_ENEMY contactEnemyCode = ON_CONTACT_ENEMY.EXCLUDE)
    {
        List<Turn> turns = new List<Turn>();

        // start on first step
        Coor testCoor = start + stepSize;

        // create a turn on each non-enemy position as long as availble
        while (AddTurnByPosition(turns, start, testCoor, ON_CONTACT_ENEMY.EXCLUDE))
        {
            testCoor = testCoor + stepSize;
        }

        // create a turn for the blocking coordinate based on enemyCode
        Coor blockingCoor = testCoor;
        AddTurnByPosition(turns, start, blockingCoor, contactEnemyCode);

        return turns;
    }

    protected List<Turn> GetTurnsByLinearMovements(Coor start, List<Coor> directions,
                   ON_CONTACT_ENEMY contactEnemyCode = ON_CONTACT_ENEMY.EXCLUDE)
    {
        List<Turn> turns = new List<Turn>();
        foreach (Coor direction in directions)
        {
            turns.AddRange(GetTurnsByLinearMovement( this.coor, direction,
                                                     contactEnemyCode));
        }
        return turns;
    }
    
    public bool AddTurnByPosition(List<Turn> turns, Coor start, Coor end, 
                   ON_CONTACT_ENEMY contactEnemyCode = ON_CONTACT_ENEMY.EXCLUDE)
    {
        // move to the position if empty
        if( board.IsEmpty(end) )
        {
            Turn turn = new Turn(board);
            turn.AddMovement(this, start, end);
            turns.Add(turn);
            return true;
        }

        // remove enemy if specified
        if( contactEnemyCode == ON_CONTACT_ENEMY.REMOVE_ENEMY )
        {
            // remove the enemy if applicable
            Piece enemy = board.GetEnemy(end);
            if( enemy != null )
            {
                Turn turn = new Turn(board);
                turn.AddMovement(this, start, end);
                turn.RemovePiece(enemy);
                turns.Add(turn);
                return true;
            }
        }
        return false;
    }

    public override string ToString()
    {
        return "Active " + PieceType() + " at " + coor.ToString();
    }
    public abstract string PieceType();
}

