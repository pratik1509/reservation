### Feedback

This project has been updated to use .Net 9 instead of .Net 3.1

### Scalability and Future Enhancements:

## API Gateway:

To scale the application effectively, we would have implemented an API Gateway. An API Gateway would provide several benefits, such as:

1> Centralized Authentication & Authorization:
2> Load Balancing:
3> Rate Limiting and Caching

## Horizontal Scaling of Services:

We could scale out the services by deploying multiple instances of key services (e.g., the new api service) behind a load balancer or API Gateway. This would ensure that the application can handle higher traffic loads and maintain responsiveness, especially during peak usage times.

## Container Orchestration with Kubernetes:

Given more time, we would have moved towards a container orchestration solution like Kubernetes to manage service scaling, health checks, and automated rollouts. Kubernetes would allow us to easily scale individual services and handle container management automatically, ensuring higher availability and fault tolerance.

## Distributed Tracing & Monitoring:

For better observability, we would have introduced distributed tracing tools (such as OpenTelemetry) along with centralized logging (e.g., using ELK Stack). This would allow us to monitor the health of each microservice, track requests across the system, and detect bottlenecks or failures in real-time.

### Feedback and Future Improvements:

1> Add Mapper:
Given more time, I would have added an automated mapping solution to streamline the conversion between DTOs (Data Transfer Objects) and response objects.

2> gRPC Call Retry Mechanism:
As it was mentioned in the ReadME the provided API is not reliable and fails a lot. In our current implementation, we are not handling retries for gRPC calls. With more time, I would have added a retry mechanism to automatically retry failed gRPC requests in cases of transient errors.

3> Handle Ticket Expiration Time Better:
I would have implemented a more robust and flexible mechanism for managing expiration times, possibly including dynamic configuration options based on application requirements or user-specific settings. This would enhance the accuracy of expiration checks and provide a better user experience by avoiding potential issues with expired data.

4> Read GRPC Call url from configuration
