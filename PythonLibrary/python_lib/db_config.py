class DbConfig:
    def __init__(self, server, database, driver="ODBC Driver 17 for SQL Server"):
        self.server = server
        self.database = database
        self.driver = driver

    def connection_string(self):
        return (
            f"DRIVER={{{self.driver}}};"
            f"SERVER={self.server};"
            f"DATABASE={self.database};"
            "Trusted_Connection=yes;"
        )
