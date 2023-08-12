using Gdk;
using Gtk;
using Cairo;
using Color = Cairo.Color;
using static Gdk.EventMask;

enum Colors {Green, Yellow, Black};


// Class used to store the states of the points as integers (0, 1 or 2)
class SimBoard {
    public int[,] numBoard;

    // Initialization with custom size, sets all states to 0
    public SimBoard (int size){
        numBoard = new int[size, size];
        for (int i = 0; i < size; i++){
            for (int j = 0; j < size; j++){
                numBoard[i, j] = 0;
            }
        }
    }

}

// Class used for the visual representation of the simulation board
class DrawBoard : DrawingArea {
    Color green = new Color(0, 1, 0),
    yellow = new Color (1, 1, 0),
    black = new Color(0, 0, 0);
    SimBoard numMatrix;
    Colors currColor;

    double xCord;
    double yCord;

    ImageSurface board;

    public DrawBoard(SimBoard inp) {
        board = new ImageSurface(Format.Rgb24, 500, 500);
        numMatrix = inp;
        currColor = Colors.Green;

        using (Context c = new Context(board)) {
            c.SetSourceColor(green);
            c.Paint();
        }

        AddEvents((int) (ButtonPressMask | ButtonReleaseMask));
    }

    protected override bool OnDrawn(Context c) {
        c.SetSourceSurface(board, 0, 0);
        c.Paint();
        
        return true;
    }
    
    protected override bool OnButtonPressEvent(EventButton e) {
        xCord = e.X;
        yCord = e.Y;
        if (xCord > 500 || yCord > 500){
            return true;
        }
        
        int xIndex = ((int) xCord) / 50;
        int yIndex = ((int) yCord) / 50;

        int fillInt;
        if (currColor == Colors.Green){
            fillInt = 0;
        } else if (currColor == Colors.Yellow){
            fillInt = 1;
        } else {
            fillInt = 2;
        }
        

        numMatrix.numBoard[xIndex, yIndex] = fillInt;
        fillBoard(numMatrix);

        QueueDraw();
        return true;
    }

    void fillCube(Context c, int inpX, int inpY, Color color) {
        int drawX  = inpX * 50;
        int drawY = inpY * 50;

        c.SetSourceColor(color);
        c.LineWidth = 1;
        c.Rectangle(x: drawX, y: drawY, width: 50, height: 50);
        c.Fill();
    }

    public void fillBoard(SimBoard InpBoard){
        Context c = new Context(board);
        int[,] matrix = InpBoard.numBoard;
        int n = matrix.GetLength(0);

        for (int i = 0; i < n; i++){
            for (int j = 0; j < n; j++){
                if (matrix[i, j] == 0){
                    fillCube(c, i, j, green);
                } else if (matrix[i, j] == 1){
                    fillCube(c, i, j, yellow);
                } else{
                    fillCube(c, i, j, black);
                }
            }
        }
    }

    public void setColor(Colors inp){
        currColor = inp;
    }
}


// Gtk class of the board window UI
class BoardWindow : Gtk.Window {
    DrawBoard mainBoard;

    public BoardWindow(SimBoard nums) : base("3-State Automata") {
        Resize(700, 500);
        mainBoard = new DrawBoard(nums);
        Box mainBox = new Box(Orientation.Horizontal, 5);
        Box rightSide = new Box(Orientation.Vertical, 5);

        rightSide.Add(new Label("Control Panel"));

        Box radioBox = new Box(Orientation.Horizontal, 5);
        RadioButton g = new RadioButton("green");
        RadioButton y = new RadioButton("yellow");
        RadioButton b = new RadioButton("black");
        g.Clicked += onGreen;
        y.Clicked += onYellow;
        b.Clicked += onBlack;
        radioBox.Add(g);
        radioBox.Add(y);
        radioBox.Add(b);

        rightSide.Add(radioBox);

        mainBox.PackStart(mainBoard, true, true, 0);
        mainBox.Add(rightSide);
        Add(mainBox);
    }

    protected override bool OnDeleteEvent(Event e) {
        Application.Quit();
        return true;
    }

    public void Draw(SimBoard board){
        mainBoard.fillBoard(board);
    }

    void onGreen(object? sender, EventArgs e){
        mainBoard.setColor(Colors.Green);
    }

    void onYellow(object? sender, EventArgs e){
        mainBoard.setColor(Colors.Yellow);
    }

    void onBlack(object? sender, EventArgs e){
        mainBoard.setColor(Colors.Black);
    }
}

// CODE NOT USED ***


// Gtk class of the control window UI (not in use)
// class ControlWindow : Gtk.Window {
//     public ControlWindow() : base("Control Window") {
//         Box mainBox = new Box(Orientation.Vertical, 5);
//         mainBox.Add(new Label("Test Label"));

//         Add(mainBox);
//     }

//     protected override bool OnDeleteEvent(Event e) {
//         Application.Quit();
//         return true;
//     }
// }

// ***

// Main execution loop of the program
class MainLoop {
    public static void Main(string[] args){
        Application.Init();

        SimBoard board = new SimBoard(10);
        BoardWindow main = new BoardWindow(board);
        main.Draw(board);
        // ControlWindow control = new ControlWindow();

        // control.ShowAll();
        main.ShowAll();
        Application.Run();
    }

}
