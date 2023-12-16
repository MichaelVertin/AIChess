using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    private Board board;
    public Coor coor; // position of piece on Board
    public bool isActive; // is the piece on the board? 
    public Player owner; // owner of the piece
    private GameObject physicalGO = null; // reference to physical piece 
                                          //     seen by user
    public GameObject physicalPrefab; // prefab to create physicalGO

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
        // initialize list of turns
        List<Turn> turns = new List<Turn>();

        // fail if not active or not owner's turn
        if (!isActive || !ReferenceEquals(board.playerTurn,owner))
        {
            return turns;
        }

        // returns all positions around the object
        /*
         * If the position contains an enemy, 
         *     removes the enemy from the board
         * If the position contains a friend (including self), 
         *     does not include the position
         */
        for (int xAdj = -1; xAdj <= 1; xAdj++)
        {
            for (int yAdj = -1; yAdj <= 1; yAdj++)
            {
                // calculate the testCoor
                Coor testCoor = new Coor(coor.x + xAdj, coor.y + yAdj);
                Piece enemyPiece = board.GetEnemy(testCoor);
                bool isEmpty = board.IsEmpty(testCoor);

                // only proceed if isEmpty of an enemyPiece
                if (isEmpty || enemyPiece != null)
                {
                    // create the turn object
                    Turn turn = new Turn(board, this.coor, testCoor, this);

                    // remove the enemy piece
                    turn.RemovePiece(enemyPiece);

                    turns.Add(turn);
                }
            }
        }

        return turns;
    }

    // return legal turns that involve this
    public List<Turn> GetLegalTurns()
    {
        // return all available turns
        List<Turn> turns = GetTurns();
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
            Destroy(physicalGO);
            physicalGO = null;
        }

        // create the updated physical GameObject
        // no physical object if inactive
        if( !isActive )
        {
            return;
        }

        // instantiate the physical object
        physicalGO = Instantiate<GameObject>(physicalPrefab,board.transform);
        
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
}
