import smtplib
from email.message import EmailMessage
from os import environ

EMAIL_HOST = environ.get('EMAIL_HOST', 'smtp.gmail.com')
EMAIL_USER = environ.get('EMAIL_USER')
EMAIL_PASSWORD = environ.get('EMAIL_PASSWORD')

def send_email(email: str, subject: str, content: str) -> bool:
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

