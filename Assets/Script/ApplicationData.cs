using System.Text.RegularExpressions;

public class AppData
{
    private static readonly char delimiter = '|';
    private static readonly char delimiterUI = '%';
    private static readonly char clientCommand = '#';
    private static readonly char serverCommand = '@';
    private static readonly string color = "CC->";
    private static readonly string backgroundClear = "BG->CLEAR";
    private static readonly string endOfLine = "EOL";

    private static readonly int bufferSize = 1024;

    private static string serverIp;
    private static int serverPort;

    private static readonly Regex ipRegex =
        new Regex(
            @"^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])$");

    private static readonly Regex roomNameRegex =
        new Regex(@"^([^" + Delimiter + DelimiterUI + ClientCommand + ServerCommand + " " + "]*)$");

    public static char Delimiter => delimiter;
    public static char DelimiterUI => delimiterUI;
    public static char ClientCommand => clientCommand;
    public static char ServerCommand => serverCommand;
    public static string ColorCommand => color;
    public static string BackgroundClearCommand => backgroundClear;
    public static string EndOfLine => endOfLine;
    public static int BufferSize => bufferSize;

    public static Regex IpRegex => ipRegex;
    public static Regex RoomNameRegex => roomNameRegex;

    public static string ServerIp
    {
        get => serverIp;
        set => serverIp = value;
    }

    public static int ServerPort
    {
        get => serverPort;
        set => serverPort = value;
    }
}

public static class CommendBook
{
    private static readonly string findRoom = AppData.ServerCommand + "FIND-ROOM";
    private static readonly string createRoom = AppData.ServerCommand + "CREATE-ROOM";
    private static readonly string enterRoom = AppData.ServerCommand + "ENTER-ROOM";
    private static readonly string startDrawing = AppData.ServerCommand + "START-DRAWING";
    private static readonly string guestDrawing = AppData.ServerCommand + "GUEST-DRAWING";
    private static readonly string errorMessage = AppData.ServerCommand + "ERROR" + AppData.DelimiterUI;
    private static readonly string roomListHeader = AppData.ServerCommand + "ROOM-LIST";
    private static readonly string roomClosed = AppData.ServerCommand + "ROOMCLOSED";
    private static readonly string drawingRoomIsFull = AppData.ServerCommand + "ROOM-IS-FULL";
    private static readonly string colorCommend = AppData.ClientCommand + AppData.ColorCommand;
    private static readonly string clearBackgroundCommend = AppData.ClientCommand + AppData.BackgroundClearCommand;
    private static readonly string endOfLine = AppData.ClientCommand + AppData.EndOfLine;
    private static readonly string connection = AppData.ServerCommand + "CONNECTION";
    private static readonly string commendError = ERROR_MESSAGE + "COMMAND";
    private static readonly string roomCreateError = ERROR_MESSAGE + "NAME";
    private static readonly string passwordError = ERROR_MESSAGE + "WRONGPW";
    private static readonly string noRoomError = ERROR_MESSAGE + "NOROOM";

    public static string FIND_ROOM => findRoom;
    public static string CREATE_ROOM => createRoom;
    public static string ENTER_ROOM => enterRoom;
    public static string HEADER_ROOMLIST => roomListHeader;
    public static string ROOM_CLOSED => roomClosed;
    public static string ERROR_MESSAGE => errorMessage;
    public static string START_DRAWING => startDrawing;
    public static string GUEST_DRAWING => guestDrawing;
    public static string COLOR_COMMEND => colorCommend;
    public static string CLEAR_BACKGROUND_COMMEND => clearBackgroundCommend;
    public static string END_OF_LINE => endOfLine;
    public static string CONNECTION => connection;
    public static string COMMEND_ERROR => commendError;
    public static string ROOM_CREATE_ERROR => roomCreateError;
    public static string PASSWORD_ERROR => passwordError;
    public static string NO_ROOM_ERROR => noRoomError;
    public static string DRAWING_ROOM_FULL => drawingRoomIsFull;
}