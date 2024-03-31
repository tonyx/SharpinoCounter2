# Sharpino Counter

Example of a Counter with Sharpino.

## Table of Contents

- [Installation](#installation)
- [Optional Postgres EvenStore](#postgesEventStore)


## Installation

clone project 


and run the following command:

```bash
dotnet run
```

## Optional Postgres EventStore

Prerequisites: dbmate, postgres

Create a file named .env with content like this (substituting the postgresusername/postgrespassword with your own):
```bash
DATABASE_URL="postgres://postgresusername:postgrespassword@127.0.0.1:5432/es_counter?sslmode=disable"
```
edit the Tests.fs file uncommenting the following line
```fsharp
        // (pgStorage, 1, 1) // uncomment to test with Postgres
```

then run the commands:

```bash
dbmate up
dotnet run

```
A 6 min reading article about this example can be found 
[here](https://www.linkedin.com/pulse/example-counter-event-sourcing-sharpino-ino-antonio-lucca-hzlof)





