using System;
using System.Data;
using Microsoft.Data.Sqlite;
using System.IO;

// Ruta de la base de datos SQLite
string dbPath = "customers.db";
string connectionString = $"Data Source={dbPath}";

// Crear la base de datos y tablas
InitializeDatabase(connectionString);

// Insertar datos de ejemplo
InsertSampleData(connectionString);

// Menú interactivo
while (true)
{
    Console.WriteLine("\n=== SQL INJECTION DEMO - CUSTOMER SEARCH ===");
    Console.WriteLine("1. Search customer by ID (VULNERABLE)");
    Console.WriteLine("2. Search customer by ID (SAFE with parameters)");
    Console.WriteLine("3. List all customers");
    Console.WriteLine("4. Exit");
    Console.Write("Select an option: ");
    
    string option = Console.ReadLine() ?? "";
    
    switch (option)
    {
        case "1":
            DemoVulnerableQuery(connectionString);
            break;
        case "2":
            DemoSafeQuery(connectionString);
            break;
        case "3":
            ListAllCustomers(connectionString);
            break;
        case "4":
            Console.WriteLine("Goodbye!");
            return;
        default:
            Console.WriteLine("Invalid option");
            break;
    }
}

/// <summary>
/// Initializes the SQLite database with the necessary tables
/// </summary>
void InitializeDatabase(string connectionString)
{
    try
    {
        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            
            // Create customers table
            string createCustomersTable = @"
                CREATE TABLE IF NOT EXISTS Customers (
                    CustomerId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    Phone TEXT NOT NULL,
                    Country TEXT NOT NULL
                );";
            
            ExecuteSQL(connection, createCustomersTable);
            
            Console.WriteLine("✓ Database initialized successfully");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error initializing database: {ex.Message}");
    }
}

/// <summary>
/// Inserts sample data into the database
/// </summary>
void InsertSampleData(string connectionString)
{
    try
    {
        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            
            // Check if data already exists
            using (SqliteCommand checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Customers", connection))
            {
                var countResult = checkCmd.ExecuteScalar();
                int count = countResult is int ? (int)countResult : 0;
                if (count > 0)
                {
                    Console.WriteLine("✓ Sample data already exists in the database");
                    return;
                }
            }
            
            // Insert sample customers
            string[] customers = new[]
            {
                "INSERT INTO Customers (Name, Email, Phone, Country) VALUES ('John Smith', 'john.smith@example.com', '+1-555-0101', 'USA')",
                "INSERT INTO Customers (Name, Email, Phone, Country) VALUES ('Mary Johnson', 'mary.johnson@example.com', '+1-555-0102', 'USA')",
                "INSERT INTO Customers (Name, Email, Phone, Country) VALUES ('Robert Williams', 'robert.williams@example.com', '+1-555-0103', 'Canada')",
                "INSERT INTO Customers (Name, Email, Phone, Country) VALUES ('Patricia Brown', 'patricia.brown@example.com', '+1-555-0104', 'USA')",
                "INSERT INTO Customers (Name, Email, Phone, Country) VALUES ('Michael Davis', 'michael.davis@example.com', '+44-20-7946-0958', 'United Kingdom')",
                "INSERT INTO Customers (Name, Email, Phone, Country) VALUES ('Jennifer Garcia', 'jennifer.garcia@example.com', '+1-555-0106', 'USA')",
                "INSERT INTO Customers (Name, Email, Phone, Country) VALUES ('David Rodriguez', 'david.rodriguez@example.com', '+34-91-123-4567', 'Spain')",
                "INSERT INTO Customers (Name, Email, Phone, Country) VALUES ('Linda Martinez', 'linda.martinez@example.com', '+33-1-42-68-53-00', 'France')"
            };
            
            foreach (string query in customers)
            {
                ExecuteSQL(connection, query);
            }
            
            Console.WriteLine("✓ Sample customers inserted successfully");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error inserting data: {ex.Message}");
    }
}

/// <summary>
/// Demonstrates a VULNERABLE query to SQL Injection
/// </summary>
void DemoVulnerableQuery(string connectionString)
{
    Console.Write("\nEnter customer ID (⚠️ VULNERABLE to SQL Injection!): ");
    string customerId = Console.ReadLine() ?? "";
    
    Console.WriteLine($"\n⚠️ VULNERABLE QUERY:\n");
    
    // This is the UNSAFE way - vulnerable to SQL Injection
    string query = $"SELECT CustomerId, Name, Email, Phone, Country FROM Customers WHERE CustomerId = {customerId}";
    
    Console.WriteLine(query);
    Console.WriteLine("\nExamples of SQL Injection you can try:");
    Console.WriteLine("  • 1 OR 1=1   (returns all customers)");
    Console.WriteLine("  • 1; DROP TABLE Customers; --   (tries to delete the table)");
    Console.WriteLine("  • 1 UNION SELECT * FROM Customers --   (unauthorized access)\n");
    
    try
    {
        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (SqliteCommand command = new SqliteCommand(query, connection))
            {
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        Console.WriteLine("📋 Results:");
                        while (reader.Read())
                        {
                            Console.WriteLine($"  ID: {reader["CustomerId"]}, Name: {reader["Name"]}, Email: {reader["Email"]}, Phone: {reader["Phone"]}, Country: {reader["Country"]}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No customers found");
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
    }
}

/// <summary>
/// Demonstrates a SAFE query using prepared parameters
/// </summary>
void DemoSafeQuery(string connectionString)
{
    Console.Write("\nEnter customer ID (✓ SAFE with parameters): ");
    string customerId = Console.ReadLine() ?? "";
    
    Console.WriteLine($"\n✓ SAFE QUERY (with prepared parameters):\n");
    
    string query = "SELECT CustomerId, Name, Email, Phone, Country FROM Customers WHERE CustomerId = @CustomerId";
    
    Console.WriteLine(query);
    Console.WriteLine($"Parameter @CustomerId = '{customerId}'\n");
    
    try
    {
        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (SqliteCommand command = new SqliteCommand(query, connection))
            {
                // This is the SAFE way - using prepared parameters
                command.Parameters.AddWithValue("@CustomerId", customerId);
                
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        Console.WriteLine("📋 Results:");
                        while (reader.Read())
                        {
                            Console.WriteLine($"  ID: {reader["CustomerId"]}, Name: {reader["Name"]}, Email: {reader["Email"]}, Phone: {reader["Phone"]}, Country: {reader["Country"]}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No customers found");
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
    }
}

/// <summary>
/// Lists all customers from the database
/// </summary>
void ListAllCustomers(string connectionString)
{
    Console.WriteLine("\n📋 ALL CUSTOMERS:\n");
    
    string query = "SELECT CustomerId, Name, Email, Phone, Country FROM Customers ORDER BY CustomerId";
    
    try
    {
        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (SqliteCommand command = new SqliteCommand(query, connection))
            {
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    int count = 0;
                    while (reader.Read())
                    {
                        Console.WriteLine($"[{reader["CustomerId"]}] {reader["Name"]} | Email: {reader["Email"]} | Phone: {reader["Phone"]} | Country: {reader["Country"]}");
                        count++;
                    }
                    Console.WriteLine($"\nTotal: {count} customers");
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
    }
}

/// <summary>
/// Executes a SQL statement
/// </summary>
void ExecuteSQL(SqliteConnection connection, string query)
{
    using (SqliteCommand command = new SqliteCommand(query, connection))
    {
        command.ExecuteNonQuery();
    }
}