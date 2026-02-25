from pathlib import Path
from concurrent.futures import ThreadPoolExecutor, as_completed
import logging
from pathlib import Path
import subprocess
import sys
import time

PROJECTS = [
    Path(r"D:\TradingApplication\NpsNav\main.py"),
    #Path(r"D:\TradingApplication\MissingCandles\main.py"),
    Path(r"D:\TradingApplication\CorporateActions\main.py"),
]

MAX_PARALLEL = 5
RETRIES = 3

LOG_DIR = Path(r"D:\TradingApplication\Logs")
LOG_DIR.mkdir(exist_ok=True)

LOG_FILE = LOG_DIR / "master_executor.log"

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s | %(levelname)s | %(message)s",
    handlers=[
        logging.FileHandler(LOG_FILE),
        logging.StreamHandler()
    ]
)

logger = logging.getLogger("master-runner")

def run_with_retries(script_path, retries=3, delay=5):
    for attempt in range(1, retries + 1):
        logger.info(f"Starting {script_path} (attempt {attempt}/{retries})")

        try:
            process = subprocess.Popen(
                [sys.executable, str(script_path)],
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                text=True,
                bufsize=1  # line-buffered
            )

            # Stream STDOUT live
            for line in process.stdout:
                logger.info(f"[{script_path.name}] {line.rstrip()}")

            # Stream STDERR live
            for line in process.stderr:
                logger.error(f"[{script_path.name}] {line.rstrip()}")

            return_code = process.wait()

            if return_code == 0:
                logger.info(f"{script_path} completed successfully")
                return True
            else:
                raise subprocess.CalledProcessError(return_code, script_path)

        except Exception as e:
            logger.error(f"{script_path} failed: {e}")

            if attempt == retries:
                logger.error(f"{script_path} FAILED after {retries} attempts")
                return False

            time.sleep(delay * attempt)

def main():
    logger.info("===== MASTER RUN STARTED =====")

    results = {}

    with ThreadPoolExecutor(max_workers=MAX_PARALLEL) as executor:
        futures = {
            executor.submit(run_with_retries, script, RETRIES): script
            for script in PROJECTS
        }

        for future in as_completed(futures):
            script = futures[future]
            success = future.result()
            results[script] = success

    logger.info("===== EXECUTION SUMMARY =====")
    for script, success in results.items():
        status = "SUCCESS" if success else "FAILED"
        logger.info(f"{script}: {status}")

    if not all(results.values()):
        logger.error("One or more projects failed")
        raise SystemExit(1)

    logger.info("All projects completed successfully")

if __name__ == "__main__":
    main()
