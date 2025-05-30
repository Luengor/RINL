import sys
import pytest


def main():
    pytest_args = sys.argv[1:] if len(sys.argv) > 1 else []
    # pytest_args.insert(0, "-m")
    # pytest_args.insert(1, "backend.tests")

    # Run pytest with the provided arguments
    pytest.main(pytest_args)
