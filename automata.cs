using Gdk;
using Gtk;
using Cairo;
using Color = Cairo.Color;


// Class used to store the states of the points as integers (0, 1 or 2)
class SimBoard {
    int[,]? numBoard = null;

    // Initialization with custom size, sets all states to 0.
    public SimBoard (int size){
        numBoard = new int[size, size];
        for (int i = 0; i < size; i++){
            for (int j = 0; j < size; j++){
                numBoard[i, j] = 0;
            }
        }
    }

}

class DrawBoard : DrawingArea {
    Color green = new Color(0, 1, 0),
    yellow = new Color (1, 1, 0),
    black = new Color(0, 0, 0);

    protected override bool OnDrawn(Context c) {
        c.SetSourceColor(green);
        c.LineWidth = 1;

        c.Rectangle(x: 0, y: 0, width: 500, height: 500);
        c.Fill();

        return true;
    }

}


// Gtk class of the board window UI
class BoardWindow : Gtk.Window {
    public BoardWindow() : base("3-State Automata") {
        Resize(500, 500);
        DrawBoard mainBoard = new DrawBoard();
        Add(mainBoard);
    }

    protected override bool OnDeleteEvent(Event e) {
        Application.Quit();
        return true;
    }
}

class ControlWindow : Gtk.Window {
    public ControlWindow() : base("Control Window") {
        Box mainBox = new Box(Orientation.Vertical, 5);
        mainBox.Add(new Label("Test Label"));

        Add(mainBox);
    }

    protected override bool OnDeleteEvent(Event e) {
        Application.Quit();
        return true;
    }
}

// Main execution loop of the program
class MainLoop {
    public static void Main(string[] args){
        Application.Init();

        BoardWindow main = new BoardWindow();
        // ControlWindow control = new ControlWindow();

        // control.ShowAll();
        main.ShowAll();
        Application.Run();
    }

}
