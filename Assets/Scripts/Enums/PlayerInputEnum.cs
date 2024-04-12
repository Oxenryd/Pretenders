using System;

[Flags]
public enum PlayerInputEnum
{
    None = 0,
    DirectionRight = 1,
    DirectionUp = 2,
    DirectionLeft = 4,
    DirectionDown = 8,
    ButtonJump = 16,
    ButtonGrab = 32,
    ButtonTrigger = 64,
    ButtonPush = 128
}

