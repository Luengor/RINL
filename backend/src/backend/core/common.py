"""Simple configurations."""

import os

ENCRIPTION_KEY = os.environ.get("ENCRYPTION_KEY", "changethis").encode('utf-8')
"""The key used for encryption and decryption of sensitive data."""

API_URL = os.environ.get("VITE_API_URL", "")
"""The base URL for the API, used for constructing full URLs in the application."""
assert API_URL != "", "VITE_API_URL must be set in the environment variables."

VERIFY_URL = os.environ.get("VERIFY_URL", "")
"""The URL for email verification, used in the verification process."""
assert VERIFY_URL != "", "VERIFY_URL must be set in the environment variables."

FRONTEND_URL = os.environ.get("FRONTEND_URL", "")
"""The URL of the frontend application, used for CORS and other integrations."""
assert FRONTEND_URL != "", "FRONTEND_URL must be set in the environment variables."
