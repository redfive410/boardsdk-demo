using UnityEngine;
using Board.Input;

public class ShipManager : MonoBehaviour
{
    [SerializeField] private ShipController[] ships;

    private void Update()
    {
        var contacts = BoardInput.GetActiveContacts(BoardContactType.Glyph);

        for (int i = 0; i < ships.Length; i++)
            ships[i].ApplyContact(i < contacts.Length ? contacts[i] : (BoardContact?)null);
    }
}
