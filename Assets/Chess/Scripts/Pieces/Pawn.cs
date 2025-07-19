using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Pawn : Piece
{
    // value of the piece (for AIPlayer)
    public override float Value
    {
        get
        {
            return 1;
        }
    }

    // prefab to create physicalGO
    public override GameObject physicalPrefab
    {
        get
        {
            return StaticPrefabs.PAWN_PREFAB[owner.id];
        }
    }


    public override List<Turn> GetPieceTurns()
    {
        List<Turn> turns = new List<Turn>();
        Coor right = new Coor(1, 0);
        Coor left = new Coor(-1, 0);

        // set direction up if id0, otherwise, direction downwards
        Coor direction = owner.direction;

        // try to move one up, excluding enemy
        if(AddAndCheckForPromotion(turns, this.coor, this.coor + direction,
                                   ON_CONTACT_ENEMY.EXCLUDE))
        {
            // if first time moving, also try to move two up (also exluding enemy)
            if( turnCount == 0 )
            {
                Coor endCoor = this.coor + direction * 2;
                AddTurnByPosition(turns, this.coor, endCoor,
                         ON_CONTACT_ENEMY.EXCLUDE);
            }
        }

        Coor forwardRight = this.coor + direction + right;
        // if any enemy is in the direction to the right, 
        //     get the turn removing the enemy
        if (board.GetEnemy(forwardRight) != null)
        {
            AddAndCheckForPromotion(turns, this.coor, forwardRight,
                                     ON_CONTACT_ENEMY.REMOVE_ENEMY);
        }

        Coor forwardLeft = this.coor + direction + left;
        // if any enemy is in the direction to the left, 
        //     get the turn removing that enemy
        if (board.GetEnemy(forwardLeft) != null)
        {
            AddAndCheckForPromotion(turns, this.coor, forwardLeft,
                                     ON_CONTACT_ENEMY.REMOVE_ENEMY);
        }

        return turns;
    }

    private bool AddAndCheckForPromotion(List<Turn> turns, 
                                         Coor startCoor, 
                                         Coor endCoor,
                                         ON_CONTACT_ENEMY contactEnemyCode)
    {
        // first, try to add the promotion
        if( AddTurnByPosition(turns, startCoor, endCoor, contactEnemyCode) )
        {
            Turn turn = turns[turns.Count - 1];

            // check for a promotion
            if (owner.promotionY == endCoor.y)
            {
                // remove the created turn
                turns.RemoveAt(turns.Count - 1);

                // create each type of piece
                List<PieceType> createdPieces = new List<PieceType>();
                createdPieces.Add(new QueenType(endCoor, owner, board));
                createdPieces.Add(new RookType(endCoor, owner, board));
                createdPieces.Add(new BishopType(endCoor, owner, board));
                createdPieces.Add(new KnightType(endCoor, owner, board));

                // create the created piece and destroy the pawn 
                //    for each created piece
                foreach( PieceType createdPiece in createdPieces )
                {
                    turn = new Turn(board);
                    turn.RemovePiece(this);
                    turn.AddPiece(createdPiece);
                    Piece enemy = board.GetEnemy(endCoor);
                    if (board.GetEnemy(endCoor) != null)
                    {
                        turn.RemovePiece(enemy);
                    }
                    turns.Add(turn);
                }
            }

            return true;
        }

        return false;
    }
    public override string PieceType()
    {
        return "pawn";
    }
}
