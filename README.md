# 🏗️ Industrial Equipment Rental & Service Yard

A full-stack database project built for the **Introduction to Databases** course at the **Faculty of Computer and Artificial Intelligence, Cairo University**.

The system models a heavy-machinery rental firm that manages industrial equipment (excavators, cranes, generators), contractor relationships, service yard operations, and safety inspections — backed by a fully normalized relational database on **Microsoft SQL Server**.

---

## 👥 Team Members

| Name |
|------|
| Khaled Yousry Mohamed |


---

## 📋 Project Overview

A heavy-machinery firm rents out specialized industrial equipment to construction contractors. The system handles:

- Cataloging industrial equipment and tracking its current location (yard or client)
- Managing contractor profiles and their full rental history
- Creating rental agreements and calculating total rental costs
- Logging safety release inspections and maintenance tasks
- Monitoring yard capacity and equipment availability for future bookings

---

## 🗄️ Database Design

### Entities & Relationships

| Table | Description |
|---|---|
| `ServiceYard` | Yards where equipment is stored when not in use |
| `Equipment` | Industrial machines with model, engine power, and hourly rate |
| `Technician` | Yard-based technicians who perform safety inspections |
| `Contractor` | Construction companies that rent equipment |
| `RentalAgreement` | Rental contracts linking equipment to contractors |
| `SafetyInspection` | Pre-release and post-return condition inspections |

### Key Design Decisions

- `Equipment.Status` is constrained to `'Available'`, `'Rented'`, or `'Under Maintenance'`
- `SafetyInspection` references both `RentalAgreement` **and** `Equipment` directly — reflecting that inspections are performed on the machine itself
- `RentalAgreement.TotalCost` uses `DECIMAL(12,2)` for financial precision
- All referential integrity is enforced via Foreign Key constraints at the database level

---

## 🗂️ Repository Structure

```
industrial-equipment-rental-db/
│
├── README.md
│
├── database/
│   ├── 01_create_tables.sql      # Schema: all CREATE TABLE statements with constraints
│   ├── 02_insert_data.sql        # Populated sample data (55+ rows across 6 tables)
│   └── 03_inquiry_queries.sql    # 6 analytical queries (reports)
│
├── diagrams/
│   ├── conceptual_erd.png        # Chen notation ERD
│   └── physical_erd.png          # Physical ERD with data types and keys
│
└── application/
    └── EquipmentRentalApp/       # C# Windows Forms application
        ├── Program.cs
        ├── Form1.cs
        ├── Form1.Designer.cs
        ├── DatabaseClient.cs
        ├── SqlStatements.cs
        └── EquipmentRentalApp.csproj
```

---

## 🖥️ Application Features

Built with **C# Windows Forms** connected to SQL Server via `Microsoft.Data.SqlClient`.

### Tables Tab
- Browse all 6 database tables via dropdown
- Load and display any table's data in a grid view

### Operations Tab
| Operation | Tables |
|---|---|
| INSERT | `Contractor`, `Equipment` |
| UPDATE | `Equipment` (status), `Contractor` (credit limit) |
| DELETE | `Contractor`, `Equipment` |

### Reports Tab
Executes all 6 inquiry queries and displays results inline:

1. Most rented equipment model
2. Service yards with no rentals last month
3. Technician with most inspections last month
4. Contractors with no new agreements last month
5. Available machines per service yard last month
6. Total rental hours per contractor last month

---

## ⚙️ Setup & Running

### Prerequisites

- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or SQL Server Express / LocalDB)
- [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
- [.NET 6+ SDK](https://dotnet.microsoft.com/download)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (or VS Code with C# extension)

### Step 1 — Set up the Database

Open SSMS and run the SQL files **in order**:

```
1. database/01_create_tables.sql
2. database/02_insert_data.sql
```

### Step 2 — Run the Application

```bash
cd application/EquipmentRentalApp
dotnet run
```

Or open `EquipmentRentalApp.csproj` in Visual Studio and press **F5**.

### Step 3 — Connect

In the application's **Connection** field at the top, enter your connection string:

```
Server=(localdb)\MSSQLLocalDB;Database=EquipmentRentalDB;Trusted_Connection=True;TrustServerCertificate=True;
```

Then click **Test** to verify, and **Load** to connect.

---

## 📊 Inquiry Queries

All 6 analytical queries are in `database/03_inquiry_queries.sql` and are also embedded in the application under the **Reports** tab.

| # | Question |
|---|---|
| 1 | Which equipment model had the most rental agreements? |
| 2 | Which service yards had no rentals originating from them last month? |
| 3 | Which technician performed the most safety inspections last month? |
| 4 | Which contractors did not form any new rental agreements last month? |
| 5 | What machines were available at each service yard last month? |
| 6 | For each contractor, what was the total hours of rented equipment last month? |

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Database | Microsoft SQL Server (LocalDB) |
| Language | C# (.NET 6) |
| UI Framework | Windows Forms |
| DB Connector | `Microsoft.Data.SqlClient` |
| IDE | Visual Studio 2022 |
| Design Tools | draw.io (ERD diagrams) |

---

## 📌 Course Information

**Course:** Introduction to Databases
**Faculty:** Faculty of Computer and Artificial Intelligence (FCAI)
**University:** Cairo University
