using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventType  {

    public const  byte CONNECT                  = 0x01;
    public const  byte CONNECT_SUCCESS          = 0x02;
    public const  byte RECONNECT                = 0x03;
    public const  byte CONNECT_FAILED           = 0x04;
    public const  byte LOG_IN                   = 0x05;
    public const  byte LOG_OUT                  = 0x06;
    public const  byte LOG_IN_SUCCESS           = 0x07;
    public const  byte LOG_IN_FAILURE           = 0x08;
    public const  byte LOG_OUT_SUCCESS          = 0x09;
    public const  byte LOG_OUT_FAILURE          = 0x0a;
    public const  byte GAME_LIST                = 0x0b;
    public const  byte ROOM_LIST                = 0x0c;
    public const  byte GAME_ROOM_JOIN           = 0x0d;
    public const  byte GAME_ROOM_LEAVE          = 0x10;
    public const  byte ROOM_LEAVE_SUCCESS       = 0x11;
    public const  byte ENEMY_LEAVE              = 0x12;
    public const  byte GAME_ROOM_JOIN_SUCCESS   = 0x13;
    public const  byte GAME_ROOM_JOIN_FAILURE   = 0x14;
    public const  byte READY                    = 0x15;
    public const  byte READY_SUCCESS            = 0x16;
    public const  byte POSSITION                = 0x17;
    public const  byte CHECKWIN                 = 0x18;
    public const  byte ENEMYDATA                = 0x19;
    public const  byte ENEMY_JOIN_ROOM          = 0x1a;
    public const  byte PLAYER2_READY            = 0x1b;
    public const  byte PLAYER2_JOINROOM         = 0x1c;
    public const  byte DISCONNECT               = 0x1d;
    public const  byte REGISTER                 = 0x20;
    public const  byte REGISTER_SUCCESS         = 0x21;
    public const  byte REGISTER_FAILURE         = 0x22;
    public const  byte ENEMY_READY              = 0x23;
    public const  byte GAME_OVER                = 0x24;
    public const  byte WIN                      = 0x25;
    public const  byte REPLAY                   = 0x26;
    public const  byte REPLAY_SUCCESS           = 0x27;
}
