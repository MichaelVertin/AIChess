using System.Collections;
using System.Collections.Generic;
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
    public Player owner; // owner of the piece
    private GameObject physicalGO = null; // reference to physical piece 
                                          //     seen by user
    public int turnCount = 0;

    // prefab to create physicalGO
    public abstract GameObject physicalPrefab
    {
        get;
    }

    public void Init( Board board, Coor coor, Player owner )
    {
        this.board = board;
        this.coor = coor;
        this.owner = owner;
        isActive = false;
    }

    // return turns that involve this
    public List<Turn> GetTurns()
    {
        // fail if not active or not owner's turn
        if (!isActive || !ReferenceEquals(board.playerTurn, owner))
        {
            return new List<Turn>();
        }

        return GetPieceTurns();
    }

    public abstract List<Turn> GetPieceTurns();

    // return legal turns that involve this
    public List<Turn> GetLegalTurns()
    {
        // TODO: implement check rules here
        // return all available turns
        return GetTurns();
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
    
    protected bool AddTurnByPosition(List<Turn> turns, Coor start, Coor end, 
                   ON_CONTACT_ENEMY contactEnemyCode = ON_CONTACT_ENEMY.EXCLUDE)
    {
        // move to the position if empty
        if( board.IsEmpty(end) )
        {
            turns.Add(new Turn(board, start, end, this));
            return true;
        }

        // remove enemy if specified
        if( contactEnemyCode == ON_CONTACT_ENEMY.REMOVE_ENEMY )
        {
            // remove the enemy if applicable
            Piece enemy = board.GetEnemy(end);
            if( enemy != null )
            {
                Turn turn = new Turn(board, start, end, this);
                turn.RemovePiece(enemy);
                turns.Add(turn);
                return true;
            }
        }
        return false;
    }
}

