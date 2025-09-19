-----------
-----------
initial setup for new user:

Inside the project folder

0. delete venv folder if it exists! and exit venv environment

1. create python venv:
    python -m venv venv

2. activate venv:
    venv\Scripts\activate

3. install dependencies in venv:
    pip install -r requirements.txt

4. set up .env file with:
    API_PORT=8000
    DATABASE_URL=postgresql+asyncpg://cataloguser:catalogpass@catalog-db:5432/catalogdb
        - DATABASE_URL will be according to the local setup, i use a docker container. this should probably get started in this app somehow with a docker compose or something.

5. update migrations:
    alembic upgrade head

6. start app:
    use docker compose
-----------

-----------
docker compose:

docker compose build

docker compose up -d

or for combined up:

docker compose up --build -d

docker compose logs -f catalog-api

docker compose down
-----------

-----------
install dependencies:

pip install -r requirements.txt
-----------

-----------
Alembic migrations:

alembic revision --autogenerate -m "added new column"
alembic upgrade head
-----------

