using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Custom sprite mask / polygon collider generator
/// Overrides sprite renderer geometry and polygon collider points
/// I lazily wrote the weapon logic in here rather than making a new class and extending this
/// </summary>
public class ColliderArc : MonoBehaviour
{

    [Header("References")] // Must define in inspector!

    SpriteRenderer renderer;
    PolygonCollider2D arcCollider;
    Texture2D texture;

    [Header("Geo Settings")] // Settings for defining collider and sprite mask
    [SerializeField] int segments = 10;
    public float angle = 30, range, offsetFromOrigin, attackDuration ;
    [SerializeField] int textureDim;
    [SerializeField] Color color;
    

    //Weapon Settings (don't edit here, edit from BaseCharacter
    [HideInInspector] public float damage;
    [HideInInspector] public float force;


    // Set inherited references
    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
        renderer.color = color;
        arcCollider = GetComponent<PolygonCollider2D>();
        renderer.enabled = false; arcCollider.enabled = false;
        transform.position = transform.parent.transform.position + Vector3.up * offsetFromOrigin;

        InitializeSprite();
    }

    // Refactored some sprite stuff I found on stack overflow
    void InitializeSprite() 
    {
        texture = new Texture2D(textureDim + 1, textureDim + 1); // create a texture larger than maximum polygon size

        // create an array and fill the texture with color
        List<Color> cols = new List<Color>();
        for (int i = 0; i < (texture.width * texture.height); i++)
            cols.Add(color);
        texture.SetPixels(cols.ToArray());
        texture.Apply();      
    }

    // Create a procedural swiping sprite anmimation and matching collider!
    public IEnumerator ArcWipe()
    {
        renderer.enabled = true;
        arcCollider.enabled = true;

        Vector2[] fullArc = ConvertVector3Array(CalculateArcVertices()); // Store the full arc
        List<Vector2> swingVertices = new List<Vector2>();
        foreach (Vector2 vert in fullArc) swingVertices.Add(vert); // Initialize our arc as full arc

        // Gave up on this animation after the sunk time, ideally it animates to full and then back to zero

        /*for (int s = 1; s <= segments; s++)
        {
            // Add the three vertices that make up the next segment's triangle
            for (int t = 0; t <= 2; t++)
                swingVertices.Add(fullArc[s + t]);

            yield return new WaitForSeconds((attackDuration / segments) / 2); // Animation delay
            ushort[] triangles = new ushort[s * 12]; // Sprite geometry info... idk rly
            for (ushort i = 0; i < s * 3; i++)
            {
                triangles[i] = i;
            }
            DrawArc(swingVertices.ToArray(), triangles); // Draw custom sprite mask with vertices
            arcCollider.points = swingVertices.ToArray(); // Set collider vertices to match          
        }*/

        // Animate arc to zero
        for (int s = segments; s > 0; s--)
        {
            yield return new WaitForSeconds((attackDuration / segments) / 2); // Animation delay
            ushort[] triangles = new ushort[s * 12]; // Sprite geometry info... idk rly, I'm p sure it's just an index to the vertices associated w/ that triangle
            for (ushort i = 0; i < s * 3 ; i++)
            {
                triangles[i] = i;
            }
            DrawArc(swingVertices.ToArray(), triangles); // Draw custom sprite mask with vertices
            arcCollider.points = swingVertices.ToArray(); // Set collider vertices to match

            // Add the three vertices that make up the next segment's triangle
            for (int t = 1; t <= 3; t++)
                swingVertices.RemoveAt(s * 3 - t);


        }

        renderer.enabled = false;
        arcCollider.enabled = false;
    }

    // Create a custom sprite mask and override it's renderer's geometry
    // Refactored from stack overflow
    void DrawArc(Vector2[] vertices, ushort[] triangles)
    {
        renderer.sprite = Sprite.Create( //create a sprite with our texture -- how costly is this to do often?
                texture, 
                new Rect(0, 0, textureDim, textureDim),
                new Vector2(.5f, 0), 1); 


        //convert y coordinate to local space -- tried tweaking these, don't understand how it works fully
        float ly = Mathf.Infinity;
        foreach (Vector2 vi in vertices)
        {
            if (vi.y < ly)
                ly = vi.y;           
        }
        Vector2[] localv = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            localv[i] = vertices[i] - new Vector2(-textureDim/2, ly);
        }

        renderer.sprite.OverrideGeometry(localv, triangles); // set the vertices and triangles of the mask  
        if (ly < 0)
            transform.position = - transform.up * ly;
    }

    // Creates a list of vertices in the shape of an arc
    Vector3[] CalculateArcVertices()
    {
        Vector3[] vertices = new Vector3[segments * 3]; // Create a new list, length is three vertices per segment triangle
        int vert = 0;

        // Create arc shape one triangle at a time from angle and distance
        // Cast an angle, define points at origin, distance at angle and delta to next segment
        float currentAngle = -angle;
        float deltaAngle = (angle * 2) / segments;
        for (int i = 0; i < segments; i++) // Add the three vertices
        {
            vertices[vert++] = Vector3.zero;
            vertices[vert++] = Quaternion.Euler(0, 0, currentAngle) * Vector3.up * range;
            vertices[vert++] = Quaternion.Euler(0, 0, currentAngle + deltaAngle) * Vector3.up * range;

            currentAngle += deltaAngle; // Increment angle for next iteration
        }
        return vertices;
    }

    // Stupid Unity 2D... PolygonCollider requires a Vector2[] so here we are
    Vector2[] ConvertVector3Array(Vector3[] vertices)
    {
        Vector2[] array = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            array[i] = new Vector2(vertices[i].x, vertices[i].y);
        }
        return array;
    }
}
