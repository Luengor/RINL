from sqlalchemy.orm import Session
from sqlalchemy.exc import IntegrityError

from fastapi import HTTPException
from models.activity import Activity as ActivityModel
from schemas.activity import ActivityFull, ActivityBase
from schemas.users import UserBase
from datetime import datetime

class ActivityDAO:
    @staticmethod
    def create_activity(activity: ActivityBase, user: UserBase, session: Session) -> ActivityFull:
        try:
            activity_model = ActivityModel(
                date=activity.date,
                user_email=user.email,
                minigame=activity.minigame,
                duration=activity.duration,
                activity_points=activity.activity_points,
                extra_data=activity.extra_data)
            
            session.add(activity_model)
            session.commit()

            return ActivityFull.model_validate(activity_model)

        except IntegrityError:
            raise HTTPException(status_code=400, detail="Invalid activity")
        
        except Exception:
            raise HTTPException(status_code=500, detail="Internal server error")
        
    @staticmethod
    def get_activities(email: str, minigame_filter: str | None, from_date: datetime, to_date: datetime, session: Session) -> list[ActivityFull]:
        if minigame_filter is None:
            activities = session.query(ActivityModel) \
                .filter(ActivityModel.user_email == email) \
                .filter(ActivityModel.date >= from_date) \
                .filter(ActivityModel.date <= to_date) \
                .all()
            return [ActivityFull.model_validate(activity) for activity in activities]
        else:
            activities = session.query(ActivityModel) \
                .filter(ActivityModel.user_email == email) \
                .filter(ActivityModel.minigame == minigame_filter) \
                .filter(ActivityModel.date >= from_date) \
                .filter(ActivityModel.date <= to_date) \
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

