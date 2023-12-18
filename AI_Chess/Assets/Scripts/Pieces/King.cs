using System.Collections;
using System.Collections.Generic;
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
            foreach( int xRookCoor in new List<int> { 0, GAME_SETTINGS.BOARD_WIDTH})
            {
                bool allCoordinatesEmpty = true;
                // iterate between rook and king's x-components (exclusive)
                for( int emptyX = Mathf.Min(xRookCoor,this.coor.x) + 1;
                         emptyX < Mathf.Max(xRookCoor,this.coor.x);
                                emptyX++)
                {
                    // skip the rook if the coordinate is not empty
                    if( !board.IsEmpty(new Coor(emptyX,this.coor.y)))
                    {
                        allCoordinatesEmpty = false;
                        break;
                    }
                }

                if( allCoordinatesEmpty )
                {
                    Coor rookCoor = new Coor(xRookCoor, this.coor.y);
                    Piece rookPiece = board.GetFriend(rookCoor);

                    if( rookPiece != null && rookPiece.turnCount == 0 )
                    {
                        Coor endCoor = new Coor(this.coor.x + System.Math.Sign(rookCoor.x - this.coor.x) * 2, this.coor.y);
                        AddTurnByPosition(turns, this.coor, rookCoor);
                    }
                }
            }

        }

        return turns;
    }

}
