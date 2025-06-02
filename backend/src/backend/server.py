"""Module to run the FastAPI server with different configurations."""
import sys
import os
from dotenv import dotenv_values
import uvicorn


def _run_test():
    os.environ["POSTGRES_HOST"] = "localhost"
    os.environ["SKIP_EMAIL"] = "true"
    uvicorn.run("backend.main:app", host="localhost", port=8000,
                log_level="info", reload=True)


def _run_production(host: str, port: str, workers: int = 4):
    uvicorn.run("backend.main:app", host=host, port=int(port),
                log_level="info", workers=workers, reload=False)


def main():
    """Main function to run the FastAPI server."""
    args = sys.argv[1:]
    if len(args) not in [1, 3] or (len(args) == 1 and args[0] != "test"):
        print("server test                    -> run in test mode")
        print("server <host> <port> <workers> -> run in production mode")
        sys.exit(1)

    values = {key: value if value else "" for key,
              value in dotenv_values(".env").items()}
    print("Using environment variables:")
    print('\n'.join(f"  {key} = {value}" for key, value in values.items()))
    os.environ.update(values)

    if len(args) == 1 and args[0] == "test":
        _run_test()
        return

    if len(args) != 3:
        print("Usage: python server.py <host> <port> <workers>")
        sys.exit(1)

    host, port, workers = args
    try:
        workers = int(workers)
    except ValueError:
        print("Workers must be an integer.")
        sys.exit(1)
    if workers <= 0:
        print("Workers must be greater than 0.")
        sys.exit(1)

    _run_production(host, port, workers)


if __name__ == "__main__":
    main()
