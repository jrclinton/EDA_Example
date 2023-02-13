# EDA_Example

Started learning dotNet and wanted to see what a basic event driven communication between two microservices would look like.


For this to run you will need to startup a rabbitMQ instance inside of docker.

> docker run -d  -p 15672:15672 -p 5672:5672 --hostname my-rabbit --name some-rabbit rabbitmq:3-management


Then you will need to create an exchange named "user" with a type of "fanout".

Create two queues named "user.otherservice", and "user.postservice".

Lastly, bind these two queues to the recently created "user" exchange.





For the basis of this, most code comes from this tutorial: https://itnext.io/how-to-build-an-event-driven-asp-net-core-microservice-architecture-e0ef2976f33f

Due to the combining of the Startup.cs and Program.cs files since the creation of this, there are some changes revolving around this.
I hope to continue developing this into something interesting and will update this README as needed.
