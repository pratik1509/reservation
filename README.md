## Movies challenge

---

## Context

We want a C# .Net Core Web Application API project that meets our requirements.

You will find the Data layer is implemented and is an In-Memory Database. 

The application represents a Cinema. We want to manage the showtimes of the cinema, getting some data from the **[Provided API](http://localhost:7172/swagger/index.html).**

The test includes a **docker-compose** with **Redis** and the provided Api, you will need Docker to be able run them.

It has following features:

- Create showtimes.
- Reserve seats.
- Buy seats.

---

## Starting the API

- You will need Docker in order to use this API and then run the next command:

```powershell
docker-compose up
```

- By default, the provided API will run on [*http://localhost:7172/swagger/index.html*](http://localhost:7172/swagger/index.html) , [https://localhost:7443/swagger/index.html](https://localhost:7443/swagger/index.html)
- For GRPC use the **HTTPS** port
- And Redis in the default port.
- When you end the test

```powershell
docker-compose down
```

## Commands and queries

- **Create showtime**
    
    Should create showtime and should grab the movie data from the ProvidedApi.
    
- **Reserve seats**
    - Reserving the seat response will contain a GUID of the reservation, also the number of seats, the auditorium used and the movie that will be played.
    - After 10 minutes after the reservation is created, the reservation is considered expired by the system.
    - It should not be possible to reserve the same seats two times in 10 minutes.
    - It shouldn't be possible to reserve an already sold seat.
    - All the seats, when doing a reservation, need to be contiguous.
- **Buy seats**
    - We will need the GUID of the reservation, it is only possible to do it while the seats are reserved.
    - It is not possible to buy the same seat two times.
    - Expired reservations (older than 10 minutes) cannot be confirmed.
    - We are not going to use a Payment abstraction for this case, just have an Endpoint which I can use to Confirm a Reservation.
    
### API communication with ProvidedApi

It uses the  **GRPC** API, you should check the [Swagger](http://localhost:7172/swagger/index.html) for more info.

The solution includes the proto of Provided API and a small piece of code that tries to connect with the GRPC API.

### Cache

Application uses the **Redis** container provided in the docker-compose.yaml

### Execution Tracking

We track the execution time of each request done to the service and log the time in the Console.
