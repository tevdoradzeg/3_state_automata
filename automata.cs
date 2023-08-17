using System.Collections.Generic;
using Gdk;
using Gtk;
using Cairo;
using Color = Cairo.Color;
using static Gdk.EventMask;
using Timeout = GLib.Timeout;


enum Colors {Green, Yellow, Black};

// Class used to store the states of the points as integers (0, 1 or 2)
class SimBoard {
    public int[,] numBoard;
    int n;
    Rules rules;

    // Initialization with custom size, sets all states to 0
    public SimBoard (int size){
        numBoard = new int[size, size];
        for (int i = 0; i < size; i++){
            for (int j = 0; j < size; j++){
                numBoard[i, j] = 0;
            }
        }
        n = size;
        rules = new Rules();
    }

    public void ApplyForestRule(){
        ApplyRule(rules.forestRuleH, rules.forestRuleV);
    }

    public void ApplyRule(Dictionary<(int, int, int), int> ruleH, Dictionary<(int, int, int), int> ruleV){
        int[,] newBoardH = new int[n,n];

        for (int i = 0; i < n; i++){
            for (int j = 0; j < n; j++){
                int left = ((j - 1) < 0) ? (n - 1) : (j - 1);
                int right = ((j + 1) >= n) ? 0 : (j + 1);

                newBoardH[i, j] = ruleH[(numBoard[i, left], numBoard[i, j], numBoard[i, right])];
            }
        }

        numBoard = newBoardH;
        int[,] newBoardV = new int[n,n];

        for (int i = 0; i < n; i++){
            for (int j = 0; j < n; j++){
                int up = ((i - 1) < 0) ? (n - 1) : (i - 1);
                int down = ((i + 1) >= n) ? 0 : (i + 1);

                newBoardV[i, j] = ruleV[(numBoard[up, j], numBoard[i, j], numBoard[down, j])];
            }
        }

        numBoard = newBoardV;
    }

    // public void ApplyModRule() {
    //     int[,] newBoardH = new int[n,n];

    //     for (int i = 0; i < n; i++){
    //         for (int j = 0; j < n; j++){
    //             newBoardH[i,j] = ModHorizontal(i, j);
    //         }
    //     }
        
    //     numBoard = newBoardH;
    //     int[,] newBoardV = new int[n,n];

    //     for (int i = 0; i < n; i++){
    //         for (int j = 0; j < n; j++){
    //             newBoardV[i,j] = ModVertical(i, j);
    //         }
    //     }

    //     numBoard = newBoardV;
    // }

    // int ModHorizontal(int i, int j){
    //     int left = ((j - 1) < 0) ? (n - 1) : (j - 1);
    //     int right = ((j + 1) >= n) ? 0 : (j + 1);

    //     return (numBoard[i, j] + numBoard[i, left] + numBoard[i, right]) % 3;
    // }

    // int ModVertical(int i, int j){
    //     int up = ((i - 1) < 0) ? (n - 1) : (i - 1);
    //     int down = ((i + 1) >= n) ? 0 : (i + 1);

    //     return (numBoard[i, j] + numBoard[down, j] + numBoard[up, j]) % 3;
    // }

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
        board = new ImageSurface(Format.Rgb24, 750, 750);
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
        if (xCord > 750 || yCord > 750){
            return true;
        }
        
        int xIndex = ((int) xCord) / 15;
        int yIndex = ((int) yCord) / 15;

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
        int drawX  = inpX * 15;
        int drawY = inpY * 15;

        c.SetSourceColor(color);
        c.LineWidth = 1;
        c.Rectangle(x: drawX, y: drawY, width: 15, height: 15);
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

    public void ApplyStep(){
        numMatrix.ApplyForestRule();
        fillBoard(numMatrix);
        QueueDraw();
    }
}


// Gtk class of the board window UI
class BoardWindow : Gtk.Window {
    DrawBoard mainBoard;
    bool playing = false;

    public BoardWindow(SimBoard nums) : base("3-State Automata") {
        Resize(950, 750);
        mainBoard = new DrawBoard(nums);
        Box mainBox = new Box(Orientation.Horizontal, 1);
        Box rightSide = new Box(Orientation.Vertical, 5);

        rightSide.Add(new Label("Control Panel"));

        Box radioBox = new Box(Orientation.Horizontal, 5);
        RadioButton g = new RadioButton("green");
        RadioButton y = new RadioButton("yellow");
        RadioButton b = new RadioButton("black");
        g.Clicked += OnGreen;
        y.Clicked += OnYellow;
        b.Clicked += OnBlack;
        radioBox.Add(g);
        radioBox.Add(y);
        radioBox.Add(b);
        rightSide.Add(radioBox);

        Button appplyStep = new Button("Apply Step");
        appplyStep.Clicked += OnStep;
        rightSide.Add(appplyStep);

        Box playBar = new Box(Orientation.Horizontal, 5);
        Button play = new Button();
        play.Clicked += OnPlay;
        play.Image = new Image("icons/play.png");
        playBar.Add(play);
        Button pause = new Button();
        pause.Clicked += OnPause;
        pause.Image = new Image("icons/pause.png");
        playBar.Add(pause);
        rightSide.Add(playBar);
        

        rightSide.Margin = 5;
        mainBox.PackStart(mainBoard, true, true, 0);
        mainBox.Add(rightSide);
        Add(mainBox);
        Timeout.Add(100, OnTimeout);
    }

    protected override bool OnDeleteEvent(Event e) {
        Application.Quit();
        return true;
    }

    public void Draw(SimBoard board){
        mainBoard.fillBoard(board);
    }

    void OnGreen(object? sender, EventArgs e){
        mainBoard.setColor(Colors.Green);
    }

    void OnYellow(object? sender, EventArgs e){
        mainBoard.setColor(Colors.Yellow);
    }

    void OnBlack(object? sender, EventArgs e){
        mainBoard.setColor(Colors.Black);
    }

    void OnStep(object? sender, EventArgs e){
        mainBoard.ApplyStep();
    }

    bool OnTimeout() {
        if (playing){
            mainBoard.ApplyStep();
        }
        return true;
    }

    void OnPlay(object? sender, EventArgs e){
        playing = true;
    }

    void OnPause(object? sender, EventArgs e){
        playing = false;
    }
}

// Main execution loop of the program
class MainLoop {
    public static void Main(string[] args){
        Application.Init();

        SimBoard board = new SimBoard(50);
        BoardWindow main = new BoardWindow(board);
        main.Draw(board);
        
        main.ShowAll();
        Application.Run();
    }

}

