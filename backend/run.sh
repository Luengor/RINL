#!/bin/env bash

# Get the environment variables 
source .env
export POSTGRES_HOST=localhost
export EMAIL_USER
export EMAIL_PASSWORD
export SKIP_EMAIL=true 

# Run the backend
cd backend
python3 -m fastapi dev main.py

