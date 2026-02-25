from typing import Any
from python_lib import StoredProcedureExecutor

class CorporateActionsRepository:
    def __init__(self, sp_executor: StoredProcedureExecutor):
        self._sp_executor = sp_executor

    def save_corporate_actions_with_tvp(self, sp_name: str,
                                  corporate_actions: Any|bytes) -> None:
       
        self._sp_executor.execute_with_tvp(
            sp_name = sp_name,            
            tvp_data = corporate_actions
        )