# NetNJE

_This project is currently not yet functional, do not try to run it_

NetNJE is an implementation of a IBM NJE (Network Job Entry) server and client in C#. 

## Usage

No usage yet since this project is in very early stages

## Roadmap

### Implement basic client with minimal viable functionality

Implement a client that can connect to another NJE node and deliver a message to a user

### Implement command execution

Expand the client to allow remote commands to be executed and jobs being sent

### Implement file transfers

Implement file transfer functionality and upload/download files.

### Implement API surface

Lift the client out of the PoC phase and add an API surface for configuration, querying and sending messages, etc.

### Implement a server

Implement a server to allow other NJE nodes to connect to this node.

### Implement a frontend

Implement a frontend to allow the use of NetNJE 

## License

Licensed under the GNU General Public License V3.0

## Thanks

Thanks to @moshix to give me the idea of looking into NJE and turning it into a quarantine project.

This project uses the EBCDIC code from Jon Skeet, see https://jonskeet.uk/csharp/ebcdic/
