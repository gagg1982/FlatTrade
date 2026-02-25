import requests
from typing import Any
from python_lib import StoredProcedureExecutor
from datetime import date, datetime
from historic_nav_model import HistoricNav

API_SCHEMES_URL = "https://npsnav.in/api/schemes"
API_HISTORIC_PER_SCHEME_URL="https://npsnav.in/api/historical"
HISTORIC_NAV_SP_NAME_TUPLE = ("", "dbo.sp_UpsertHistoricNpsNav",)
BATCH_SIZE_FOR_WRITING_IN_DB = 50000

#=================================================================================================
class HistoricNavRepository:
    def __init__(self, sp_executor: StoredProcedureExecutor):
        self._sp_executor = sp_executor

    #[sp_SanityOnOneMinuteCandles_MissingDates] '2025-12-01'
    #[sp_SanityOnDailyCandles_MissingDates] '2025-12-01'

    def get_schemes(self):
        response = requests.get(API_SCHEMES_URL)
        response.raise_for_status()   # Stop if HTTP error
        json_data = response.json()
        scheme_list = json_data.get("data", [])
        print(f"#Schemes : {len(scheme_list):>5}")
        return scheme_list



    def fetch_all_historic_nav_from_nps(self):
        scheme_list = self.get_schemes()
        all_historic_records = []
        s_no =1
        print("=========Complete Historic-NAVs=========")
        for scheme_code, scheme_name in scheme_list:
            history_url = API_HISTORIC_PER_SCHEME_URL + f"/{scheme_code}"
            response = requests.get(history_url)
            response.raise_for_status()   # Stop if HTTP error
            json_data = response.json()
            historic_nav_per_scheme = json_data.get("data", [])
            print(f"{s_no:<3} {scheme_name:<100} [{scheme_code}] : {len(historic_nav_per_scheme):>5}")
            s_no +=1
            for entry in historic_nav_per_scheme:
                enriched_record = {
                    "scheme_code": scheme_code,
                    "scheme_name": scheme_name,
                    **entry   # keeps date, nav, etc.
                }        
                all_historic_records.append(enriched_record)
        return all_historic_records


    def __convert_dates(self, rows):
        for row in rows:
            row["date"] = datetime.strptime(row["date"], "%d-%m-%Y").date()

    def __batch_list(self, data, batch_size):
        return [data[i:i + batch_size] 
            for i in range(0, len(data), batch_size)]

    def save_historic_nav_with_tvp(self, nav: Any|bytes) -> None:
        self.__convert_dates(nav)
        data = [list(d.values()) for d in nav]
        print("Writing Historic Navs into db of batch size [{}]".format(BATCH_SIZE_FOR_WRITING_IN_DB));
        batches = self.__batch_list(data, BATCH_SIZE_FOR_WRITING_IN_DB)
        for batch in batches:
            self._sp_executor.execute_with_tvp(
                sp_name = HISTORIC_NAV_SP_NAME_TUPLE[1],            
                tvp_data = data
            )