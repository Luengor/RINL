from sqlalchemy.orm import Session
from sqlalchemy.exc import IntegrityError

from fastapi import HTTPException
from backend.models.activity import Activity as ActivityModel
from backend.schemas.activity import ActivityFull, ActivityBase
from backend.schemas.users import UserBase
from datetime import datetime
from functools import lru_cache

class ActivityDAO:
    @staticmethod
    def create_activity(activity: ActivityBase, user: UserBase, session: Session) -> ActivityFull:
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

            # Clear the lru_cache for the get_activities method
            ActivityDAO.get_activities.cache_clear()

            return ActivityFull.model_validate(activity_model)

        except IntegrityError:
            raise HTTPException(status_code=400, detail="Invalid activity")
        
        except Exception:
            raise HTTPException(status_code=500, detail="Internal server error")
        
    @staticmethod
    @lru_cache(maxsize=128)
    def get_activities(email: str, minigame_filter: str | None, from_date: datetime, to_date: datetime, session: Session) -> list[ActivityFull]:
        if minigame_filter is None:
            activities = session.query(ActivityModel) \
                .filter(ActivityModel.user_email == email) \
                .filter(ActivityModel.date >= from_date) \
                .filter(ActivityModel.date <= to_date) \
                .order_by(ActivityModel.date) \
                .all()
            return [ActivityFull.model_validate(activity) for activity in activities]
        else:
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
        activity = session.query(ActivityModel) \
            .filter(ActivityModel.uuid == activity_id) \
            .filter(ActivityModel.user_email == user_email) \
            .first()
        
        if activity is None:
            raise HTTPException(status_code=404, detail="Activity not found")

        model = ActivityFull.model_validate(activity)
        session.delete(activity)
        session.commit()

        return model 

