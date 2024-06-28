using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OuterEdgePiece : Piece
{
    // protected override Vector2[] projection_vectors;
    [SerializeField] public OuterWingPiece adjacent_front {get; private set;}
    [SerializeField] public OuterWingPiece adjacent_rear {get; private set;}
    [SerializeField] public CornerPiece adjacent_left {get; private set;}
    [SerializeField] public InnerEdgePiece adjacent_right {get; private set;}
    public Sticker sticker_front;
    public Sticker sticker_rear;

    private Vector3[] shape_vertices_front;
    private Vector3[] shape_vertices_rear;


    public void Build(OuterWingPiece front, OuterWingPiece rear, CornerPiece left, InnerEdgePiece right, Face belongs_face, Face face_opposite) {
        if (!isBuilt) {
            isBuilt = true;
            adjacent_front = front;
            adjacent_rear = rear;
            adjacent_left = left;
            adjacent_right = right;
            sticker_front = Sticker.Create(belongs_face, this);
            sticker_rear = Sticker.Create(face_opposite, this);
        }
    }

    public override IEnumerable<Piece> listAdjacents() {
        yield return adjacent_front;
        yield return adjacent_rear;
        yield return adjacent_left;
        yield return adjacent_right;
    }

    public OuterWingPiece GetAdjacentOppositeTo(Face f) {
        return GetAdjacentOppositeTo(f.id);
    }

    public OuterWingPiece GetAdjacentOppositeTo(int face_id) {
        if (face_id == adjacent_front.sticker.belongs_face.id) {
            return adjacent_rear;
        }
        if (face_id == adjacent_rear.sticker.belongs_face.id) {
            return adjacent_front;
        }
        Debug.LogError("Given face ID is unknown to this piece.");
        return null;
    }

    public override Sticker GetSticker(Face face) {
        if (face.id == sticker_front.belongs_face.id) {
            return sticker_front;
        }
        if (face.id == sticker_rear.belongs_face.id) {
            return sticker_rear;
        }
        return null;
    }

    public override Vector3[] GetShapeVertices(Face face) {
        if (face.id == sticker_front.belongs_face.id) {
            return shape_vertices_front;
        }
        if (face.id == sticker_rear.belongs_face.id) {
            return shape_vertices_rear;
        }
        return null;
    }

    public override void SetVertices(Face face, int side) {
        // Prevent from defining vertices multiple times
        if (shape_vertices_front != null) {
            return;
        }
        
        // Fetch needed information
        Vector3 base_left = (face.transform.localPosition + 2*face.vertices[side]) /3;
        Vector3 base_right = (face.transform.localPosition + 2*face.vertices[Face.toBounds(side+1)]) /3;
        Face face_opposite = face.adjacent_faces[side];
        int side_opposite = face_opposite.getIndexOf(face);
        Vector3 base_right_opposite = (face_opposite.transform.localPosition + 2*face_opposite.vertices[side_opposite]) /3;
        Vector3 base_left_opposite = (face_opposite.transform.localPosition + 2*face_opposite.vertices[Face.toBounds(side_opposite+1)]) /3;
        float innerwing_width = 2*Planet.edgeSize/3 -1 -(Mathf.Tan(54*Mathf.Deg2Rad) / Mathf.Tan(72*Mathf.Deg2Rad));
        float outerwing_width = Planet.edgeSize/3 - innerwing_width/2;
        float inneredge_width = Planet.edgeSize -2*(1 +Mathf.Tan(54*Mathf.Deg2Rad)/Mathf.Tan(72*Mathf.Deg2Rad));
        float outeredge_width = (Planet.edgeSize - inneredge_width)/2;

        // Calculate front sticker extreme vertices
        shape_vertices_front = new Vector3[] {
            base_left,
            base_left + (base_right-base_left)*outerwing_width/(2*outerwing_width+innerwing_width),
            ((inneredge_width+outeredge_width)*face.vertices[side] + outeredge_width*face.vertices[Face.toBounds(side+1)]) /Planet.edgeSize,
            face.vertices[side] + (face.vertices[Face.toBounds(side+1)]-face.vertices[side]) *(outeredge_width/2) /Planet.edgeSize
        };

        // Calculate rear sticker extreme vertices
        shape_vertices_rear = new Vector3[] {
            shape_vertices_front[2],
            shape_vertices_front[3],
            base_left_opposite,
            base_left_opposite + (base_right_opposite-base_left_opposite)*outerwing_width/(2*outerwing_width+innerwing_width)
        };

        // Prepare projection
        sticker_front.SetProjections(
            new [] {
                (face.vertices[side] - face.vertices[Face.toBounds(side+1)]).normalized,
                (face.vertices[Face.toBounds(side-1)] - face.vertices[side]).normalized
            },
            new [] {
                face_opposite,
                face.adjacent_faces[Face.toBounds(side-1)]
            },
            new [] {
                false,
                true
            }
        );
        sticker_rear.SetProjections(
            new [] {
                (face.vertices[Face.toBounds(side+1)] - face.vertices[side]).normalized,
                (face.vertices[side] - face_opposite.vertices[Face.toBounds(side_opposite+2)]).normalized
            },
            new [] {
                face,
                face.adjacent_faces[Face.toBounds(side-1)]
            },
            new [] {
                false,
                true
            }
        );

        // Create mesh
        vertices = new Vector3[] {
            shape_vertices_front[0],
            shape_vertices_front[1],
            shape_vertices_front[2],
            shape_vertices_front[3],
            shape_vertices_rear[2],
            shape_vertices_rear[3]
        };
        
        transform.localPosition = (vertices[0] +vertices[1] +vertices[2] +vertices[3] +vertices[4]+vertices[5]) /6;

        MeshFilter mesh_f = gameObject.GetComponent<MeshFilter>();
        Mesh m = CreateMesh();
        mesh_f.mesh = m;
    }

    private Mesh CreateMesh() {
        Mesh m = new Mesh();

        Vector3[] newVertices = new [] {
            vertices[0]-transform.localPosition,
            vertices[1]-transform.localPosition,
            vertices[2]-transform.localPosition,
            vertices[3]-transform.localPosition,
            vertices[4]-transform.localPosition,
            vertices[5]-transform.localPosition
        };
        int[] newTriangles = new [] {
            3, 1, 0,
            2, 1, 3,
            3, 4, 2,
            4, 5, 2
        };
        m.vertices = newVertices;
        m.triangles = newTriangles;

        return m;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
