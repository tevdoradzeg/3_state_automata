using Gdk;
using Gtk;

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


// Gtk class of the main window UI
class MainWindow : Gtk.Window {
    public MainWindow() : base("3-State Automata") {
    }

    protected override bool OnDeleteEvent(Event evnt) {
        Application.Quit();
        return true;
    }
}


// Main execution loop of the program
class MainLoop {
    public static void Main(string[] args){
        Application.Init();
        MainWindow main = new MainWindow();
        main.ShowAll();
        Application.Run();
    }

}
