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

    private int posInd = 0;
    //private List<KeyPoint> points = new List<KeyPoint>();
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        skeleton = GetNode<Skeleton>("mh1/mh1/Skeleton");

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
        foreach (var kp in points)
        {
            var bone = skeleton.FindBone(getNameForPart(kp.bodyPart));
            if (bone >= 0)
            {
                var boneP = skeleton.GetBoneGlobalPose(bone);

                boneP.origin.x = kp.position.x;
                boneP.origin.z = kp.position.y;



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
                var pos = new Vector3();
                pos.x = kp.position.x;
                pos.z = kp.position.y;
                pos.y = boneP.origin.y;
                // pos.z = 1;

                mesh.Transform = boneP;
                var scale = new Vector3();
                scale.x = 0.3F;
                scale.y = 0.3F;
                scale.z = 0.3F;
                mesh.Scale = scale;

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
                GD.Print("Applying for ", kp.bodyPart);

                // skeleton.SetBoneGlobalPoseOverride(bone, boneP, 1.0F, true);
            }
        }
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
    public void OnHttpRequestComplete(long result, long ResponseCode, string[] headers, byte[] body)
    {
        //result
        string sbody = System.Text.UTF8Encoding.UTF8.GetString(body);
        var points = JsonConvert.DeserializeObject<List<KeyPoint>>(sbody);
        applyPose(points);

        httpRequest.Request("https://cp303-f1482-default-rtdb.asia-southeast1.firebasedatabase.app/Stream.json");

        GD.Print(sbody);

    }
    public String getNameForPart(BodyPart part)
    {
        switch (part)
        {
            case BodyPart.NOSE:
                return "";
            case BodyPart.LEFT_ANKLE:
                return "foot_L";
            case BodyPart.RIGHT_ANKLE:
                return "foot_R";
            case BodyPart.LEFT_KNEE:
                return "lowerleg01_L";
            case BodyPart.RIGHT_KNEE:
                return "lowerleg01_R";
            case BodyPart.LEFT_HIP:
                return "upperleg02_L";
            case BodyPart.RIGHT_HIP:
                return "upperleg02_R";
            case BodyPart.LEFT_SHOULDER:
                return "upperarm01_L";
            case BodyPart.RIGHT_SHOULDER:
                return "upperarm01_R";
            case BodyPart.LEFT_ELBOW:
                return "lowerarm01_L";
            case BodyPart.RIGHT_ELBOW:
                return "lowerarm01_R";
            case BodyPart.LEFT_WRIST:
                return "wrist_L";
            case BodyPart.RIGHT_WRIST:
                return "wrist_R";

        }
        return "";
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
                return "upperleg01_L";
            case BodyPart.RIGHT_HIP:
                return "upperleg01_R";
            case BodyPart.LEFT_SHOULDER:
                return "shoulder01_L";
            case BodyPart.RIGHT_SHOULDER:
                return getNameForPart(BodyPart.RIGHT_SHOULDER);
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