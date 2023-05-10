using System.Diagnostics;
using Npgsql;

namespace PTMKTest;

internal class Program
{
    static void Main(string[] args)
    {
        switch (args[0])
        {
            case "1":
            {
                CreateTable();
                break;
            }

            case "2":
            {
                InsertIntoTable(args);
                break;
            }

            case "3":
            {
                ShowData();
                break;
            }
            case "4":
            {
                AddValues();
                break;
            }

            case "5":
            {
                ShowFNames();
                break;
            }
        }
    }
    private static void CreateTable()
    {
        try
        {
            using var connection =
                new NpgsqlConnection(
                    "Server = localhost; User Id = postgres; Password = user; Database = PTMKDB");
            connection.Open();

            using var command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText =
                "CREATE TABLE persons (id SERIAL PRIMARY KEY, fullname VARCHAR(255) NOT NULL, birth_date DATE NOT NULL, gender VARCHAR(10) NOT NULL)";
            command.ExecuteNonQuery();
            Console.WriteLine("Table Created");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static void InsertIntoTable(string[] args)
    {
        try
        {
            using var connection =
                new NpgsqlConnection(
                    "Server = localhost; User Id = postgres; Password = user; Database = PTMKDB");
            connection.Open();

            using var command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText =
                "INSERT INTO persons (fullname, birth_date, gender) VALUES (@fullname, @birthdate, @gender)";
            command.Parameters.AddWithValue("@fullname", args[1]);
            command.Parameters.AddWithValue("@birthdate", DateTime.ParseExact(args[2], "dd-mm-yyyy", null));
            command.Parameters.AddWithValue("@gender", args[3]);
            command.ExecuteNonQuery();
            Console.WriteLine("Inserted");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    private static void ShowData()
    {
        try
        {
            using var connection =
                new NpgsqlConnection(
                    "Server = localhost; User Id = postgres; Password = user; Database = PTMKDB");
            connection.Open();

            using var command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText =
                "Select fullname, birth_date, gender, " +
                "DATE_PART('year', NOW()) - DATE_PART('year', birth_date) - " +
                "CASE WHEN DATE_PART('doy', NOW()) < DATE_PART('doy', birth_date) " +
                "THEN 1 ELSE 0 " +
                "END AS age " +
                "FROM persons " +
                "GROUP BY fullname, birth_date, gender " +
                "ORDER BY fullname";
            using var reader = command.ExecuteReader();
            Console.WriteLine("Name\t\t\t Birthday\t\tAge\t\tGender");

            while (reader.Read())
            {
                var name = reader.GetString(0);
                var birthDay = reader.GetDateTime(1);
                var age = reader.GetDouble(3);
                var gender = reader.GetString(2);
                Console.WriteLine($"{name}\t\t{birthDay:d}\t\t{age}\t\t {gender}\t");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    private static void AddValues()
    {
        try
        {
            using var connection =
                new NpgsqlConnection(
                    "Server = localhost; User Id = postgres; Password = user; Database = PTMKDB");
            connection.Open();

            using var command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText =
                "INSERT INTO persons (fullname, birth_date, gender) VALUES (@fullname, @birthdate, @gender)";
            var random = new Random();
            var now = DateTime.Now;
            var start = new DateTime(1960, 1, 1);
            for (int i = 0; i < 1000000; i++)
            {
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@fullname", RandomName(random));
                command.Parameters.AddWithValue("@birthdate", RandomDate(random, start, now));
                command.Parameters.AddWithValue("@gender", RandomGender(random));
                command.ExecuteNonQuery();
            }

            for (int i = 0; i < 100; i++)
            {
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@fullname", RandomFName(random));
                command.Parameters.AddWithValue("@birthdate", RandomDate(random, start, now));
                command.Parameters.AddWithValue("@gender", "male");
                command.ExecuteNonQuery();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private static void ShowFNames()
    {
        try
        {
            using var connection = new NpgsqlConnection(
                "Server = localhost; User Id = postgres; Password = user; Database = PTMKDB");
            connection.Open();

            using var command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText = "SELECT * FROM persons WHERE gender = 'male' AND fullname LIKE 'F%'";

            Console.WriteLine("id\t\tname\t\t\tbirthday\t\tgender");

            var stopwatch = Stopwatch.StartNew();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var id = reader.GetInt32(0);
                var name = reader.GetString(1);
                var birthday = reader.GetDateTime(2);
                var gender = reader.GetString(3);

                Console.WriteLine($"{id}\t\t{name}\t\t{birthday:d}\t\t{gender}");
            }

            stopwatch.Stop();
            Console.WriteLine();
            Console.WriteLine($"Taken time = {stopwatch.ElapsedMilliseconds}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    private static string RandomName(Random random)
    {
        var nameList = new[]
        {
            "Adam", "Alice", "Bob", "Carol", "David", "Eve", "Frank", "Grace", "Harry", "Ivy", "Jack", "Kate",
            "Luke", "Mary", "Nancy", "Oliver", "Peggy", "Quentin", "Rachel", "Steve", "Tina", "Uma", "Vera",
            "Walter", "Xavier", "Yara", "Zoe"
        };
        var surnameList = new[]
        {
            "Adams", "Baker", "Clark", "Davis", "Evans", "Foster", "Garcia", "Harris", "Irwin", "Jackson", "Klein",
            "Lopez", "Miller", "Nelson", "Owens", "Parker", "Quinn", "Roberts", "Smith", "Thomas", "Ulman",
            "Vargas", "Wilson", "Xu", "Young", "Zhang"
        };

        var name = nameList[random.Next(nameList.Length)];
        var surname = surnameList[random.Next(surnameList.Length)];

        return $"{name} {surname}";
    }

    private static DateTime RandomDate(Random random, DateTime start, DateTime now)
    {
        var range = now - start;
        var days = range.Days;
        var randDay = random.Next(days);

        return start.AddDays(randDay);
    }

    private static string RandomGender(Random random)
    {
        var gender = new[] { "male", "female" };

        var randomGender = gender[random.Next(gender.Length)];

        return randomGender;
    }

    private static string RandomFName(Random random)
    {
        var FName = new[]
        {
            "Frank", "Fred", "Felix", "Finn", "Floyd", "Ford", "Forrest", "Foster", "Fox", "Franco", "Franklin",
            "Fraser", "Fletcher", "Flynn", "Fidel", "Fyodor", "Fabian", "Fergus", "Fitzgerald", "Ferdinand"
        };

        var FSurname = new[]
        {
            "Ford", "Foster", "Fisher", "Fleming", "Ferguson", "Faulkner", "Freeman", "Franklin", "Farrell",
            "Frost", "Flowers", "Fritz", "Fuentes", "Flynn", "Farrar", "Finnegan", "Frost", "Feldman", "Fletcher",
            "Fuller"
        };

        var name = FName[random.Next(FSurname.Length)];
        var surname = FSurname[random.Next(FSurname.Length)];

        return $"{name} {surname}";
    }
}
