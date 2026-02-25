import requests
import csv
from typing import Any
from python_lib import StoredProcedureExecutor
from datetime import date, datetime
from latest_nav_model import LatestNav

API_LATEST_NAV_ALL_FUNDS_URL ="https://npsnav.in/api/latest"
LATEST_NAV_SP_NAME_TUPLE = ("", "dbo.sp_UpsertLatestNpsNav",) 

#=================================================================================================

class LatestNavRepository:
    def __init__(self, sp_executor: StoredProcedureExecutor):
        self._sp_executor = sp_executor

    #[sp_SanityOnOneMinuteCandles_MissingDates] '2025-12-01'
    #[sp_SanityOnDailyCandles_MissingDates] '2025-12-01'

    def fetch_all_latest_nav_from_nps(self):
        response = requests.get(API_LATEST_NAV_ALL_FUNDS_URL)
        response.raise_for_status()   # Stop if HTTP error
        json_data = response.json()
        latest_nav_for_all_schemes = json_data.get("data", [])
        print("=========Latest-NAVs=========")
        print(f"#Schemes : {len(latest_nav_for_all_schemes):>5}")
        return latest_nav_for_all_schemes

    def __convert_dates(self, rows):
        for row in rows:
            row["Last Updated"] = datetime.strptime(row["Last Updated"], "%d-%m-%Y").date()


    def save_latest_nav_with_tvp(self, nav: Any|bytes) -> None:
        self.__convert_dates(nav)
        data = [list(d.values()) for d in nav]
        self._sp_executor.execute_with_tvp(
            sp_name = LATEST_NAV_SP_NAME_TUPLE[1],            
            tvp_data = data
        )