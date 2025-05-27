# QueriesTool
This Windows Forms application in C# (.NET Framework 4.7.2) allows you to manage SQL queries and their related parameters through a graphical interface.  
The main project files are `Form1.cs`, `LibraryQuery.cs`, and `LibraryScript.cs`.
[Download Here](https://github.com/Melassacelo/QueriesTool/blob/master/QueriesTool.zip)

## Table of Contents
* [General Info](#general-info)
* [Main Components](#main-components)
* [Requirements](#requirements)
* [Screenshots](#screenshots)

---
## General Info
This project is a Windows Forms application developed in C# (.NET Framework 4.7.2) that provides a graphical interface for managing and generating SQL scripts related to database queries and their associated modules and parameters. The application connects to a SQL Server database using a configurable connection string and allows users to:
- View, search, and filter records in the main "Queries" table by name or ID.
- Create new query records or duplicate existing ones through a dynamic form that adapts to the table schema.
- Manage related records in the "Queries_CrossModules" and "Queries_Parameter" tables using embedded DataGridViews.
- Automatically generate SQL script files based on the data entered, ensuring required fields are validated and foreign key relationships are respected.
- Interact with the database schema dynamically, retrieving column names, required fields, and foreign key information at runtime.

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

## Screenshots
<img src="https://github.com/user-attachments/assets/d78d8d83-e34e-438a-a5c7-5b5ea7d7d747" height="225">
<img src="https://github.com/user-attachments/assets/9b5a2a07-397d-4087-8a93-ee24a66cd460" height="225">
<img src="https://github.com/user-attachments/assets/baeae23c-adf3-4d3b-a48e-b4fb8e44557f" height="225">

---
