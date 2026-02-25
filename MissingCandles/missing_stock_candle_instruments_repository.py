from typing import Any
from python_lib import StoredProcedureExecutor
from datetime import date
from missing_stock_candle_instruments_model import MissingStockCandleInstruments

class MissingStockCandleInstrumentsRepository:
    def __init__(self, sp_executor: StoredProcedureExecutor):
        self._sp_executor = sp_executor

    #[sp_SanityOnOneMinuteCandles_MissingDates] '2025-12-01'
    #[sp_SanityOnDailyCandles_MissingDates] '2025-12-01'

    def get_all_missing_stock_candle_instruments(self, sp_name: str, interval: str, start_time: str, end_time: str, missing_date: date) -> list[MissingStockCandleInstruments]:
        rows, _ = self._sp_executor.execute(sp_name, (missing_date,))

        return [
            MissingStockCandleInstruments(
                trading_symbol=row.TradingSymbol,
                exchange_code=row.ExchangeCode,
                token=row.Token,
                missing_trade_date=row.MissingTradeDate,
                interval=interval,
                start_time= start_time,
                end_time= end_time
            )
            for row in rows
        ]

    def save_candle_data_with_tvp(self, sp_name: str,
                                  candles: Any|bytes) -> None:
       
        self._sp_executor.execute_with_tvp(
            sp_name = sp_name,            
            tvp_data = candles
        )