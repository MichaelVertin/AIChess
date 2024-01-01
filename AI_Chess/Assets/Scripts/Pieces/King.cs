using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class King : Piece
{
    // prefab to create physicalGO
    public override GameObject physicalPrefab
    {
        get
        {
            return StaticPrefabs.KING_PREFAB[owner.id];
        }
    }

    public override bool isKing
    {
        get
        {
            return true;
        }
    }


    public override List<Turn> GetPieceTurns()
    {
        // 1 in any direction
        List<Coor> moves = new List<Coor>
        {
            new Coor(1,1),
            new Coor(1,0),
            new Coor(1,-1),
            new Coor(0,-1),
            new Coor(-1,-1),
            new Coor(-1,0),
            new Coor(-1,1),
            new Coor(0,1),
        };

        // do all moves, remove enemy if necessary
        List<Turn> turns = new List<Turn>();
        foreach (Coor move in moves)
        {
            AddTurnByPosition(turns, this.coor, this.coor + move,
                                     ON_CONTACT_ENEMY.REMOVE_ENEMY);
        }


        ////////////////////// castling /////////////////////
        /*
         * Only check if never moved:
         *     check the friend piece on the right/leftmost coordinate hasn't moved
         *         create turn at that position
         */
        if( this.turnCount == 0 )
        {
            // check for left/right castle
            foreach( int xDir in new List<int> { -1, 1 } )
            {
                // iterate from the king to the next blocking position
                Coor coorIter = new Coor(this.coor);
                coorIter.x += xDir;
                while( board.IsEmpty(coorIter ) )
                {
                    coorIter.x += xDir;
                }

                // check stopped at friendly rook that hasn't moved yet
                Piece rook = board.GetFriend(coorIter);
                if( ( coorIter.x == 0 || coorIter.x == GAME_SETTINGS.BOARD_WIDTH - 1 ) && 
                    rook != null && rook.turnCount == 0 )
                {
                    // store rook/king start
                    Coor rookStart = coorIter;
                    Coor kingStart = this.coor;

                    // king moves 2 in the x direction
                    // rook moves 1 in opposite x direction from king's end coordinate
                    Coor kingEnd = new Coor(this.coor.x + xDir * 2, this.coor.y);
                    Coor rookEnd = new Coor(kingEnd.x - xDir, kingEnd.y);

                    Turn turn = new Turn(board);
                    turn.AddMovement(this, kingStart, kingEnd);
                    turn.AddMovement(rook, rookStart, rookEnd);
                    turns.Add(turn);
                }

            }
        }

        return turns;
    }

    public override string PieceType()
    {
        return "king";
    }


}
