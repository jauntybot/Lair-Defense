using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleClass : MonoBehaviour
{
    private SpriteRenderer spriteR;
    private Rect buttonPos1;
    private Rect buttonPos2;

    void Start()
    {
        spriteR = gameObject.GetComponent<SpriteRenderer>();
        // Create a blank Texture and Sprite to override later on.
        var texture2D = new Texture2D(64, 64);
        spriteR.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2 (.5f, 0), 100);

        buttonPos1 = new Rect(10.0f, 10.0f, 200.0f, 30.0f);
        buttonPos2 = new Rect(10.0f, 50.0f, 200.0f, 30.0f);
    }

    void OnGUI()
    {
        if (GUI.Button(buttonPos1, "Draw Debug"))
            DrawDebug();

        if (GUI.Button(buttonPos2, "Perform OverrideGeometry"))
            ChangeSprite();
    }

    // Show the sprite triangles
    void DrawDebug()
    {
        Sprite sprite = spriteR.sprite;

        ushort[] t = sprite.triangles;
        Vector2[] v = sprite.vertices;
        int a, b, c;

        // draw the triangles using grabbed vertices
        for (int i = 0; i < t.Length; i = i + 3)
        {
            a = t[i];
            b = t[i + 1];
            c = t[i + 2];
            Debug.DrawLine(v[a], v[b], Color.white, 100.0f);
            Debug.DrawLine(v[b], v[c], Color.white, 100.0f);
            Debug.DrawLine(v[c], v[a], Color.white, 100.0f);
        }
    }

    // Edit the vertices obtained from the sprite.  Use OverrideGeometry to
    // submit the changes.
    void ChangeSprite()
    {
        Sprite o = spriteR.sprite;
        Vector2[] sv = o.vertices;

        for (int i = 0; i < sv.Length; i++)
        {
            sv[i].x = Mathf.Clamp(
                (o.vertices[i].x - o.bounds.center.x -
                    (o.textureRectOffset.x / o.texture.width) + o.bounds.extents.x) /
                (2.0f * o.bounds.extents.x) * o.rect.width,
                0.0f, o.rect.width);

            sv[i].y = Mathf.Clamp(
                (o.vertices[i].y - o.bounds.min.y -
                    (o.textureRectOffset.y / o.texture.height) + o.bounds.extents.y) /
                (2.0f * o.bounds.extents.y) * o.rect.height,
                0.0f, o.rect.height);

            // make a small change to the 3rd vertex
            if (i == 2)
                sv[i].x = sv[i].x - 64;
        }

        o.OverrideGeometry(sv, o.triangles);
    }
}
