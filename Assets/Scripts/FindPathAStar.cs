using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathMarker
{
    public MapLocation location;
    public float G;
    public float H;
    public float F;
    public GameObject marker;
    public PathMarker parent;

    public PathMarker(MapLocation l, float g, float h, float f, GameObject marker, PathMarker p)
    {
        location = l;
        G = g;
        H = h;
        F = f;
        this.marker = marker;
        parent = p;
    }
    public override bool Equals(object obj)
    {
        if (obj == null || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }

        return location.Equals(((PathMarker) obj).location);
    }

    public override int GetHashCode()
    {
        return 0;
    }
}

public class FindPathAStar : MonoBehaviour
{
    public MazeGenerator maze;
    public Material closedMaterial;
    public Material openMaterial;

    List<PathMarker> open = new List<PathMarker>();
    List<PathMarker> closed = new List<PathMarker>();

    public GameObject start;
    public GameObject end;
    public GameObject pathP;

    PathMarker goalNode;
    PathMarker startNode;
    PathMarker lastPos;

    bool done = false;
    float yPos = 0.71f;
    void RemoveAllMarkers()
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");
        foreach (GameObject m in markers)
        {
            Destroy(m);
        }
    }

    void BeginSearch()
    {
        done = false;
        RemoveAllMarkers();

        List<MapLocation> locations = new List<MapLocation>();
        for (int z = 0; z < maze.mazeDepth - 1; z++)
        {
            for (int x = 0; x < maze.mazeWidth - 1; x++) {
                //TODO: Inverti o x e o z na validação
                if (maze.maze[z,x] != 1)
                {
                    locations.Add(new MapLocation(x,z));
                }
            }
        }

        var lastIndex = locations.Count() - 1;

        Vector3 startLocation = new Vector3(maze.xi + locations[0].x * maze.tileWidth, yPos, maze.zi - locations[0].z * maze.tileDepth);
        startNode = new PathMarker(new MapLocation(locations[0].x, locations[0].z), 0, 0, 0, Instantiate(start, startLocation, Quaternion.identity), null);

        Vector3 goalLocation = new Vector3(maze.xi + locations[lastIndex].x * maze.tileWidth, yPos, maze.zi - locations[lastIndex].z * maze.tileDepth);
        goalNode = new PathMarker(new MapLocation(locations[lastIndex].x, locations[lastIndex].z), 0, 0, 0, Instantiate(end, goalLocation, Quaternion.identity), null);

        open.Clear();
        closed.Clear();
        open.Add(startNode);
        lastPos = startNode;
    }

    void Search(PathMarker thisNode)
    {
        if (thisNode.Equals(goalNode))
        {
            done = true;
            return;
        }

        foreach(MapLocation dir in maze.directions)
        {
            MapLocation neighbour = dir + thisNode.location;
            // Todo coloquei essa validação antes de acessar o maze.maze[neighbour.z, neighbour.x]
            if (neighbour.x < 1 || neighbour.x >= maze.mazeWidth || neighbour.z < 1 || neighbour.z >= maze.mazeDepth)
            {
                continue;
            }

            //TODO: inverti o x e o z no if
            if (maze.maze[neighbour.z, neighbour.x] == 1)
            {
                continue;
            }

            if (IsClosed(neighbour))
            {
                continue;
            }

            float G = Vector2.Distance(thisNode.location.ToVector(), neighbour.ToVector()) + thisNode.G;
            float H = Vector2.Distance(neighbour.ToVector(), goalNode.location.ToVector());
            float F = G + H;

            GameObject pathBlock = Instantiate(
                pathP,
                new Vector3(maze.xi + neighbour.x * maze.tileWidth, yPos, maze.zi - neighbour.z * maze.tileDepth),
                Quaternion.identity
            );

            TextMesh[] values = pathBlock.GetComponentsInChildren<TextMesh>();
            values[0].text = "G: " + G.ToString("0.00");
            values[1].text = "H: " + H.ToString("0.00");
            values[2].text = "F: " + F.ToString("0.00");

            if (!UpdateMarker(neighbour, G, H, F, thisNode))
            {
                open.Add(new PathMarker(neighbour, G, H, F, pathBlock, thisNode));
            }
        }

        open = open.OrderBy(p => p.F).ToList<PathMarker>();
        PathMarker pm = (PathMarker)open.ElementAt(0);
        closed.Add(pm);

        open.RemoveAt(0);
        //pm.marker.GetComponent<Renderer>().material = closedMaterial;

        lastPos = pm;
    }

    bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt)
    {
        foreach(PathMarker p in open)
        {
            if (p.location.Equals(pos))
            {
                p.G = g;
                p.H = h;
                p.F = f;
                p.parent = prt;
                return true;
            }
        }
        return false;
    }

    bool IsClosed(MapLocation marker)
    {
        foreach(PathMarker p in closed)
        {
            if (p.location.Equals(marker))
            {
                return true;
            }
        }

        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void GetPath()
    {
        RemoveAllMarkers();
        PathMarker begin = lastPos;

        while(!startNode.Equals(begin) && begin != null)
        {
            Instantiate(pathP, new Vector3(maze.xi + begin.location.x * maze.tileWidth, yPos, maze.zi - begin.location.z * maze.tileDepth), Quaternion.identity);

            begin = begin.parent;
        }

        //Instantiate(pathP, new Vector3(maze.xi + startNode.location.x * maze.tileWidth, yPos, maze.zi - startNode.location.z * maze.tileDepth), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            BeginSearch();
        }

        if (Input.GetKeyDown(KeyCode.C) && !done)
        {
            Search(lastPos);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            GetPath();
        }
    }
}
