using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Position : MonoBehaviour
{
    public Piece piece;
    public Coor coor;
    public Board board;

    public void Init( Board board, Coor coor )
    {
        this.board = board;
        this.coor = coor;
        float scale = 1f / (float)GAME_SETTINGS.BOARD_WIDTH;
        this.transform.localScale = new Vector2(scale, scale);
        float offset = -.5f * (float)GAME_SETTINGS.BOARD_WIDTH + .5f;
        Vector2 relativePosition = new Vector2(this.coor.x, this.coor.y);
        this.transform.position = relativePosition + new Vector2(offset, offset);
    }

    public void OnMouseDown()
    {
        board.OnSelectCoordinate(coor);
    }
}
