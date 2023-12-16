using System.Collections;
using System.Collections.Generic;
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
    public abstract List<Turn> GetTurns();

    // return legal turns that involve this
    public List<Turn> GetLegalTurns()
    {
        // fail if not active or not owner's turn
        // TODO (before implementing check):
        //     this block needs to be added to GetTurns
        if (!isActive || !ReferenceEquals(board.playerTurn, owner))
        {
            return new List<Turn>();
        }

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

        // calcuate the color
        physicalGO.GetComponent<Renderer>().material.color = owner.color;
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

        // iterate until non-empty position
        while (board.IsEmpty(testCoor))
        {
            // add the coordinate to the moves
            turns.Add(new Turn(board, start, testCoor, this));

            testCoor = testCoor + stepSize;
        }

        // apply contactEnemyCode
        Coor blockingCoor = testCoor;
        switch(contactEnemyCode)
        {
            // case EXCLUDE: don't do anything more
            case ON_CONTACT_ENEMY.EXCLUDE:
                break;

            // case REMOVE_ENEMY: move to position destroying enemy
            case ON_CONTACT_ENEMY.REMOVE_ENEMY:
                // check blocked by enemy
                Piece enemy = board.GetEnemy(blockingCoor);
                if (enemy != null)
                {
                    // create turn at blockingCoor,
                    // remove the enemy at that coor
                    Turn turn = new Turn(board, start, blockingCoor, this);
                    turn.RemovePiece(enemy);
                    turns.Add(turn);
                }
                break;

            // case not accounted for: warn user
            default:
                Debug.Log("Warning: GetTurnsByLinearMovement " +
                              "was given an unrecognized contactEnemyCode");
                break;
        }

        return turns;
    }
}

