
# Task management web application

Simple task management app that is build using Vue.js, ASP.NET API and MSSQL, every component runs on a separate docker container.

## Prerequisites

- Docker
- Patience

# Setup
0. Make sure this option in Docker is enabled `Add the *.docker.internal names to the host's etc/hosts file (Requires password)`
1. Clone the repository `git clone https://github.com/mvd3/tm_app.git`
2. Go to the folder `cd tm_app`
3. Start the containers `docker compose up`
4. Once all services are started, open `localhost:5000` in your browser, and possibly wait until the app initializes the database (for some reason, the server container takes up to a few minutes to accept calls from outside). The app will automatically reattempt to initialize the database. 
5. Once the app is loaded (you will see 2 tasks loaded), you can start working.
