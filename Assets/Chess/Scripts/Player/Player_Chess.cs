using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public abstract class Player_Chess
{
    public int id;
    public Coor direction;
    public int promotionY;
    public int baseY;
    protected Board board;

    public Player_Chess( int id, Board board )
    {
        this.id = id;
        this.board = board;

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
    public abstract void OnControlStart();

    public virtual void OnSelectCoordinate(Coor coor)
    {
        Debug.Log("Clicked " + coor + " not handled by player");
    }

    // called when Undo Button is selected
    // default: no response to button
    //          can be overridden in subclasses (UserPlayer)
    public virtual void OnUndoButtonSelect()
    {
        Debug.Log("WARNING: Player did not respond to Undo Button\n");
    }

    public virtual void Update()
    {

    }
}
