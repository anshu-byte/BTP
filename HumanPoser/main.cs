using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;

//body part enum same as in kotlin 
[JsonConverter(typeof(StringEnumConverter))]
public enum BodyPart
{
    NOSE,
    LEFT_EYE,
    RIGHT_EYE,
    LEFT_EAR,
    RIGHT_EAR,
    LEFT_SHOULDER,
    RIGHT_SHOULDER,
    LEFT_ELBOW,
    RIGHT_ELBOW,
    LEFT_WRIST,
    RIGHT_WRIST,
    LEFT_HIP,
    RIGHT_HIP,
    LEFT_KNEE,
    RIGHT_KNEE,
    LEFT_ANKLE,
    RIGHT_ANKLE
}


public struct Position
{
    public float x;
    public float y;

}


public struct KeyPoint
{
    public BodyPart bodyPart;
    public Position position;
    public float score;
}


public class main : Spatial
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    private Skeleton skeleton;

    private Dictionary<int, MeshInstance> ballMaps = new Dictionary<int, MeshInstance>();
    private Dictionary<int, SkeletonIK> IKMaps = new Dictionary<int, SkeletonIK>();

    private HTTPRequest httpRequest;

    // private Node2D board2d;

    private int posInd = 0;
    //private List<KeyPoint> points = new List<KeyPoint>();
    // Called when the node enters the scene tree for the first time.

    public DrawPose drawPose;

    private List<KeyPoint> points = new List<KeyPoint>();

    public Camera camera;

    public Spatial newguy2;


    public override void _Ready()
    {
        skeleton = GetNode<Skeleton>("newguy2/newguy2/Skeleton");

        camera = GetNode<Camera>("Camera");

        newguy2 = GetNode<Spatial>("newguy2");

        drawPose = (DrawPose)GetNode<Node2D>("DrawPose");

        // board2d = GetNode<Node2D>("2DBoard");

        var bodyParts = Enum.GetValues(typeof(BodyPart)).Cast<BodyPart>().ToList();
        // foreach (var bodyPart in bodyParts)
        // {
        //     var bone = skeleton.FindBone(getNameForPart(bodyPart));

        //     GD.Print("finding pose for ", getNameForPart(bodyPart), bone);
        //     if (bone >= 0)
        //     {
        //         var boneP = skeleton.GetBoneGlobalPose(bone).origin;
        //         var positon = new Position();
        //         positon.x = boneP.x;
        //         positon.y = boneP.z;

        //         var kp = new KeyPoint();
        //         kp.bodyPart = bodyPart;
        //         kp.score = 1;
        //         kp.position = positon;
        //         points.Add(kp);
        //     }
        // }

        // var json = JsonConvert.SerializeObject(points);

        // GD.Print(json);
        //this is will be changes as we are initially doing it using a file, we will later do this through network 
        httpRequest = new HTTPRequest();
        AddChild(httpRequest);
        httpRequest.Connect("request_completed", this, "OnHttpRequestComplete");

        httpRequest.Request("https://cp303-f1482-default-rtdb.asia-southeast1.firebasedatabase.app/Stream.json");

        var GFile = new Godot.File();
        GFile.Open("res://defaultpose.json", File.ModeFlags.Read);
        string json = GFile.GetAsText();
        GFile.Close();
        //json convert
        // var points = JsonConvert.DeserializeObject<List<KeyPoint>>(json);
        // applyPose(points);

        // var timer = new Timer();
        // AddChild(timer);
        // //timer.Start(1);

        // timer.Connect("timeout", this, "timeout");
    }


    public void timeout()
    {
        posInd += 1;
        var GFile = new Godot.File();
        if (!GFile.FileExists("res://poses/" + posInd.ToString() + ".json"))
        {
            return;
        }
        GFile.Open("res://poses/" + posInd.ToString() + ".json", File.ModeFlags.Read);
        string json = GFile.GetAsText();
        var points = JsonConvert.DeserializeObject<List<KeyPoint>>(json);
        applyPose(points);
        GFile.Close();
    }


    public void applyPose(List<KeyPoint> points)
    {

        this.points = points;
                drawPose.drawPose(points);

        var size = drawPose.GetViewportRect().Size;
        float scalefactor;
        Vector2 offset;
        if (size.x > size.y)
        {
            scalefactor = size.y;
            offset = new Vector2((size.x - size.y) / 2, 0);
        }
        else
        {
            scalefactor = size.x;

            offset = new Vector2(0, (size.y - size.x) / 2);
        }

        // var upperPartbones = upperParts();

        // for(var i=0;  i<points.Count; i++){
        //     var kp = points[i];
        //     if(upperPartbones.Contains(kp.bodyPart)){
        //         var pos = kp.position;
        //         pos.y -= 0.07F;
        //         kp.position = pos;
        //         points[i]=kp;
        //     }
        // }
        Vector2? LE = null;
        Vector2? RE = null;
        Vector2? minY = null;
        Vector2? maxY = null;

        Vector2? minX = null;
        Vector2? maxX = null;
        foreach (var kp in points)
        {
            if (minY == null || ((Vector2)minY).y > kp.position.y)
            {
                minY = new Vector2(kp.position.x, kp.position.y);
            }
            if (maxY == null || ((Vector2)maxY).y < kp.position.y)
            {
                maxY = new Vector2(kp.position.x, kp.position.y);
            }

            if (minX == null || ((Vector2)minX).x > kp.position.x)
            {
                minX = new Vector2(kp.position.x, kp.position.y);
            }
            if (maxX == null || ((Vector2)maxX).x < kp.position.x)
            {
                maxX = new Vector2(kp.position.x, kp.position.y);
            }
            if (kp.bodyPart == BodyPart.LEFT_EYE || kp.bodyPart == BodyPart.RIGHT_EYE)
            {
                var kp_pos = new Vector2(kp.position.x, kp.position.y);

                if (kp.bodyPart == BodyPart.LEFT_EYE)
                {
                    LE = kp_pos;
                }
                else
                {
                    RE = kp_pos;
                }
            }
        }
        GD.Print("minXY ", minX,minY, "maxXY", maxX, maxY);
        if (LE != null && RE != null)
        {
            var distance = ((Vector2) maxY).y-((Vector2) minY).y;
            var idealDisntace  = 1.4F;
            var scale = distance/idealDisntace;


            GD.Print("distance ", distance, "ideal Distance", idealDisntace, "scale ", scale);

            newguy2.Scale = new Vector3(scale,scale,scale);

        }



        GD.Print("Reset");

        Vector3? LK = null;
        Vector3? RK = null;
        foreach (var kp in points)
        {
            if (kp.bodyPart == BodyPart.LEFT_ANKLE || kp.bodyPart == BodyPart.RIGHT_ANKLE)
            {
                var kp_pos = new Vector2(kp.position.x, kp.position.y);
                kp_pos = kp_pos * scalefactor;
                kp_pos = kp_pos + offset;
                // GD.Print("Projecting ",kp_pos);
                var camera_origin = camera.ProjectRayOrigin(kp_pos);
                // GD.Print("Pos is ",camera_origin);
                camera_origin.z = 0;

                if (kp.bodyPart == BodyPart.LEFT_ANKLE)
                {
                    LK = camera_origin;
                }
                else
                {
                    RK = camera_origin;
                }
            }
        }
        if (LK != null && RK != null)
        {
            var center = (RK - LK) * 0.5F + LK;
            var gt = skeleton.GlobalTransform;
            gt.origin = (Vector3)center;
            skeleton.GlobalTransform = gt;
        }


        foreach (var kp in points)
        {
            if (kp.bodyPart.ToString().Contains("LEFT"))
            {
                applyKP(scalefactor, offset, kp);
            }
        }

        foreach (var kp in points)
        {
            if (kp.bodyPart.ToString().Contains("RIGHT"))
            {
                applyKP(scalefactor, offset, kp);
            }
        }

    }

    private void applyKP(float scalefactor, Vector2 offset, KeyPoint kp)
    {
        var kp_pos = new Vector2(kp.position.x, kp.position.y);
        kp_pos = kp_pos * scalefactor;
        kp_pos = kp_pos + offset;
        // GD.Print("Projecting ",kp_pos);
        var camera_origin = camera.ProjectRayOrigin(kp_pos);
        // GD.Print("Pos is ",camera_origin);
        camera_origin.z = 0;

        var bone = skeleton.FindBone(getNameForPart(kp.bodyPart));
        if (bone >= 0)
        {


            var skeletonGlobalTransform = skeleton.GlobalTransform;

            var boneP = skeleton.GetBoneGlobalPose(bone);
            boneP.origin = camera_origin + skeletonGlobalTransform.origin;



            // skeleton.SetBoneGlobalPoseOverride(bone, boneP, 1, true);
            MeshInstance mesh;

            if (ballMaps.ContainsKey(bone))
            {
                mesh = ballMaps[bone];
            }
            else
            {

                mesh = new MeshInstance();
                mesh.Mesh = new SphereMesh();

                skeleton.AddChild(mesh);
                ballMaps[bone] = mesh;
            }
            // pos.z = 1;

            // mesh.Transform = boneP;
            var gd = mesh.GlobalTransform;
            gd.origin = camera_origin;
            mesh.GlobalTransform = gd;
            var scale = new Vector3();
            scale.x = 0.001F;
            scale.y = 0.001F;
            scale.z = 0.001F;
            mesh.Scale = scale;


            if (getIKParentForPart(kp.bodyPart) != "")
            {

                SkeletonIK ik;
                if (IKMaps.ContainsKey(bone))
                {
                    ik = IKMaps[bone];
                }
                else
                {
                    ik = new SkeletonIK();
                    skeleton.AddChild(ik);


                }
                ik.RootBone = getIKParentForPart(kp.bodyPart);

                // ik.RootBone = "root";
                ik.TipBone = skeleton.GetBoneName(bone);
                ik.Interpolation = 1.0F;
                IKMaps[bone] = ik;

                ik.Target = mesh.GlobalTransform;
                ik.Start(true);
                GD.Print("Applying for ", kp.bodyPart, "root ", getIKParentForPart(kp.bodyPart));

                skeleton.SetBoneGlobalPoseOverride(bone, boneP, 0F, true);
            }

        }
    }

    // public override void _PhysicsProcess(float delta)
    // {
    //     var state = GetWorld().getP
    //     base._PhysicsProcess(delta);
    // }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
    public void OnHttpRequestComplete(long result, long ResponseCode, string[] headers, byte[] body)
    {
        //result
        string sbody = System.Text.UTF8Encoding.UTF8.GetString(body);
        //string sbody = "[\n    {\n        \"bodyPart\": \"LEFT_SHOULDER\",\n        \"position\": {\n            \"x\": 1.77348745,\n            \"y\": -13.3850117\n        },\n        \"score\": 1.0\n    },\n    {\n        \"bodyPart\": \"RIGHT_SHOULDER\",\n        \"position\": {\n            \"x\": -1.77348745,\n            \"y\": -13.3850117\n        },\n        \"score\": 1.0\n    },\n    {\n        \"bodyPart\": \"LEFT_ELBOW\",\n        \"position\": {\n            \"x\": 3.41652,\n            \"y\": -11.5344677\n        },\n        \"score\": 1.0\n    },\n    {\n        \"bodyPart\": \"RIGHT_ELBOW\",\n        \"position\": {\n            \"x\": -3.41652,\n            \"y\": -11.5344677\n        },\n        \"score\": 1.0\n    },\n    {\n        \"bodyPart\": \"LEFT_WRIST\",\n        \"position\": {\n            \"x\": 4.64924335,\n            \"y\": -11.3484888\n        },\n        \"score\": 1.0\n    },\n    {\n        \"bodyPart\": \"RIGHT_WRIST\",\n        \"position\": {\n            \"x\": -4.64924335,\n            \"y\": -10.3484888\n        },\n        \"score\": 1.0\n    },\n    {\n        \"bodyPart\": \"LEFT_HIP\",\n        \"position\": {\n            \"x\": 0.9362055,\n            \"y\": -8.220213\n        },\n        \"score\": 1.0\n    },\n    {\n        \"bodyPart\": \"RIGHT_HIP\",\n        \"position\": {\n            \"x\": -0.9362055,\n            \"y\": -8.220213\n        },\n        \"score\": 1.0\n    },\n    {\n        \"bodyPart\": \"LEFT_KNEE\",\n        \"position\": {\n            \"x\": 1.38631427,\n            \"y\": -4.78811836\n        },\n        \"score\": 1.0\n    },\n    {\n        \"bodyPart\": \"RIGHT_KNEE\",\n        \"position\": {\n            \"x\": -1.38631427,\n            \"y\": -4.78811836\n        },\n        \"score\": 1.0\n    },\n    {\n        \"bodyPart\": \"LEFT_ANKLE\",\n        \"position\": {\n            \"x\": 1.96998892,\n            \"y\": -1.678689\n        },\n        \"score\": 1.0\n    },\n    {\n        \"bodyPart\": \"RIGHT_ANKLE\",\n        \"position\": {\n            \"x\": -1.96998892,\n            \"y\": -1.678689\n        },\n        \"score\": 1.0\n    }\n]";
        var points = JsonConvert.DeserializeObject<List<KeyPoint>>(sbody);
        applyPose(points);

        httpRequest.Request("https://cp303-f1482-default-rtdb.asia-southeast1.firebasedatabase.app/Stream.json");

        // GD.Print(sbody);

    }
    public String getNameForPart(BodyPart part)
    {
        switch (part)
        {
            case BodyPart.NOSE:
                return "";
            case BodyPart.LEFT_ANKLE:
                return "foot_l";
            case BodyPart.RIGHT_ANKLE:
                return "foot_r";
            case BodyPart.LEFT_KNEE:
                return "calf_l";
            case BodyPart.RIGHT_KNEE:
                return "calf_r";
            case BodyPart.LEFT_HIP:
                return "thigh_l";
            case BodyPart.RIGHT_HIP:
                return "thigh_r";
            case BodyPart.LEFT_SHOULDER:
                return "upperarm_l";
            case BodyPart.RIGHT_SHOULDER:
                return "upperarm_r";
            case BodyPart.LEFT_ELBOW:
                return "lowerarm_l";
            case BodyPart.RIGHT_ELBOW:
                return "lowerarm_r";
            case BodyPart.LEFT_WRIST:
                return "hand_l";
            case BodyPart.RIGHT_WRIST:
                return "hand_r";

        }
        return "";
    }

    List<BodyPart> upperParts()
    {
        var list = new List<BodyPart>();
        list.Add(BodyPart.NOSE);
        list.Add(BodyPart.LEFT_EYE);
        list.Add(BodyPart.RIGHT_EYE);
        list.Add(BodyPart.LEFT_EAR);
        list.Add(BodyPart.RIGHT_EAR);
        list.Add(BodyPart.LEFT_SHOULDER);
        list.Add(BodyPart.RIGHT_SHOULDER);
        list.Add(BodyPart.LEFT_ELBOW);
        list.Add(BodyPart.RIGHT_ELBOW);
        list.Add(BodyPart.LEFT_WRIST);
        list.Add(BodyPart.RIGHT_WRIST);

        return list;
    }

    public String getIKParentForPart(BodyPart part)
    {
        switch (part)
        {
            case BodyPart.NOSE:
                return "";
            case BodyPart.LEFT_ANKLE:
                return getNameForPart(BodyPart.LEFT_KNEE);
            case BodyPart.RIGHT_ANKLE:
                return getNameForPart(BodyPart.RIGHT_KNEE);
            case BodyPart.LEFT_KNEE:
                return getNameForPart(BodyPart.LEFT_HIP);
            case BodyPart.RIGHT_KNEE:
                return getNameForPart(BodyPart.RIGHT_HIP);
            case BodyPart.LEFT_HIP:
                return "";
            case BodyPart.RIGHT_HIP:
                return "";
            case BodyPart.LEFT_SHOULDER:
                return "clavicle_l";
            case BodyPart.RIGHT_SHOULDER:
                return "clavicle_r";
            case BodyPart.LEFT_ELBOW:
                return getNameForPart(BodyPart.LEFT_SHOULDER);
            case BodyPart.RIGHT_ELBOW:
                return getNameForPart(BodyPart.RIGHT_SHOULDER);
            case BodyPart.LEFT_WRIST:
                return getNameForPart(BodyPart.LEFT_ELBOW);
            case BodyPart.RIGHT_WRIST:
                return getNameForPart(BodyPart.RIGHT_ELBOW);

        }
        return "";
    }


    // public String getIKParentForPart(BodyPart part)
    // {
    //     switch (part)
    //     {
    //         case BodyPart.NOSE:
    //             return "";
    //         case BodyPart.LEFT_ANKLE:
    //             return "upperleg01_L";
    //         case BodyPart.RIGHT_ANKLE:
    //             return "upperleg01_R";
    //         case BodyPart.LEFT_KNEE:
    //             return "upperleg01_L";
    //         case BodyPart.RIGHT_KNEE:
    //             return "upperleg01_R";
    //         case BodyPart.LEFT_HIP:
    //             return "upperleg01_L";
    //         case BodyPart.RIGHT_HIP:
    //             return "upperleg01_R";
    //         case BodyPart.LEFT_SHOULDER:
    //             return "shoulder01_L";
    //         case BodyPart.RIGHT_SHOULDER:
    //             return "shoulder01_R";
    //         case BodyPart.LEFT_ELBOW:
    //             return "shoulder01_L";
    //         case BodyPart.RIGHT_ELBOW:
    //             return "shoulder01_R";
    //         case BodyPart.LEFT_WRIST:
    //             return "shoulder01_L";
    //         case BodyPart.RIGHT_WRIST:
    //             return "shoulder01_R";

    //     }
    //     return "";
    // }

}