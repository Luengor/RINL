"""Simple configurations."""

import os

ENCRIPTION_KEY = os.environ.get("ENCRYPTION_KEY", "changethis").encode('utf-8')
"""The key used for encryption and decryption of sensitive data."""

API_URL = os.environ.get("VITE_API_URL", "http://localhost:8000/api")
"""The base URL for the API, used for constructing full URLs in the application."""

VERIFY_REDIRECT_URL = os.environ.get(
    "VERIFY_REDIRECT_URL", "http://localhost:5173")
"""The URL to redirect to after email verification, typically the frontend application URL."""

FRONTEND_URL = os.environ.get("FRONTEND_URL", "http://localhost:5173")
"""The URL of the frontend application, used for CORS and other integrations."""
