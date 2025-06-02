"""Mail dependencies."""

import smtplib
from typing import Callable
from email.message import EmailMessage
from os import environ

EMAIL_HOST = environ.get('EMAIL_HOST', 'smtp.gmail.com')
EMAIL_USER = environ.get('EMAIL_USER')
EMAIL_PASSWORD = environ.get('EMAIL_PASSWORD')


SendEmailType = Callable[[str, str, str], bool]
"""Type alias for the send_email function.

The function takes the email address, subject, and content as parameters
and returns a boolean indicating success or failure.
"""


def get_send_email() -> SendEmailType:
    """Dependency that provides the send_email function.

    Returns:
        SendEmailType: A callable that sends an email.
    """
    return send_email


def send_email(email: str, subject: str, content: str) -> bool:
    """Sends an email to the specified address with the given subject and content.

    If the environment variable `SKIP_EMAIL` is set, the email sending is skipped and the function returns True.

    Args:
        email (str): The recipient's email address.
        subject (str): The subject of the email.
        content (str): The content of the email.

    Returns:
        bool: True if the email was sent successfully, False otherwise.
    """
    if environ.get('SKIP_EMAIL', False):
        return True

    # Create the email
    msg = EmailMessage()
    msg['Subject'] = subject
    msg['From'] = EMAIL_USER
    msg['To'] = email
    msg.set_content(content)

    # Send the email
    s = smtplib.SMTP(EMAIL_HOST, 587)
    s.starttls()

    assert EMAIL_USER is not None, 'EMAIL_USER is not set'
    assert EMAIL_PASSWORD is not None, 'EMAIL_PASSWORD is not set'
    s.login(user=EMAIL_USER, password=EMAIL_PASSWORD)

    try:
        s.send_message(msg)
    except smtplib.SMTPRecipientsRefused:
        return False
    finally:
        s.quit()

    return True
