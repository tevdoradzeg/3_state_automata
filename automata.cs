using System.Collections.Generic;
using Gdk;
using Gtk;
using Cairo;
using Color = Cairo.Color;
using static Gdk.EventMask;
using Timeout = GLib.Timeout;


// Enumerables storing the possible Colors and Rules picked by the User
enum Colors {Green, Yellow, Black};
enum RuleSet {Forest, Scroll, Stair};

// Class used to store the states of the points as integers (0, 1 or 2)
class SimBoard {
    // The int board
    public int[,] numBoard;
    // Size of one row
    int n;
    // A new copy of a Rules class objerct
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

    // Fills the board with the input integer
    public void IntFill (int k){
        for (int i = 0; i < n; i++){
            for (int j = 0; j < n; j++){
                numBoard[i,j] = k;
            }
        }
    }

    // Specific cases for ApplyRule()
    public void ApplyForestRule(){
        ApplyRule(rules.forestRuleH, rules.forestRuleV);
    }

    public void ApplyScrollRule(){
        ApplyRule(rules.scrollRule, rules.scrollRule);
    }

    public void ApplyStairRule(){
        ApplyRule(rules.H, rules.V);
    }

    // Applies one step of a rule specified by two dictionaries, the first one applied horizontally and the other vertically
    // The board state is changed once after the horizontal application and once after the vertical one.
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

}

// Class used for the visual representation of the simulation board
class DrawBoard : DrawingArea {
    // Color mappings
    Color green = new Color(0, 1, 0),
    yellow = new Color (1, 1, 0),
    black = new Color(0, 0, 0);
    // An variable of the SimBoard custom class
    SimBoard numMatrix;
    // Current color in use
    Colors currColor;
    // Current rule in use 
    RuleSet currRule;

    double xCord;
    double yCord;

    // Cairo ImageSurface object which represents the clickable/drawable surface
    ImageSurface board;

    // Initialization method which specifies the size and default color/rule of the board
    public DrawBoard(SimBoard inp) {
        board = new ImageSurface(Format.Rgb24, 750, 750);
        numMatrix = inp;
        currColor = Colors.Green;
        currRule = RuleSet.Forest;

        using (Context c = new Context(board)) {
            c.SetSourceColor(green);
            c.Paint();
        }

        // Adds OnButtonPress event
        AddEvents((int) (ButtonPressMask | ButtonReleaseMask));
    }

    // Cairo method which is called automatically after QueueDraw()
    protected override bool OnDrawn(Context c) {
        c.SetSourceSurface(board, 0, 0);
        c.Paint();
        
        return true;
    }
    
    // Method which is called after pressing the board area
    // Switches the state of the number matrix and updates the visuals
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
        FillBoard(numMatrix);

        QueueDraw();
        return true;
    }

    // Fills a singular cube visually with a specified color using coordinates
    void fillCube(Context c, int inpX, int inpY, Color color) {
        int drawX  = inpX * 15;
        int drawY = inpY * 15;

        c.SetSourceColor(color);
        c.LineWidth = 1;
        c.Rectangle(x: drawX, y: drawY, width: 15, height: 15);
        c.Fill();
    }

    // Fills the entire board area with colors corresponding to the number matrix
    public void FillBoard(SimBoard InpBoard){
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

    // Fills the visual area with a single color entirely and switches the integer board states
    public void ColorFill(){
        int fillInt;

        if (currColor == Colors.Green){
            fillInt = 0;
        } else if (currColor == Colors.Yellow){
            fillInt = 1;
        } else {
            fillInt = 2;
        }

        numMatrix.IntFill(fillInt);
        FillBoard(numMatrix);
        QueueDraw();
    }

    // Methods to set chosen color and rule
    public void SetColor(Colors inp){
        currColor = inp;
    }

    public void SetRule(RuleSet inp){
        currRule = inp;
    }

    // Applies the required rule, based on the currRule variable.
    public void ApplyStep(){
        if (currRule == RuleSet.Forest){
            numMatrix.ApplyForestRule();
        } else if (currRule == RuleSet.Scroll){
            numMatrix.ApplyScrollRule();
        } else {
            numMatrix.ApplyStairRule();
        }

        FillBoard(numMatrix);
        QueueDraw();
    }
}


// Gtk class of the window UI
class BoardWindow : Gtk.Window {
    // The visual board variable
    DrawBoard mainBoard;
    // Variable which determines if ApplyStep() should be running automatically
    // (true if Play is pressed)
    bool playing = false;

    // Initialization method which sets up the window class with GTK objects.
    public BoardWindow(SimBoard nums) : base("3-State Automata") {
        // Sets up the main containers
        Resize(950, 750);
        mainBoard = new DrawBoard(nums);
        Box mainBox = new Box(Orientation.Horizontal, 1);
        Box rightSide = new Box(Orientation.Vertical, 5);

        rightSide.Add(new Label("Control Panel"));

        // Color choice radio buttons
        Box radioBox = new Box(Orientation.Horizontal, 5);
        RadioButton g = new RadioButton("green");
        RadioButton y = new RadioButton(g, "yellow");
        RadioButton b = new RadioButton(g, "black");
        g.Clicked += OnGreen;
        y.Clicked += OnYellow;
        b.Clicked += OnBlack;
        radioBox.Add(g);
        radioBox.Add(y);
        radioBox.Add(b);
        rightSide.Add(radioBox);

        // Fill button
        Button fill = new Button("Fill Board");
        fill.Clicked += OnFill;
        rightSide.Add(fill);

        // "Apply Step" button
        Button appplyStep = new Button("Apply Step");
        appplyStep.Clicked += OnStep;
        rightSide.Add(appplyStep);

        // Play/Pause buttons
        Box playBar = new Box(Orientation.Horizontal, 5);
        Button play = new Button();
        play.Clicked += OnPlay;
        play.Image = new Image("icons/play.png");
        playBar.PackStart(play, true, true, 1);
        Button pause = new Button();
        pause.Clicked += OnPause;
        pause.Image = new Image("icons/pause.png");
        playBar.PackStart(pause, true, true, 1);
        rightSide.Add(playBar);

        // Rule choice radio buttons
        RadioButton ruleOne = new RadioButton("Forest Rule");
        ruleOne.Clicked += OnForest;
        RadioButton ruleTwo = new RadioButton(ruleOne, "Scroll Rule");
        ruleTwo.Clicked += OnScroll;
        RadioButton ruleThree = new RadioButton(ruleOne, "Stair Rule");
        ruleThree.Clicked += OnStair;

        rightSide.Add(ruleOne);
        rightSide.Add(ruleTwo);
        rightSide.Add(ruleThree);

        // Final container adjustments
        rightSide.Margin = 5;
        mainBox.PackStart(mainBoard, true, true, 0);
        mainBox.Add(rightSide);
        Add(mainBox);
        Timeout.Add(100, OnTimeout);
    }

    // Terminates the program if this window is closed
    protected override bool OnDeleteEvent(Event e) {
        Application.Quit();
        return true;
    }

    // Updates the board visuals
    public void Draw(SimBoard board){
        mainBoard.FillBoard(board);
    }

    // Methods for color changes
    void OnGreen(object? sender, EventArgs e){
        mainBoard.SetColor(Colors.Green);
    }

    void OnYellow(object? sender, EventArgs e){
        mainBoard.SetColor(Colors.Yellow);
    }

    void OnBlack(object? sender, EventArgs e){
        mainBoard.SetColor(Colors.Black);
    }

    // Applies a single step of the chosen ru;e
    void OnStep(object? sender, EventArgs e){
        mainBoard.ApplyStep();
    }

    // Methods for rule changes
    void OnForest(object? sender, EventArgs e){
        mainBoard.SetRule(RuleSet.Forest);
    }

    void OnScroll(object? sender, EventArgs e){
        mainBoard.SetRule(RuleSet.Scroll);
    }

    void OnStair(object? sender, EventArgs e){
        mainBoard.SetRule(RuleSet.Stair);
    }

    // Runs ApplyStep() passively if "play" is pressed (playing is true)
    bool OnTimeout() {
        if (playing){
            mainBoard.ApplyStep();
        }
        return true;
    }

    // Sets playing to true
    void OnPlay(object? sender, EventArgs e){
        playing = true;
    }

    // Sets playing to false
    void OnPause(object? sender, EventArgs e){
        playing = false;
    }

    // Fills the board with a single color (and number)
    void OnFill(object? sender, EventArgs e){
        mainBoard.ColorFill();
    }
}

// Main execution loop of the program
class MainLoop {
    public static void Main(string[] args){
        // Starts the GTK window
        Application.Init();
        
        // Sets up SimBoard and BoardWindow variables
        SimBoard board = new SimBoard(50);
        BoardWindow main = new BoardWindow(board);

        // Draws a first plain board
        main.Draw(board);
        
        // Shows the window
        main.ShowAll();
        // Runs the window
        Application.Run();
    }

}

