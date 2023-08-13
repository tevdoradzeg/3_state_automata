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

    // Initialization with custom size, sets all states to 0
    public SimBoard (int size){
        numBoard = new int[size, size];
        for (int i = 0; i < size; i++){
            for (int j = 0; j < size; j++){
                numBoard[i, j] = 0;
            }
        }
        n = size;
    }

    public void ApplyModRule() {
        int[,] newBoardH = new int[n,n];

        for (int i = 0; i < n; i++){
            for (int j = 0; j < n; j++){
                newBoardH[i,j] = ModHorizontal(i, j);
            }
        }
        
        numBoard = newBoardH;
        int[,] newBoardV = new int[n,n];

        for (int i = 0; i < n; i++){
            for (int j = 0; j < n; j++){
                newBoardV[i,j] = ModVertical(i, j);
            }
        }

        numBoard = newBoardV;
    }

    int ModHorizontal(int i, int j){
        int left = j - 1;
        if (left < 0){
            left = n - 1;
        }

        int right = j + 1;
        if (right >= n){
            right = 0;
        }

        return (numBoard[i, j] + numBoard[i, left] + numBoard[i, right]) % 3;
    }

    int ModVertical(int i, int j){
        int up = i - 1;
        if (up < 0){
            up = n - 1;
        }

        int down = i + 1;
        if (down >= n){
            down = 0;
        }

        return (numBoard[i, j] + numBoard[down, j] + numBoard[up, j]) % 3;
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
        
        int xIndex = ((int) xCord) / 25;
        int yIndex = ((int) yCord) / 25;

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
        int drawX  = inpX * 25;
        int drawY = inpY * 25;

        c.SetSourceColor(color);
        c.LineWidth = 1;
        c.Rectangle(x: drawX, y: drawY, width: 25, height: 25);
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
        numMatrix.ApplyModRule();
        fillBoard(numMatrix);
        QueueDraw();
    }
}


// Gtk class of the board window UI
class BoardWindow : Gtk.Window {
    DrawBoard mainBoard;
    bool playing = false;

    public BoardWindow(SimBoard nums) : base("3-State Automata") {
        Resize(700, 500);
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

        rightSide.Margin = 5;
        mainBox.PackStart(mainBoard, true, true, 0);
        mainBox.Add(rightSide);
        Add(mainBox);
        Timeout.Add(500, OnTimeout);
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
}

// Main execution loop of the program
class MainLoop {
    public static void Main(string[] args){
        Application.Init();

        SimBoard board = new SimBoard(20);
        BoardWindow main = new BoardWindow(board);
        main.Draw(board);
        // ControlWindow control = new ControlWindow();

        // control.ShowAll();
        main.ShowAll();
        Application.Run();
    }

}
