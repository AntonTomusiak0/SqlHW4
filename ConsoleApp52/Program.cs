using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StudentGradesApp
{
    class Program
    {
        private static DbProviderFactory _providerFactory;
        private static string _connectionString;

        static async Task Main(string[] args)
        {
            LoadConfiguration();

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\nВиберіть дію:");
                Console.WriteLine("1 - Відобразити всі записи");
                Console.WriteLine("2 - Додати новий запис");
                Console.WriteLine("3 - Оновити запис");
                Console.WriteLine("4 - Видалити запис");
                Console.WriteLine("5 - Змінити СКБД");
                Console.WriteLine("0 - Вихід");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await DisplayAllRecordsAsync();
                        break;
                    case "2":
                        await AddNewRecordAsync();
                        break;
                    case "3":
                        await UpdateRecordAsync();
                        break;
                    case "4":
                        await DeleteRecordAsync();
                        break;
                    case "5":
                        ChangeDatabaseProvider();
                        break;
                    case "0":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Некоректний вибір. Спробуйте ще раз.");
                        break;
                }
            }
        }
        static void LoadConfiguration()
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            string providerName = config["Database:Provider"];
            _connectionString = config["Database:ConnectionString"];

            _providerFactory = DbProviderFactories.GetFactory(providerName);
        }
        static async Task DisplayAllRecordsAsync()
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                using (DbConnection connection = _providerFactory.CreateConnection())
                {
                    connection.ConnectionString = _connectionString;
                    await connection.OpenAsync();

                    DbCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT * FROM Students";

                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        Console.WriteLine("ID\tІм'я\tОцінка");
                        while (await reader.ReadAsync())
                        {
                            Console.WriteLine($"{reader["Id"]}\t{reader["Name"]}\t{reader["Grade"]}");
                        }
                    }
                }

                stopwatch.Stop();
                Console.WriteLine($"Час виконання запиту: {stopwatch.Elapsed.TotalSeconds} секунд.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }
        }
        static async Task AddNewRecordAsync()
        {
            try
            {
                Console.Write("Введіть ім'я студента: ");
                string name = Console.ReadLine();

                Console.Write("Введіть оцінку студента: ");
                int grade = int.Parse(Console.ReadLine());

                Stopwatch stopwatch = Stopwatch.StartNew();

                using (DbConnection connection = _providerFactory.CreateConnection())
                {
                    connection.ConnectionString = _connectionString;
                    await connection.OpenAsync();

                    DbCommand command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO Students (Name, Grade) VALUES (@name, @grade)";

                    DbParameter nameParam = command.CreateParameter();
                    nameParam.ParameterName = "@name";
                    nameParam.Value = name;
                    command.Parameters.Add(nameParam);

                    DbParameter gradeParam = command.CreateParameter();
                    gradeParam.ParameterName = "@grade";
                    gradeParam.Value = grade;
                    command.Parameters.Add(gradeParam);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    Console.WriteLine($"Додано {rowsAffected} запис(ів).");
                }

                stopwatch.Stop();
                Console.WriteLine($"Час виконання запиту: {stopwatch.Elapsed.TotalSeconds} секунд.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }
        }
        static async Task UpdateRecordAsync()
        {
            try
            {
                Console.Write("Введіть ID запису для оновлення: ");
                int id = int.Parse(Console.ReadLine());

                Console.Write("Введіть нову оцінку: ");
                int grade = int.Parse(Console.ReadLine());

                Stopwatch stopwatch = Stopwatch.StartNew();

                using (DbConnection connection = _providerFactory.CreateConnection())
                {
                    connection.ConnectionString = _connectionString;
                    await connection.OpenAsync();

                    DbCommand command = connection.CreateCommand();
                    command.CommandText = "UPDATE Students SET Grade = @grade WHERE Id = @id";

                    DbParameter idParam = command.CreateParameter();
                    idParam.ParameterName = "@id";
                    idParam.Value = id;
                    command.Parameters.Add(idParam);

                    DbParameter gradeParam = command.CreateParameter();
                    gradeParam.ParameterName = "@grade";
                    gradeParam.Value = grade;
                    command.Parameters.Add(gradeParam);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    Console.WriteLine($"Оновлено {rowsAffected} запис(ів).");
                }

                stopwatch.Stop();
                Console.WriteLine($"Час виконання запиту: {stopwatch.Elapsed.TotalSeconds} секунд.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }
        }
        static async Task DeleteRecordAsync()
        {
            try
            {
                Console.Write("Введіть ID запису для видалення: ");
                int id = int.Parse(Console.ReadLine());

                Stopwatch stopwatch = Stopwatch.StartNew();

                using (DbConnection connection = _providerFactory.CreateConnection())
                {
                    connection.ConnectionString = _connectionString;
                    await connection.OpenAsync();

                    DbCommand command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM Students WHERE Id = @id";

                    DbParameter idParam = command.CreateParameter();
                    idParam.ParameterName = "@id";
                    idParam.Value = id;
                    command.Parameters.Add(idParam);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    Console.WriteLine($"Видалено {rowsAffected} запис(ів).");
                }

                stopwatch.Stop();
                Console.WriteLine($"Час виконання запиту: {stopwatch.Elapsed.TotalSeconds} секунд.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }
        }
        static void ChangeDatabaseProvider()
        {
            Console.WriteLine("Доступні провайдери:");
            Console.WriteLine("1 - SQL Server");
            Console.WriteLine("2 - SQLite");
            Console.Write("Виберіть провайдера: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    _providerFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");
                    _connectionString = "Server=YOUR_SERVER_NAME;Database=StudentGrades;Trusted_Connection=True;";
                    break;
                case "2":
                    _providerFactory = DbProviderFactories.GetFactory("Microsoft.Data.Sqlite");
                    _connectionString = "Data Source=StudentGrades.db;";
                    break;
                default:
                    Console.WriteLine("Некоректний вибір.");
                    break;
            }

            Console.WriteLine("Провайдер змінено.");
        }
    }
}