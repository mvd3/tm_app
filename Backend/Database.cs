using System.Collections;
using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;

public class Database {
    private SqlConnection _connection;
    private SqlCommand _command;
    private const int PAGE_SIZE = 20;
    private string _connectionString, _mainConnectionString, _server, _database, _password;
    private string _tmDatabase = "TaskManagement";
    private string _statusTable = "Status";
    private string _tasksTable = "Tasks";
    private string _statusIdField = "Id";
    private string _statusNameField = "Name";
    private string _tasksIdField = "Id";
    private string _tasksNameField = "Name";
    private string _tasksDescriptionField = "Description";
    private string _tasksCreatedDateField = "CreatedDate";
    private string _tasksStatusIdField = "StatusId";

    public Database() 
    {
        _server = Environment.GetEnvironmentVariable("DB_HOST") ?? "null";
        _database = Environment.GetEnvironmentVariable("DB_NAME") ?? "null";
        _password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "null";
        _connectionString = $"Server={_server};Database={_database};User Id=sa;Password={_password};TrustServerCertificate=true;Integrated Security=False;";
        _mainConnectionString = $"Server={_server};Database={_tmDatabase};User Id=sa;Password={_password};TrustServerCertificate=true;Integrated Security=False;";
        _connection = new(_connectionString);
    }

    public int InitDatabase() 
    {
        string checkDatabaseExistance = @$"
        SELECT COUNT(*) 
        FROM sys.databases 
        WHERE name = '{_tmDatabase}'
        ";
        string createDatabase = @$"CREATE DATABASE {_tmDatabase}";
        string createTableStatus = @$"
            CREATE TABLE {_statusTable}(
                {_statusIdField} INT IDENTITY(1,1) PRIMARY KEY,
                {_statusNameField} NVARCHAR(100)
            )
        ";
        string createTableTasks = @$"
            CREATE TABLE {_tasksTable}(
                {_tasksIdField} INT IDENTITY(1,1) PRIMARY KEY,
                {_tasksNameField} NVARCHAR(100),
                {_tasksDescriptionField} NVARCHAR(1000),
                {_tasksCreatedDateField} DATETIME,
                {_tasksStatusIdField} INT,
                FOREIGN KEY ({_tasksStatusIdField}) REFERENCES {_statusTable}({_statusIdField})
            )
        ";
        string insertStatus = @$"
            INSERT INTO {_statusTable}({_statusNameField})
            VALUES ('In progress'), ('Done')
        ";
        string insertTasks = @$"
            INSERT INTO {_tasksTable}({_tasksNameField}, {_tasksDescriptionField}, {_tasksCreatedDateField}, {_tasksStatusIdField})
            VALUES
            ('First', 'Some tasks', GETDATE(), 1),
            ('Finished task', 'My work here is done.', GETDATE(), 2)
        ";
        
        try
        {
            _connection.Open();

            _command = new(checkDatabaseExistance, _connection);
            bool databaseExists = ((int) _command.ExecuteScalar()) > 0;
            if (!databaseExists) {
                _command = new(createDatabase, _connection);
                _command.ExecuteNonQuery();
                
                _connection.Close();
                _connection = new(_mainConnectionString);
                _connection.Open();

                _command = new(createTableStatus, _connection);
                _command.ExecuteNonQuery();

                _command = new(createTableTasks, _connection);
                _command.ExecuteNonQuery();

                _command = new(insertStatus, _connection);
                _command.ExecuteNonQuery();

                _command = new(insertTasks, _connection);
                _command.ExecuteNonQuery();
            }
        } catch
        {
            return -1;
        } finally {
            _connection.Close();
        }

        return 0;
    }

    public List<Status> GetStatuses()
    {
        List<Status> list = new();
        SqlConnection connection = new(_mainConnectionString);
        SqlCommand command;
        string getStatuses = @$"
            SELECT *
            FROM {_statusTable}
        ";
        
        try {
            connection.Open();
            command = new(getStatuses, connection);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
                list.Add(new Status((int) reader[_statusIdField], (string) reader[_statusNameField]));
        } catch {
        } finally {
            connection.Close();
        }
        
        return list;
    }

    public List<Task> GetTasks(string package)
    {
        List<Task> list = new();
        SqlConnection connection = new(_mainConnectionString);
        SqlCommand command;
        SearchTasks searchTasks = JsonSerializer.Deserialize<SearchTasks>(package);
        string whereClause = "";
        string statusCriterium = @$"{_tasksStatusIdField} = {searchTasks.Status}";
        string termCriterium = @$"{_tasksNameField} LIKE '%' + @term + '%' OR {_tasksDescriptionField} LIKE '%' + @term + '%'";

        if (searchTasks.SearchTerm != null && searchTasks.SearchTerm.Length > 0 && searchTasks.Status >= 0)
            whereClause = $@"WHERE {statusCriterium} AND ({termCriterium})";
        else if (searchTasks.Status >= 0)
            whereClause = @$"WHERE {statusCriterium}";
        else if (searchTasks.SearchTerm != null && searchTasks.SearchTerm.Length > 0)
            whereClause = @$"WHERE {termCriterium}";
        

        string getTasks = @$"
            SELECT *
            FROM {_tasksTable}
            {whereClause}
            ORDER BY {_tasksStatusIdField} ASC, {_tasksCreatedDateField} DESC
            OFFSET {PAGE_SIZE * searchTasks.Page} ROWS
            FETCH NEXT {PAGE_SIZE} ROWS ONLY
        ";

        try 
        {
            connection.Open();
            command = new(getTasks, connection);
            if (searchTasks.SearchTerm != null && searchTasks.SearchTerm.Length > 0)
                command.Parameters.AddWithValue("@term", searchTasks.SearchTerm);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
                list.Add(new Task((int) reader[_tasksIdField], 
                (string) reader[_tasksNameField],
                (string) reader[_tasksDescriptionField],
                (DateTime) reader[_tasksCreatedDateField],
                (int) reader[_tasksStatusIdField]));
        } catch {
        } finally {
            connection.Close();
        }

        return list;
    }

    public int GetNumberOfTasks(string package)
    {
        int result = 0;
        SearchTasks searchTasks = JsonSerializer.Deserialize<SearchTasks>(package);
        SqlConnection connection = new(_mainConnectionString);
        SqlCommand command;
        string whereClause = "";
        string statusCriterium = @$"{_tasksStatusIdField} = {searchTasks.Status}";
        string termCriterium = @$"{_tasksNameField} LIKE '%' + @term + '%' OR {_tasksDescriptionField} LIKE '%' + @term + '%'";

        if (searchTasks.SearchTerm != null && searchTasks.SearchTerm.Length > 0 && searchTasks.Status >= 0)
            whereClause = $@"WHERE {statusCriterium} AND ({termCriterium})";
        else if (searchTasks.Status >= 0)
            whereClause = @$"WHERE {statusCriterium}";
        else if (searchTasks.SearchTerm != null && searchTasks.SearchTerm.Length > 0)
            whereClause = @$"WHERE {termCriterium}";
        

        string getTasks = @$"
            SELECT COUNT(*)
            FROM {_tasksTable}
            {whereClause}
        ";

        try
        {
            connection.Open();
            command = new(getTasks, connection);
            if (searchTasks.SearchTerm != null && searchTasks.SearchTerm.Length > 0)
                command.Parameters.AddWithValue("@term", searchTasks.SearchTerm);
            result = (int) command.ExecuteScalar();
        } catch {
        } finally {
            connection.Close();
        }
        
        return result;
    }

    public int UpdateTask(string package)
    {
        int result = -1;
        TaskObject task = JsonSerializer.Deserialize<TaskObject>(package);
        SqlConnection connection = new(_mainConnectionString);
        SqlCommand command;
        string updateTask = @$"
            UPDATE {_tasksTable}
            SET {_tasksNameField} = @name, {_tasksDescriptionField} = @description, {_tasksStatusIdField} = {task.StatusId}
            WHERE {_tasksIdField} = {task.Id}
        ";

        try
        {
            connection.Open();
            command = new(updateTask, connection);
            command.Parameters.AddWithValue("@name", task.Name);
            command.Parameters.AddWithValue("@description", task.Description);
            result = (int) command.ExecuteNonQuery();
        } catch {

        } finally {
            connection.Close();
        }

        return result;
    }

    public int AddTask(string package)
    {
        int result = -1;
        TaskObject task = JsonSerializer.Deserialize<TaskObject>(package);
        SqlConnection connection = new(_mainConnectionString);
        SqlCommand command;
        string addTask = @$"
            INSERT INTO {_tasksTable}({_tasksNameField}, {_tasksDescriptionField}, {_tasksCreatedDateField}, {_tasksStatusIdField})
            VALUES (@name, @description, GETDATE(), {task.StatusId})
        ";

        try
        {
            connection.Open();
            command = new(addTask, connection);
            command.Parameters.AddWithValue("@name", task.Name);
            command.Parameters.AddWithValue("@description", task.Description);
            result = (int) command.ExecuteNonQuery();
        } catch {
        } finally{
            connection.Close();
        }

        return result;
    }

    public int DeleteTask(int id)
    {
        int result = -1;
        SqlConnection connection = new(_mainConnectionString);
        SqlCommand command;
        string deleteTask = @$"
            DELETE FROM {_tasksTable}
            WHERE {_tasksIdField} = {id}
        ";

        try
        {
            connection.Open();
            command = new(deleteTask, connection);
            result = (int) command.ExecuteNonQuery();
        } catch {
        } finally {
            connection.Close();
        }

        return result;
    }

    public int SetTaskAsDone(int id)
    {
        int result = -1;
        SqlConnection connection = new(_mainConnectionString);
        SqlCommand command;
        string setTaskAsDone = @$"
            UPDATE {_tasksTable}
            SET {_tasksStatusIdField} = 2
            WHERE {_tasksIdField} = {id}
        ";

        try
        {
            connection.Open();
            command = new(setTaskAsDone, connection);
            result = (int) command.ExecuteNonQuery();
        } catch {
        } finally {
            connection.Close();
        }

        return result;
    }
}