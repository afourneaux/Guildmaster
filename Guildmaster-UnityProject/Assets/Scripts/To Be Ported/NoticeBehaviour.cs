using System;
using System.Collections.Generic;
using UnityEngine;

public class NoticeBehaviour {
    // See WanderBehaviour for a description of how these Behaviour classes are meant to work

    // The Notice component will enable a character to be aware of other characters.
    // On each step, it will check how far away each other character is. If another character
    // is within a certain range (defined by both characters' stats and terrain conditions) and
    // line of sight is unobstructed, that other character is noticed. Characters will remember
    // other characters for a certain amount of time after they lose sight, then forget about them.

}