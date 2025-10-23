using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

// SQLite database path
string dbPath = "invoices.db";
string connectionString = $"Data Source={dbPath};Version=3;";

// Create database and tables
InitializeDatabase(connectionString);

// Insert sample data
InsertSampleData(connectionString);

// Interactive menu
while (true)
{
    Console.WriteLine("\n╔══════════════════════════════════════════╗");
    Console.WriteLine("║    🔒 SQL INJECTION DEMO - SQLITE       ║");
    Console.WriteLine("╚══════════════════════════════════════════╝");
    Console.WriteLine("\n🔍 Query Testing Options:");
    Console.WriteLine("  1️⃣  Search Invoice by ID (VULNERABLE to SQL Injection)");
    Console.WriteLine("  2️⃣  Search Invoice by ID (SAFE with Parameters)");
    Console.WriteLine("  3️⃣  List All Invoices");
    Console.WriteLine("  4️⃣  Exit");
    Console.Write("\n📌 Select an option: ");
    
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
            ListAllInvoices(connectionString);
            break;
        case "4":
            Console.WriteLine("\n👋 Goodbye! Stay secure!\n");
            return;
        default:
            Console.WriteLine("\n❌ Invalid option. Please try again.");
            break;
    }
}

/// <summary>
/// Initializes the SQLite database with required tables
/// </summary>
void InitializeDatabase(string connectionString)
{
    try
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            
            // Create Customers table
            string createCustomersTable = @"
                CREATE TABLE IF NOT EXISTS Customers (
                    CustomerId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    Phone TEXT
                );";
            
            // Create Invoices table
            string createInvoicesTable = @"
                CREATE TABLE IF NOT EXISTS Invoices (
                    InvoiceId INTEGER PRIMARY KEY AUTOINCREMENT,
                    InvoiceNumber TEXT UNIQUE NOT NULL,
                    CustomerId INTEGER NOT NULL,
                    IssueDate DATE NOT NULL,
                    Total DECIMAL(10,2) NOT NULL,
                    Status TEXT DEFAULT 'Pending',
                    FOREIGN KEY(CustomerId) REFERENCES Customers(CustomerId)
                );";
            
            // Create Invoice Details table
            string createDetailsTable = @"
                CREATE TABLE IF NOT EXISTS InvoiceDetails (
                    DetailId INTEGER PRIMARY KEY AUTOINCREMENT,
                    InvoiceId INTEGER NOT NULL,
                    Description TEXT NOT NULL,
                    Quantity INTEGER NOT NULL,
                    UnitPrice DECIMAL(10,2) NOT NULL,
                    FOREIGN KEY(InvoiceId) REFERENCES Invoices(InvoiceId)
                );";
            
            ExecuteSQL(connection, createCustomersTable);
            ExecuteSQL(connection, createInvoicesTable);
            ExecuteSQL(connection, createDetailsTable);
            
            Console.WriteLine("✅ Database initialized successfully\n");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error initializing database: {ex.Message}");
    }
}

/// <summary>
/// Inserts sample invoice data into the database
/// </summary>
void InsertSampleData(string connectionString)
{
    try
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            
            // Check if data already exists
            using (SQLiteCommand checkCmd = new SQLiteCommand("SELECT COUNT(*) FROM Invoices", connection))
            {
                int count = (int)checkCmd.ExecuteScalar();
                if (count > 0)
                {
                    Console.WriteLine("✅ Sample data already exists in the database\n");
                    return;
                }
            }
            
            // Insert customers
            string[] customers = new[]
            {
                ("Tech Solutions S.A.", "contact@techsol.com", "555-0001"),
                ("Global Imports Ltd.", "sales@globalimports.com", "555-0002"),
                ("Local Services Inc.", "info@localservices.com", "555-0003"),
                ("Premium Consulting Group", "admin@premiumconsulting.com", "555-0004"),
                ("Digital Marketing Pro", "support@digitalmarketingpro.com", "555-0005")
            };
            
            var customerIds = new List<int>();
            
            foreach (var (name, email, phone) in customers)
            {
                string insertCustomer = $"INSERT INTO Customers (Name, Email, Phone) VALUES ('{name}', '{email}', '{phone}')";
                ExecuteSQL(connection, insertCustomer);
            }
            
            // Get customer IDs
            using (SQLiteCommand getCustomersCmd = new SQLiteCommand("SELECT CustomerId FROM Customers", connection))
            {
                using (SQLiteDataReader reader = getCustomersCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        customerIds.Add((int)reader["CustomerId"]);
                    }
                }
            }
            
            // Insert invoices with details
            var invoices = new[]
            {
                ("INV-2025-001", 1, "2025-01-15", 1500.00m),
                ("INV-2025-002", 2, "2025-01-18", 2300.50m),
                ("INV-2025-003", 3, "2025-02-01", 850.00m),
                ("INV-2025-004", 1, "2025-02-05", 3200.75m),
                ("INV-2025-005", 4, "2025-02-10", 1100.00m),
                ("INV-2025-006", 5, "2025-02-15", 2650.25m),
                ("INV-2025-007", 2, "2025-02-20", 1900.00m),
                ("INV-2025-008", 3, "2025-02-25", 2200.50m),
            };
            
            foreach (var (numInvoice, customerId, date, total) in invoices)
            {
                string insertInvoice = $"INSERT INTO Invoices (InvoiceNumber, CustomerId, IssueDate, Total, Status) VALUES ('{numInvoice}', {customerId}, '{date}', {total.ToString().Replace(',', '.')}, 'Paid')";
                ExecuteSQL(connection, insertInvoice);
            }
            
            // Insert invoice details
            var details = new[]
            {
                (1, "Consulting Services", 5, 300.00m),
                (1, "Software License", 1, 0.00m),
                (2, "System Implementation", 1, 2300.50m),
                (3, "Annual Maintenance", 1, 850.00m),
                (4, "Custom Development", 10, 320.00m),
                (5, "Online Training", 2, 550.00m),
                (6, "Security Audit", 1, 2650.25m),
                (7, "Technical Support", 6, 316.67m),
                (8, "System Updates", 4, 550.00m),
            };
            
            foreach (var (invoiceId, description, quantity, price) in details)
            {
                string insertDetail = $"INSERT INTO InvoiceDetails (InvoiceId, Description, Quantity, UnitPrice) VALUES ({invoiceId}, '{description}', {quantity}, {price.ToString().Replace(',', '.')})";
                ExecuteSQL(connection, insertDetail);
            }
            
            Console.WriteLine("✅ Sample data inserted successfully\n");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error inserting data: {ex.Message}");
    }
}

/// <summary>
/// Demonstrates a VULNERABLE query susceptible to SQL Injection
/// </summary>
void DemoVulnerableQuery(string connectionString)
{
    Console.Write("\n🎯 Enter the Invoice ID (VULNERABLE to SQL Injection!): ");
    string invoiceId = Console.ReadLine() ?? "";
    
    Console.WriteLine($"\n⚠️  VULNERABLE QUERY:\n");
    
    // This is the INSECURE way - vulnerable to SQL Injection
    string query = $"SELECT i.InvoiceId, i.InvoiceNumber, c.Name, i.IssueDate, i.Total FROM Invoices i JOIN Customers c ON i.CustomerId = c.CustomerId WHERE i.InvoiceId = '{invoiceId}'";
    
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(query);
    Console.ResetColor();
    
    Console.WriteLine("\n💡 SQL Injection Examples You Can Try:");
    Console.WriteLine("  • 1 OR 1=1 --   (returns all invoices)");
    Console.WriteLine("  • 1; DROP TABLE Invoices; --   (attempts to delete table)");
    Console.WriteLine("  • 1 UNION SELECT * FROM Customers --   (unauthorized access)\n");
    
    try
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        Console.WriteLine("📋 Results:");
                        while (reader.Read())
                        {
                            Console.WriteLine($"  📌 ID: {reader["InvoiceId"]}, Number: {reader["InvoiceNumber"]}, Customer: {reader["Name"]}, Date: {reader["IssueDate"]}, Total: ${reader["Total"]}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("❌ No invoices found");
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
/// Demonstrates a SAFE query using parameterized statements
/// </summary>
void DemoSafeQuery(string connectionString)
{
    Console.Write("\n🎯 Enter the Invoice ID (SAFE with Parameters): ");
    string invoiceId = Console.ReadLine() ?? "";
    
    Console.WriteLine($"\n✅ SECURE QUERY (with Parameterized Queries):\n");
    
    string query = "SELECT i.InvoiceId, i.InvoiceNumber, c.Name, i.IssueDate, i.Total FROM Invoices i JOIN Customers c ON i.CustomerId = c.CustomerId WHERE i.InvoiceId = @InvoiceId";
    
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(query);
    Console.ResetColor();
    Console.WriteLine($"Parameter @InvoiceId = '{invoiceId}'\n");
    
    try
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                // This is the SECURE way - using parameterized queries
                command.Parameters.AddWithValue("@InvoiceId", invoiceId);
                
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        Console.WriteLine("📋 Results:");
                        while (reader.Read())
                        {
                            Console.WriteLine($"  📌 ID: {reader["InvoiceId"]}, Number: {reader["InvoiceNumber"]}, Customer: {reader["Name"]}, Date: {reader["IssueDate"]}, Total: ${reader["Total"]}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("❌ No invoices found");
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
/// Lists all invoices in the database
/// </summary>
void ListAllInvoices(string connectionString)
{
    Console.WriteLine("\n📋 ALL INVOICES:\n");
    
    string query = "SELECT i.InvoiceId, i.InvoiceNumber, c.Name, i.IssueDate, i.Total, i.Status FROM Invoices i JOIN Customers c ON i.CustomerId = c.CustomerId ORDER BY i.InvoiceId";
    
    try
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    int count = 0;
                    Console.WriteLine("┌────┬──────────────┬──────────────────────┬────────────┬──────────┬─────────┐");
                    Console.WriteLine("│ ID │ Invoice Num  │ Customer             │ Date       │ Total    │ Status  │");
                    Console.WriteLine("├────┼──────────────┼──────────────────────┼────────────┼──────────┼─────────┤");
                    
                    while (reader.Read())
                    {
                        string id = reader["InvoiceId"].ToString();
                        string invNum = reader["InvoiceNumber"].ToString();
                        string customer = reader["Name"].ToString().PadRight(20);
                        string date = reader["IssueDate"].ToString();
                        string total = String.Format("{0:C}", reader["Total"]);
                        string status = reader["Status"].ToString();
                        
                        Console.WriteLine($"│ {id.PadRight(2)} │ {invNum.PadRight(12)} │ {customer} │ {date} │ {total.PadRight(8)} │ {status.PadRight(7)} │");
                        count++;
                    }
                    Console.WriteLine("└────┴──────────────┴──────────────────────┴────────────┴──────────┴─────────┘");
                    Console.WriteLine($"\n📊 Total: {count} invoices\n");
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
void ExecuteSQL(SQLiteConnection connection, string query)
{
    using (SQLiteCommand command = new SQLiteCommand(query, connection))
    {
        command.ExecuteNonQuery();
    }
}