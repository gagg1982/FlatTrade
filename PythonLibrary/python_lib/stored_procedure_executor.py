from .sql_connection import SqlConnection

class StoredProcedureExecutor:
    def __init__(self, connection_provider: SqlConnection):
        self._connection_provider = connection_provider

    def execute(self, sp_name: str, params: tuple = ()):
        """
        Executes a stored procedure and returns pyodbc rows
        """
        with self._connection_provider.get_connection() as conn:
            with conn.cursor() as cursor:
                param_placeholders = ",".join("?" * len(params))
                call_stmt = f"{{CALL {sp_name} ({param_placeholders})}}"

                cursor.execute(call_stmt, params)
                return cursor.fetchall(), cursor.description

    def execute_with_tvp(
        self,
        sp_name: str,
        tvp_data: list[list]
    ):
        """
        Executes a stored procedure that accepts a TVP using a temp table
        """

        with self._connection_provider.get_connection() as conn:
            cursor = conn.cursor()
            cursor.fast_executemany = True

            sql = f"{{CALL {sp_name} (?)}}"
            cursor.execute(sql, (tvp_data,) )
