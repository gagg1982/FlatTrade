from datetime import datetime, date

def to_datetime_safe(value):
    if value in (None, "-", ""):
        return date(1970, 1, 1)

    for fmt in ("%d-%b-%Y", "%d %b %Y", "%d-%m-%Y", "%Y-%m-%d", "%d/%m/%Y"):
        try:
            return datetime.strptime(value, fmt)
        except ValueError:
            continue

    raise ValueError(f"Invalid date format: {value}")

def to_sql_date(value):
    return to_datetime_safe(value).strptime(value, "%d-%m-%Y").date()

def dicts_to_tuple_rows(data: list[dict], TVP_COLUMN_MAP: dict) -> list[tuple]:
    rows = []

    for item in data:
        row = tuple(
            item.get(src_key)
            for src_key in TVP_COLUMN_MAP.keys()
        )
        rows.append(row)

    return rows

def normalize_record(item: dict) -> dict:
    """
    Converts all keys ending with 'Date':
    - string → datetime.date
    - "-"    → epoch date (1970-01-01)
    - None   → None
    """
    result = {}

    for key, value in item.items():
        if key.endswith("Date"):
            if value == "-":
                result[key] = date(1970, 1, 1)
            elif value is None:
                result[key] = date(1970, 1, 1)
            elif isinstance(value, date):
                result[key] = value
            elif isinstance(value, str):
                result[key] = to_datetime_safe(value).date()
            else:
                raise TypeError(f"Invalid value for {key}: {value}")
        else:
            result[key] = value

    return result
