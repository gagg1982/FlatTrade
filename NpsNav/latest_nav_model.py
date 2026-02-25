from datetime import date

class LatestNav:
    def __init__(self, pfm_code: str, pfm_name: str, scheme_code: str, scheme_name: str, nav: float,
                one_day: float, seven_day: float, one_month: float, three_month: float, six_month: float,
                one_year: float, three_year: float, five_year: float, last_updated: date):
        self.pfm_code = pfm_code
        self.pfm_name = pfm_name
        self.scheme_code = scheme_code
        self.scheme_name = scheme_name
        self.nav_date = last_updated
        self.nav = nav
        self.one_day = one_day
        self.seven_day = seven_day
        self.one_month = one_month
        self.three_month = three_month
        self.six_month = six_month
        self.one_year = one_year
        self.three_year = three_year
        self.five_year = five_year

    def __repr__(self):
        return f"LatestNav({self.pfm_code}, {self.pfm_name}, {self.scheme_code}, {self.scheme_name}, {self.nav_date}, {self.nav},\
        {self.one_day}, {self.seven_day}, {self.one_month}, {self.three_month}, {self.six_month}, {self.one_year},\
        {self.three_year}, {self.five_year})"
