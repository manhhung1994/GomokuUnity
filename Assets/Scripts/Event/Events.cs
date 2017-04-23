using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventType  {
    public const byte PROTCOL_VERSION = 0x01;
    /**
	 * Events should <b>NEVER</b> have this type. But event handlers can choose
	 * to have this type to signify that they will handle any type of incoming
	 * event. For e.g. {@link DefaultSessionEventHandler}
	 */
    public const  byte ANY = 0x00;

    // Lifecycle events.
    public  const byte CONNECT = 0x02;
    /**
	 * Similar to LOG_IN but parameters are different. This event is sent from
	 * client to server.
	 */
    public const byte RECONNECT = 0x3;
    public const byte CONNECT_FAILED = 0x06;
    /**
	 * Event used to log in to a server from a remote client. Example payload
	 * will be <b>login opcode 0x08-protocl version 0x01- username as string
	 * bytes- password as string bytes - connection key as string bytes -
	 * optional udp client address as bytes</b>
	 */
    public const byte LOG_IN = 0x08;
    public const byte LOG_OUT = 0x0a;
    public const byte LOG_IN_SUCCESS = 0x0b;
    public const byte LOG_IN_FAILURE = 0x0c;
    public const byte LOG_OUT_SUCCESS = 0x0e;
    public const byte LOG_OUT_FAILURE = 0x0f;

    // Metadata events
    public const byte GAME_LIST = 0x10;
    public const byte ROOM_LIST = 0x12;
    public const byte GAME_ROOM_JOIN = 0x14;
    public const byte GAME_ROOM_LEAVE = 0x16;
    public const byte GAME_ROOM_JOIN_SUCCESS = 0x18;
    public const byte GAME_ROOM_JOIN_FAILURE = 0x19;

    /**
	 * Event sent from server to client to start message sending from client to server.
	 */
    public const byte START = 0x1a;

    /**
	 * Event sent from server to client to stop messages from being sent to server.
	 */
    public const byte STOP = 0x1b;
    /**
	 * Incoming data from another machine/JVM to this JVM (server or client)
	 */
    public const byte SESSION_MESSAGE = 0x1c;

    /**
	 * This event is used to send data from the current machine to remote
	 * machines using TCP or UDP transports. It is an out-going event.
	 */
    public const byte NETWORK_MESSAGE = 0x1d;


    public const byte CHANGE_ATTRIBUTE = 0x20;

    /**
	 * If a remote connection is disconnected or closed then raise this event.
	 */
    public const byte DISCONNECT = 0x22;

    /**
	 * A network exception will in turn cause this even to be raised.
	 */
    public const byte EXCEPTION = 0x24;

}
