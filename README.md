# CloudComputing

To run the containers use: 
docker-compose up --build
docker-compose down


# Build the Docker image for the Web API
docker build -t web-api .
docker run -d -p 8080:80 web-api


# Build the Docker image for the Web App
docker build -t web-app .
docker run -d -p 8081:80 web-app


Add a Service Registration Library
For Consul: Use a NuGet package like Consul.AspNetCore.
For consul visit: http://localhost:8500/uis
