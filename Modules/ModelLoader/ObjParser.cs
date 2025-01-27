using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class ObjParser : Object
{
    private const bool debug = false;
    private const int MAX_FILE_SIZE = 1024 * 1024 * 10; // bytes

    // Create mesh from obj and mtl paths
    public static Mesh LoadObj(string objPath, string mtlPath = "")
    {
        string fullPath = ProjectSettings.GlobalizePath(objPath);
        int fileSize = GetFileSize(fullPath);
        if (fileSize > MAX_FILE_SIZE)
        {
            //ContextMenu.ShowMessageS($"Размер модели больше {(int)MAX_FILE_SIZE /1024 / 1024}мб. Модель не загружена.");
            GD.PrintErr($"Размер модели больше {(int)MAX_FILE_SIZE / 1024 / 1024}мб. Модель не загружена.");
            return null;
        }

        string objStr = _ReadFileStr(objPath);

        if (mtlPath == "")
        {
            string mtlFilename = _GetMtlFilename(objStr);
            mtlPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(objPath), mtlFilename);
        }
        var mats = new Dictionary<string, StandardMaterial3D>();
        if (mtlPath != "")
        {
            mats = _CreateMtl(_ReadFileStr(mtlPath), _GetMtlTex(mtlPath));
        }
        if (string.IsNullOrEmpty(objStr))
            return null;

        //ContextMenu.ShowMessageS($"Загружена модель {objPath}");
        GD.Print($"Загружена модель {objPath}");

        return _CreateObj(objStr, mats);
    }

    // Create mesh from obj, materials. Materials should be { "matname": data }
    public static Mesh LoadObjFromBuffer(string objData, Dictionary<string, StandardMaterial3D> materials)
    {
        return _CreateObj(objData, materials);
    }

    // Create materials
    public static Dictionary<string, StandardMaterial3D> LoadMtlFromBuffer(string mtlData, Dictionary<string, Image> textures)
    {
        return _CreateMtl(mtlData, textures);
    }

    // Get data from file path
    private static string _ReadFileStr(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "";
        string fullPath = ProjectSettings.GlobalizePath(path);
        var file = FileAccess.Open(fullPath, FileAccess.ModeFlags.Read);
        if (file == null)
            return "";
        string content = file.GetAsText();
        return content;
    }

    // Internal functions

    // Get textures from mtl path (returns { "tex_path": data })
    public static Dictionary<string, Image> _GetMtlTex(string mtlPath)
    {
        var filePaths = _GetMtlTexPaths(mtlPath);
        var textures = new Dictionary<string, Image>();

        foreach (var k in filePaths)
        {
            var img = _GetImage(mtlPath, k);

            if (img != null)
            {
                textures[k] = img;
            }
            else
            {
                GD.Print("Warning: Could not load image for texture: ", k);
            }
        }
        return textures;
    }

    // Get textures paths from mtl path
    public static List<string> _GetMtlTexPaths(string mtlPath)
    {
        var file = FileAccess.Open(mtlPath, FileAccess.ModeFlags.Read);
        if (file == null)
            return new List<string>();
        var paths = new List<string>();
        var lines = file.GetAsText().Split(new[] { '\n' }, StringSplitOptions.None);
        foreach (var line in lines)
        {
            //var parts = line.Replace("\t", "").Replace("\r", "").Replace("\\\\", "/").Replace("\\", "/").Split(" ").ToList();

            if (SplitData(line, out var key, out var data))
            {
                if (key == "map_Kd" || key == "map_Ks" || key == "map_Ka")
                {
                    if (!paths.Contains(data))
                    {
                        paths.Add(data);
                    }
                }
            }
        }
        return paths;
    }

    private static bool SplitData(string line, out string key, out string data)
    {
        var parts = line.Replace("\\\\", "/").Replace("\\", "/").Replace("\r", "").Replace("\t", "").Split(" ").ToList();
        key = "";
        data = "";

        if (parts.Count == 0) return false;

        key = parts[0];

        parts.RemoveAt(0);
        data = string.Join(' ', parts);

        if (data.Length > 0)
            if (data[0] == ' ')
                data = data.Substring(1);

        return true;
    }

    private static string _GetMtlFilename(string obj)
    {
        var lines = obj.Split('\n');
        foreach (var line in lines)
        {
            var split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2) continue;
            if (split[0] != "mtllib") continue;
            return split[1].Trim();
        }
        return "";
    }

    private static Dictionary<string, StandardMaterial3D> _CreateMtl(string obj, Dictionary<string, Image> textures)
    {
        var mats = new Dictionary<string, StandardMaterial3D>();
        StandardMaterial3D currentMat = null;

        var lines = obj.Split(new[] { '\n' }, StringSplitOptions.None);
        foreach (var line in lines)
        {
            var parts = line.Replace("\t", "").Replace("\r", "").Replace(".", ",").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                continue;
            switch (parts[0])
            {
                case "#":
                    // Comment
                    break;
                case "newmtl":
                    // Create a new material
                    if (debug)
                    {
                        GD.Print("Adding new material ", parts[1]);
                    }
                    currentMat = new StandardMaterial3D();
                    mats[parts[1]] = currentMat;
                    break;
                case "Ka":
                    // Ambient color
                    //currentMat.AlbedoColor = new Color(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
                    break;
                case "Kd":
                    // Diffuse color
                    if (currentMat != null && parts.Length >= 4)
                    {
                        currentMat.AlbedoColor = new Color(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
                        if (debug)
                        {
                            GD.Print("Setting material color ", currentMat.AlbedoColor.ToString());
                        }
                    }
                    break;
                default:
                    if (SplitData(line, out var key, out var data))
                    {
                        if (key == "map_Kd" || key == "map_Ks" || key == "map_Ka")
                        {
                            if (textures.ContainsKey(data) && currentMat != null)
                            {
                                currentMat.AlbedoTexture = ImageTexture.CreateFromImage(textures[data]);
                            }
                        }
                    }
                    break;
            }
        }
        return mats;
    }

    private static Dictionary<string, StandardMaterial3D> _ParseMtlFile(string path)
    {
        return _CreateMtl(_ReadFileStr(path), _GetMtlTex(path));
    }

    private static Image _GetImage(string mtlFilepath, string texFilename)
    {
        if (debug)
        {
            GD.Print("Debug: Mapping texture file ", texFilename);
        }

        mtlFilepath = ProjectSettings.GlobalizePath(mtlFilepath);

        string texFilepath = texFilename.Replace("/","\\");
        if (!System.IO.Path.IsPathRooted(texFilename))
        {
            texFilepath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(mtlFilepath), texFilename);
        }

        // Check if file exists at the specified path
        if (!FileAccess.FileExists(texFilepath))
        {
            // If not, search in the directory where mtlFilepath is
            texFilepath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(mtlFilepath), System.IO.Path.GetFileName(texFilename));
            if (!FileAccess.FileExists(texFilepath))
            {
                GD.Print("Error: Texture file not found: ", texFilename);
                return null;
            }
        }

        if (debug)
        {
            GD.Print("Debug: Texture file path: ", texFilepath);
        }

        Image img = new Image();
        var err = img.Load(texFilepath);

        System.IO.FileInfo fileInfo = new System.IO.FileInfo(texFilepath);
        long fileSizeInBytes = fileInfo.Length;

        if (fileSizeInBytes > MAX_FILE_SIZE)
        {
            GD.Print($"Размер текстуры {texFilepath} больше {(int)MAX_FILE_SIZE / 1024 / 1024}мб. Текстура не загружена.");
            return null;
        }

        if (err != Error.Ok)
        {
            GD.Print("Error: Failed to load image from path: ", texFilepath);
            return null;
        }

        return img;
    }

    private static Mesh _CreateObj(string obj, Dictionary<string, StandardMaterial3D> mats)
    {
        // Setup
        ArrayMesh mesh = new ArrayMesh();
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var faces = new Dictionary<string, List<Dictionary<string, List<int>>>>();

        string matName = "default";
        int countMtl = 0;

        // Parse
        var lines = obj.Split(new[] { '\n' }, StringSplitOptions.None);
        foreach (var lineRaw in lines)
        {
            var line = lineRaw.Replace("\r", "").Replace(".", ",");

            if (string.IsNullOrEmpty(line))
                continue;

            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                continue;

            switch (parts[0])
            {
                case "#":
                    // Comment
                    break;
                case "v":
                    // Vertex
                    vertices.Add(new Vector3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3])));
                    break;
                case "vn":
                    // Normal
                    normals.Add(new Vector3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3])));
                    break;
                case "vt":
                    // UV
                    uvs.Add(new Vector2(float.Parse(parts[1]), 1 - float.Parse(parts[2])));
                    break;
                case "usemtl":
                    // Material group
                    countMtl += 1;
                    matName = parts[1].Trim();
                    if (!faces.ContainsKey(matName))
                    {
                        var matsKeys = new List<string>(mats.Keys);
                        if (!mats.ContainsKey(matName))
                        {
                            if (matsKeys.Count > countMtl)
                            {
                                matName = matsKeys[countMtl];
                            }
                        }
                        faces[matName] = new List<Dictionary<string, List<int>>>();
                    }
                    break;
                case "f":
                    if (!faces.ContainsKey(matName))
                    {
                        var matsKeys = new List<string>(mats.Keys);
                        if (matsKeys.Count > countMtl)
                        {
                            matName = matsKeys[countMtl];
                        }
                        faces[matName] = new List<Dictionary<string, List<int>>>();
                    }
                    // Face
                    if (parts.Length == 4)
                    {
                        // Tri
                        var face = new Dictionary<string, List<int>>
                        {
                            { "v", new List<int>() },
                            { "vt", new List<int>() },
                            { "vn", new List<int>() }
                        };
                        foreach (var map in parts)
                        {
                            var verticesIndex = map.Split('/');
                            if (verticesIndex[0] != "f")
                            {
                                face["v"].Add(int.Parse(verticesIndex[0]) - 1);
                                face["vt"].Add(int.Parse(verticesIndex[1]) - 1);
                                if (verticesIndex.Length > 2)
                                {
                                    face["vn"].Add(int.Parse(verticesIndex[2]) - 1);
                                }
                            }
                        }
                        if (faces.ContainsKey(matName))
                        {
                            faces[matName].Add(face);
                        }
                    }
                    else if (parts.Length > 4)
                    {
                        // Triangulate
                        var points = new List<List<int>>();
                        foreach (var map in parts)
                        {
                            if (!string.IsNullOrEmpty(map))
                            {
                                var verticesIndex = map.Split('/');
                                if (verticesIndex[0] != "f")
                                {
                                    var point = new List<int>();
                                    point.Add(int.Parse(verticesIndex[0]) - 1);
                                    point.Add(int.Parse(verticesIndex[1]) - 1);
                                    if (verticesIndex.Length > 2)
                                    {
                                        point.Add(int.Parse(verticesIndex[2]) - 1);
                                    }
                                    points.Add(point);
                                }
                            }
                        }
                        for (int i = 0; i < points.Count; i++)
                        {
                            if (i != 0)
                            {
                                var face = new Dictionary<string, List<int>>
                                {
                                    { "v", new List<int>() },
                                    { "vt", new List<int>() },
                                    { "vn", new List<int>() }
                                };
                                var point0 = points[0];
                                var point1 = points[i];
                                var point2 = points[i - 1];
                                face["v"].Add(point0[0]);
                                face["v"].Add(point2[0]);
                                face["v"].Add(point1[0]);
                                face["vt"].Add(point0[1]);
                                face["vt"].Add(point2[1]);
                                face["vt"].Add(point1[1]);
                                if (point0.Count > 2)
                                {
                                    face["vn"].Add(point0[2]);
                                }
                                if (point2.Count > 2)
                                {
                                    face["vn"].Add(point2[2]);
                                }
                                if (point1.Count > 2)
                                {
                                    face["vn"].Add(point1[2]);
                                }
                                faces[matName].Add(face);
                            }
                        }
                    }
                    break;
            }
        }

        // Make tri
        foreach (var matgroup in faces.Keys)
        {
            if (debug)
            {
                GD.Print("Creating surface for matgroup ", matgroup, " with ", faces[matgroup].Count.ToString(), " faces");
            }

            // Mesh Assembler
            SurfaceTool st = new SurfaceTool();
            st.Begin(Mesh.PrimitiveType.Triangles);
            if (!mats.ContainsKey(matgroup))
            {
                mats[matgroup] = new StandardMaterial3D();
            }
            st.SetMaterial(mats[matgroup]);
            foreach (var face in faces[matgroup])
            {
                if (face["v"].Count == 3)
                {
                    // Vertices
                    var fanV = new List<Vector3>
                    {
                        vertices[face["v"][0]],
                        vertices[face["v"][2]],
                        vertices[face["v"][1]]
                    };

                    // Normals
                    var fanVn = new List<Vector3>();
                    if (face["vn"].Count > 0)
                    {
                        fanVn.Add(normals[face["vn"][0]]);
                        fanVn.Add(normals[face["vn"][2]]);
                        fanVn.Add(normals[face["vn"][1]]);
                    }

                    // UVs
                    var fanVt = new List<Vector2>();
                    if (face["vt"].Count > 0)
                    {
                        foreach (var k in new int[] { 0, 2, 1 })
                        {
                            int f = face["vt"][k];
                            if (f > -1)
                            {
                                var uv = uvs[f];
                                fanVt.Add(uv);
                            }
                        }
                    }

                    // Add triangle fan
                    for (int i = 0; i < 3; i++)
                    {
                        if (face["vn"].Count > 0)
                        {
                            st.SetNormal(fanVn[i]);
                        }
                        if (face["vt"].Count > 0)
                        {
                            st.SetUV(fanVt[i]);
                        }
                        st.AddVertex(fanV[i]);
                    }
                }
            }

            st.Index();

            st.Commit(mesh);
        }

        for (int k = 0; k < mesh.GetSurfaceCount(); k++)
        {
            var mat = mesh.SurfaceGetMaterial(k);
            matName = "";
            foreach (var m in mats.Keys)
            {
                if (mats[m] == mat)
                {
                    matName = m;
                }
            }
            mesh.SurfaceSetName(k, matName);
        }

        // Finish
        return mesh;
    }

    private static int GetFileSize(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return -1;
        //string fullPath = ProjectSettings.GlobalizePath(filePath);

        try
        {
            var fileInfo = new System.IO.FileInfo(filePath);
            return (int)fileInfo.Length;
        }
        catch (Exception e)
        {
            return -1; // Возвращаем -1 в случае ошибки
        }
    }
}
