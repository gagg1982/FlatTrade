import requests
import time
from datetime import date, datetime
import utility
from typing import List, Dict, Any


BSE_HOME_URL = "https://www.bseindia.com"
BSE_MARKET_URL = "https://www.bseindia.com/market-data/corporate-actions"
BSE_CORPORATE_ACTIONS_API = (
    "https://api.bseindia.com/BseIndiaAPI/api/DefaultData/w?Fdate={}&Purposecode=&TDate={}&ddlcategorys=E&ddlindustrys=&scripcode=&segment=0&strSearch=S"
)
BSE_TVP_COLUMN_MAP = {
    "short_name": "Symbol",
    "exchange": "Exchange",  
    "series" : "Series",
    "ind" : "Indicative",
    "faceVal"  : "FaceValue",
    "Purpose": "Subject",
    "Ex_date": "ExDate",
    "RD_Date": "RecordDate",
    "BCRD_FROM": "BookClosureStartDate",
    "BCRD_TO": "BookClosureEndDate",
    "ND_START_DATE" : "NoDeliveryStartDate",
    "ND_END_DATE" : "NoDeliveryEndDate",
    "long_name": "CompanyName",
    "isin" : "Isin",
    "caBroadcastDate" : "AnnouncementDate",
}

HEADERS = {
    "User-Agent": (
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
        "AppleWebKit/537.36 (KHTML, like Gecko) "
        "Chrome/122.0.0.0 Safari/537.36"
    ),
    "Accept": "application/json, text/plain, */*",
    "Accept-Language": "en-US,en;q=0.9",
    "Accept-Encoding": "gzip, deflate",
    "Referer": "https://www.bseindia.com/",
    "Connection": "keep-alive",

    "sec-ch-ua": '"Chromium";v="122", "Not(A:Brand";v="24", "Google Chrome";v="122"',
    "sec-ch-ua-mobile": "?0",
    "sec-ch-ua-platform": '"Windows"',
    "sec-fetch-site": "same-origin",
    "sec-fetch-mode": "cors",
    "sec-fetch-dest": "empty",
}


class BSEClient:
    def __init__(self, timeout: int = 15):
        self.session = requests.Session()
        self.session.headers.update(HEADERS)
        self.timeout = timeout
        self._initialize_session()

        
    def _normalize_record(self, item: dict) -> dict:
        """
        Converts all keys ending with 'Date':
        - string → datetime.date
        - "-"    → epoch date (1970-01-01)
        """
        result = {}

        for key, value in item.items():
            if key in ["Ex_date","RD_Date", "BCRD_FROM", "BCRD_TO", "ND_START_DATE", "ND_END_DATE", "caBroadcastDate" ]:
                if value in [ "-","", None, " " ]:
                    result[key] = datetime(1970, 1, 1)
                elif isinstance(value, date):
                    result[key] = value
                elif isinstance(value, str):
                    result[key] = utility.to_datetime_safe(value)
                else:
                    raise TypeError(f"Invalid value for {key}: {value}")
            else:
                result[key] = value

        return result

    def _initialize_session(self) -> None:
        """
        Warm up NSE session with multiple page hits
        """
        self.session.get(
            BSE_HOME_URL,
            timeout=self.timeout,
            allow_redirects=True,
        )

        time.sleep(1)

    def get_corporate_actions(
        self, from_date: str, to_date: str
    ) -> List[Dict[str, Any]]:

        response = self.session.get(
            BSE_CORPORATE_ACTIONS_API.format(from_date, to_date),
            timeout=self.timeout,
        )

        response.raise_for_status()
        corporate_actions = response.json()
        normalize_rows = []
        print(f"Bse fetched corporate actions between {from_date} and {to_date} : {len(corporate_actions)} records")
        for item in corporate_actions:
            item["exchange"] = "BSE"  # Adding exchange code as the first column
            item["series"] = '-'
            item["ind"]  = '-'
            item["isin"]  = '-'
            item["faceVal"] = 0
            item["caBroadcastDate"] = '-'
            normalize_rows.append(self._normalize_record(item))

        tvp_rows = utility.dicts_to_tuple_rows(normalize_rows, BSE_TVP_COLUMN_MAP)
        return tvp_rows

