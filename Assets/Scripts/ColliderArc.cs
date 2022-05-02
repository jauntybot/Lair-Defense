using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderArc : MonoBehaviour
{

    [Header("References")]
    //[SerializeField] Stats stats;
    //[SerializeField] Weapon weapon;
    [SerializeField] GameObject arc;
    [SerializeField] Transform dir;

    SpriteRenderer renderer;
    PolygonCollider2D arcCollider;
    Texture2D texture;

    [Header("Settings")]
    [SerializeField] int segments = 10;
    [SerializeField] float distance;
    [SerializeField] float angle = 30;

    public float arcWipeDuration;

    [SerializeField] int textureDim;
    [SerializeField] Color color;


    private void Start()
    {
        renderer = arc.GetComponent<SpriteRenderer>();
        arcCollider = arc.GetComponent<PolygonCollider2D>();

        InitializeSprite();
        //StartCoroutine(ArcWipe());
    }

    void InitializeSprite()
    {
        texture = new Texture2D(textureDim + 1, textureDim + 1); // create a texture larger than maximum polygon size

        // create an array and fill the texture with your color
        List<Color> cols = new List<Color>();
        for (int i = 0; i < (texture.width * texture.height); i++)
            cols.Add(color);
        texture.SetPixels(cols.ToArray());
        texture.Apply();

        
    }

    private void OnValidate()
    {
        arcCollider = GetComponentInChildren<PolygonCollider2D>();
        arcCollider.points = ConvertVector3Array(CalculateArcVertices());
    }
    public IEnumerator ArcWipe()
    {
        renderer.enabled = true;
        arcCollider.enabled = true; 

        Vector2[] fullArc = ConvertVector3Array(CalculateArcVertices());
        List<Vector2> swingVertices = new List<Vector2>();
        foreach (Vector2 vert in fullArc) swingVertices.Add(vert);
        float time = 0;
        //while (time < arcSwipeDuration)
        //{
            for (int s = segments; s > 0; s--)
            {
                yield return new WaitForFixedUpdate();
                ushort[] triangles = new ushort[s * 12];
                for (ushort i = 0; i < s * 3 ; i++)
                {
                    triangles[i] = i;
                }
                DrawArc(swingVertices.ToArray(), triangles);
                arcCollider.points = swingVertices.ToArray();

                swingVertices.RemoveAt(s * 3 - 1);
                swingVertices.RemoveAt(s * 3 - 2);
                swingVertices.RemoveAt(s * 3 - 3);
                time += Time.deltaTime;
            }

        //}
        renderer.enabled = false;
        arcCollider.enabled = false;
    }

    void DrawArc(Vector2[] vertices, ushort[] triangles)
    {

        renderer.color = color; //you can also add that color to the sprite renderer
        renderer.sprite = Sprite.Create(texture, new Rect(0, 0, textureDim, textureDim), new Vector2(.5f, 0), 1); //create a sprite with the texture we just created and colored in

        //convert y coordinate to local space
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

        renderer.sprite.OverrideGeometry(localv, triangles); // set the vertices and triangles    
        if (ly < 0)
            transform.position = - transform.up * ly;
    }

    Vector2[] ConvertVector3Array(Vector3[] vertices)
    {
    
        Vector2[] array = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            array[i] = new Vector2(vertices[i].x, vertices[i].y);
        }
        return array;
    }

    Vector3[] CalculateArcVertices()
    {
        int numTriangles = segments * 4;
        int numVertices = numTriangles * 3;
        Vector3[] vertices = new Vector3[segments * 3];
        int vert = 0;

        //Divide mesh into radial segments
        float currentAngle = -angle;
        float deltaAngle = (angle * 2) / segments;
        for (int i = 0; i < segments; i++)
        {
            vertices[vert++] = Vector3.zero;
            vertices[vert++] = Quaternion.Euler(0, 0, currentAngle) * Vector3.up * distance;
            vertices[vert++] = Quaternion.Euler(0, 0, currentAngle + deltaAngle) * Vector3.up * distance;

            currentAngle += deltaAngle;
        }
        return vertices;
    }

    //private void OnDrawGizmos() { 
    //    if (mesh) {
    //        Gizmos.color = meshColor;
    //        Gizmos.DrawMesh(mesh, playerDir.position, playerDir.rotation);
    //    }
    //}
}
