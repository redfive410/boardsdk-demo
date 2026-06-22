using UnityEngine;
using Board.Input;
using System.Collections.Generic;

public class ShipManager : MonoBehaviour
{
    [SerializeField] private ShipController[] ships;
    [SerializeField] private AudioSource musicSource;

    private int[] assignedIds;
    private bool musicStarted;

    private void Awake()
    {
        assignedIds = new int[ships.Length];
        for (int i = 0; i < assignedIds.Length; i++)
            assignedIds[i] = -1;
    }

    private void Update()
    {
        var contacts = BoardInput.GetActiveContacts(BoardContactType.Glyph);

        var active = new Dictionary<int, BoardContact>();
        foreach (var c in contacts)
            active[c.contactId] = c;

        for (int i = 0; i < assignedIds.Length; i++)
            if (assignedIds[i] != -1 && !active.ContainsKey(assignedIds[i]))
                assignedIds[i] = -1;

        foreach (var c in contacts)
        {
            bool alreadyAssigned = false;
            for (int i = 0; i < assignedIds.Length; i++)
                if (assignedIds[i] == c.contactId) { alreadyAssigned = true; break; }

            if (!alreadyAssigned)
                for (int i = 0; i < assignedIds.Length; i++)
                    if (assignedIds[i] == -1) { assignedIds[i] = c.contactId; break; }
        }

        for (int i = 0; i < ships.Length; i++)
        {
            if (assignedIds[i] != -1 && active.TryGetValue(assignedIds[i], out var c))
                ships[i].ApplyContact(c);
            else
                ships[i].ApplyContact(null);
        }

        // Play the background music only while every ship has a piece on the board;
        // pause it as soon as fewer than that are placed, resuming where it left off.
        if (musicSource != null)
        {
            if (AllShipsPlaced())
            {
                if (!musicSource.isPlaying)
                {
                    if (musicStarted)
                        musicSource.UnPause();
                    else
                    {
                        musicSource.Play();
                        musicStarted = true;
                    }
                }
            }
            else if (musicSource.isPlaying)
            {
                musicSource.Pause();
            }
        }
    }

    private bool AllShipsPlaced()
    {
        for (int i = 0; i < assignedIds.Length; i++)
            if (assignedIds[i] == -1)
                return false;
        return true;
    }
}
