# Console SQL Injection Demo

A comprehensive C# console application demonstrating SQL Injection vulnerabilities and how to prevent them using parameterized queries. This project also showcases how GitHub Advanced Security (GHAS) can detect and help remediate these vulnerabilities.

## 📋 Overview

This educational project demonstrates:
- **Vulnerable SQL queries** that are susceptible to SQL Injection attacks
- **Secure SQL queries** using parameterized queries and prepared statements
- **SQLite database** with sample invoice data for testing
- **Interactive menu** to experiment with both vulnerable and secure approaches
- **GHAS detection** capabilities for identifying SQL Injection risks

## 🎯 Project Structure

```
console-sql-injection-demo/
├── Program.cs                          # Main application with vulnerable and secure queries
├── console-sql-injection-demo.csproj   # Project configuration
├── customers.db                         # SQLite database (auto-generated)
└── README.md                           # This file
```

## 📊 Database Schema

The application uses SQLite with three main tables:

### Customers
```sql
CREATE TABLE Customers (
    CustomerId INTEGER PRIMARY KEY,
    Name TEXT NOT NULL,
    Email TEXT NOT NULL,
    Phone TEXT
);
```

### Invoices
```sql
CREATE TABLE Invoices (
    InvoiceId INTEGER PRIMARY KEY,
    InvoiceNumber TEXT UNIQUE NOT NULL,
    CustomerId INTEGER NOT NULL,
    IssueDate DATE NOT NULL,
    Total DECIMAL(10,2) NOT NULL,
    Status TEXT DEFAULT 'Pending',
    FOREIGN KEY(CustomerId) REFERENCES Customers(CustomerId)
);
```

### InvoiceDetails
```sql
CREATE TABLE InvoiceDetails (
    DetailId INTEGER PRIMARY KEY,
    InvoiceId INTEGER NOT NULL,
    Description TEXT NOT NULL,
    Quantity INTEGER NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    FOREIGN KEY(InvoiceId) REFERENCES Invoices(InvoiceId)
);
```

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK or higher
- Visual Studio Code or Visual Studio

### Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd console-sql-injection-demo
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the project:
```bash
dotnet build
```

4. Run the application:
```bash
dotnet run
```

## 🔍 Testing SQL Injection

The application provides an interactive menu with different options:

```
=== DEMO SQL INJECTION CON SQLITE ===
1. Buscar factura por ID (VULNERABLE)
2. Buscar factura por ID (SEGURO con parametros)
3. Listar todas las facturas
4. Salir
```

### Testing Vulnerable Queries (Option 1)

**Option 1** demonstrates a VULNERABLE SQL query that concatenates user input directly:

```csharp
string query = $"SELECT f.FacturaId, f.NumeroFactura, c.Nombre, f.FechaEmision, f.Total 
                FROM Facturas f 
                JOIN Clientes c ON f.ClienteId = c.ClienteId 
                WHERE f.FacturaId = '{facturaId}'";
```

#### SQL Injection Attack Examples

Try these inputs to see SQL Injection in action:

1. **Bypass Authentication (Return all records):**
   ```
   1 OR 1=1 --
   ```
   This will return ALL invoices instead of just one.

2. **Comment out the rest of the query:**
   ```
   1; --
   ```
   This safely terminates the query with a comment.

3. **Union-based injection (Access unauthorized data):**
   ```
   1 UNION SELECT ClienteId, Nombre, Email, Telefono, NULL FROM Clientes --
   ```
   Attempts to retrieve customer data through a UNION query.

4. **Test for database structure:**
   ```
   1' AND '1'='1
   ```
   Returns results if the database accepts the injection.

### Testing Secure Queries (Option 2)

**Option 2** demonstrates the SECURE approach using parameterized queries:

```csharp
string query = "SELECT f.FacturaId, f.NumeroFactura, c.Nombre, f.FechaEmision, f.Total 
                FROM Facturas f 
                JOIN Clientes c ON f.ClienteId = c.ClienteId 
                WHERE f.FacturaId = @FacturaId";

using (SQLiteCommand command = new SQLiteCommand(query, connection))
{
    // Bind parameters - user input is treated as data, not code
    command.Parameters.AddWithValue("@FacturaId", facturaId);
    // Execute query
}
```

With parameterized queries:
- User input `1 OR 1=1 --` is treated as a string literal, not SQL code
- The query will search for an invoice with ID exactly matching the input
- SQL Injection attacks are completely mitigated

## 🛡️ Security Comparison

| Aspect | Vulnerable Code | Secure Code |
|--------|-----------------|------------|
| **Query Construction** | String concatenation | Parameterized queries |
| **User Input Treatment** | Interpreted as SQL code | Treated as literal data |
| **SQL Injection Risk** | ❌ High Risk | ✅ Protected |
| **Example** | `"WHERE id = '" + input + "'"` | `"WHERE id = @id"` + `Parameters.Add()` |

## 🔐 GitHub Advanced Security (GHAS)

This project is designed to work with GitHub Advanced Security to detect and prevent SQL Injection vulnerabilities.

### What is GHAS?

GitHub Advanced Security (GHAS) provides:
- **Code scanning** with custom queries to identify security vulnerabilities
- **Secret scanning** to prevent accidental credential exposure
- **Dependency scanning** to identify vulnerable third-party libraries
- **Security advisories** and recommendations

### Detecting SQL Injection with GHAS

#### 1. Enable Code Scanning
- Go to your repository on GitHub
- Navigate to **Settings** → **Security & analysis**
- Enable **Code scanning** with **CodeQL**

#### 2. Custom Queries for SQL Injection
GHAS can be configured with custom CodeQL queries to detect:
- String concatenation in SQL queries
- Missing parameterized queries
- Dynamic SQL construction

#### 3. What GHAS Will Flag in This Project

GHAS will identify the vulnerable code in Option 1:
```csharp
string query = $"SELECT ... WHERE f.FacturaId = '{facturaId}'";  // ⚠️ FLAGGED
```

And recognize the secure pattern in Option 2:
```csharp
command.Parameters.AddWithValue("@FacturaId", facturaId);  // ✅ SAFE
```

#### 4. Review Alerts
- Check the **Security** tab → **Code scanning alerts**
- Each alert shows:
  - The vulnerable code location
  - Severity level (Critical, High, Medium, Low)
  - Recommended fixes
  - Links to documentation

### Benefits of Using GHAS

1. **Early Detection**: Catches vulnerabilities before they reach production
2. **Education**: Developers learn secure coding practices
3. **Automation**: Continuous scanning on every push
4. **Reporting**: Track security metrics over time
5. **Remediation Guidance**: Specific recommendations for fixes

## 📚 Security Best Practices

### ✅ DO's - Secure Coding Practices

1. **Always use parameterized queries:**
   ```csharp
   using (SqlCommand command = new SqlCommand(query, connection))
   {
       command.Parameters.AddWithValue("@userId", userId);
       // Execute
   }
   ```

2. **Use Object-Relational Mapping (ORM) frameworks:**
   ```csharp
   // Entity Framework Core
   var invoice = dbContext.Facturas
       .Where(f => f.FacturaId == facturaId)
       .FirstOrDefault();
   ```

3. **Validate and sanitize input:**
   ```csharp
   if (!int.TryParse(input, out int id))
   {
       throw new ArgumentException("Invalid ID format");
   }
   ```

4. **Use principle of least privilege** for database connections

5. **Enable security headers and CORS policies** in web applications

### ❌ DON'Ts - Vulnerable Patterns

1. **Never concatenate user input in SQL:**
   ```csharp
   // VULNERABLE - DO NOT USE
   string query = "SELECT * FROM Users WHERE UserId = '" + userId + "'";
   ```

2. **Avoid dynamic SQL without parameterization:**
   ```csharp
   // VULNERABLE - DO NOT USE
   string query = $"SELECT * FROM Users WHERE UserName = '{userName}'";
   ```

3. **Don't trust user input without validation:**
   ```csharp
   // VULNERABLE - DO NOT USE
   int id = int.Parse(userInput); // Could throw or be manipulated
   ```

4. **Avoid displaying detailed error messages** that reveal database structure

## 🧪 Sample Data

The application automatically creates sample data with:
- **5 customers** with contact information
- **8 invoices** with different states and totals
- **9 invoice line items** with descriptions, quantities, and prices

### Sample Queries You Can Try

**Find invoice by ID (Secure):**
```
Input: 1
Output: Invoice INV-2025-001 for Tech Solutions S.A., Total: $1500.00
```

**Test SQL Injection (Vulnerable):**
```
Input: 1 OR 1=1 --
Output: All invoices displayed (vulnerability demonstrated)
```

## 📖 Learning Resources

- [OWASP SQL Injection Prevention Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/SQL_Injection_Prevention_Cheat_Sheet.html)
- [Microsoft Secure Coding Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/security/secure-coding-guidelines)
- [GitHub Advanced Security Documentation](https://docs.github.com/en/get-started/learning-about-github/about-github-advanced-security)
- [CodeQL SQL Injection Queries](https://codeql.github.com/docs/)

## 🐛 Common Vulnerabilities

| Vulnerability | Description | Mitigation |
|---|---|---|
| **SQL Injection** | User input interpreted as SQL code | Use parameterized queries |
| **Insecure Deserialization** | Untrusted data parsed as objects | Validate data types |
| **Authentication Bypass** | Weak credential validation | Use parameterized queries + strong auth |
| **Data Exposure** | Sensitive data in error messages | Handle exceptions gracefully |

## 🔄 Development Workflow with GHAS

1. **Write code** with potential vulnerabilities (for demonstration)
2. **Push to GitHub** and GHAS automatically scans
3. **Review alerts** in the Security tab
4. **Implement fixes** using secure patterns
5. **Verify** that alerts are resolved
6. **Monitor** continuous scanning results

## 📝 Contributing

This is an educational project. Feel free to:
- Add more SQL Injection examples
- Implement additional secure patterns
- Create more sample data
- Add unit tests for secure queries

## ⚖️ License

This project is provided for educational purposes.

## 📞 Support

For issues, questions, or suggestions, please open an issue in the repository.

## 🎓 Educational Purpose

**IMPORTANT**: This code contains intentional vulnerabilities for educational purposes. Do NOT use the vulnerable patterns in production code. Always follow secure coding practices as demonstrated in Option 2.

---

**Last Updated**: October 2025
**Framework**: .NET 9.0
**Database**: SQLite
