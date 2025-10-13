import pyodbc

# --- Configuration ---
# Replace with your SQL Server connection details
SERVER_NAME = 'your_server_name'  # e.g., 'localhost', 'your_server\SQLEXPRESS'
DATABASE_NAME = 'your_database_name' # e.g., 'FinancialData'
USERNAME = 'your_username'        # e.g., 'sa' (if using SQL Server Authentication)
PASSWORD = 'your_password'        # Your password (if using SQL Server Authentication)
TABLE_NAME = 'OHLCV_Data'         # Name of the table to store OHLCV data

# --- Database Connection Function ---
def get_db_connection():
    """
    Establishes a connection to the SQL Server database.
    Assumes SQL Server Authentication. For Windows Authentication, modify the connection string.
    """
    try:
        # Connection string for SQL Server Authentication
        conn_str = (
            f"DRIVER={{ODBC Driver 17 for SQL Server}};"
            f"SERVER={SERVER_NAME};"
            f"DATABASE={DATABASE_NAME};"
            f"UID={USERNAME};"
            f"PWD={PASSWORD};"
        )
        # For Windows Authentication, use:
        # conn_str = (
        #     f"DRIVER={{ODBC Driver 17 for SQL Server}};"
        #     f"SERVER={SERVER_NAME};"
        #     f"DATABASE={DATABASE_NAME};"
        #     f"Trusted_Connection=yes;"
        # )
        conn = pyodbc.connect(conn_str)
        print("Successfully connected to SQL Server.")
        return conn
    except pyodbc.Error as ex:
        sqlstate = ex.args[0]
        print(f"Error connecting to SQL Server: {sqlstate} - {ex}")
        return None

# --- DDL Execution Function ---
def execute(cursor):
    """
    Executes all necessary DDL statements:
    1. Creates the OHLCV_Data table.
    2. Creates the OHLCVType user-defined table type.
    3. Creates the InsertOHLCVDataTVP stored procedure.
    """
    print(f"\n--- Executing DDL for table '{TABLE_NAME}' ---")

    # 1. Create the OHLCV_Data table
    create_table_sql = f"""
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{TABLE_NAME}' AND xtype='U')
    BEGIN
        CREATE TABLE {TABLE_NAME} (
            --[ID] INT IDENTITY(1,1) PRIMARY KEY,
            [Token] INT NOT NULL PRIMARY KEY,
            [Date] DATE NOT NULL,
            [Open] DECIMAL(18, 4) NOT NULL,
            [High] DECIMAL(18, 4) NOT NULL,
            [Low] DECIMAL(18, 4) NOT NULL,
            [Close] DECIMAL(18, 4) NOT NULL,
            [Volume] BIGINT NOT NULL,
            CONSTRAINT UQ_Symbol_Date UNIQUE ([Token], [Date]) -- Ensure unique entries per token per date
        );
        PRINT 'Table {TABLE_NAME} created.';
    END
    ELSE
    BEGIN
        PRINT 'Table {TABLE_NAME} already exists.';
    END
    """
    try:
        cursor.execute(create_table_sql)
        cursor.commit()
        print(f"Table '{TABLE_NAME}' DDL executed.")
    except pyodbc.Error as ex:
        sqlstate = ex.args[0]
        print(f"Error creating table: {sqlstate} - {ex}")
        return # Exit if table creation fails, as subsequent steps depend on it

    print("\n--- Executing DDL for OHLCVType and InsertOHLCVDataTVP Stored Procedure ---")

    # 2. Create User-Defined Table Type
    create_type_sql = """
    IF TYPE_ID('OHLCVType') IS NOT NULL
        DROP TYPE OHLCVType;
    GO

    CREATE TYPE OHLCVType AS TABLE (
        Token INT,
        Date DATE,
        [Open] DECIMAL(18, 4),
        High DECIMAL(18, 4),
        Low DECIMAL(18, 4),
        Close DECIMAL(18, 4),
        Volume BIGINT
    );
    """
    # pyodbc requires separate execution for GO statements or batches
    try:
        # Execute the DROP TYPE and CREATE TYPE statements separately if GO is used
        # Or, remove GO and execute as a single batch if possible.
        # For simplicity and robustness with GO, we'll split it.
        # Note: Some drivers/versions might handle GO differently.
        # A common approach is to split by 'GO' and execute each part.
        statements = create_type_sql.split('GO')
        for statement in statements:
            if statement.strip(): # Ensure it's not an empty string
                cursor.execute(statement)
        cursor.commit()
        print("User-Defined Table Type 'OHLCVType' created/updated.")
    except pyodbc.Error as ex:
        sqlstate = ex.args[0]
        print(f"Error creating OHLCVType: {sqlstate} - {ex}")
        return

    # 3. Create Stored Procedure to Insert Data using TVP
    create_sp_sql = f"""
    IF OBJECT_ID('InsertOHLCVDataTVP', 'P') IS NOT NULL
        DROP PROCEDURE InsertOHLCVDataTVP;
    GO

    CREATE PROCEDURE InsertOHLCVDataTVP
        @OHLCVData OHLCVType READONLY
    AS
    BEGIN
        SET NOCOUNT ON;

       MERGE {TABLE_NAME} AS target
        USING @OHLCVData AS source
        ON (target.Token = source.Token AND target.Date = source.Date)
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (Symbol, Date, [Open], High, Low, Close, Volume)
            VALUES (source.Symbol, source.Date, source.[Open], source.High, source.Low, source.Close, source.Volume);
        WHEN MATCHED THEN
            UPDATE SET
            [Open] = source.[Open],
            High = source.High,
            Low = source.Low,
            Close = source.Close,
            Volume = source.Volume;
    END;
    """
    try:
        statements = create_sp_sql.split('GO')
        for statement in statements:
            if statement.strip():
                cursor.execute(statement)
        cursor.commit()
        print("Stored Procedure 'InsertOHLCVDataTVP' created/updated.")
    except pyodbc.Error as ex:
        sqlstate = ex.args[0]
        print(f"Error creating InsertOHLCVDataTVP stored procedure: {sqlstate} - {ex}")

# --- Main Execution Block for DDL Script ---
if __name__ == "__main__":
    conn = get_db_connection()
    if conn:
        cursor = None
        try:
            cursor = conn.cursor()
            execute(cursor)
        except pyodbc.Error as ex:
            sqlstate = ex.args[0]
            print(f"An error occurred during DDL operations: {sqlstate} - {ex}")
        finally:
            if cursor:
                cursor.close()
                print("Database cursor closed.")
            if conn:
                conn.close()
                print("Database connection closed.")
    else:
        print("Failed to establish a database connection. Exiting script.")

