// See https://aka.ms/new-console-template for more information
using System.Data.SqlClient;

Console.WriteLine("Hello, World!");
// user id
string userId = Console.ReadLine();

Console.WriteLine($"User ID is: {userId}");

string connectionString = "YourConnectionString";
string query = "SELECT * FROM Users WHERE UserId = '" + userId + "'";
using (SqlConnection connection = new SqlConnection(connectionString))
{
    SqlCommand command = new SqlCommand(query, connection);
    connection.Open();
    SqlDataReader reader = command.ExecuteReader();
    while (reader.Read())
    {
        Console.WriteLine($"{reader["UserName"]}, {reader["Email"]}");
    }
}
