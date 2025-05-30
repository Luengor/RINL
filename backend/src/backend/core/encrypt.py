import os

ENCRIPTION_KEY = os.environ.get("ENCRYPTION_KEY", "changethis").encode('utf-8')
