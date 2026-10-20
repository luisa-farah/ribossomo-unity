using UnityEngine;
using UnityEngine.InputSystem;

// Passos 9–15: movimento com Rigidbody2D, coleta por trigger e diagonal normalizada.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;

    Rigidbody2D rb;
    Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        movement = ReadInput();
        if (movement.sqrMagnitude > 1f) movement.Normalize(); // passo 15
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movement * speed;
    }

    // Teclado (WASD/setas) + controle Xbox (analógico/d-pad).
    Vector2 ReadInput()
    {
        Vector2 v = Vector2.zero;
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) v.x -= 1;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) v.x += 1;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v.y -= 1;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v.y += 1;
        }
        var gp = Gamepad.current;
        if (gp != null)
        {
            v += gp.leftStick.ReadValue();
            v += gp.dpad.ReadValue();
        }
        return v;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<AminoAcid>(out var amino) && !amino.collected)
        {
            amino.collected = true;
            ProteinSynthesis.Instance.OnAminoCollected(amino);
        }
    }
}
