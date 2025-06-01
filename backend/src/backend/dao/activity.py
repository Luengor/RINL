# pylint: disable=raise-missing-from
from datetime import datetime
from functools import lru_cache
from sqlalchemy.orm import Session
from sqlalchemy.exc import IntegrityError
from fastapi import HTTPException
from backend.models.activity import Activity as ActivityModel
from backend.schemas.activity import ActivityFull, ActivityBase
from backend.schemas.users import UserBase


class ActivityDAO:
    """Data Access Object for handling activities in the database."""

    @staticmethod
    def create_activity(activity: ActivityBase, user: UserBase, session: Session) -> ActivityFull:
        """Create a new activity in the database.

        Args:
            activity (ActivityBase): The activity data to create.
            user (UserBase): The user associated with the activity.
            session (Session): The SQLAlchemy session to use for the database operations.

        Returns:
            ActivityFull: The created activity with full details.

        Raises:
            HTTPException: If the activity is invalid or if there is an internal server error.
        """
        try:
            activity_model = ActivityModel(
                date=activity.date,
                user_email=user.email,
                minigame=activity.minigame,
                duration=activity.duration,
                score=activity.score,
                activity_points=activity.activity_points,
                extra_data=activity.extra_data)

            session.add(activity_model)
            session.commit()

            # Clear the lru_cache for the get function
            ActivityDAO.get_activities.cache_clear()

            return ActivityFull.model_validate(activity_model)

        except IntegrityError:
            raise HTTPException(status_code=400, detail="Invalid activity")

        except Exception:
            raise HTTPException(
                status_code=500, detail="Internal server error")

    @staticmethod
    @lru_cache(maxsize=128)
    def get_activities(email: str, minigame_filter: str | None, from_date: datetime, to_date: datetime, session: Session) -> list[ActivityFull]:
        """Retrieve activities for a user within a date range, optionally filtered by minigame.

        This method is cached to improve performance for frequently accessed data.
        The cache is cleared whenever a new activity is created or deleted.

        Args:
            email (str): The email of the user whose activities are to be retrieved.
            minigame_filter (str | None): Optional filter for the minigame type.
            from_date (datetime): The start date for filtering activities.
            to_date (datetime): The end date for filtering activities.
            session (Session): The SQLAlchemy session to use for the database operations.

        Returns:
            list[ActivityFull]: A list of activities matching the criteria, sorted by date.

        Raises:
            HTTPException: If there is an internal server error.
        """
        if minigame_filter is None:
            activities = session.query(ActivityModel) \
                .filter(ActivityModel.user_email == email) \
                .filter(ActivityModel.date >= from_date) \
                .filter(ActivityModel.date <= to_date) \
                .order_by(ActivityModel.date) \
                .all()
            return [ActivityFull.model_validate(activity) for activity in activities]

        activities = session.query(ActivityModel) \
            .filter(ActivityModel.user_email == email) \
            .filter(ActivityModel.minigame == minigame_filter) \
            .filter(ActivityModel.date >= from_date) \
            .filter(ActivityModel.date <= to_date) \
            .order_by(ActivityModel.date) \
            .all()

        return [ActivityFull.model_validate(activity) for activity in activities]

    @staticmethod
    def delete_activity(activity_id: int, user_email: str, session: Session) -> ActivityFull | None:
        """Delete an activity by its ID and user email.

        Args:
            activity_id (int): The ID of the activity to delete.
            user_email (str): The email of the user who owns the activity.
            session (Session): The SQLAlchemy session to use for the database operations.

        Returns:
            ActivityFull | None: The deleted activity details if found, otherwise None.

        Raises:
            HTTPException: If the activity is not found or if there is an internal server error.
        """
        activity = session.query(ActivityModel) \
            .filter(ActivityModel.uuid == activity_id) \
            .filter(ActivityModel.user_email == user_email) \
            .first()

        if activity is None:
            raise HTTPException(status_code=404, detail="Activity not found")

        # Clear the lru_cache for the get function
        ActivityDAO.get_activities.cache_clear()

        model = ActivityFull.model_validate(activity)
        session.delete(activity)
        session.commit()

        return model
