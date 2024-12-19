# .NET IPC Demo

This is a simplified application to demo .NET IPC.

## Projects

* [DuplexNamedPipeService](./DuplexNamedPipeService/): The IPC functionality encapsulation to make the consuming of the IPC easier.
* [Example.Client](./Example.Client/): IPC Client example, only run once.
* [Example.Server](./Example.Server/): IPC Server example, only serve the client once and will then exit.
* [Example.WorkerClient](./Example.WorkerClient/): IPC Client example as worker, will try to contact the server constantly.
* [Example.WorkerServer](./Example.WorkerServer/): IPC Server example, that will serve the client constantly.

## Scenarios

### Run IPC once

1. Start the server:

    ```shell
    > dotnet run --project .\Example.Server\Example.Server.csproj
    [SERVER] Waiting for connection.
    ```

1. In another console, run the client:

    ```shell
    > dotnet run --project .\Example.Client\Example.Client.csproj
    [CLIENT] Connecting to named pipe server...
    [CLIENT] Connected to named pipe server.
    [CLIENT] The server says: Hello~from server
    [CLIENT] The server says(2): Hello again from server
    [CLIENT] Telling the server: Hey from the client
    [CLIENT] Server customer name: Server Customer
    PS C:\AIR\IPC\src> 
    ```

1. At the same time, the server will have output

    ```shell
    [SERVER] Connected.
    [SERVER] Sending greeting...
    [SERVER] Greeting sent.
    [SERVER] Guessing what will the client say...
    [SERVER] The client says: Hey from the client
    [SERVER] Client customer name: Client Customer
    ```

### Run IPC continuously

Instead of using the `Example.Server` and `Example.Client`, use `Example.WorkerServer` and `Example.WorkerClient`.

### Run IPC in sidecar in K8s (Linux)

We firstly need to containerize the server and client. And then need to deployment in a K8s Pod, the demo uses sidecar to run the server, and the main app to run the client. That is to mimic the situation to have a sidecar serve some functionalities to the main app.

* Containerize

  Refer to [dockerfile-worker-server](./dockerfile-worker-server) & [dockerfile-worker-server](./dockerfile-worker-client) for containerize the apps.

  The BuildContainer.ps1 for the [WorkerClient](./Example.WorkerClient/BuildContainer.ps1) and [WorkerServer](./Example.WorkerServer/BuildContainer.ps1) are used to help building the images.

* Deploy the K8s cluster

  * For the first time, deploy the namespace or any other infrastructure once, we are deploying to the namespace of `ipc-example`:

    ```shell
    kubectl create -f ./k8sinfra.yml
    ```
  
  * Then, deploy the app using [k8s.yml](./k8s.yml).

    ```shell
    kubectl create -f k8s.yml
    ```

  Notice that the `/tmp` is mounted to a shared storage, and that's the key to make the IPC work:

    ```yml
    containers:
      - name: myapp
        image: chattyipcclient:0.0.2
        volumeMounts:
          - name: tmp
            mountPath: /tmp
    initContainers:
      - name: ipc-server-sidecar
        image: ipc-example-worker-server:0.0.5
        restartPolicy: Always
        volumeMounts:
          - name: tmp
            mountPath: /tmp
    volumes:
      - name: tmp
        emptyDir: {}
    ```
