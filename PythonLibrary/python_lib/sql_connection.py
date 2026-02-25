import pyodbc
from .db_config import DbConfig

class SqlConnection:
    def __init__(self, config: DbConfig):
        self._config = config

    def get_connection(self):
        return pyodbc.connect(self._config.connection_string(), autocommit=True)
