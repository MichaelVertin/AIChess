using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Position : MonoBehaviour
{
    public Piece piece;
    public Coor coor;
    public Board board;

    List<Color> colors = new List<Color>();

    public Color defaultColor
    {
        get
        {
            if ((coor.x + coor.y) % 2 == 0)
            {
                return new Color(5f / 8f, 3f / 8f, 0);
            }
            return new Color(1f, 1f, .0625f);
        }
    }

    public void Init( Board board, Coor coor )
    {
        this.board = board;
        this.coor = coor;
        float scale = 1f / (float)GAME_SETTINGS.BOARD_WIDTH;
        this.transform.localScale = new Vector3(scale, scale, 1);
        float offset = -.5f * (float)GAME_SETTINGS.BOARD_WIDTH + .5f;
        Vector2 relativePosition = new Vector2(this.coor.x, this.coor.y);
        this.transform.position = relativePosition + new Vector2(offset, offset);
        Recolor();
    }

    public void OnMouseDown()
    {
        board.OnSelectCoordinate(coor);
    }

    // recolor the object
    private void Recolor()
    {
        // start with default color
        Color color = defaultColor;

        // if one or more colors are provided, 
        //    override with last provided
        if( colors.Count > 0 )
        {
            color = colors[colors.Count - 1];
        }

        // set the color into the renderer material
        GetComponent<Renderer>().material.color = color;
    }

    public void AddColor( Color color )
    {
        colors.Add( color );
        Recolor();
    }

    public void RemoveColor( Color color )
    {
        colors = new List<Color>();
        Recolor();
    }
}
