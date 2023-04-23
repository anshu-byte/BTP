using Godot;
using System;
using System.Collections.Generic;

public class DrawPose : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    private List<KeyPoint> points = new List<KeyPoint>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    public override void _Draw(){
        var size= GetViewportRect().Size;
        float scalefactor;
        Vector2 offset;
        if(size.x > size.y){
            scalefactor = size.y;
            offset = new Vector2((size.x-size.y)/2,0);
        }else{
            scalefactor = size.x;

            offset = new Vector2(0,(size.y-size.x)/2);
        }
        foreach(var kp in points){
            var pos = new Vector2(kp.position.x,kp.position.y);
            pos = pos*scalefactor;
            pos = pos + offset;
            DrawCircle(pos, 10, new Color(1,1,1,1));
        }
    }

    public void drawPose(List<KeyPoint> pointsPose){
        points = pointsPose;
        Update();
    }


//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    
}
