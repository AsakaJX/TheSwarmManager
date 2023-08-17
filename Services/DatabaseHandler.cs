using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using Oracle.ManagedDataAccess.Client;
using Pastel;
using TheSwarmManager.Modules.Logging;

namespace TheSwarmManager.Services.Database {
    // public struct KeyTypePair {
    //     public string Key { get; set; }
    //     public OracleDbType Type { get; set; }
    //     public KeyTypePair(string key, OracleDbType type) {
    //         Key = key;
    //         Type = type;
    //     }
    // }
    // public class TripleValueDictionary<T> : Dictionary<KeyTypePair, T> where T : notnull {
    //     public void Add(string kvpKey, OracleDbType kvpType, T value) {
    //         KeyTypePair kvp = new KeyTypePair(kvpKey, kvpType);
    //         this.Add(kvp, value);
    //     }
    // }
    class DBHandler {
        private IConfigurationRoot _config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddYamlFile("config.yml")
            .Build();
        private readonly Logger Log = new Logger();
        private static string _user = "asaka";
        private static string _db = "localhost/XEPDB1";
#pragma warning disable
        private static string _connectionString;
        private static OracleConnection _connection;
        private static OracleCommand _command;
#pragma warning restore
        // <------------------- Test section ------------------->
        /// <summary>
        /// Testing connection to database.
        /// </summary>
        /// <returns>true if succeeded and false if not.</returns>
        public bool TestConnection() {
            try {
                _connection.Open();
                _connection.Close();
            } catch (Exception ex) {
                Log.NewCriticalError(104, "Database Handler|Test", ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Test case for reading data.
        /// </summary>
        public void TestReadingData() {
            var output = Read("todoitem", "description, id");
            foreach (var element in output.Keys) {
                foreach (var element2 in output[element])
                    Log.NewLog(LogSeverity.Debug, "Database Handler|Test", $"{element}: {element2}");
            }
        }

        // <------------------- Usage section ------------------->
        /// <summary>
        /// Execute command method to prevent repeating myself.
        /// </summary>
        /// <param name="cmd">Oracle Command object</param>
        private void TryToExecuteCommand(OracleCommand cmd) {
            try {
                cmd.ExecuteNonQuery();
                Log.NewLog(LogSeverity.Info, "Database Handler|Command Runner", $"Command \"{cmd.CommandText}\" has been executed {"successfully".Pastel("#70ff38")}!");
            } catch (Exception ex) {
                Log.NewLog(LogSeverity.Error, "Database Handler|Command Runner", $"{"Exception".Pastel("#ff3434")} caught during execution of \"{cmd.CommandText}\" command!");
                Log.NewLog(LogSeverity.Error, "Database Handler|Command Runner", ex.Message, 1);
            }
        }
        /// <summary>
        /// Read data from the table by specified column(-s) / and with specified range.
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="columns">Column names splited by ","</param>
        /// <param name="rangeMin">(Optional) Min value of range</param>
        /// <param name="rangeMax">(Optional) Max value of range</param>
        /// <returns></returns>
        public Dictionary<string, string[]> Read(string table, string columns, int rangeMin = 0, int rangeMax = 0) {
            _command.CommandText = $"SELECT {columns} FROM {table} {(rangeMin > 0 && rangeMax > 0 ? $"WHERE ID BETWEEN {rangeMin} AND {rangeMax}" : "")}";
            string[] columnsArray = columns.Replace(" ", "").Split(",");

            Dictionary<string, string> output = new Dictionary<string, string>();
            foreach (var element in columnsArray) {
                output.Add(element, "");
            }

            try {
                OracleDataReader reader = _command.ExecuteReader();
                while (reader.Read()) {
                    foreach (var element in columnsArray) {
                        var index = reader.GetOrdinal(element);
                        output[element] += reader.GetString(index) + "ORACLE_NEWLINE";
                    }
                }
            } catch (Exception ex) {
                Log.NewLog(LogSeverity.Error, "Database Handler|Read", ex.Message);
            }

            if (output.Values.Last().IndexOf("ORACLE_NEWLINE") == -1) {
                Dictionary<string, string[]> empty = new Dictionary<string, string[]>();
                foreach (var element in output.Keys) {
                    empty.Add(element, Array.Empty<string>());
                }
                Log.NewLog(LogSeverity.Warning, "Database Handler|Read", "Output array is empty. I either didn't find a column or table is empty!");
                return empty;
            }

            Dictionary<string, string[]> final = new Dictionary<string, string[]>();
            foreach (var element in output) {
                final.Add(element.Key, element.Value.Remove(element.Value.LastIndexOf("ORACLE_NEWLINE"), 14).Split("ORACLE_NEWLINE"));
            }

            return final;
        }
        /// <summary>
        /// Setup connection information.
        /// </summary>
        /// <param name="pwd">Password from database.</param>
        //! Password is currently not really secured here.
        public void SetupConnectionInformation(string pwd) {
            _connectionString = $"User Id={_user};Password={pwd};Data Source={_db};";
            try {
                _connection = new OracleConnection(_connectionString);
            } catch (Exception ex) {
                Log.NewCriticalError(103, "Database Handler|Setup Info", ex.Message);
            }
        }
        /// <summary>
        /// Get's and returns current connection.
        /// </summary>
        /// <returns>Oracle Connection object</returns>
        public OracleConnection GetConnection() {
            return _connection;
        }
        /// <summary>
        /// Opens the database connection.
        /// </summary>
        public void OpenConnection() {
            _connection.Open();
            _command = _connection.CreateCommand();
        }
        /// <summary>
        /// Closes the database connection.
        /// </summary>
        public void CloseConnection() {
            _connection.Close();
            _connection.Dispose();
        }
        /// <summary>
        /// Deletes one entry from the table.
        /// </summary>
        /// <param name="table">Table where we're gonna to nuke something</param>
        /// <param name="column">(Specific column) Finding exact coordinates of the motherfucker by his IP Adress</param>
        /// <param name="filter">Filter</param>
        /// <param name="dataType">Type of the column(maybe could be automated idk)</param>
        public void Delete(string table, string column, string filter) {
            _command.CommandText = $"DELETE FROM {table} WHERE {column} = {filter}";
            TryToExecuteCommand(_command);
        }
        /// <summary>
        /// Delete rows by ID COLUMN! in specific range (INCLUDING FIRST AND LAST!).
        /// (if id column doesn't exist - YOU SHOULD CREATE IT YOURSELF)
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="startIndex">Start index (including)</param>
        /// <param name="endIndex">End index (including)</param>
        public void DeleteInRange(string table, uint startIndex, uint endIndex) {
            _command.CommandText = $"DELETE FROM {table} WHERE ID BETWEEN {startIndex} AND {endIndex}";
            TryToExecuteCommand(_command);
        }
        /// <summary>
        /// Reseed specific column.
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="column">Column name</param>
        /// <param name="columnIdentifiesAs">Column identifies as... (eg. IDENTITY for ID column)</param>
        /// <param name="startWith">From what value to start with</param>
        public void ReseedColumn<T>(string table, string column, string columnIdentifiesAs, T startWith) {
            _command.CommandText = $"ALTER TABLE {table} MODIFY({column} GENERATED AS {columnIdentifiesAs} (START WITH {startWith}))";
            TryToExecuteCommand(_command);
        }
        /// <summary>
        /// Insert one item to table with specific columns and their values.
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="columns">Column names separated with "," !!!</param>
        /// <param name="values">Column names separated with "," AND if it's string put it in " " !!!</param>
        public void Insert(string table, string columns, string values) {
            _command.CommandText = $"INSERT INTO {table} ({columns}) VALUES ({values})";
            TryToExecuteCommand(_command);
        }
        /// <summary>
        /// Update one-or-many columns in table.
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="newColumn">New column dictionary with key - column name and value - new column value (if it's string put it in " ") !!!</param>
        /// <param name="id">ID of the column</param>
        public void Update(string table, Dictionary<string, string> newColumn, int id) {
            foreach (var element in newColumn) {
                _command.CommandText = $"UPDATE {table} SET {element.Key} = {element.Value} WHERE id = {id}";
                TryToExecuteCommand(_command);
            }
        }
        /// <summary>
        /// Create new table in database.
        /// Id column would be created aswell.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnArguments">Arguments for pre made columns (Default is only id column)</param>
        public void CreateTable(string tableName, string columnArguments) {
            columnArguments = $"id NUMBER GENERATED ALWAYS AS IDENTITY{(columnArguments != "" ? $", {columnArguments}, " : ", ")}PRIMARY KEY(id)";
            _command.CommandText = $"CREATE TABLE {tableName} ({columnArguments})";
            TryToExecuteCommand(_command);
        }
        /// <summary>
        /// Create new table in database but only with id column.
        /// </summary>
        /// <param name="tableName">Table name</param>
        public void CreateTable(string tableName) {
            CreateTable(tableName, "");
        }
        /// <summary>
        /// Drops the table.
        /// </summary>
        /// <param name="tableName">Table name</param>
        public void DeleteTable(string tableName) {
            _command.CommandText = $"DROP TABLE {tableName} PURGE";
            TryToExecuteCommand(_command);
        }
        /// <summary>
        /// Creates new column to the table.
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="columnName">Column name</param>
        /// <param name="columnType">Column type</param>
        /// <param name="columnModifiers">Column modifiers</param>
        public void CreateColumn(string table, string columnName, string columnType, string columnModifiers = "") {
            _command.CommandText = $"ALTER TABLE {table} ADD {columnName} {columnType} {columnModifiers}";
            TryToExecuteCommand(_command);
        }
        /// <summary>
        /// Drops column from the table.
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="columnName">Column name</param>
        public void DeleteColumn(string table, string columnName) {
            _command.CommandText = $"ALTER TABLE {table} DROP COLUMN {columnName}";
            TryToExecuteCommand(_command);
        }
        /// <summary>
        /// Execute custom command, that hasn't been implemented already.
        /// </summary>
        /// <param name="command">Command string</param>
        public void Execute(string command) {
            _command.CommandText = command;
            TryToExecuteCommand(_command);
        }
    }
}