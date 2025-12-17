using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCDialogue_SO", menuName = "BarSimulator/NPCDialogue")]
public class NPCDialogue_SO : ScriptableObject
{
    [TextArea]
    public string dialogue_ordering;

    [TextArea]
    public string dialogue_perfectDrink;
    [TextArea]
    public string dialogue_ratiosOffDrink;
    [TextArea]
    public string dialogue_wrongDrink;
}
