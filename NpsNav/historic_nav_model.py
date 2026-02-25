from datetime import date

class HistoricNav:
    def __init__(self, scheme_code: str, scheme_name: str, nav_date: date, nav: float):
        self.scheme_code = scheme_code
        self.scheme_name = scheme_name
        self.nav_date = nav_date
        self.nav = nav

    def __repr__(self):
        return f"LatestNav({self.scheme_code}, {self.scheme_name}, {self.nav_date}, {self.nav})"
