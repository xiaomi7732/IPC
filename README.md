# Two-way named pipe stream wrapper

## Description

This is a simple warper around named-pipe for two-way communication.

* **src/DuplexNamedPipeService** contains the implementations.
* **src/Example.Server** spins up a named pipe server;
* **src/Example.Client** spins up a named pipe client;

There are string transmitting from either side; there are serialized object transfer too.

See examples of usage in [src/Readme.md](./src/Readme.md)

## Limitation

For the simplicity, EOL is used as separator for messages; Messages with EOL in it requires escaping to work.
