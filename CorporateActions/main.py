from nse_client import NSEClient
from bse_client import BSEClient
import utility
from datetime import datetime, date
from python_lib import DbConfig, SqlConnection, StoredProcedureExecutor
from corporate_actions_repository import CorporateActionsRepository

Server= "dbserver\\appdb,1436"
Database = "TradingApplication"
Insert_sp_name = "dbo.sp_UpsertCorporateActions"
From_date = "01-01-2026"
To_date = "01-01-2027"

def main():   
    config = DbConfig(server = Server, database= Database)

    connection = SqlConnection(config)
    executor = StoredProcedureExecutor(connection)
    repository = CorporateActionsRepository(executor)

    nse_client = NSEClient()
    corporate_actions_nse = nse_client.get_corporate_actions(
        from_date=From_date,
        to_date=To_date,
    )
    repository.save_corporate_actions_with_tvp(Insert_sp_name, corporate_actions_nse)

    bse_client = BSEClient()
    corporate_actions_bse = bse_client.get_corporate_actions(
        from_date=utility.to_sql_date(From_date),
        to_date=utility.to_sql_date(To_date),
    )    
    repository.save_corporate_actions_with_tvp(Insert_sp_name, corporate_actions_bse)
   

if __name__ == "__main__":
    print("========== Importing corporate actions STARTING====================", datetime.now())
    main()
    print("========== Importing corporate actions FINISHED====================", datetime.now())
