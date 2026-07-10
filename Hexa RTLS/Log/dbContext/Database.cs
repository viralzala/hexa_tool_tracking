using System.Data.SQLite;
using System.IO;


namespace Log.dbContext
{

    class Database
    {
        public SQLiteConnection conn;
        public Database()
        {
            conn = new SQLiteConnection("Data Source=database.sqlite3");
            if (!File.Exists("./database.sqlite3"))
            {
                System.Console.WriteLine("database.sqlite3");
                SQLiteConnection.CreateFile("database.sqlite3");
            }
        }
        public void OpenConnection()
        {
            if (conn.State != System.Data.ConnectionState.Open)
            {
                conn.Open();
            }
        }
        public void CloseConnection()
        {
            if (conn.State != System.Data.ConnectionState.Closed)
            {
                conn.Close();
            }
        }
        //public static void registerFunctions(SQLiteConnection connection)
        //{
        //    connection.CreateFunction("UPPER", (string str) => str?.ToUpper());
        //}
        public void execute(SQLiteConnection con, string cmdText)
        {
            var cmd = con.CreateCommand();
            cmd.CommandText = cmdText;
            cmd.ExecuteNonQuery();
        }
    }
}
