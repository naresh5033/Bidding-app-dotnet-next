# dotnet Microservices

You can run this app locally on your computer by following these instructions:

1. Using your terminal or command prompt clone the repo onto your machine in a user folder

```
2. Change into the Carsties directory
```

cd Carsties

```
3. Ensure you have Docker Desktop installed on your machine.  If not download and install from Docker and review their installation instructions for your Operating system [here](https://docs.docker.com/desktop/).
4. Build the services locally on your computer by running (NOTE: this may take several minutes to complete):
```

docker compose build

```
5. Once this completes you can use the following to run the services:
```

docker compose up -d

```
6. To see the app working you will need to provide it with an SSL certificate.   To do this please install 'mkcert' onto your computer which you can get from [here](https://github.com/FiloSottile/mkcert).  Once you have this you will need to install the local Certificate Authority by using:
```

mkcert -install

```
7. You will then need to create the certificate and key file on your computer to replace the certificates that I used.   You will need to change into the 'devcerts' directory and then run the following command:

```

8.  You will also need to create an entry in your host file so you can reach the app by its domain name. Please use this [guide](https://phoenixnap.com/kb/how-to-edit-hosts-file-in-windows-mac-or-linux) if you do not know how to do this. Create the following entry:

### Auction service

- first lets build the auction service, `dotnet new sln` to create a solution file.
- `dotnet new webapi -o src/AuctionService`
- and add the service to our solution - `dotnet sln add src/AuctionService`

- for the dev mode .. we will be using the appsetting.development.json for our configuration

- to install the entity `dotnet tool install dotnet-ef -g` and to check the installed tool `dotnet tool install dotnet-ef -g`
- for creating migrations `dotnet ef migrations add "initialCreate" -o Data/Migrations` this will look after the Db context and create a schema

-**docker for the db** we can make a docker compose file to make our postgresql connection up and running. - to run the docker compose file `docker compose up -d` will pull down the posgre image and run the container - now that we ve our db we can update our migrations `dotnet ef database update` - `dotnet watch` to watch the activities created by our db.

- note : when we use static kw in the class so we can initialize that method inside a different class w/o needing to initialize its class(parent)

- **Ef** - in this ef the data creation flow goes like this different steps.
- 1. we create **Entities**
- 2. then the **DB context** ex Auction DB context
- 3. then we run our migrations to create a iniial migrations
- 4. then the **DB intializer** / also in the migrations dir .. to create the seed . in our db

**Dtos and automapper**

- once we done with the above all the process now ve to find a way to return our data to the client from our database, this is where the dtos and the automapper comes in.
- the dtos can be used to restrict the data props that we mentioned in the entities and flatten some obj into strings ex - in entiy we ve status field of enum val but we can flatten that obj into strings in the dto
- and also in the dtos we can do things like remove the optional props (?) and remove the default vals.
- and finally in our auction dto we mix the auction entity and the item entity with onlyt the required properties.

- **AutoMapper**
- now that we ve our dtos in place we need a tool to map our Entity to the dto, and that's where the automapper comes in
- as long we ve the same name in the entity and the dto the auto mapper will take care for us.
- inside the RequestHelper dir we ve the "Profile Mapper" automapper.. once we create all the auto mappers
- mapping Propiles class impl the profile interface from the Auto mapper.
- then register the mapper in the prog.cs file // so it will look for any class that derives from the prof class(MappingProfile.cs), and register the mappings in mem

### second Microservice (full Search)

- this ms is responsible for the search functionality of the application. `dotnet new webapi -o /src/SearchService`and add the solution `dotnet sln add src/SearchService`
- this serivice will be using mongo and mongo Entity(for querying) and service bus **Rabbit Mq**
- and we konw the **law of Ms** is we don't share the db amongst em
- we could've gone with elasticsearch but its resource hunk, so we can go for this simpler approach

- **pagination** - when we search thru the query, we don't want to return millions of the results back to the user instead we use pagination.

  - to use pagination we can use the query instead of find we use **PageSearch** and now we can make use of the page number and the page size(limit) params to restrict the pagination

- **synchronous msg b/w services** - there are 2 types of synch service in the **http and Grpc** - but this will only work if both services are available if one fails then there will be no communication b/w em.
- so when it comes to ms we want those service to be independent regardless of the availability of other services
- in the messaging world if the client has to wait for the response then its a "Synchronous" communication
- and the services that uses synchronous communication are called **Distributed Monolith**

- **Synchronous communication** - where the service A doessn't need to know anything about the service B they just use the message bus to communicate each other
- event driven approach, just fire an event and forget, the msg bus will take care of the rest, it delivers to the corresponding service. and the service B will pick up the message from the bus w/o knowing anything about service a.

- to establish the synchronous connection b/w aution service, create a service/class called "AuctionSvcHttpClient" in search service ..
  - and we config the auction service in the app setting prod file and register the service in the prog.cs
  - **http polling** when any of the service is down, and comes back alive then instead of sending the http req to the service we can create a polling (and specify the no.of req we want send in case of failure) that will send automatic req when the service is back
  - we can use a package called **ms.extns.http.polly** with this we can now create the http **policy**
  - and we will set the policy in the prog.cs like keep on trying for every 3 secs until the auction service is back alive

### Asynchronous communication (event Bus)

- **Rabbit Mq** is our event bus.
- now with this approach our auction service will publish the msg (event) to the service bus (rabbitMQ) and the search service will subscribe to (event) the msg and take the action to evntually get consitent with the mongo db with the postgres db
- No req or res just the **fire and forget** approach.
- some of the transport are (rabbitMQ, amazon SQS, azure service bus)

- **Rabbit Mq**
- is msg broaker which accepts and forwards the msg to the queue
- producer and consumer.. pub/sub model
- msgs are stored on queues (msg buffer) ..this msg buffer are persistent if the bus goes down and the another bus takes over it will still gets the msg from the buffer
- can use persistent storage (in the event of failure)
- **Exchanges** can be used for routing functionality (there are diff types of exchanges)
  - 1. direct 2. Fanout 3. topic 4.Header
  - 1. direct - delivers the msgs based on the routing key (its a direct or unicast type of message)
  - 2. fanout - exchange bounds to one or more queues and that queues will wait for the consumer to pick up
  - 3. Topic - its a combination of both , has routing keys and exchages bound to more than one queue
  - 4. header - this type will allow us to specify header with the msg with one or more queues
- - rabbitmq uses **AMQP** (advance messaging queue protocol)

-**Mass Transit** we can use rabbit mq client to connect rabbit ma but we will be using Mass transit which is dotnet alternative for the rabbitmq client. - this also supports different transports in the future not only rabbit mq

- **Contracts** - is gon to represent the events that we will be sending thru the msg bus..b/w the services..
- - to create a contract`dotnet new classlib -o src/Contracts`//its a contract class library. and then add it to the soln - `dotnet sln add src/Contracts`
- and now add the reference to the contracts in both the service `cd AuctionService` then `dotnet add reference ../../src/Contracts` and lly repeat this command in the search service to create a reference to the contract

- and the idea behind is when we create a contract both the auction service and the search service will ve access to the contract obj..

- **events** some of the events we will be creating is ex auction created event which is essentially the auction created dto that we will be sending into our service bus

- **creating consumer** once we added the events in the contracts we now can create the consumer to consume those events, ex - in the search service create a Auction created "Consumer" class **the naming convention** matters for the mass transmit..
- once we register our consumer in the prog.cs and restart the search svc`dotnet watch`, in the Rabbit mq we can see the exchanges created one for the producer (auction created) and the other for the consumer (search-auction-created)
- **search-auction-created** is the queue that is ready to receive msgs for our search service

- **publishing event to the bus** - make some changes in the auction svc controller (ex imp the Ipublisher interface) and register the automapper in prof with the contracts of ex - CreateMap<AuctionDto, AuctionCreated>(); // Auction created is the contract .. and both the auction and search svc knows about it.
- again restart the svc we can see the exchange created

- **what could go wrong** now our arch looks like this when the post req comes in the 1. auction svc 2. which saves in postgresql 3, then create a msg queue in msg bu 4. then the search svc will sub the msg and consumes it 5. and saves it in to its mongo db

  - incase if any of the svc fails among the all 5 svc only if the mongo or the msg bus fails then we won't ve the data consistency.
  - if we any of the other services fails we still ve our data consistency..

- **outbox** if the rabbit mq fails our msgs will sit in the outbox and if our svc is back alive, from the outbox it will pick up the messages and pub into the msg bus

  - **retry** and we can retry if it won't succeed.
  - inside the auction svc install the "mass transit ef core package" to create the Outbox(pesistent)
  - and config in the prog.cs in the auction svc
  - and add it in the db context ..and run the migrations `dotnet ef migrations add Outbox` will create a migrations with 3 more tables included (that we defined in the db c)

- now that we ve our outbox configured if we stop the rabbitmq and make a post req our req will be successful and the msg will be wait in the db to be delivered when the svc is back up again it will pick up the msg from there

- **what if mongo fails** - with the outbox implementation we fix the data consistency if the rabbitmq fails.. now what if mongo fails ..
- we can configure the **retry policies** on per endpoint basis.. inside the prog.cs in search svc

- **consuming fault queues** - if we ve error msgs (when the msg bus is down) that exception will be added to the queue as well
- we can also consume those fault queues ex - Auction Created Faults
- and then consum it in the auction svc (prog.cs)
- ex - we create a car model foo which we consider as fault and the auction will pick up and change it to foobar and pub it and then our search will pick up the corrected foobar model

### Identity service

- is diff from other since its not the part of the msg bus, we can think of it as a outsiders service
- in this we will create identity service and add authentication endpoints , with the help of openId and oAuth2.0
- best alternative is we could use Azure AD for this..
- the identity service is an authentication server which implements 2 standards **openID** connect OIDC and **oAuth2.0** standards

  - the OIDC is no longer open source require licenses in prod
  - the identity server is a single sign on solution

- **Oauth2.0** security standards to give one app permission to access our data in other fapp, it gives key (jwt) instead of password

  - **redirect uri** we ve the redirect uri https://app.com/callback - the url where the auth server redirects back the user to after granting permission to the client callback url
  - the response code is the **key** the client receives as auth code.
  - we also ve scope such as read only consent form etc..
  - and we also ve the **client id** to identify the client with the auth server.
  - we also ve **client Secret** the app, "nextjs app" to securely sharely the info privately bts. to the client server not the client browser.
  - we also ve the **authorization code** the identity server sends back to the client. the client then sends the authorization code along with the client secret in exchange for an access token
  - **access token** is the key the client will use from that point to access the the resource from the server.

- **open ID connect** OIDC .. as the oAuth2.0 is only for authorization and granting access to data
- the OIDC will sits on top of the oauth and adds additional functionality around login such as..profile information about the person info who is logged in..
- this oidc enables the client to establish login **session** and gain as well the info about the person and this is referred to as **identity**
- and when an auth server supports oidc its often referred to as **identity provider**

- to install the identity server `dotnet new install Duende.IdentityServer.Templates` then
- `dotnet new isaspid -o src/IdentityService` and also add soln for this service `dotnet sln add src/IdentityService`
- by default the identity server will comes with the sql server installed, but we will be using the "postgresql" for this

- **seeding Data** and add migrations

  - go to http://localhost:5000/well-known/openid-configuration ..
  - the asp net has the user mgr svc to add and get the users from our db .. in the "seedData.cs" we can see the default user "alice"

- the identity server by default comes with all the pages such as login, logout etc we can create only the register user page

- adding client credentials to allow clients to to req a token..

- **adding a custom profile service to identity server** - rn we re having the jwt token but we don't ve the user info, we can extend the token by adding the user info/profile to it.(like the user id, user name)
- we need the user name prop for the seller and the winner fns

  - create a custom profile service class .. impl the IProfileService

- "configuring auth on the resource server (autcion server)" - this will ve auth endpoints, so we can pass the token to the server, and it will validate it against the identity server
- for that we need authentication.JWTbearer package installed in the auction service. then register in the prog.cs and also add the "mw" **authentication mw** and it has to come b4 the user authorization mw

### Gateway SErvice

- will provide the single access point to our app, will act as the single surface for reqs (our clients need to know single url to access all our services).
- and we can use it for security, url rewriting, ssl termination, Load Balancing, caching.. etc.

- to create the service `dotnet new web -o src/GatewayService` (this time around its not the web api, since we re not gon use the api endpoints) and then add the solns `dotnet sln add src/GatewayService`

- **reverse proxy** as we know the proxy that sits on b/w our client app to the browser, and the reverse proxy is in reverse order it sits on close to the backend/resource services.
- and we need to install the reverse proxy package called **yarp reverse proxy**
- and the jwt bearer package
- we will config the authentication process on the proxy and will result in the authentication cookie and that cookie will flow to the destn svc as a normal req header(refer the yarp reverse proxy docs for more details)

- and then in the app.config file we can specify like ex for the auction service (get method) we don't need to config the authentication, but for the post method routes we ve to configure .. lly for all the other services as well.

### Dockerizing the application (aka containerizing our application)

- lets dockerize all our services so far we ve..
- so we gon be having docker file .. one per service and then we build the docker img based on that docker file.
- once we create our docker file we can run `docker build -f src/AuctionService/Dockerfile -t testing123 .` to create the docker image
- for the first time it will take some time, but for the future builds the docker will cache lot of things and packages..
- initially docker won't ve any idea about the our postgresql db, we need to tell the docker about our configuration.
- once we ve the docker image we can now add the image to the docker compose file with our docker hub user name followed by the img name.
- make sure to config the prog.cs with the rabbitmq configuration.
- once we configure the auction serive in the docker compose file we can build by `docker compose build auction-svc`
- and we can start `docker compose up -d` can see the auction svc running inside our docker container with rest of our services.

**Search service** lly repeat the process for the search svc..create the docker file for the search service and then add the configuration about the mongo db and then create the img and add the config it to the compose file and run the

- then build the service `docker compose build search-svc` and to up the service `docker compose up -d` will start the service.

- **identiy svc** repeat the same process for the identity service
- **gateway svc** and also for the gateway service..

- **debugging .net service in a docker container** in the cmd pallete we can search for the generate .net assets for build and debug..- and in that we can ve the option to add the configuration.
- and there we can also ve the option to add the .net debugger to the container ..
- and in that list we can see the "docker" .net attach preview - just select that which will add the configuration to the "launch.json" file

- to run all our containers with the clean data seeded then we can run `docker compose down ` and run it again `docker compose up -d` ..

## Client Side app

- is a next js app .. `npx create-next-app@latest`

- **deps** for the client app
- react-icons `npm i react-icons`
- tailwind aspect ratio `npm i -D @tailwindcss/assets-ratio` will provide the aspect ratio for our imgs .. and register this plugin in the tailwind config.
- react count down `npm i -D react-countdown` for the count down timer for our auctions
- fowbite react `npm i flowbite fowbite-react` for the **pagination**
- zustand `npm i zustand` for the state management.. we can create a store(hooks) and then we can use the store inside our comps easily.
- query string `npm i query-string` for creating qs to all our query parameters, that we need to send to our search service, so we get the correct res back from the service..
- next auth `npm i next-auth` for authentication
- react hook form `npm i react-hook-form` for handling the form..
- react date picker `npm i react-datepicker @types/react-datepicker` for handling the date picking capabilities.
- react hot toast `npm i react-hot-toast` for the pop up notification.
- signal `npm i @microsoft/signalR` for the notification in the client side..(client side package)
- **fetch** - to fetch the data from our endpoints from our service, and retrieve the list of auctions (from the gateway )

### next Auth

- to authenticate our client with the next auth or (auth js).. next auth will soon become auth js (and can be used to many other frameworks)
- the identity server will send the authorization code to the browser which will then redirect us back to client app to the callback url,.. from there the client app will send the authorization code and the client id and client secret to the identity server..which will response with a access token.
- at this point next auth will save the encrypted cookie into our browser, which it will use to maintain the **session** with itself..

- then finally the client app can use the token to req the resource server

---

- **login** and for the login if the user is logged in then we can use the session cookie to get the info about the user and display (user prof)
- "authActinon.ts" we can get the user session information from the next auth js getSession()

- **getting the access token to use to authenticate to our resource server ** - in the next auth doc there is a util fn - getToken()- will allow us to get the decrypted jwt
- in the session comp "page.tsx" we ve the session info and the token info (we pull the prof from the session info), and we also ve to add the access token to the token data to access our resource server.. (we send em in the headers)

- althoug we re storing the token in the client browser.. we still encrypted the token with the client secret..and token is an http cookie and can't be accessed by the js

### Client side CRUD

- building the rest funtionality, react hook form, routing and error handling..
- **img upload** we ve not covered the image upload functionality, but we can use the "cloudinary" for the image upload, for that we need the auction Id, seller of the auction, authentication

## The Bidding Service

- lets add the bidding service.. and make the facilities to give the user to bid on the auctions `dotnet new webapi -o src/BiddingService` and then add the solutions `dotnet sln add src/BiddingService` ..
- and will be using mongo for the database.
- some of the other packages are mongo entity, jwt bearer, mass transit rabbit mq.. and config our app settings and the prog.cs
- add the reference to the contracts `dotnet add reference ../../src/contracts` now that we ve the ref for the contract lets add the consumer.. only one consumer for this service auction created consumer. then add it to the mass transit consumer settings in the prog.cs
- same as the other svc create a dto and install the automapper and then create a mapping prof inside the req helper dir and map our entity(bid) to the bidDto.. and the bid to bid placed contract.
  - and register the automapper in the prog.cs
- **bit placed producer** - just add the createMap<Bid, BidPlaced>() in the mapping prof, so when we save this to our db the Ipublisher interface (that we impl in the controller) will publish the event

- **adding a background service for the auction finished event** - we will ve the bg svc that will poll the db, if there's any auction met the end date but ve not marked as finished and for each of those we will send an event on the service bus.
- create a service inside the bidding service

- **Grpc** supports both sync and async communication..for our requirement we will set up synchronous communication, high performance **roughly 7 times faster** than the http rest when receiving data and 10 times faster than the http rest when sending data
- since the msgs are binary ..designed for low latency and high thru put communication.
- and it relies on protocol buffers/ contract b/w the services.. and each svc will ve the proto file that defines how these services are communicate

- **Grpc client** ex - the .proto file from the bit service is our, and the .proto file from the auction service is our **grpc server** .. this will set up the contract b/w our client and the server.
- and these services are gon to use the http2.0 for the communication
- once we write the proto file then register it to our proj file
- once we create the proto file build the app and see (in auction svc) in the obj/Debug/protos/ we can see auction.cs and auction.grpc.cs created based on our proto file

- **grpc service** inside the auction service create a service called **Grpc auction services** that impls the GrpcAuction.GrpcAuctionBase

- for the client side of our grpc service ie (in the bidding service ) we gon install diff package "google.protobuf" and 2 other packages "grpc tools" and grpc.net.client
- and create the exact same proto file as the auction svc
- and similar to the auction svc also create a grpc client service inside bidding service,

- **update the gateway service** then update the gateway service as well. in the appsetting.json and the app setting docker.json add the bids svc endpoints

- **dockerize the bidding service** create a doker file and then add the service to the docker compose file.

### Notification service (SignalR)

- for the notification service we gon use the signalR for the live communication b/w our clients and the server.
- some of the notifications are auction created, bid placed or auction finishing etc.
- and we want our client to client to and maintain the connection over **web socket** so we can receive the live updates from the backend services.
- `dotnet new web -o src/NotificationService` and `dotnet sln add src/NotificationService`
- and this is gon be the consumer of the events effectively and add the ref to the contracts `dotnet add reference ../../src/contracts`
- packages - mass transit,

- **SignalR Hub** - inside the hub dir create a service which will impl from the Hub from the signalR

- **Cors Support for the G/w** - our browser will make a connection to signalR, that means its a cors and we need to send back a header, so our browser allows the req to complete
- in the g/w service prog.cs add the cors policy settings

### Adding bids notifications to the client

- lets add the bids functionality to the client and the signalR notification

**adding signalR to the client** currently from the client, user can update the bids and add more bids but its not updating ie bcoz our client and svr not in sync with each other. so we can use the signalR notification to the client, to get the live update notifications.

- and its like a provide that wraps the app.

### Publishing the app to production (locally )

- as we re gon to dockerize even our client .. we are gon to use the ingress to access the internet..
- the ingres will receive the external traffic and f/w it based on the rules to the correct location.
- we ve to ve 3 rules here the identity is considered to be external eventhough we re running it inside the docker
- and our client will also need to find the identity
- and our clinet will ve to maintain the connection with the notification server (SignalR) via ingress as well.

- so in this we will dockerize (docker compose) our services, adding ingress control with Nginx and adding the ssl

- **assigning static ip to the identity** we ve to assign the static ip to the identity server
- we can add a subnet (of 10.5.0.0/16) under the custom ip manager config in the n/w sections
- this private ip addrs we can use internally for the docker .. and for the every service we ve to specify this cust static ip.

- **adding Nginx ingress to the docker compose** - so the external client will access our services via this proxy

- **adding ssl to the ingress** for the local/dev.. we can use a tool mkcert to add the ssl to our ingress proxy . `brew install mkcert`
- and then `mkcert -install` and followed by that `mkcert -key-file carsties.com.key -cert-file carsties.com.crt app.carsties.com api.carsties.com id.carsties.com`
- this will create a new certificate for all the mentioned local host addresses(3)
