"""Simple configurations."""

import os

ENCRIPTION_KEY = os.environ.get("ENCRYPTION_KEY", "changethis").encode('utf-8')
"""The key used for encryption and decryption of sensitive data."""

API_URL = os.environ.get("VITE_API_URL", "http://localhost:8000/api")

VERIFY_REDIRECT_URL = os.environ.get(
    "VERIFY_REDIRECT_URL", "http://localhost:5173")
