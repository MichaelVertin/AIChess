using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Coor_Turn_Pair
{
    public Turn turn;
    public Coor coor;
    public Coor_Turn_Pair(Coor coor, Turn turn)
    {
        this.turn = turn;
        this.coor = coor;
    }
}


// map of start/end coor to list of applicable turns
public class UserOptionsMap
{
    Dictionary<Coor, Dictionary<Coor,List<Turn>>> options =
               new Dictionary<Coor, Dictionary<Coor, List<Turn>>>();

    public List<Turn> this[Coor start, Coor end]
    {
        get
        {
            Ensure(start, end);
            return options[start][end];
        }
    }

    public List<Turn> this[Coor start]
    {
        get
        {
            List<Turn> turns = new List<Turn>();
            Ensure(start);
            foreach(KeyValuePair<Coor, List<Turn>> pair in options[start] )
            {
                turns.AddRange(pair.Value);
            }
            return turns;
        }
    }

    private void Ensure( Coor start, Coor end )
    {
        Ensure(start);
        if( !options[start].ContainsKey(end))
        {
            options[start][end] = new List<Turn>();
        }
    }

    private void Ensure(Coor start)
    {
        if (!options.ContainsKey(start))
        {
            options[start] = new Dictionary<Coor, List<Turn>>();
        }
    }

    public void Add(Turn turn)
    {
        this[turn.mainStartCoor, turn.mainEndCoor].Add(turn);
    }

    public bool StartsWith(Coor start)
    {
        return options.ContainsKey(start);
    }
}

public class UserPlayer : Player
{
    private Board board;
    private List<Coor> highlightedCoors = new List<Coor>();
    UserOptionsMap options;

    // the last selected valid startMainCoor
    //   (if none, selectedCoor is null)
    private Coor selectedCoor = null;

    public UserPlayer(int id) : base(id)
    {
        
    }

    // called when given control of the Board
    public override void OnControlStart(Board board)
    {
        this.board = board;
        List<Turn> turns = new List<Turn>();
        foreach (Piece piece in board.pieces)
        {
            turns.AddRange(piece.GetLegalTurns());
        }

        // add turn data into options
        options = new UserOptionsMap();
        foreach ( Turn turn in turns)
        {
            options.Add(turn);
        }

        Unselect();
    }

    // called when turns ended, 
    // does a random turn before passing control
    public void DoTurn(Turn turn)
    {
        turn.Do();
        board.PassControl();
    }

    public override void OnSelectCoordinate(Coor coor)
    {
        // first try to select the final coordinate
        if( SelectFinal(coor) )
        {
            return;
        }

        // if not able to select the final cooordinate, unselect everything
        Unselect();

        // try to select the start
        if( SelectStart(coor) )
        {
            return;
        }
    }

    // unselect the selected coordinate
    private void Unselect()
    {
        selectedCoor = null;
        unhighlightAll();
        Debug.Log("Unselected Piece");
    }

    // select the first main coordinate
    // returns if the coordinate was successfully selected
    private bool SelectStart(Coor coor)
    {
        // fail if a coordinate has already been selected
        if( selectedCoor != null )
        {
            return false;
        }

        List<Turn> turns = options[coor];

        // check the options would allow at least one turn
        if (turns.Count > 0)
        {
            // mark the coordinate as selected
            selectedCoor = coor;
            highlightCoor(coor);

            // notify the user that the piece was selected, 
            //   and display summary of turns
            Debug.Log("Selected Piece");
            foreach (Turn turn in turns)
            {
                Debug.Log(turn.mainStartCoor + " -> " + turn.mainEndCoor);
                highlightCoor(turn.mainEndCoor);
            }

            return true;
        }
        return false;
    }

    // selects the final coordinate and do the corresponding turn
    // returns false if unable to do the turn
    private bool SelectFinal(Coor coor)
    {
        // fail if a startCoor hasn't been selected
        if( selectedCoor == null)
        {
            return false;
        }

        List<Turn> turns = options[selectedCoor,coor];
        
        // fail if there are no turns
        if( turns.Count == 0 )
        {
            return false;
        }

        // if there is exactly one turn, 
        //    do the turn
        if (turns.Count == 1)
        {
            Debug.Log("Moving Piece");
            DoTurn(turns[0]);
        }
        // otherwise if there are multiple turns, 
        //    have the user make a decision
        else
        {
            // for now, mark as an ambigious decision
            Debug.Log("Ambiguous Decision");
            return false;
        }

        return true;

    }

    private void highlightCoor( Coor coor )
    {
        Position pos = board.GetPosition( coor );
        if( pos != null )
        {
            pos.AddColor(new Color(1f, 1f, 1f));
            highlightedCoors.Add(coor);
        }
    }

    private void unhighlightCoor( Coor coor )
    {
        Position pos = board.GetPosition(coor);
        if (pos != null)
        {
            pos.RemoveColor(new Color(1f, 1f, 1f));
            highlightedCoors.Remove(coor);
        }
    }

    private void unhighlightAll()
    {
        foreach(Coor coor in highlightedCoors)
        {
            Position pos = board.GetPosition(coor);
            if (pos != null)
            {
                pos.RemoveColor(new Color(1f, 1f, 1f));
            }
        }
        highlightedCoors = new List<Coor>();
    }
}

