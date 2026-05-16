namespace EquipmentRentalApp;

internal static class SqlStatements
{
    public const string DefaultConnectionString =
        "Server=localhost;Database=EquipmentRentalDB;Trusted_Connection=True;TrustServerCertificate=True;";

    public const string InsertContractor = """
        INSERT INTO Contractor (CompanyName, CreditLimit, ContactPerson, Phone, Address)
        VALUES (@CompanyName, @CreditLimit, @ContactPerson, @Phone, @Address);
        """;

    public const string InsertEquipment = """
        INSERT INTO Equipment (YardID, Model, EnginePower, HourlyRate, Status)
        VALUES (@YardID, @Model, @EnginePower, @HourlyRate, @Status);
        """;

    public const string UpdateEquipmentStatus = """
        UPDATE Equipment
        SET Status = @Status
        WHERE EquipmentID = @EquipmentID;
        """;

    public const string UpdateContractorCreditLimit = """
        UPDATE Contractor
        SET CreditLimit = @CreditLimit
        WHERE ContractorID = @ContractorID;
        """;

    public const string DeleteContractorWithoutRentals = """
        DELETE FROM Contractor
        WHERE ContractorID = @ContractorID
          AND NOT EXISTS (
              SELECT 1
              FROM RentalAgreement
              WHERE ContractorID = @ContractorID
          );
        """;

    public const string DeleteAvailableEquipmentWithoutHistory = """
        DELETE FROM Equipment
        WHERE EquipmentID = @EquipmentID
          AND Status = 'Available'
          AND NOT EXISTS (
              SELECT 1
              FROM RentalAgreement
              WHERE EquipmentID = @EquipmentID
          )
          AND NOT EXISTS (
              SELECT 1
              FROM SafetyInspection
              WHERE EquipmentID = @EquipmentID
          );
        """;

    public static readonly Dictionary<string, string> TableQueries = new()
    {
        ["ServiceYard"] = """
            SELECT YardID, Location, Capacity, ContactNumber
            FROM ServiceYard
            ORDER BY YardID;
            """,
        ["Technician"] = """
            SELECT t.TechnicianID, t.Name, t.Specialization, t.Phone,
                   y.Location AS YardLocation
            FROM Technician AS t
            INNER JOIN ServiceYard AS y ON y.YardID = t.YardID
            ORDER BY t.TechnicianID;
            """,
        ["Equipment"] = """
            SELECT e.EquipmentID, e.Model, e.EnginePower, e.HourlyRate, e.Status,
                   y.Location AS YardLocation
            FROM Equipment AS e
            INNER JOIN ServiceYard AS y ON y.YardID = e.YardID
            ORDER BY e.EquipmentID;
            """,
        ["Contractor"] = """
            SELECT ContractorID, CompanyName, CreditLimit, ContactPerson, Phone, Address
            FROM Contractor
            ORDER BY ContractorID;
            """,
        ["RentalAgreement"] = """
            SELECT ra.AgreementID, e.Model AS EquipmentModel, c.CompanyName,
                   ra.StartDate, ra.EndDate, ra.TotalCost, ra.Status
            FROM RentalAgreement AS ra
            INNER JOIN Equipment AS e ON e.EquipmentID = ra.EquipmentID
            INNER JOIN Contractor AS c ON c.ContractorID = ra.ContractorID
            ORDER BY ra.AgreementID;
            """,
        ["SafetyInspection"] = """
            SELECT si.InspectionID, si.InspectionDate, si.InspectionType, si.ConditionStatus,
                   e.Model AS EquipmentModel, t.Name AS TechnicianName,
                   si.ChecklistNotes, si.MaintenanceNotes
            FROM SafetyInspection AS si
            INNER JOIN Equipment AS e ON e.EquipmentID = si.EquipmentID
            INNER JOIN Technician AS t ON t.TechnicianID = si.TechnicianID
            ORDER BY si.InspectionID;
            """
    };

    public static readonly Dictionary<string, string> Reports = new()
    {
        ["1. Most rented equipment model"] = """
            SELECT TOP 1 WITH TIES
                   e.Model,
                   COUNT(*) AS AgreementCount
            FROM Equipment AS e
            INNER JOIN RentalAgreement AS ra ON ra.EquipmentID = e.EquipmentID
            GROUP BY e.Model
            ORDER BY COUNT(*) DESC;
            """,

        ["2. Service yards with no rentals last month"] = """
            DECLARE @StartLastMonth date =
                DATEFROMPARTS(YEAR(DATEADD(MONTH, -1, GETDATE())),
                              MONTH(DATEADD(MONTH, -1, GETDATE())), 1);
            DECLARE @StartThisMonth date =
                DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);

            SELECT y.YardID, y.Location, y.Capacity, y.ContactNumber
            FROM ServiceYard AS y
            WHERE NOT EXISTS (
                SELECT 1
                FROM Equipment AS e
                INNER JOIN RentalAgreement AS ra ON ra.EquipmentID = e.EquipmentID
                WHERE e.YardID = y.YardID
                  AND ra.StartDate >= @StartLastMonth
                  AND ra.StartDate < @StartThisMonth
            )
            ORDER BY y.YardID;
            """,

        ["3. Technician with max inspections last month"] = """
            DECLARE @StartLastMonth date =
                DATEFROMPARTS(YEAR(DATEADD(MONTH, -1, GETDATE())),
                              MONTH(DATEADD(MONTH, -1, GETDATE())), 1);
            DECLARE @StartThisMonth date =
                DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);

            SELECT TOP 1 WITH TIES
                   t.TechnicianID,
                   t.Name,
                   t.Specialization,
                   COUNT(*) AS InspectionCount
            FROM Technician AS t
            INNER JOIN SafetyInspection AS si ON si.TechnicianID = t.TechnicianID
            WHERE si.InspectionDate >= @StartLastMonth
              AND si.InspectionDate < @StartThisMonth
            GROUP BY t.TechnicianID, t.Name, t.Specialization
            ORDER BY COUNT(*) DESC;
            """,

        ["4. Contractors with no new rentals last month"] = """
            DECLARE @StartLastMonth date =
                DATEFROMPARTS(YEAR(DATEADD(MONTH, -1, GETDATE())),
                              MONTH(DATEADD(MONTH, -1, GETDATE())), 1);
            DECLARE @StartThisMonth date =
                DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);

            SELECT c.ContractorID, c.CompanyName, c.CreditLimit, c.ContactPerson, c.Phone
            FROM Contractor AS c
            WHERE NOT EXISTS (
                SELECT 1
                FROM RentalAgreement AS ra
                WHERE ra.ContractorID = c.ContractorID
                  AND ra.StartDate >= @StartLastMonth
                  AND ra.StartDate < @StartThisMonth
            )
            ORDER BY c.CompanyName;
            """,

        ["5. Available machines at each yard last month"] = """
            DECLARE @StartLastMonth date =
                DATEFROMPARTS(YEAR(DATEADD(MONTH, -1, GETDATE())),
                              MONTH(DATEADD(MONTH, -1, GETDATE())), 1);
            DECLARE @StartThisMonth date =
                DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);

            SELECT y.YardID,
                   y.Location,
                   e.EquipmentID,
                   e.Model,
                   e.EnginePower,
                   e.HourlyRate,
                   e.Status
            FROM ServiceYard AS y
            INNER JOIN Equipment AS e ON e.YardID = y.YardID
            WHERE e.Status = 'Available'
              AND NOT EXISTS (
                  SELECT 1
                  FROM RentalAgreement AS ra
                  WHERE ra.EquipmentID = e.EquipmentID
                    AND ra.Status <> 'Cancelled'
                    AND ra.StartDate < @StartThisMonth
                    AND ra.EndDate >= @StartLastMonth
              )
            ORDER BY y.YardID, e.EquipmentID;
            """,

        ["6. Contractor rental hours last month"] = """
            DECLARE @StartLastMonth date =
                DATEFROMPARTS(YEAR(DATEADD(MONTH, -1, GETDATE())),
                              MONTH(DATEADD(MONTH, -1, GETDATE())), 1);
            DECLARE @StartThisMonth date =
                DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);

            SELECT c.ContractorID,
                   c.CompanyName,
                   COALESCE(SUM(DATEDIFF(HOUR,
                       CAST(ra.StartDate AS datetime),
                       CAST(ra.EndDate AS datetime))), 0) AS TotalRentalHoursLastMonth
            FROM Contractor AS c
            LEFT JOIN RentalAgreement AS ra
                   ON ra.ContractorID = c.ContractorID
                  AND ra.StartDate >= @StartLastMonth
                  AND ra.StartDate < @StartThisMonth
            GROUP BY c.ContractorID, c.CompanyName
            ORDER BY TotalRentalHoursLastMonth DESC, c.CompanyName;
            """
    };
}
