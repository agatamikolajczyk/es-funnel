## Introduction

The Funnel application is an educational project designed to explore the principles and practices of event sourcing within the context of .NET Core applications. Event sourcing is an approach to system design where application state changes are stored as a sequence of events rather than as the current state. This method provides a complete history of all changes, which can be extremely useful for auditing, state restoration, or event analysis.

## Project Goal

The aim of this project is to create an application that illustrates and demonstrates the practical application of event sourcing. The project seeks to:

- Show how to implement basic event sourcing patterns and techniques.
- Provide tools and practical advice for working with event sourcing in .NET Core.
- Educate about the benefits and challenges associated with this approach.

## Main Features

- **Event Capture**: The application records all state changes as events, which are stored in an event store.
- **State Reconstruction**: By utilizing the stored events, the application is capable of reconstructing the current state of objects.
- **Performance and Scalability**: The project demonstrates techniques that can be used to optimize performance and scalability in applications using event sourcing.
- **User Interface**: A simple interface for data presentation and interaction with the application, allowing users to test and observe event sourcing in action.

## Technologies

The project was built using:

- **.NET Core**: A platform for building applications that provides flexibility and performance.
- **C#**: The programming language used for implementing application logic.
- **Dapper**: Used for data management and database interaction.
- **PostgreSQL**: For educational purposes, the application uses an event store in a PostgreSQL database.

## Acknowledgements

I would like to sincerely thank you for providing the repository - [EventSourcing.NetCore](https://github.com/oskardudycz/EventSourcing.NetCore/tree/main). It has been extremely helpful in my learning process and served as an excellent model for event sourcing in .NET Core. Thank you for your work and contribution!
