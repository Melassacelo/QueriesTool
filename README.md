# QueriesTool
This Windows Forms application in C# (.NET Framework 4.7.2) allows you to manage SQL queries and their related parameters through a graphical interface.  
The main project files are `Form1.cs`, `LibraryQuery.cs`, and `LibraryScript.cs`.

## Table of Contents
* [General Info](#general-info)
* [Main Components](#main-components)
* [Main Features](#main-features)
* [Requirements](#requirements)

---
## General Info

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

## Main Features

- **Search by Name or ID**: Filters displayed records using search TextBoxes.
- **Create/Duplicate Records**: Allows creation of new records or duplication of existing ones through dynamic forms.
- **Parameter and Module Management**: Add, edit, or remove parameters and modules associated with a query.
- **Automatic Validation**: Ensures entered data matches the expected SQL column data type.
- **SQL Script Export**: Saves the generated SQL insert script to a file.

---

## Requirements

- .NET Framework 4.7.2
- SQL Server (as backend database)
- Visual Studio 2022

---
