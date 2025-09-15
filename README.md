# QueriesTool
This Windows Forms application in C# (.NET Framework 4.7.2) allows you to manage SQL queries and their related parameters through a graphical interface.  
The main project files are `Form1.cs`, `LibraryQuery.cs`, and `LibraryScript.cs`.


## Table of Contents
* [General Info](#general-info)
* [Versions](#versions)
* [Main Components](#main-components)
* [Requirements](#requirements)
* [Guide](#guide)

---
## General Info
This project is a Windows Forms application developed in C# (.NET Framework 4.7.2) that provides a graphical interface for managing and generating SQL scripts related to database queries and their associated modules and parameters. The application connects to a SQL Server database using a configurable connection string and allows users to:
- View, search, and filter records in the main "Queries" table by name or ID.
- Create new query records or duplicate existing ones through a dynamic form that adapts to the table schema.
- Manage related records in the "Queries_CrossModules" and "Queries_Parameter" tables using embedded DataGridViews.
- Automatically generate SQL script files based on the data entered, ensuring required fields are validated and foreign key relationships are respected.
- Interact with the database schema dynamically, retrieving column names, required fields, and foreign key information at runtime.

---

## Versions
- [v1.0.0](https://github.com/Melassacelo/QueriesTool/releases/download/v1.0.0/QueriesTool.zip)
- [v1.0.1](https://github.com/Melassacelo/QueriesTool/releases/download/v1.0.1/QueriesTool.zip)

---
## Main Components

### Form1.cs

- **UI Management**: Provides the main graphical interface for viewing, searching, creating, and duplicating SQL records.
- **DataGridView**: Displays table data and allows selection and editing of records.
- **Dynamic Form Creation**: Dynamically generates forms for inserting new records or duplicating existing ones, with adaptive controls (TextBox, ComboBox) based on data type and foreign keys.
- **SQL File Generation**: Generates and saves `.SQL` files with the data insertion scripts.

### LibraryQuery.cs

- **Data Access**: Contains methods to execute SQL queries and retrieve data from the database using `SqlConnection`.
- **Table Metadata**: Retrieves column names, required columns, foreign keys, and referenced tables using `INFORMATION_SCHEMA` queries.

### LibraryScript.cs

- **SQL Script Generation**: Creates SQL insert scripts based on user input and the involved tables.
- **Data Validation**: Validates data entered in controls (TextBox, ComboBox) according to the expected column data type.
- **DataGridView Support**: Generates scripts for inserting multiple rows using DataGridView.

---

## Requirements

- .NET Framework 4.7.2
- SQL Server (as backend database)
- Visual Studio 2022

---

## Guide

### Database Connection

To change the database connection, open the WA_Progetto.exe.config file and locate the <connectionStrings> section. Update the connectionString attribute of the DefaultConnection entry with the new server name, database name, user, and password as needed. Save the file and restart the application for the changes to take effect.


### Overview

When you start the program, the main table displays all records from the **"Queries"** table.  
You can search for specific queries by entering a name or an ID in the search fields at the top and pressing the **Search** button.  
The table will update to show only the records that match your criteria.

### Searching and Filtering

You can refine results using the search fields for query name or ID. Once you click the **Search** button, the results will filter accordingly.

![Main Table](https://github.com/user-attachments/assets/3418f409-305f-464a-b0d5-a60f485bf44e)

---

### Creating and Duplicating Queries

- Click **Create New** to add a new query.
- A form will appear allowing entry for each field.
- Fields referencing other tables are shown as dropdown menus.
- On the right side of the form, manage related **Modules** and **Parameters**.

To **duplicate** an existing query:

1. Select a row from the main table.
2. Click the **Duplicate** button.
3. The form will pre-fill with the selected record's data.
4. Modify as needed and save.

<p align="center">
  <img src="https://github.com/user-attachments/assets/558fa1a1-5581-46a7-8055-5fcd758251d5" width="30.9%">
  <img src="https://github.com/user-attachments/assets/21cbff67-88b4-4be3-b9e0-ae8891d0e29b" width="33.9%">
  <img src="https://github.com/user-attachments/assets/99dde8f8-e128-4999-b7e3-5a7201362b8e" width="33.9%">
</p>

---

### Managing Related Data

Use the embedded grids in the form to edit:

- `Queries_CrossModules`
- `Queries_Parameter`
- `Queries_Parameter_Detail`

#### Actions:

- **Edit:** Select and double-click a row.
- **Add New:** Double-click on the Header of the DataGridView or double-click with no selection.
- **Delete:** Select a row and press the **Backspace** key.

![Create or Duplicate Form](https://github.com/user-attachments/assets/207b44b1-3e74-42f9-a787-f2c9cdda34a7)

---

### Generating SQL Scripts

When ready to save your changes:

1. Click the **Generate .sql script** button.
2. Choose the file name and location.
3. A well-formatted SQL script will be created with your data.

> ⚠️ If required fields are missing, the application will notify you.

![Related Data Grids](https://github.com/user-attachments/assets/b1bcabd4-3f58-422a-8a59-f24842277e86)

---
