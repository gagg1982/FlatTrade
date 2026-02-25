import requests
import time
from datetime import datetime
from dateutil import parser
import pyodbc
from python_lib import DbConfig, SqlConnection, StoredProcedureExecutor
from latest_nav_model import LatestNav
from latest_nav_repository import LatestNavRepository
from historic_nav_model import HistoricNav
from historic_nav_repository import HistoricNavRepository

GET_HISTORICAL = False

Server= "dbserver\\appdb,1436"
Database = "TradingApplication"

#===========================================================================================
#===========================================================================================

def main():  

    config = DbConfig(server = Server, database= Database)

    connection = SqlConnection(config)
    executor = StoredProcedureExecutor(connection)

    if GET_HISTORICAL == True:
        repository = HistoricNavRepository(executor)
        historic_nav = repository.fetch_all_historic_nav_from_nps()
        repository.save_historic_nav_with_tvp(historic_nav)
    else:
        repository = LatestNavRepository(executor)
        latest_nav = repository.fetch_all_latest_nav_from_nps()
        repository.save_latest_nav_with_tvp(latest_nav)

if __name__ == "__main__":
    print("========== Importing NPS NAV data STARTING====================", datetime.now())
    main()
    print("========== Importing NPS NAV data FINISHED====================", datetime.now())