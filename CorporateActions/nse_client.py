import requests
import time
import utility
from typing import List, Dict, Any


NSE_HOME_URL = "https://www.nseindia.com"
NSE_MARKET_URL = "https://www.nseindia.com/market-data/corporate-actions"
NSE_CORPORATE_ACTIONS_API = (
    "https://www.nseindia.com/api/corporates-corporateActions"
)
NSE_TVP_COLUMN_MAP = {
    "symbol": "Symbol",
    "exchange": "Exchange",
    "series": "Series",
    "ind" : "Indicative",
    "faceVal": "FaceValue",
    "subject": "Subject",
    "exDate": "ExDate",
    "recDate": "RecordDate",
    "bcStartDate": "BookClosureStartDate",
    "bcEndDate": "BookClosureEndDate",
    "ndStartDate" : "NoDeliveryStartDate",
    "ndEndDate" : "NoDeliveryEndDate",
    "comp": "CompanyName",
    "isin": "Isin",
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
    "Referer": "https://www.nseindia.com/",
    "Connection": "keep-alive",

    # 🔐 Critical anti-bot headers
    "sec-ch-ua": '"Chromium";v="122", "Not(A:Brand";v="24", "Google Chrome";v="122"',
    "sec-ch-ua-mobile": "?0",
    "sec-ch-ua-platform": '"Windows"',
    "sec-fetch-site": "same-origin",
    "sec-fetch-mode": "cors",
    "sec-fetch-dest": "empty",
}


class NSEClient:
    def __init__(self, timeout: int = 15):
        self.session = requests.Session()
        self.session.headers.update(HEADERS)
        self.timeout = timeout
        self._initialize_session()

    def _initialize_session(self) -> None:
        """
        Warm up NSE session with multiple page hits
        """
        self.session.get(
            NSE_HOME_URL,
            timeout=self.timeout,
            allow_redirects=True,
        )

        time.sleep(1)

        self.session.get(
            NSE_MARKET_URL,
            timeout=self.timeout,
            allow_redirects=True,
        )

        time.sleep(1)

    def get_corporate_actions(
        self, from_date: str, to_date: str
    ) -> List[Dict[str, Any]]:

        params = {
            "index": "equities",
            "from_date": from_date,
            "to_date": to_date,
        }

        response = self.session.get(
            NSE_CORPORATE_ACTIONS_API,
            params=params,
            timeout=self.timeout,
        )

        response.raise_for_status()
        corporate_actions = response.json()
        normalize_rows = []
        print(f"Nse fetched corporate actions between {from_date} and {to_date} : {len(corporate_actions)} records")
        for item in corporate_actions:
            item["exchange"] = "NSE"  # Adding exchange code as the first column
            normalize_rows.append(utility.normalize_record(item))

        tvp_rows = utility.dicts_to_tuple_rows(normalize_rows, NSE_TVP_COLUMN_MAP)
        return tvp_rows

