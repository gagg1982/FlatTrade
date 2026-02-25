#Api Key:     
#secret:      
ApiKey = ""
ClientId = ""
Password = ""
QrValue = ""
Totp=""

Server= "dbserver\\appdb,1436"
Database = "TradingApplication"
StartMissingDate = "2026-02-01"
SpNameIntervalTuple = [ 
                        ("dbo.sp_SanityOnOneMinuteCandles_MissingDates", "ONE_MINUTE", "09:00", "16:00", "dbo.sp_UpsertStocksOhlcv_1"),                        
                        ("dbo.sp_SanityOnDailyCandles_MissingDates", "ONE_DAY", "00:00", "00:00", "dbo.sp_UpsertStocksOhlcv_1440"),                        
                      ]

# package import statement
import time
from datetime import datetime
from dateutil import parser
from SmartApi import SmartConnect #or from smartapi.smartConnect import SmartConnect
import pyodbc
import pyotp
from python_lib import DbConfig, SqlConnection, StoredProcedureExecutor
from missing_stock_candle_instruments_model import MissingStockCandleInstruments
from missing_stock_candle_instruments_repository import MissingStockCandleInstrumentsRepository

#import smartapi.smartExceptions(for smartExceptions)

def fill_missing_candles(obj: SmartConnect ,repository: MissingStockCandleInstrumentsRepository, sp_name: str, interval: str, start_time: str, end_time: str, missing_date: str, insert_sp_name: str):
    total_cnt = 0
    not_processed_cnt =0
    processed_cnt =0
    missingStockCandleInstruments = repository.get_all_missing_stock_candle_instruments(sp_name, interval, start_time, end_time, missing_date)
    print("Total missing instruments for interval {}: {}".format(interval, len(missingStockCandleInstruments)))

    for instrument in missingStockCandleInstruments:
        try:
            if total_cnt % 100 == 0:
                print("Processed missing candles Interval={}, Total={}, Processed={}, NotProcessed={}, Remaining={}, CurrentTime={}".
                      format(instrument.interval , len(missingStockCandleInstruments), processed_cnt, not_processed_cnt, len(missingStockCandleInstruments) - total_cnt, datetime.now().time()))
            total_cnt = total_cnt +1
            time.sleep(0.333)
            historicParam={
                "exchange": instrument.exchange_code,
                "symboltoken": str(instrument.token),
                "interval": str(instrument.interval),
                "fromdate": str(instrument.missing_trade_date) + " {}".format(instrument.start_time),
                "todate": str(instrument.missing_trade_date) + " {}".format(instrument.end_time), 
            }
            candle_data = obj.getCandleData(historicParam)

            if len(candle_data['data']) ==0:
                #print("No candle data returned for instrument {}, date {}".format(instrument.trading_symbol, instrument.missing_trade_date))
                not_processed_cnt = not_processed_cnt +1
                continue
            
            for row in candle_data['data']:
                row.insert(0, instrument.token)
                dt = parser.isoparse(row[1])
                dt_naive = dt.replace(tzinfo=None)
                row.insert(1, dt_naive)
                row.remove(row[2])
           
            #print(candle_data['data'])
            repository.save_candle_data_with_tvp(insert_sp_name, candle_data['data'])
            processed_cnt = processed_cnt +1
        except Exception as e:
            print("Historic Api failed for instrument {}, date {}: {}".format(instrument.trading_symbol, instrument.missing_trade_date, e))
            not_processed_cnt = not_processed_cnt +1
            time.sleep(5 * 60)

    print("Processed missing candles Interval={}, Total={}, Processed={}, NotProcessed={}, CurrentTime={}".
                      format(instrument.interval , total_cnt, processed_cnt, not_processed_cnt, len(missingStockCandleInstruments), datetime.now().time()))

def main():
    #create object of call
    obj=SmartConnect(api_key=ApiKey)

    try:
        Totp = pyotp.TOTP(QrValue).now()
    except Exception as e:
        print("Invalid Token: The provided token is not valid.")
        raise e.Message

    #login api call
    data = obj.generateSession(ClientId,Password,Totp)
    refreshToken= data['data']['refreshToken']

    #fetch the feedtoken
    feedToken=obj.getfeedToken()

    #fetch User Profile
    userProfile= obj.getProfile(refreshToken)

    config = DbConfig(server = Server, database= Database)

    connection = SqlConnection(config)
    executor = StoredProcedureExecutor(connection)
    repository = MissingStockCandleInstrumentsRepository(executor)

    for nm in SpNameIntervalTuple:
        fill_missing_candles(obj, repository, nm[0], nm[1], nm[2], nm[3], StartMissingDate, nm[4])
        
    #logout
    try:
        logout=obj.terminateSession(ClientId)
        print("Logout Successfull")
    except Exception as e:
        print("Logout failed: {}".format(e))

if __name__ == "__main__":
    print("========== Importing missing candles data STARTING====================", datetime.now())
    main()
    print("========== Importing missing candles data FINISHED====================", datetime.now())