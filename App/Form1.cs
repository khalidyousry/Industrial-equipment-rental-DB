using System.Globalization;
using Microsoft.Data.SqlClient;

namespace EquipmentRentalApp;

public partial class Form1 : Form
{
    private readonly DatabaseClient _database = new(SqlStatements.DefaultConnectionString);

    private readonly TextBox _connectionTextBox = CreateTextBox(SqlStatements.DefaultConnectionString);
    private readonly Label _statusLabel = new()
    {
        AutoSize = true,
        Dock = DockStyle.Fill,
        Padding = new Padding(4, 6, 4, 4),
        Text = "Ready."
    };

    private readonly ComboBox _tableComboBox = CreateComboBox();
    private readonly DataGridView _tableGrid = CreateGrid();
    private readonly ComboBox _reportComboBox = CreateComboBox();
    private readonly DataGridView _reportGrid = CreateGrid();

    private readonly TextBox _contractorNameTextBox = CreateTextBox("Future Build Demo");
    private readonly TextBox _contractorCreditTextBox = CreateTextBox("100000");
    private readonly TextBox _contractorContactTextBox = CreateTextBox("Demo Contact");
    private readonly TextBox _contractorPhoneTextBox = CreateTextBox("0100-0000000");
    private readonly TextBox _contractorAddressTextBox = CreateTextBox("Cairo");

    private readonly TextBox _equipmentYardIdTextBox = CreateTextBox("1");
    private readonly TextBox _equipmentModelTextBox = CreateTextBox("Demo Excavator");
    private readonly TextBox _equipmentPowerTextBox = CreateTextBox("250");
    private readonly TextBox _equipmentRateTextBox = CreateTextBox("700");
    private readonly ComboBox _equipmentStatusComboBox = CreateComboBox("Available", "Rented", "Under Maintenance");

    private readonly TextBox _updateEquipmentIdTextBox = CreateTextBox("2");
    private readonly ComboBox _updateEquipmentStatusComboBox = CreateComboBox("Available", "Rented", "Under Maintenance");

    private readonly TextBox _updateContractorIdTextBox = CreateTextBox("1");
    private readonly TextBox _updateContractorCreditTextBox = CreateTextBox("550000");

    private readonly TextBox _deleteContractorIdTextBox = CreateTextBox();
    private readonly TextBox _deleteEquipmentIdTextBox = CreateTextBox("12");

    public Form1()
    {
        InitializeComponent();
        BuildLayout();
        LoadComboBoxes();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(10)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var tabs = new TabControl
        {
            Dock = DockStyle.Fill
        };
        tabs.TabPages.Add(BuildTablesTab());
        tabs.TabPages.Add(BuildOperationsTab());
        tabs.TabPages.Add(BuildReportsTab());

        root.Controls.Add(BuildConnectionPanel(), 0, 0);
        root.Controls.Add(tabs, 0, 1);
        root.Controls.Add(_statusLabel, 0, 2);

        Controls.Add(root);
    }

    private Control BuildConnectionPanel()
    {
        var panel = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 4,
            Dock = DockStyle.Top,
            Padding = new Padding(0, 0, 0, 8)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var testButton = CreateButton("Test");
        testButton.Click += async (_, _) => await RunSafely(async () =>
        {
            await _database.TestConnectionAsync();
            SetStatus("Connection OK.");
        });

        var loadButton = CreateButton("Load");
        loadButton.Click += async (_, _) => await RunSafely(LoadTableAsync);

        panel.Controls.Add(CreateLabel("Connection"), 0, 0);
        panel.Controls.Add(_connectionTextBox, 1, 0);
        panel.Controls.Add(testButton, 2, 0);
        panel.Controls.Add(loadButton, 3, 0);

        return panel;
    }

    private TabPage BuildTablesTab()
    {
        var page = new TabPage("Tables");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(8)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var toolbar = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            WrapContents = false
        };

        _tableComboBox.Width = 240;
        var loadButton = CreateButton("Load Table");
        loadButton.Click += async (_, _) => await RunSafely(LoadTableAsync);

        toolbar.Controls.Add(CreateLabel("Table"));
        toolbar.Controls.Add(_tableComboBox);
        toolbar.Controls.Add(loadButton);

        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(_tableGrid, 0, 1);
        page.Controls.Add(layout);
        return page;
    }

    private TabPage BuildOperationsTab()
    {
        const int insertRowHeight = 330;
        const int updateRowHeight = 230;
        const int deleteRowHeight = 185;

        var page = new TabPage("Operations");
        var scrollPanel = new Panel
        {
            AutoScroll = true,
            Dock = DockStyle.Fill
        };
        var layout = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Top,
            Height = insertRowHeight + updateRowHeight + deleteRowHeight + 24,
            RowCount = 3,
            Padding = new Padding(8)
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, insertRowHeight));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, updateRowHeight));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, deleteRowHeight));

        layout.Controls.Add(BuildInsertContractorGroup(), 0, 0);
        layout.Controls.Add(BuildInsertEquipmentGroup(), 1, 0);
        layout.Controls.Add(BuildUpdateEquipmentGroup(), 0, 1);
        layout.Controls.Add(BuildUpdateContractorGroup(), 1, 1);
        layout.Controls.Add(BuildDeleteContractorGroup(), 0, 2);
        layout.Controls.Add(BuildDeleteEquipmentGroup(), 1, 2);

        scrollPanel.Controls.Add(layout);
        page.Controls.Add(scrollPanel);
        return page;
    }

    private TabPage BuildReportsTab()
    {
        var page = new TabPage("Reports");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(8)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var toolbar = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            WrapContents = false
        };

        _reportComboBox.Width = 360;
        var runButton = CreateButton("Run Report");
        runButton.Click += async (_, _) => await RunSafely(RunReportAsync);

        toolbar.Controls.Add(CreateLabel("Inquiry"));
        toolbar.Controls.Add(_reportComboBox);
        toolbar.Controls.Add(runButton);

        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(_reportGrid, 0, 1);
        page.Controls.Add(layout);
        return page;
    }

    private GroupBox BuildInsertContractorGroup()
    {
        var grid = CreateFormGrid(6);
        AddRow(grid, 0, "Company", _contractorNameTextBox);
        AddRow(grid, 1, "Credit Limit", _contractorCreditTextBox);
        AddRow(grid, 2, "Contact", _contractorContactTextBox);
        AddRow(grid, 3, "Phone", _contractorPhoneTextBox);
        AddRow(grid, 4, "Address", _contractorAddressTextBox);

        var button = CreateButton("Insert Contractor");
        button.Click += async (_, _) => await RunSafely(InsertContractorAsync);
        AddButtonRow(grid, 5, button);

        return CreateGroup("Insert Contractor", grid);
    }

    private GroupBox BuildInsertEquipmentGroup()
    {
        var grid = CreateFormGrid(6);
        AddRow(grid, 0, "Yard ID", _equipmentYardIdTextBox);
        AddRow(grid, 1, "Model", _equipmentModelTextBox);
        AddRow(grid, 2, "Engine Power", _equipmentPowerTextBox);
        AddRow(grid, 3, "Hourly Rate", _equipmentRateTextBox);
        AddRow(grid, 4, "Status", _equipmentStatusComboBox);

        var button = CreateButton("Insert Equipment");
        button.Click += async (_, _) => await RunSafely(InsertEquipmentAsync);
        AddButtonRow(grid, 5, button);

        return CreateGroup("Insert Equipment", grid);
    }

    private GroupBox BuildUpdateEquipmentGroup()
    {
        var grid = CreateFormGrid(3);
        AddRow(grid, 0, "Equipment ID", _updateEquipmentIdTextBox);
        AddRow(grid, 1, "New Status", _updateEquipmentStatusComboBox);

        var button = CreateButton("Update Equipment");
        button.Click += async (_, _) => await RunSafely(UpdateEquipmentAsync);
        AddButtonRow(grid, 2, button);

        return CreateGroup("Update Equipment", grid);
    }

    private GroupBox BuildUpdateContractorGroup()
    {
        var grid = CreateFormGrid(3);
        AddRow(grid, 0, "Contractor ID", _updateContractorIdTextBox);
        AddRow(grid, 1, "Credit Limit", _updateContractorCreditTextBox);

        var button = CreateButton("Update Contractor");
        button.Click += async (_, _) => await RunSafely(UpdateContractorAsync);
        AddButtonRow(grid, 2, button);

        return CreateGroup("Update Contractor", grid);
    }

    private GroupBox BuildDeleteContractorGroup()
    {
        var grid = CreateFormGrid(2);
        AddRow(grid, 0, "Contractor ID", _deleteContractorIdTextBox);

        var button = CreateButton("Delete Contractor");
        button.Click += async (_, _) => await RunSafely(DeleteContractorAsync);
        AddButtonRow(grid, 1, button);

        return CreateGroup("Delete Contractor", grid);
    }

    private GroupBox BuildDeleteEquipmentGroup()
    {
        var grid = CreateFormGrid(2);
        AddRow(grid, 0, "Equipment ID", _deleteEquipmentIdTextBox);

        var button = CreateButton("Delete Equipment");
        button.Click += async (_, _) => await RunSafely(DeleteEquipmentAsync);
        AddButtonRow(grid, 1, button);

        return CreateGroup("Delete Equipment", grid);
    }

    private void LoadComboBoxes()
    {
        foreach (var tableName in SqlStatements.TableQueries.Keys)
        {
            _tableComboBox.Items.Add(tableName);
        }

        foreach (var reportName in SqlStatements.Reports.Keys)
        {
            _reportComboBox.Items.Add(reportName);
        }

        _tableComboBox.SelectedIndex = 0;
        _reportComboBox.SelectedIndex = 0;
    }

    private async Task LoadTableAsync()
    {
        var tableName = SelectedText(_tableComboBox);
        var data = await _database.QueryAsync(SqlStatements.TableQueries[tableName]);
        _tableGrid.DataSource = data;
        SetStatus($"Loaded {data.Rows.Count} rows from {tableName}.");
    }

    private async Task RunReportAsync()
    {
        var reportName = SelectedText(_reportComboBox);
        var data = await _database.QueryAsync(SqlStatements.Reports[reportName]);
        _reportGrid.DataSource = data;
        SetStatus($"Report returned {data.Rows.Count} rows.");
    }

    private async Task InsertContractorAsync()
    {
        var rows = await _database.ExecuteAsync(
            SqlStatements.InsertContractor,
            Parameter("@CompanyName", RequiredText(_contractorNameTextBox, "Company")),
            Parameter("@CreditLimit", ReadDecimal(_contractorCreditTextBox, "Credit Limit")),
            Parameter("@ContactPerson", DbValue(_contractorContactTextBox.Text)),
            Parameter("@Phone", DbValue(_contractorPhoneTextBox.Text)),
            Parameter("@Address", DbValue(_contractorAddressTextBox.Text)));

        SetStatus($"{rows} contractor row inserted.");
        await RefreshTableAsync("Contractor");
    }

    private async Task InsertEquipmentAsync()
    {
        var rows = await _database.ExecuteAsync(
            SqlStatements.InsertEquipment,
            Parameter("@YardID", ReadInt(_equipmentYardIdTextBox, "Yard ID")),
            Parameter("@Model", RequiredText(_equipmentModelTextBox, "Model")),
            Parameter("@EnginePower", ReadDecimal(_equipmentPowerTextBox, "Engine Power")),
            Parameter("@HourlyRate", ReadDecimal(_equipmentRateTextBox, "Hourly Rate")),
            Parameter("@Status", SelectedText(_equipmentStatusComboBox)));

        SetStatus($"{rows} equipment row inserted.");
        await RefreshTableAsync("Equipment");
    }

    private async Task UpdateEquipmentAsync()
    {
        var rows = await _database.ExecuteAsync(
            SqlStatements.UpdateEquipmentStatus,
            Parameter("@EquipmentID", ReadInt(_updateEquipmentIdTextBox, "Equipment ID")),
            Parameter("@Status", SelectedText(_updateEquipmentStatusComboBox)));

        SetStatus($"{rows} equipment row updated.");
        await RefreshTableAsync("Equipment");
    }

    private async Task UpdateContractorAsync()
    {
        var rows = await _database.ExecuteAsync(
            SqlStatements.UpdateContractorCreditLimit,
            Parameter("@ContractorID", ReadInt(_updateContractorIdTextBox, "Contractor ID")),
            Parameter("@CreditLimit", ReadDecimal(_updateContractorCreditTextBox, "Credit Limit")));

        SetStatus($"{rows} contractor row updated.");
        await RefreshTableAsync("Contractor");
    }

    private async Task DeleteContractorAsync()
    {
        var rows = await _database.ExecuteAsync(
            SqlStatements.DeleteContractorWithoutRentals,
            Parameter("@ContractorID", ReadInt(_deleteContractorIdTextBox, "Contractor ID")));

        SetStatus($"{rows} contractor row deleted.");
        await RefreshTableAsync("Contractor");
    }

    private async Task DeleteEquipmentAsync()
    {
        var rows = await _database.ExecuteAsync(
            SqlStatements.DeleteAvailableEquipmentWithoutHistory,
            Parameter("@EquipmentID", ReadInt(_deleteEquipmentIdTextBox, "Equipment ID")));

        SetStatus($"{rows} equipment row deleted.");
        await RefreshTableAsync("Equipment");
    }

    private async Task RefreshTableAsync(string tableName)
    {
        SelectComboItem(_tableComboBox, tableName);
        await LoadTableAsync();
    }

    private async Task RunSafely(Func<Task> action)
    {
        try
        {
            var connectionString = _connectionTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string is required.");
            }

            _database.ConnectionString = connectionString;
            UseWaitCursor = true;
            await action();
        }
        catch (Exception ex)
        {
            SetStatus("Operation failed.");
            MessageBox.Show(this, ex.Message, "Database Operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private void SetStatus(string message)
    {
        _statusLabel.Text = $"{DateTime.Now:t}  {message}";
    }

    private static SqlParameter Parameter(string name, object value)
    {
        return new SqlParameter(name, value);
    }

    private static object DbValue(string value)
    {
        var trimmed = value.Trim();
        return trimmed.Length == 0 ? DBNull.Value : trimmed;
    }

    private static string RequiredText(TextBox textBox, string fieldName)
    {
        var value = textBox.Text.Trim();
        if (value.Length == 0)
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return value;
    }

    private static int ReadInt(TextBox textBox, string fieldName)
    {
        if (int.TryParse(textBox.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        throw new InvalidOperationException($"{fieldName} must be a whole number.");
    }

    private static decimal ReadDecimal(TextBox textBox, string fieldName)
    {
        var text = textBox.Text.Trim();
        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out var value))
        {
            return value;
        }

        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
        {
            return value;
        }

        throw new InvalidOperationException($"{fieldName} must be a number.");
    }

    private static string SelectedText(ComboBox comboBox)
    {
        return comboBox.SelectedItem?.ToString()
            ?? throw new InvalidOperationException("Please select a value.");
    }

    private static void SelectComboItem(ComboBox comboBox, string value)
    {
        for (var index = 0; index < comboBox.Items.Count; index++)
        {
            if (string.Equals(comboBox.Items[index]?.ToString(), value, StringComparison.Ordinal))
            {
                comboBox.SelectedIndex = index;
                return;
            }
        }
    }

    private static GroupBox CreateGroup(string title, Control content)
    {
        var group = new GroupBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(6),
            Padding = new Padding(10, 18, 10, 12),
            Text = title
        };
        group.Controls.Add(content);
        return group;
    }

    private static TableLayoutPanel CreateFormGrid(int rows)
    {
        var grid = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Padding = new Padding(4),
            RowCount = rows
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        for (var row = 0; row < rows; row++)
        {
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        return grid;
    }

    private static void AddRow(TableLayoutPanel grid, int row, string labelText, Control control)
    {
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(0, 3, 0, 3);

        grid.Controls.Add(CreateLabel(labelText), 0, row);
        grid.Controls.Add(control, 1, row);
    }

    private static void AddButtonRow(TableLayoutPanel grid, int row, Button button)
    {
        button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        button.Margin = new Padding(0, 10, 0, 0);
        grid.Controls.Add(button, 1, row);
    }

    private static Label CreateLabel(string text)
    {
        return new Label
        {
            Anchor = AnchorStyles.Left,
            AutoSize = true,
            Margin = new Padding(0, 7, 8, 6),
            Text = text
        };
    }

    private static Button CreateButton(string text)
    {
        return new Button
        {
            AutoSize = true,
            Margin = new Padding(6, 2, 0, 2),
            Padding = new Padding(10, 4, 10, 4),
            Text = text,
            UseVisualStyleBackColor = true
        };
    }

    private static TextBox CreateTextBox(string text = "")
    {
        return new TextBox
        {
            Dock = DockStyle.Fill,
            Text = text
        };
    }

    private static ComboBox CreateComboBox(params string[] items)
    {
        var comboBox = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        if (items.Length > 0)
        {
            comboBox.Items.AddRange(items);
            comboBox.SelectedIndex = 0;
        }

        return comboBox;
    }

    private static DataGridView CreateGrid()
    {
        return new DataGridView
        {
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
            BackgroundColor = SystemColors.Window,
            BorderStyle = BorderStyle.Fixed3D,
            Dock = DockStyle.Fill,
            MultiSelect = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };
    }
}
