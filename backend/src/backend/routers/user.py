"""User API router for managing user-related operations."""

from fastapi import APIRouter, Depends, Response, HTTPException
from fastapi.responses import RedirectResponse
from jwt.exceptions import InvalidTokenError
from sqlalchemy.orm import Session
from backend.core.auth import get_current_user, get_current_verified_user
from backend.core.auth_utils import create_access_token, decode_token
from backend.core.common import API_URL, VERIFY_REDIRECT_URL, FRONTEND_URL
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
      <img src="{rivulet}" alt="Bailando" style="width: 200px; height: 200px;">
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
      <img src="{rivulet}" alt="Bailando" style="width: 200px; height: 200px;">
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
            url=f"{API_URL}/user/verify?token={token}",
            rivulet=f"{FRONTEND_URL}/rivulet.gif"
        ),
    )

    if not email:
        raise HTTPException(
            status_code=500, detail="Error sending verification email"
        )

    # Create user
    return UserDAO.create_user(user, session)


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
            url=f"{API_URL}/user/verify?token={token}",
            rivulet=f"{FRONTEND_URL}/rivulet.gif"
        ),
    )

    if not email:
        raise HTTPException(
            status_code=500, detail="Error sending verification email"
        )

    return Response(status_code=200, content="ok")


@router.get("/verify")
async def verify_user(
    token: str,
    session: Session = Depends(get_db),
):
    """Verify a user using the verification code from the token.

    This function is an endpoint.

    Args:
        token(dict[str, Any]): The token containing the verification code, automatically injected by FastAPI.
        session (Session): The database session, automatically injected by FastAPI.
    Returns:
        Response: A response indicating the user has been verified.
    """
    # Get the user from the verification token
    try:
        dict_token = decode_token(token)
    except InvalidTokenError:
        raise HTTPException(
            status_code=400, detail="Invalid verification token")

    email = dict_token.get("sub")
    assert email

    if "type" not in dict_token or dict_token["type"] != "verify":
        raise HTTPException(
            status_code=400, detail="Invalid verification token"
        )

    # The token is a verification token, verify the user
    if not UserDAO.verify_user(email, session):
        raise HTTPException(
            status_code=400, detail="Invalid verification code"
        )

    return RedirectResponse(
        f"{VERIFY_REDIRECT_URL}?verified",
        status_code=303
    )


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
                url=f"{API_URL}/user/me/email?token={token}",
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


@router.get('/me/email')
async def update_email(
    token: str,
    session: Session = Depends(get_db),
):
    """Update the current user's email using a verification token.

    This function is an endpoint.

    Args:
        token (str): The verification token to update the email.
        session (Session): The database session, automatically injected by FastAPI.
    Returns:
        str: The new email of the user.
    """
    # Get the user from the verification token
    try:
        dict_token = decode_token(token)
    except InvalidTokenError:
        raise HTTPException(
            status_code=400, detail="Invalid verification token")

    email = dict_token.get("sub")
    assert email
    print(dict_token)

    user = UserDAO.get_user(email, session)
    if not user:
        raise HTTPException(
            status_code=404, detail="User not found"
        )

    if "type" not in dict_token or dict_token["type"] != "update_email":
        raise HTTPException(
            status_code=400, detail="Invalid verification token"
        )

    # The token is an update email token, update the user email
    new_email = dict_token["new_email"]
    assert new_email

    UserDAO.update_user(
        user,
        ModifyUser(email=new_email),
        session,
    )

    return RedirectResponse(
        f"{VERIFY_REDIRECT_URL}?email={new_email}",
        status_code=303
    )


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
