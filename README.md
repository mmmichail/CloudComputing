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



All the ports:
Backend https: https://localhost:7000
Backend http: http://localhost:5199
swagger: https://localhost:7000/swagger/index.html
health: https://localhost:7000/health-ui

Frontend https: https://localhost:7072/
Frontend http: http://localhost:5096/

deploy to azure
https://learn.microsoft.com/en-us/azure/aks/learn/quick-kubernetes-deploy-cli

Scale Up the Redaction Microservice
kubectl scale deployment/web-api-deployment --replicas=3

Deploy to Kubernetes
kubectl apply -f web-api-deployment.yaml
kubectl apply -f web-app-deployment.yaml
kubectl apply -f consul-deployment.yaml


Build New Version
docker build -t <your-repo>/web-api:latest .

Rolling Update on Kubernetes
kubectl set image deployment/web-api-deployment web-api=<your-repo>/web-api:latest


