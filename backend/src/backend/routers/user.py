"""User API router for managing user-related operations."""

from fastapi import APIRouter, Depends, Response, HTTPException
from jwt.exceptions import InvalidTokenError
from sqlalchemy.orm import Session
from backend.core.auth import get_current_user, get_current_verified_user
from backend.core.auth_utils import create_access_token, decode_token
from backend.core.common import VERIFY_URL, FRONTEND_URL
from backend.core.db import get_db
from backend.core.mail import get_send_email, SendEmailType
from backend.dao.user import UserDAO
from backend.schemas.users import UserBase, RegisterUser, ModifyUser

router = APIRouter(
    prefix="/user",
    tags=["user"],
)

VERICIATION_EMAIL = ("Verifica tu correo electrónico",
                     """<!DOCTYPE html>
<html>
  <head>
    <meta http-equiv="content-type" content="text/html; charset=UTF-8">
  </head>
  <body>
    <p>Para verificar tu correo electrónico haz click en la criatura azul bailando:</p>
    <a href="{url}" target="_blank">
      <img src="{rivulet}" alt="Criatura azul bailando" style="width: 200px; height: 200px;">
    </a>
    <p>Si no puedes hacer click, copia y pega la siguiente URL en tu navegador:</p>
    <p><a href="{url}" target="_blank">{url}</a></p>
    <p>Si no has solicitado esta verificación, ignora este correo.</p>
  </body>
</html>""")

UPDATE_EMAIL = ("Actualiza tu correo electrónico",
                """<!DOCTYPE html>
<html>
  <head>
    <meta http-equiv="content-type" content="text/html; charset=UTF-8">
  </head>
  <body>
    <p>Para cambiar tu correo a "{nuevo_correo}" haz click en la criatura azul bailando:</p>
    <a href="{url}" target="_blank">
      <img src="{rivulet}" alt="Criatura azul bailando" style="width: 200px; height: 200px;">
    </a>
    <p>Si no puedes hacer click, copia y pega la siguiente URL en tu navegador:</p>
    <p><a href="{url}" target="_blank">{url}</a></p>
    <p>Si no has solicitado esta verificación, ignora este correo.</p>
  </body>
</html>""")


@router.post("/", response_model=UserBase)
async def create_user(
    user: RegisterUser, session: Session = Depends(get_db), send_email: SendEmailType = Depends(get_send_email)
):
    """Create a new user.

    This function is an endpoint.

    Args:
        user (RegisterUser): The user to create.
        session (Session): The database session, automatically injected by FastAPI.
        send_email (SendEmailType): The email sending function, automatically injected by FastAPI.
    Returns:
        UserBase: The created user with basic details.
    """
    # Check if user already exists
    if UserDAO.get_user(user.email, session):
        raise HTTPException(status_code=400, detail="User already exists")

    # Try to send verification email
    token = create_access_token(
        data={"sub": user.email, "type": "verify"},
    )

    email = send_email(
        user.email,
        VERICIATION_EMAIL[0],
        VERICIATION_EMAIL[1].format(
            url=f"{VERIFY_URL}?verify={token}",
            rivulet=f"{FRONTEND_URL}/rivulet.gif"
        ),
    )

    if not email:
        raise HTTPException(
            status_code=500, detail="Error sending verification email"
        )

    # Create user
    return UserDAO.create_user(user, session)


@router.post("/me/email-token")
async def verify_email_token(
    token: str,
    user: UserBase = Depends(get_current_user),
    session: Session = Depends(get_db),
):
    """Verify the email token for the current user.

    This function is an endpoint.

    Args:
        token (str): The verification token to verify.
        user (UserBase): The current user, automatically injected by FastAPI.
        session (Session): The database session, automatically injected by FastAPI.
    Returns:
        Response: A response indicating the email has been verified.
    """
    # Decode the token
    try:
        dict_token = decode_token(token)
    except InvalidTokenError:
        raise HTTPException(
            status_code=400, detail="Invalid verification token")

    # Check that the token is for the correct user
    email = dict_token.get("sub")
    if not email or email != user.email:
        raise HTTPException(
            status_code=400, detail="Verification token does not match user"
        )

    # Get the type of the token
    token_type = dict_token.get("type")
    if not token_type:
        raise HTTPException(
            status_code=400, detail="Invalid token type"
        )

    # If the type is 'verify', verify the user
    if token_type == "verify":
        if not UserDAO.verify_user(email, session):
            raise HTTPException(
                status_code=404, detail=f"User not found {email}"
            )

        return Response(status_code=200, content="ok")

    # If the type is 'update_email', update the user's email
    if token_type == "update_email":
        new_email = dict_token.get("new_email")
        if not new_email:
            raise HTTPException(
                status_code=400, detail="New email not provided"
            )

        # Update the user's email
        UserDAO.update_user(
            user,
            ModifyUser(email=new_email),
            session,
        )

        return Response(status_code=200, content="ok")

    # If the type is not recognized, raise an error
    raise HTTPException(
        status_code=400, detail="Invalid verification token"
    )


@router.post("/verify-email")
async def send_verification_email(
    user: UserBase = Depends(get_current_user),
    send_email: SendEmailType = Depends(get_send_email),
):
    """Send a verification email to the user if they are not verified.

    This function is an endpoint.

    Args:
        user (UserBase): The current user, automatically injected by FastAPI.
        session (Session): The database session, automatically injected by FastAPI.
        send_email (SendEmailType): The email sending function, automatically injected by FastAPI.
    Returns:
        Response: A response indicating the email has been sent.
    """
    if user.verified:
        raise HTTPException(
            status_code=400, detail="User already verified"
        )

    token = create_access_token(
        data={"sub": user.email, "type": "verify"},
    )

    email = send_email(
        user.email,
        VERICIATION_EMAIL[0],
        VERICIATION_EMAIL[1].format(
            url=f"{VERIFY_URL}?verify={token}",
            rivulet=f"{FRONTEND_URL}/rivulet.gif"
        ),
    )

    if not email:
        raise HTTPException(
            status_code=500, detail="Error sending verification email"
        )

    return Response(status_code=200, content="ok")


@router.get("/me", response_model=UserBase)
async def get_me(user: UserBase = Depends(get_current_user)):
    """Get the current user's details.

    This function is an endpoint.

    Args:
        user (UserBase): The current user, automatically injected by FastAPI.
    Returns:
        UserBase: The current user's details.
    """
    return user


@router.put("/me", response_model=UserBase)
async def update_me(
    modifications: ModifyUser,
    user: UserBase = Depends(get_current_verified_user),
    session: Session = Depends(get_db),
    send_email: SendEmailType = Depends(get_send_email),
):
    """Update the current user's details.

    This function is an endpoint.

    Args:
        modifications (ModifyUser): The modifications to apply to the user.
        user (UserBase): The current user, automatically injected by FastAPI.
        session (Session): The database session, automatically injected by FastAPI.
        send_email (SendEmailType): The email sending function, automatically injected by FastAPI.
    Returns:
        UserBase: The updated user with basic details.
    """
    if modifications.email and modifications.email != user.email:
        # Check if the new email is already in use
        new_mail_user = UserDAO.get_user(modifications.email, session)
        if new_mail_user:
            raise HTTPException(
                status_code=400, detail="Email already in use"
            )

        # If the email is being changed, send a verification email
        token = create_access_token(
            data={
                "sub": user.email,
                "type": "update_email",
                "new_email": modifications.email,
            },
        )
        email = send_email(
            modifications.email,
            UPDATE_EMAIL[0],
            UPDATE_EMAIL[1].format(
                url=f"{VERIFY_URL}?verify={token}",
                nuevo_correo=modifications.email,
                rivulet=f"{FRONTEND_URL}/rivulet.gif"
            ),
        )

        if not email:
            raise HTTPException(
                status_code=500, detail="Error sending verification email"
            )

        modifications.email = None  # Clear the email to avoid updating it directly

    return UserDAO.update_user(user, modifications, session)


@router.delete("/me")
async def delete_me(
    user: UserBase = Depends(get_current_user), session: Session = Depends(get_db)
):
    """Delete the current user.

    This function is an endpoint.

    Args:
        user (UserBase): The current user, automatically injected by FastAPI.
        session (Session): The database session, automatically injected by FastAPI.
    Returns:
        Response: An ok response indicating the user has been deleted.
    """
    UserDAO.delete_user(user.email, session)
    response = Response(status_code=200, content="ok")
    return response
