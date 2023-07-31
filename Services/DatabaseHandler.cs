using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using Oracle.ManagedDataAccess.Client;
using Pastel;
using TheSwarmManager.Modules.Logging;

namespace TheSwarmManager.Services.Database {
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
                Log.NewCriticalError(104, "Database Handler|Connection", ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Test case for reading data.
        /// </summary>
        public void TestReadingData() {
            if (_connection == null) {
                Log.NewCriticalError(101, "Bot Handler|Database", "Connection for some reason is null.");
                return;
            }
            OracleCommand command = _connection.CreateCommand();

            //? Retrieve sample data
            try {
                command.CommandText = "SELECT id, description, done FROM todoitem";
                OracleDataReader reader = command.ExecuteReader();
                while (reader.Read()) {
                    if (reader.GetBoolean(2))
                        Console.WriteLine($"Index: {reader.GetInt32(0)}, Description: {reader.GetString(1)} is done.");
                    else
                        Console.WriteLine($"Index: {reader.GetInt32(0)}, Description: {reader.GetString(1)} is NOT done.");
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        // <------------------- Usage section ------------------->
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
        public OracleConnection? GetConnection() {
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
        }
        /// <summary>
        /// Deletes one entry in table.
        /// </summary>
        /// <param name="table">Table where we're gonna to nuke something</param>
        /// <param name="column">(Specific column) Finding exact coordinates of the motherfucker by his IP Adress</param>
        /// <param name="filter">Filter</param>
        /// <param name="dataType">Type of the column(maybe could be automated idk)</param>
        public void Delete(string table, string column, string filter, OracleDbType dataType) {
            _command.CommandText = $"DELETE FROM {table} WHERE {column} = :p{column}";
            _command.Parameters.Add($":p{column}", dataType).Value = filter;
            try {
                _command.ExecuteNonQuery();
            } catch (Exception ex) {
                Log.NewLog(LogSeverity.Error, "Database Handler", ex.Message);
            }
        }
        /// <summary>
        /// Delete rows by ID COLUMN! (if that doesn't exist - YOU SHOULD CREATE IT YOURSELF) in specific range (INCLUDING FIRST AND LAST!).
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="startIndex">Start index (including)</param>
        /// <param name="endIndex">End index (including)</param>
        public void DeleteInRange(string table, uint startIndex, uint endIndex) {
            try {
                _command.CommandText = $"DELETE FROM {table} WHERE ID BETWEEN {startIndex} AND {endIndex}";
                _command.ExecuteNonQuery();
            } catch (Exception ex) {
                Log.NewLog(LogSeverity.Error, "Database Handler", ex.Message);
            }
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
            try {
                _command.ExecuteNonQuery();
            } catch (Exception ex) {
                Log.NewLog(LogSeverity.Error, "Database Handler", ex.Message);
            }
        }
        //! IN PROGRESS
        /// <summary>
        /// Insert one-or-many items in table.
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="columnsAndValues">Dictionary with column names and their values</param>
        //! IN PROGRESS
        public void Insert(string table, Dictionary<Dictionary<string, OracleDbType>, string> columnsAndValues) {
            //? First method
            // _command.CommandText = "INSERT INTO todoitem (DESCRIPTION, DONE) VALUES (:pDESCRIPTION, :pDONE)";
            // _command.Parameters.Add(":pDONE", OracleDbType.Bit).Value = 0;
            // _command.Parameters.Add(":pDESCRIPTION", OracleDbType.NVarchar2).Value = "ULTRA COOL NEW ROW #2";

            //? Second method
            // _command.CommandText = "INSERT INTO todoitem (DESCRIPTION, DONE) VALUES ('ULTRA COOL NEW ROW', 0)";

            string ColumnString = "";
            foreach (var element in columnsAndValues.Keys) {
                ColumnString += $"{element}, ";
            }
            string ValuesString = "";
            foreach (var element in columnsAndValues.Values) {
                ValuesString += $"{element}, ";
            }
            ColumnString = ColumnString.Remove(ColumnString.Length - 2, 2);
            ValuesString = ValuesString.Remove(ValuesString.Length - 2, 2);

            for (int i = 0; i < columnsAndValues.Count; i++) {
                _command.CommandText += $"INSERT INTO {table} ({ColumnString}) VALUES ({ValuesString});\n";
            }

            try {
                System.Console.WriteLine(_command.CommandText);
                _command.ExecuteNonQuery();
            } catch (Exception ex) {
                Console.WriteLine("Error: " + ex);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void Main() {
            OracleCommand command = _connection.CreateCommand();

            //? Trying to create new data elements
            try {
                //? SQL Command for Inserting Items
                // command.CommandText = "INSERT INTO todoitem (DESCRIPTION, DONE) VALUES (:pDESCRIPTION, :pDONE)";

                // //? Writing data
                // command.Parameters.Add(":pDESCRIPTION", OracleDbType.NVarchar2).Value = "ULTRA COOL NEW ROW #2";
                // command.Parameters.Add(":pDONE", OracleDbType.Int32).Value = 1;
                command.CommandText = "INSERT INTO todoitem (DESCRIPTION, DONE) VALUES ('ULTRA COOL NEW ROW', 0)";
                command.ExecuteNonQuery();

                //? Shorter version below
                command.CommandText = "INSERT INTO todoitem (DESCRIPTION, DONE) VALUES ('OMEGALUL', 0)";

                //? Executing Inserting Command as NON QUERY (Used for delete, insert, update).
                int rowCount = command.ExecuteNonQuery();

                Console.WriteLine("Row Count affected = " + rowCount);
            } catch (Exception ex) {
                Console.WriteLine("Error: " + ex);
                Console.WriteLine(ex.StackTrace);
            }

            //? Closing and Disposing connection.
            _connection.Close();
            _connection.Dispose();
        }
    }
}