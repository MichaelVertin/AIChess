using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/* Threading:
create a queue of commands executed by the user:
    regularly check for a command is in the queue
        when the player has control of the board, 
        and execute it if another command is not running

    when the user 'selects' a command, 
        the command is stored onto the commandQueue

    when the user passes control or gains control, 
        the queue automatically empties
        NOTE: need to ensure commands can't be placed on Queue 
              after user passes and regains control
               - do not know if Unity guarantees this
*/

public abstract class Command
{
    public abstract void Execute();
}

public class SelectCoordinate : Command
{
    UserPlayer owner;
    Coor coor;

    public SelectCoordinate(UserPlayer owner, Coor coor)
    {
        this.owner = owner;
        this.coor = coor;
    }

    public override void Execute()
    {
        owner.ExecuteSelectCoordinate(coor);
    }
}

public class UndoTurn : Command
{
    UserPlayer owner;
    Board board;

    public UndoTurn(UserPlayer owner, Board board)
    {
        this.owner = owner;
        this.board = board;
    }

    public override void Execute()
    {
        if (!ReferenceEquals(board.playerInControl, this.owner))
        {
            Debug.Log("Attempted to Undo when not in control");
            return;
        }

        if (board.history.Count > 0)
        {
            board.history.Peek().Undo();
            while (board.history.Count > 0 &&
                    !ReferenceEquals(board.playerInControl, board.playerTurn)
                  )
            {
                board.history.Peek().Undo();
            }
        }

        owner.OnControlStart();
    }
}


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

public class UserPlayer : Player_Chess
{
    private List<Coor> highlightedCoors = new List<Coor>();
    private UserOptionsMap options;
    private PieceMenu menu = null;
    
    // list of commands from user
    Queue<Command> commands = new Queue<Command>();

    // the last selected valid startMainCoor
    //   (if none, selectedCoor is null)
    private Coor selectedCoor = null;

    public UserPlayer(int id, Board board) : base(id, board)
    {
        
    }

    // called when given control of the Board
    public override void OnControlStart()
    {
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

        commands = new Queue<Command>();
        Unselect();
    }

    public override void Update()
    {
        // execute all commands
        // NOTE: when a command executes, 
        //       commands may empty
        Command command;
        while( commands.TryDequeue(out command) )
        {
            command.Execute();

            // update the physical board
            board.UpdatePhysical();
        }
    }

    // called when turns ended, 
    // does a random turn before passing control
    public void DoTurn(Turn turn)
    {
        Unselect();
        turn.Do();
        commands = new Queue<Command>();
        board.PassControl();
    }

    public void ExecuteSelectCoordinate(Coor coor)
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
    public void Unselect()
    {
        selectedCoor = null;
        if( menu != null )
        {
            menu.Destroy();
        }
        unhighlightAll();
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
            foreach (Turn turn in turns)
            {
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
            DoTurn(turns[0]);
        }
        // otherwise if there are multiple turns, 
        //    have the user make a decision
        else
        {
            // create the menu, allow user to select turns
            if( menu == null )
            {
                menu = board.InstantiatePieceMenu();
                menu.Init(turns, this);
                return true;
            }

            // this code is ran when a user selects the final coordinate of 
            // promotion when that coordinate has already been selected
            return false;
        }

        return true;

    }

    // highlights the specified coordinate
    private void highlightCoor( Coor coor )
    {
        Position pos = board.GetPosition( coor );
        if( pos != null )
        {
            pos.AddColor(new Color(1f, 1f, 1f));
            highlightedCoors.Add(coor);
        }
    }

    // unhightlights all specified coordinates
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

    // called when a piece option is selected from menu
    public void OnPieceOptionSelect(Turn turn)
    {
        Unselect();
        DoTurn(turn);
    }

    // called when undo button pressed
    //  - undoes turns until this player is in control again
    public override void OnUndoButtonSelect()
    {
        commands.Enqueue(new UndoTurn(this, this.board));
    }

    public override void OnSelectCoordinate(Coor coor)
    {
        // add a command to select the coordinate
        commands.Enqueue(new SelectCoordinate(this, coor));
    }
}

