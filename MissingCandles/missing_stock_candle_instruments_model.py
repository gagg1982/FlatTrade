from datetime import date

class MissingStockCandleInstruments:
    def __init__(self, trading_symbol: str, exchange_code: str, token: int, missing_trade_date: date, interval: str, start_time:str, end_time: str):
        self.trading_symbol = trading_symbol
        self.exchange_code = exchange_code
        self.token = token
        self.missing_trade_date = missing_trade_date
        self.interval= interval
        self.start_time = start_time
        self.end_time = end_time

    def __repr__(self):
        return f"MissingStockCandleInstruments({self.trading_symbol}, {self.exchange_code}, {self.token}, {self.missing_trade_date}, {self.interval}, {self.start_time}, {self.end_time})"
