using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public abstract class Player
{
    public int id;
    public Coor direction;
    public int promotionY;
    public int baseY;

    public Player( int id )
    {
        this.id = id;

        // first player moves forward
        if( id == 0 )
        {
            direction = new Coor(0, 1);
            promotionY = GAME_SETTINGS.BOARD_WIDTH - 1;
            baseY = 0;
        }
        // other players move backward
        else
        {
            direction = new Coor(0, -1);
            promotionY = 0;
            baseY = GAME_SETTINGS.BOARD_WIDTH - 1;
        }
    }

    // called when given control of the Board
    public abstract void OnControlStart(Board board);
}
